-- Server-authoritative pet base stats.
CREATE TABLE IF NOT EXISTS public.pet_templates (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    attack_type TEXT NOT NULL DEFAULT 'physical',
    base_hp INT NOT NULL DEFAULT 0,
    base_atk_phy INT NOT NULL DEFAULT 0,
    base_atk_mag INT NOT NULL DEFAULT 0,
    base_def_phy INT NOT NULL DEFAULT 0,
    base_def_mag INT NOT NULL DEFAULT 0,
    base_speed INT NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS public.pet_progression_templates (
    pet_base_id TEXT NOT NULL,
    progression_type TEXT NOT NULL,
    step_index INT NOT NULL,
    hp_multiplier NUMERIC NOT NULL DEFAULT 1,
    atk_multiplier NUMERIC NOT NULL DEFAULT 1,
    def_multiplier NUMERIC NOT NULL DEFAULT 1,
    bonus_hp INT NOT NULL DEFAULT 0,
    bonus_atk INT NOT NULL DEFAULT 0,
    bonus_def INT NOT NULL DEFAULT 0,
    PRIMARY KEY (pet_base_id, progression_type, step_index)
);

ALTER TABLE public.item_templates
ADD COLUMN IF NOT EXISTS bonus_hp INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS bonus_atk_phy INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS bonus_atk_mag INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS bonus_def_phy INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS bonus_def_mag INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS bonus_speed INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS percent_hp NUMERIC DEFAULT 0,
ADD COLUMN IF NOT EXISTS percent_atk NUMERIC DEFAULT 0,
ADD COLUMN IF NOT EXISTS percent_speed NUMERIC DEFAULT 0;

CREATE OR REPLACE FUNCTION public.get_pet_final_stats(p_pet_id UUID)
RETURNS TABLE (
    hp INT,
    atk_phy INT,
    atk_mag INT,
    def_phy INT,
    def_mag INT,
    speed INT,
    combat_power INT
)
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_user_id UUID;
    v_pet RECORD;
    v_base RECORD;
    v_level_bonus NUMERIC;
    v_hp NUMERIC;
    v_atk_phy NUMERIC;
    v_atk_mag NUMERIC;
    v_def_phy NUMERIC;
    v_def_mag NUMERIC;
    v_speed NUMERIC;
    v_flat_hp NUMERIC := 0;
    v_flat_atk_phy NUMERIC := 0;
    v_flat_atk_mag NUMERIC := 0;
    v_flat_def_phy NUMERIC := 0;
    v_flat_def_mag NUMERIC := 0;
    v_flat_speed NUMERIC := 0;
    v_pct_hp NUMERIC := 0;
    v_pct_atk NUMERIC := 0;
    v_pct_speed NUMERIC := 0;
    v_prog RECORD;
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

    IF v_pet.pet_base_id IS NULL OR v_pet.pet_base_id = '' THEN
        RAISE EXCEPTION 'Pet % has no pet_base_id. Backfill user_pets.pet_base_id before calculating server stats.', p_pet_id;
    END IF;

    SELECT * INTO v_base
    FROM public.pet_templates
    WHERE id = v_pet.pet_base_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Pet template not found: %', v_pet.pet_base_id;
    END IF;

    v_level_bonus := 1 + ((GREATEST(v_pet.level, 1) - 1) * 0.05);
    v_hp := v_base.base_hp * v_level_bonus;
    v_atk_phy := v_base.base_atk_phy * v_level_bonus;
    v_atk_mag := v_base.base_atk_mag * v_level_bonus;
    v_def_phy := v_base.base_def_phy * v_level_bonus;
    v_def_mag := v_base.base_def_mag * v_level_bonus;
    v_speed := v_base.base_speed;

    FOR v_prog IN
        SELECT *
        FROM public.pet_progression_templates
        WHERE pet_base_id = v_pet.pet_base_id
          AND progression_type = 'star'
          AND step_index >= 1
          AND step_index < GREATEST(v_pet.star, 1)
        ORDER BY step_index
    LOOP
        v_hp := (v_hp * v_prog.hp_multiplier) + v_prog.bonus_hp;
        v_atk_phy := (v_atk_phy * v_prog.atk_multiplier) + v_prog.bonus_atk;
        v_atk_mag := (v_atk_mag * v_prog.atk_multiplier) + v_prog.bonus_atk;
        v_def_phy := (v_def_phy * v_prog.def_multiplier) + v_prog.bonus_def;
        v_def_mag := (v_def_mag * v_prog.def_multiplier) + v_prog.bonus_def;
    END LOOP;

    FOR v_prog IN
        SELECT *
        FROM public.pet_progression_templates
        WHERE pet_base_id = v_pet.pet_base_id
          AND progression_type = 'realm'
          AND step_index >= 1
          AND step_index < GREATEST(v_pet.realm, 1)
        ORDER BY step_index
    LOOP
        v_hp := (v_hp * v_prog.hp_multiplier) + v_prog.bonus_hp;
        v_atk_phy := (v_atk_phy * v_prog.atk_multiplier) + v_prog.bonus_atk;
        v_atk_mag := (v_atk_mag * v_prog.atk_multiplier) + v_prog.bonus_atk;
        v_def_phy := (v_def_phy * v_prog.def_multiplier) + v_prog.bonus_def;
        v_def_mag := (v_def_mag * v_prog.def_multiplier) + v_prog.bonus_def;
    END LOOP;

    SELECT
        COALESCE(SUM(it.bonus_hp), 0),
        COALESCE(SUM(it.bonus_atk_phy), 0),
        COALESCE(SUM(it.bonus_atk_mag), 0),
        COALESCE(SUM(it.bonus_def_phy), 0),
        COALESCE(SUM(it.bonus_def_mag), 0),
        COALESCE(SUM(it.bonus_speed), 0),
        COALESCE(SUM(it.percent_hp), 0),
        COALESCE(SUM(it.percent_atk), 0),
        COALESCE(SUM(it.percent_speed), 0)
    INTO
        v_flat_hp,
        v_flat_atk_phy,
        v_flat_atk_mag,
        v_flat_def_phy,
        v_flat_def_mag,
        v_flat_speed,
        v_pct_hp,
        v_pct_atk,
        v_pct_speed
    FROM public.item_templates it
    WHERE it.id IN (
        v_pet.helmet_id,
        v_pet.armor_id,
        v_pet.weapon_id,
        v_pet.boots_id,
        v_pet.wings_id,
        v_pet.amulet_id
    );

    hp := ROUND((v_hp * (1 + v_pct_hp)) + v_flat_hp)::INT;
    atk_phy := ROUND((v_atk_phy * (1 + v_pct_atk)) + v_flat_atk_phy)::INT;
    atk_mag := ROUND((v_atk_mag * (1 + v_pct_atk)) + v_flat_atk_mag)::INT;
    def_phy := ROUND(v_def_phy + v_flat_def_phy)::INT;
    def_mag := ROUND(v_def_mag + v_flat_def_mag)::INT;
    speed := ROUND((v_speed * (1 + v_pct_speed)) + v_flat_speed)::INT;
    combat_power := ROUND((hp * 0.1) + ((atk_phy + atk_mag) * 1.0) + ((def_phy + def_mag) * 0.8) + (speed * 1.5))::INT;

    RETURN NEXT;
END;
$$;

GRANT EXECUTE ON FUNCTION public.get_pet_final_stats(UUID) TO authenticated;

CREATE OR REPLACE FUNCTION public.get_pet_final_stats_preview(
    p_pet_id UUID,
    p_level INT,
    p_star INT,
    p_realm INT
)
RETURNS TABLE (
    hp INT,
    atk_phy INT,
    atk_mag INT,
    def_phy INT,
    def_mag INT,
    speed INT,
    combat_power INT
)
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_user_id UUID;
    v_pet RECORD;
    v_base RECORD;
    v_level INT;
    v_star INT;
    v_realm INT;
    v_level_bonus NUMERIC;
    v_hp NUMERIC;
    v_atk_phy NUMERIC;
    v_atk_mag NUMERIC;
    v_def_phy NUMERIC;
    v_def_mag NUMERIC;
    v_speed NUMERIC;
    v_flat_hp NUMERIC := 0;
    v_flat_atk_phy NUMERIC := 0;
    v_flat_atk_mag NUMERIC := 0;
    v_flat_def_phy NUMERIC := 0;
    v_flat_def_mag NUMERIC := 0;
    v_flat_speed NUMERIC := 0;
    v_pct_hp NUMERIC := 0;
    v_pct_atk NUMERIC := 0;
    v_pct_speed NUMERIC := 0;
    v_prog RECORD;
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

    IF v_pet.pet_base_id IS NULL OR v_pet.pet_base_id = '' THEN
        RAISE EXCEPTION 'Pet % has no pet_base_id. Backfill user_pets.pet_base_id before calculating server stats.', p_pet_id;
    END IF;

    SELECT * INTO v_base
    FROM public.pet_templates
    WHERE id = v_pet.pet_base_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Pet template not found: %', v_pet.pet_base_id;
    END IF;

    v_level := GREATEST(COALESCE(p_level, v_pet.level, 1), 1);
    v_star := GREATEST(COALESCE(p_star, v_pet.star, 1), 1);
    v_realm := GREATEST(COALESCE(p_realm, v_pet.realm, 1), 1);

    v_level_bonus := 1 + ((v_level - 1) * 0.05);
    v_hp := v_base.base_hp * v_level_bonus;
    v_atk_phy := v_base.base_atk_phy * v_level_bonus;
    v_atk_mag := v_base.base_atk_mag * v_level_bonus;
    v_def_phy := v_base.base_def_phy * v_level_bonus;
    v_def_mag := v_base.base_def_mag * v_level_bonus;
    v_speed := v_base.base_speed;

    FOR v_prog IN
        SELECT *
        FROM public.pet_progression_templates
        WHERE pet_base_id = v_pet.pet_base_id
          AND progression_type = 'star'
          AND step_index >= 1
          AND step_index < v_star
        ORDER BY step_index
    LOOP
        v_hp := (v_hp * v_prog.hp_multiplier) + v_prog.bonus_hp;
        v_atk_phy := (v_atk_phy * v_prog.atk_multiplier) + v_prog.bonus_atk;
        v_atk_mag := (v_atk_mag * v_prog.atk_multiplier) + v_prog.bonus_atk;
        v_def_phy := (v_def_phy * v_prog.def_multiplier) + v_prog.bonus_def;
        v_def_mag := (v_def_mag * v_prog.def_multiplier) + v_prog.bonus_def;
    END LOOP;

    FOR v_prog IN
        SELECT *
        FROM public.pet_progression_templates
        WHERE pet_base_id = v_pet.pet_base_id
          AND progression_type = 'realm'
          AND step_index >= 1
          AND step_index < v_realm
        ORDER BY step_index
    LOOP
        v_hp := (v_hp * v_prog.hp_multiplier) + v_prog.bonus_hp;
        v_atk_phy := (v_atk_phy * v_prog.atk_multiplier) + v_prog.bonus_atk;
        v_atk_mag := (v_atk_mag * v_prog.atk_multiplier) + v_prog.bonus_atk;
        v_def_phy := (v_def_phy * v_prog.def_multiplier) + v_prog.bonus_def;
        v_def_mag := (v_def_mag * v_prog.def_multiplier) + v_prog.bonus_def;
    END LOOP;

    SELECT
        COALESCE(SUM(it.bonus_hp), 0),
        COALESCE(SUM(it.bonus_atk_phy), 0),
        COALESCE(SUM(it.bonus_atk_mag), 0),
        COALESCE(SUM(it.bonus_def_phy), 0),
        COALESCE(SUM(it.bonus_def_mag), 0),
        COALESCE(SUM(it.bonus_speed), 0),
        COALESCE(SUM(it.percent_hp), 0),
        COALESCE(SUM(it.percent_atk), 0),
        COALESCE(SUM(it.percent_speed), 0)
    INTO
        v_flat_hp,
        v_flat_atk_phy,
        v_flat_atk_mag,
        v_flat_def_phy,
        v_flat_def_mag,
        v_flat_speed,
        v_pct_hp,
        v_pct_atk,
        v_pct_speed
    FROM public.item_templates it
    WHERE it.id IN (
        v_pet.helmet_id,
        v_pet.armor_id,
        v_pet.weapon_id,
        v_pet.boots_id,
        v_pet.wings_id,
        v_pet.amulet_id
    );

    hp := ROUND((v_hp * (1 + v_pct_hp)) + v_flat_hp)::INT;
    atk_phy := ROUND((v_atk_phy * (1 + v_pct_atk)) + v_flat_atk_phy)::INT;
    atk_mag := ROUND((v_atk_mag * (1 + v_pct_atk)) + v_flat_atk_mag)::INT;
    def_phy := ROUND(v_def_phy + v_flat_def_phy)::INT;
    def_mag := ROUND(v_def_mag + v_flat_def_mag)::INT;
    speed := ROUND((v_speed * (1 + v_pct_speed)) + v_flat_speed)::INT;
    combat_power := ROUND((hp * 0.1) + ((atk_phy + atk_mag) * 1.0) + ((def_phy + def_mag) * 0.8) + (speed * 1.5))::INT;

    RETURN NEXT;
END;
$$;

GRANT EXECUTE ON FUNCTION public.get_pet_final_stats_preview(UUID, INT, INT, INT) TO authenticated;
