-- Use this to inspect old pets that cannot use server-authoritative stats yet.
SELECT id, pet_name, pet_type, element, tier, pet_base_id
FROM public.user_pets
WHERE pet_base_id IS NULL OR pet_base_id = '';

-- Example backfill. Replace the WHERE condition and value with your real data.
-- pet_base_id must match public.pet_templates.id and PetBaseSO.petBaseID.
--
-- UPDATE public.user_pets
-- SET pet_base_id = 'wolf_001'
-- WHERE pet_base_id IS NULL
--   AND pet_name = 'Wolf';

-- After backfilling, verify every pet_base_id has a matching template.
SELECT p.id, p.pet_name, p.pet_base_id
FROM public.user_pets p
LEFT JOIN public.pet_templates t ON t.id = p.pet_base_id
WHERE p.pet_base_id IS NULL
   OR p.pet_base_id = ''
   OR t.id IS NULL;
