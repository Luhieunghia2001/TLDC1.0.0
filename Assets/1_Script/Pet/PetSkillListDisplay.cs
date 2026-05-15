using UnityEngine;
using System.Collections.Generic;

public class PetSkillListDisplay : MonoBehaviour
{
    [Header("Skill Slots (Kéo các ô UI đã có sẵn vào đây)")]
    [SerializeField] private PetSkillUIItem[] skillSlots;

    private void Start()
    {
        // Tự động cập nhật khi có một con Pet được chọn
        if (PetManager.Instance != null)
        {
            PetManager.Instance.OnPetSelected += HandlePetSelected;
            
            // Cập nhật ngay nếu đã có Pet đang được chọn
            if (PetManager.Instance.CurrentPet != null)
            {
                HandlePetSelected(PetManager.Instance.CurrentPet);
            }
        }
    }

    private void OnDestroy()
    {
        if (PetManager.Instance != null)
        {
            PetManager.Instance.OnPetSelected -= HandlePetSelected;
        }
    }

    private void HandlePetSelected(PetModel pet)
    {
        if (pet == null) 
        {
            HideAllSlots();
            return;
        }

        var baseInfo = PetManager.Instance.GetPetBaseByID(pet.petBaseId);
        if (baseInfo == null) return;

        UpdateSkillsUI(baseInfo.skills);
    }

    private void UpdateSkillsUI(List<PetSkillSO> skills)
    {
        if (skillSlots == null) return;

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] == null) continue;

            if (skills != null && i < skills.Count && skills[i] != null)
            {
                // Có dữ liệu skill: Hiện ô và Setup nội dung
                skillSlots[i].gameObject.SetActive(true);
                skillSlots[i].Setup(skills[i]);
            }
            else
            {
                // Không có skill ở vị trí này: Ẩn ô đi
                skillSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void HideAllSlots()
    {
        if (skillSlots == null) return;
        foreach (var slot in skillSlots)
        {
            if (slot != null) slot.gameObject.SetActive(false);
        }
    }
}
