-- Add enhancement_level column to user_inventory table.
ALTER TABLE public.user_inventory
ADD COLUMN IF NOT EXISTS enhancement_level INT DEFAULT 0;

-- Expand item_templates so Supabase can validate and serve item metadata.
ALTER TABLE public.item_templates
ADD COLUMN IF NOT EXISTS description TEXT DEFAULT '',
ADD COLUMN IF NOT EXISTS equip_slot TEXT DEFAULT NULL,
ADD COLUMN IF NOT EXISTS tier TEXT DEFAULT NULL,
ADD COLUMN IF NOT EXISTS bonus_hp INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS bonus_atk_phy INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS bonus_atk_mag INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS bonus_def_phy INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS bonus_def_mag INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS bonus_speed INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS percent_hp NUMERIC DEFAULT 0,
ADD COLUMN IF NOT EXISTS percent_atk NUMERIC DEFAULT 0,
ADD COLUMN IF NOT EXISTS percent_speed NUMERIC DEFAULT 0;

-- Create/update enhance_equipment function.
CREATE OR REPLACE FUNCTION public.enhance_equipment(
    p_inventory_id UUID,
    p_item_id TEXT,
    p_max_level INT
)
RETURNS VOID
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_user_id UUID;
    v_current_level INT;
    v_gold INT;
    v_cost INT;
    v_item_type TEXT;
BEGIN
    v_user_id := auth.uid();
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'Please log in.';
    END IF;

    SELECT COALESCE(enhancement_level, 0)
    INTO v_current_level
    FROM public.user_inventory
    WHERE id = p_inventory_id
      AND user_id = v_user_id
      AND item_id = p_item_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Inventory item not found or does not belong to you.';
    END IF;

    SELECT item_type
    INTO v_item_type
    FROM public.item_templates
    WHERE id = p_item_id;

    IF v_item_type IS NULL THEN
        RAISE EXCEPTION 'Item template not found.';
    END IF;

    IF lower(v_item_type) != 'equipment' THEN
        RAISE EXCEPTION 'Only equipment can be enhanced.';
    END IF;

    IF v_current_level >= p_max_level THEN
        RAISE EXCEPTION 'Equipment already reached max enhancement level (%).', p_max_level;
    END IF;

    v_cost := 100 * (v_current_level + 1);

    SELECT gold
    INTO v_gold
    FROM public.players
    WHERE id = v_user_id;

    IF v_gold IS NULL THEN
        RAISE EXCEPTION 'Player data not found.';
    END IF;

    IF v_gold < v_cost THEN
        RAISE EXCEPTION 'Not enough gold. Need %, current %.', v_cost, v_gold;
    END IF;

    UPDATE public.players
    SET gold = gold - v_cost
    WHERE id = v_user_id;

    UPDATE public.user_inventory
    SET enhancement_level = COALESCE(enhancement_level, 0) + 1
    WHERE id = p_inventory_id
      AND user_id = v_user_id
      AND item_id = p_item_id;
END;
$$;

GRANT EXECUTE ON FUNCTION public.enhance_equipment(UUID, TEXT, INT) TO authenticated;
