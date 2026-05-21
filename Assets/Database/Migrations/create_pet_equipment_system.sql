-- Equipment must be instance-based. Two swords with the same item_id can have
-- different enhancement levels, so user_inventory must not force uniqueness by
-- (user_id, item_id).
ALTER TABLE public.user_inventory
DROP CONSTRAINT IF EXISTS unique_user_item;

DO $$
DECLARE
    r RECORD;
BEGIN
    FOR r IN
        SELECT ui.id, ui.user_id, ui.item_id, ui.quantity, COALESCE(ui.enhancement_level, 0) AS enhancement_level
        FROM public.user_inventory ui
        JOIN public.item_templates it ON it.id = ui.item_id
        WHERE lower(it.item_type) = 'equipment'
          AND ui.quantity > 1
    LOOP
        UPDATE public.user_inventory
        SET quantity = 1
        WHERE id = r.id;

        FOR i IN 1..(r.quantity - 1) LOOP
            INSERT INTO public.user_inventory (user_id, item_id, quantity, enhancement_level)
            VALUES (r.user_id, r.item_id, 1, r.enhancement_level);
        END LOOP;
    END LOOP;
END;
$$;

ALTER TABLE public.user_pets
ADD COLUMN IF NOT EXISTS helmet_id TEXT DEFAULT NULL,
ADD COLUMN IF NOT EXISTS armor_id TEXT DEFAULT NULL,
ADD COLUMN IF NOT EXISTS weapon_id TEXT DEFAULT NULL,
ADD COLUMN IF NOT EXISTS boots_id TEXT DEFAULT NULL,
ADD COLUMN IF NOT EXISTS wings_id TEXT DEFAULT NULL,
ADD COLUMN IF NOT EXISTS amulet_id TEXT DEFAULT NULL,
ADD COLUMN IF NOT EXISTS helmet_enhancement_level INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS armor_enhancement_level INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS weapon_enhancement_level INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS boots_enhancement_level INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS wings_enhancement_level INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS amulet_enhancement_level INT DEFAULT 0;

DROP FUNCTION IF EXISTS public.equip_pet_item(UUID, TEXT, TEXT);
DROP FUNCTION IF EXISTS public.equip_pet_item(UUID, TEXT, UUID);

CREATE OR REPLACE FUNCTION public.equip_pet_item(
    p_pet_id UUID,
    p_slot TEXT,
    p_inventory_id UUID
)
RETURNS VOID
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_user_id UUID;
    v_item_id TEXT;
    v_equipped_level INT := 0;
    v_item_type TEXT;
    v_old_item_id TEXT;
    v_old_enhancement_level INT := 0;
    v_pet RECORD;
BEGIN
    v_user_id := auth.uid();
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'Please log in.';
    END IF;

    SELECT * INTO v_pet
    FROM public.user_pets
    WHERE id = p_pet_id AND user_id = v_user_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Pet not found or does not belong to you.';
    END IF;

    SELECT ui.item_id, COALESCE(ui.enhancement_level, 0), it.item_type
    INTO v_item_id, v_equipped_level, v_item_type
    FROM public.user_inventory ui
    LEFT JOIN public.item_templates it ON it.id = ui.item_id
    WHERE ui.id = p_inventory_id
      AND ui.user_id = v_user_id
      AND ui.quantity > 0;

    IF v_item_id IS NULL THEN
        RAISE EXCEPTION 'Inventory equipment instance not found.';
    END IF;

    IF lower(COALESCE(v_item_type, '')) != 'equipment' THEN
        RAISE EXCEPTION 'Only equipment can be equipped.';
    END IF;

    DELETE FROM public.user_inventory
    WHERE id = p_inventory_id AND user_id = v_user_id;

    IF p_slot = 'helmet' THEN
        v_old_item_id := v_pet.helmet_id;
        v_old_enhancement_level := COALESCE(v_pet.helmet_enhancement_level, 0);
        UPDATE public.user_pets SET helmet_id = v_item_id, helmet_enhancement_level = v_equipped_level WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'armor' THEN
        v_old_item_id := v_pet.armor_id;
        v_old_enhancement_level := COALESCE(v_pet.armor_enhancement_level, 0);
        UPDATE public.user_pets SET armor_id = v_item_id, armor_enhancement_level = v_equipped_level WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'weapon' THEN
        v_old_item_id := v_pet.weapon_id;
        v_old_enhancement_level := COALESCE(v_pet.weapon_enhancement_level, 0);
        UPDATE public.user_pets SET weapon_id = v_item_id, weapon_enhancement_level = v_equipped_level WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'boots' THEN
        v_old_item_id := v_pet.boots_id;
        v_old_enhancement_level := COALESCE(v_pet.boots_enhancement_level, 0);
        UPDATE public.user_pets SET boots_id = v_item_id, boots_enhancement_level = v_equipped_level WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'wings' THEN
        v_old_item_id := v_pet.wings_id;
        v_old_enhancement_level := COALESCE(v_pet.wings_enhancement_level, 0);
        UPDATE public.user_pets SET wings_id = v_item_id, wings_enhancement_level = v_equipped_level WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'amulet' THEN
        v_old_item_id := v_pet.amulet_id;
        v_old_enhancement_level := COALESCE(v_pet.amulet_enhancement_level, 0);
        UPDATE public.user_pets SET amulet_id = v_item_id, amulet_enhancement_level = v_equipped_level WHERE id = p_pet_id AND user_id = v_user_id;
    ELSE
        RAISE EXCEPTION 'Invalid equipment slot: %', p_slot;
    END IF;

    IF v_old_item_id IS NOT NULL AND v_old_item_id <> '' THEN
        INSERT INTO public.user_inventory (user_id, item_id, quantity, enhancement_level)
        VALUES (v_user_id, v_old_item_id, 1, v_old_enhancement_level);
    END IF;
END;
$$;

CREATE OR REPLACE FUNCTION public.unequip_pet_item(
    p_pet_id UUID,
    p_slot TEXT
)
RETURNS VOID
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_user_id UUID;
    v_equipped_item_id TEXT;
    v_enhancement_level INT := 0;
    v_pet RECORD;
BEGIN
    v_user_id := auth.uid();
    IF v_user_id IS NULL THEN
        RAISE EXCEPTION 'Please log in.';
    END IF;

    SELECT * INTO v_pet
    FROM public.user_pets
    WHERE id = p_pet_id AND user_id = v_user_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Pet not found or does not belong to you.';
    END IF;

    IF p_slot = 'helmet' THEN
        v_equipped_item_id := v_pet.helmet_id;
        v_enhancement_level := COALESCE(v_pet.helmet_enhancement_level, 0);
        UPDATE public.user_pets SET helmet_id = NULL, helmet_enhancement_level = 0 WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'armor' THEN
        v_equipped_item_id := v_pet.armor_id;
        v_enhancement_level := COALESCE(v_pet.armor_enhancement_level, 0);
        UPDATE public.user_pets SET armor_id = NULL, armor_enhancement_level = 0 WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'weapon' THEN
        v_equipped_item_id := v_pet.weapon_id;
        v_enhancement_level := COALESCE(v_pet.weapon_enhancement_level, 0);
        UPDATE public.user_pets SET weapon_id = NULL, weapon_enhancement_level = 0 WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'boots' THEN
        v_equipped_item_id := v_pet.boots_id;
        v_enhancement_level := COALESCE(v_pet.boots_enhancement_level, 0);
        UPDATE public.user_pets SET boots_id = NULL, boots_enhancement_level = 0 WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'wings' THEN
        v_equipped_item_id := v_pet.wings_id;
        v_enhancement_level := COALESCE(v_pet.wings_enhancement_level, 0);
        UPDATE public.user_pets SET wings_id = NULL, wings_enhancement_level = 0 WHERE id = p_pet_id AND user_id = v_user_id;
    ELSIF p_slot = 'amulet' THEN
        v_equipped_item_id := v_pet.amulet_id;
        v_enhancement_level := COALESCE(v_pet.amulet_enhancement_level, 0);
        UPDATE public.user_pets SET amulet_id = NULL, amulet_enhancement_level = 0 WHERE id = p_pet_id AND user_id = v_user_id;
    ELSE
        RAISE EXCEPTION 'Invalid equipment slot: %', p_slot;
    END IF;

    IF v_equipped_item_id IS NOT NULL AND v_equipped_item_id <> '' THEN
        INSERT INTO public.user_inventory (user_id, item_id, quantity, enhancement_level)
        VALUES (v_user_id, v_equipped_item_id, 1, v_enhancement_level);
    END IF;
END;
$$;

GRANT EXECUTE ON FUNCTION public.equip_pet_item(UUID, TEXT, UUID) TO authenticated;
GRANT EXECUTE ON FUNCTION public.unequip_pet_item(UUID, TEXT) TO authenticated;
