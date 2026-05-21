-- Example pet stat seeds. Replace ids/values with PetBaseSO.petBaseID data.

INSERT INTO public.pet_templates (
    id,
    name,
    attack_type,
    base_hp,
    base_atk_phy,
    base_atk_mag,
    base_def_phy,
    base_def_mag,
    base_speed
)
VALUES
    ('wolf_001', 'Wolf', 'physical', 120, 35, 5, 18, 10, 20)
ON CONFLICT (id) DO UPDATE SET
    name = EXCLUDED.name,
    attack_type = EXCLUDED.attack_type,
    base_hp = EXCLUDED.base_hp,
    base_atk_phy = EXCLUDED.base_atk_phy,
    base_atk_mag = EXCLUDED.base_atk_mag,
    base_def_phy = EXCLUDED.base_def_phy,
    base_def_mag = EXCLUDED.base_def_mag,
    base_speed = EXCLUDED.base_speed;

INSERT INTO public.pet_progression_templates (
    pet_base_id,
    progression_type,
    step_index,
    hp_multiplier,
    atk_multiplier,
    def_multiplier,
    bonus_hp,
    bonus_atk,
    bonus_def
)
VALUES
    ('wolf_001', 'star', 1, 1.05, 1.05, 1.05, 10, 3, 2),
    ('wolf_001', 'realm', 1, 1.10, 1.10, 1.10, 20, 5, 4)
ON CONFLICT (pet_base_id, progression_type, step_index) DO UPDATE SET
    hp_multiplier = EXCLUDED.hp_multiplier,
    atk_multiplier = EXCLUDED.atk_multiplier,
    def_multiplier = EXCLUDED.def_multiplier,
    bonus_hp = EXCLUDED.bonus_hp,
    bonus_atk = EXCLUDED.bonus_atk,
    bonus_def = EXCLUDED.bonus_def;
