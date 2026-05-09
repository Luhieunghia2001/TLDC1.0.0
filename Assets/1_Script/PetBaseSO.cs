using UnityEngine;

public enum PetTier { D, C, B, A, S, SS, SSS }
public enum PetElement { Fire, Wind, Earth, Water }
public enum PetAttackType { Physical, Magic }

[CreateAssetMenu(fileName = "NewPetBase", menuName = "GameData/PetBase")]
public class PetBaseSO : ScriptableObject
{
    [Header("General Info")]
    public string petBaseID; // ID duy nhất để khớp với Database
    public string speciesName;
    public PetElement element;
    public PetAttackType attackType;
    public PetTier defaultTier;

    [Header("Base Stats")]
    public int baseHP;
    public int baseAtkPhy;
    public int baseAtkMag;
    public int baseDefPhy;
    public int baseDefMag;
    public int baseSpeed;

    [Header("Visuals")]
    public Sprite icon;
    public GameObject petPrefab;
}
