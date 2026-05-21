-- Equipment is instance-based; consumables/materials remain stackable.
DROP FUNCTION IF EXISTS public.add_item(TEXT, INT);

CREATE OR REPLACE FUNCTION public.add_item(
    p_item_id TEXT,
    p_qty INT
)
RETURNS VOID
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_user_id UUID;
    v_item_type TEXT;
    v_qty INT;
BEGIN
    v_user_id := auth.uid();
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'Please log in.';
    END IF;

    v_qty := GREATEST(COALESCE(p_qty, 0), 0);
    IF v_qty <= 0 THEN
        RETURN;
    END IF;

    SELECT item_type
    INTO v_item_type
    FROM public.item_templates
    WHERE id = p_item_id;

    IF v_item_type IS NULL THEN
        RAISE EXCEPTION 'Item template not found: %', p_item_id;
    END IF;

    IF lower(v_item_type) = 'equipment' THEN
        FOR i IN 1..v_qty LOOP
            INSERT INTO public.user_inventory (user_id, item_id, quantity, enhancement_level)
            VALUES (v_user_id, p_item_id, 1, 0);
        END LOOP;
    ELSE
        UPDATE public.user_inventory
        SET quantity = quantity + v_qty
        WHERE id = (
            SELECT id
            FROM public.user_inventory
            WHERE user_id = v_user_id
              AND item_id = p_item_id
              AND COALESCE(enhancement_level, 0) = 0
            ORDER BY created_at
            LIMIT 1
        );

        IF NOT FOUND THEN
            INSERT INTO public.user_inventory (user_id, item_id, quantity, enhancement_level)
            VALUES (v_user_id, p_item_id, v_qty, 0);
        END IF;
    END IF;
END;
$$;

GRANT EXECUTE ON FUNCTION public.add_item(TEXT, INT) TO authenticated;
