using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PetTeamSelectionUI : MonoBehaviour
{
    public static PetTeamSelectionUI Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;

    [Header("Lineup Slots (Top Panel)")]
    [SerializeField] private PetUIItem[] teamSlots; 

    [Header("All Pets List (Bottom Panel)")]
    [SerializeField] private Transform allPetsContainer;
    [SerializeField] private PetUIItem petItemPrefab;

    private List<PetModel> selectedTeam = new List<PetModel>();
    private const int MAX_TEAM_SIZE = 5;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        UpdateTeamVisuals();
        _ = RefreshAllPetsList();
    }

    public async Task RefreshAllPetsList()
    {
        var myPets = await PetManager.Instance.GetMyPets();
        
        foreach (Transform child in allPetsContainer) Destroy(child.gameObject);

        foreach (var pet in myPets)
        {
            var item = Instantiate(petItemPrefab, allPetsContainer);
            item.Setup(pet, () => AddToTeam(pet));

            var cg = item.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = selectedTeam.Exists(p => p.id == pet.id) ? 0.5f : 1.0f;
            }
        }
    }

    private void AddToTeam(PetModel pet)
    {
        if (selectedTeam.Exists(p => p.id == pet.id)) return;

        if (selectedTeam.Count < MAX_TEAM_SIZE)
        {
            selectedTeam.Add(pet);
            UpdateTeamVisuals();
            _ = RefreshAllPetsList(); 
        }
    }

    private void RemoveFromTeam(int index)
    {
        if (index >= 0 && index < selectedTeam.Count)
        {
            selectedTeam.RemoveAt(index);
            UpdateTeamVisuals();
            _ = RefreshAllPetsList(); 
        }
    }

    private void UpdateTeamVisuals()
    {
        for (int i = 0; i < teamSlots.Length; i++)
        {
            if (teamSlots[i] == null) continue;

            int index = i; 
            if (i < selectedTeam.Count)
            {
                teamSlots[i].SetEmpty(false);
                teamSlots[i].Setup(selectedTeam[i], () => RemoveFromTeam(index));
            }
            else
            {
                teamSlots[i].SetEmpty(true);
            }
        }
    }

    public List<PetModel> GetSelectedTeam()
    {
        return selectedTeam;
    }
}
