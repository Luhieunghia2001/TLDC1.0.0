using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Spawn Points")]
    [SerializeField] private Transform allySpawnPoint;
    [SerializeField] private Transform enemySpawnPoint;

    [Header("Fixed UI")]
    [SerializeField] private BattleUnit allyUI;
    [SerializeField] private BattleUnit enemyUI;

    [Header("Visual Settings")]
    [SerializeField] private bool flipAlly = true; // Cho phép bật/tắt lật Pet người chơi trong Inspector

    [Header("Teams")]
    public List<BattlePet> allyTeam = new List<BattlePet>();
    public List<BattlePet> enemyTeam = new List<BattlePet>();

    private BattlePet activeAllyData;
    private BattlePet activeEnemyData;
    private GameObject allyModel;
    private GameObject enemyModel;
    private bool isBattleEnded = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (BattleDataStore.selectedAllies != null && BattleDataStore.selectedAllies.Count > 0)
        {
            StartBattle(BattleDataStore.selectedAllies, BattleDataStore.selectedEnemies);
            BattleDataStore.selectedAllies = null;
            BattleDataStore.selectedEnemies = null;
        }
    }

    public async void StartBattle(List<PetModel> allies, List<PetModel> enemies)
    {
        ClearBattlefield();
        allyTeam.Clear();
        enemyTeam.Clear();

        foreach (var p in allies)
        {
            var baseData = PetManager.Instance.GetPetBaseByID(p.petBaseId);
            if (baseData != null) allyTeam.Add(new BattlePet(p, baseData));
        }

        foreach (var p in enemies)
        {
            var baseData = PetManager.Instance.GetPetBaseByID(p.petBaseId);
            if (baseData != null) enemyTeam.Add(new BattlePet(p, baseData));
        }

        isBattleEnded = false;
        activeAllyData = allyTeam.Count > 0 ? allyTeam[0] : null;
        activeEnemyData = enemyTeam.Count > 0 ? enemyTeam[0] : null;

        if (activeAllyData != null && activeEnemyData != null)
        {
            if (allyUI != null) allyUI.Setup(activeAllyData);
            if (enemyUI != null) enemyUI.Setup(activeEnemyData);
            
            // Spawn Ally (có lật) và Enemy (không lật)
            allyModel = SpawnPetModel(activeAllyData, allySpawnPoint, flipAlly);
            enemyModel = SpawnPetModel(activeEnemyData, enemySpawnPoint, false);
            
            await Task.Delay(1000); 
            await BattleLoop();
        }
    }

    private GameObject SpawnPetModel(BattlePet pet, Transform spawnPoint, bool flipX)
    {
        if (pet.baseData.petPrefab == null || spawnPoint == null) return null;
        GameObject go = Instantiate(pet.baseData.petPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
        
        // Thực hiện lật bằng cách đổi Scale X thành -1
        float scaleX = flipX ? -1f : 1f;
        go.transform.localScale = new Vector3(scaleX, 1f, 1f);
        
        return go;
    }

    private void ClearBattlefield()
    {
        if (allySpawnPoint != null) foreach (Transform t in allySpawnPoint) Destroy(t.gameObject);
        if (enemySpawnPoint != null) foreach (Transform t in enemySpawnPoint) Destroy(t.gameObject);
    }

    private async Task BattleLoop()
    {
        while (!isBattleEnded)
        {
            BattlePet first = activeAllyData.stats.Speed >= activeEnemyData.stats.Speed ? activeAllyData : activeEnemyData;
            BattlePet second = (first == activeAllyData) ? activeEnemyData : activeAllyData;

            await ExecuteTurn(first, second);
            if (isBattleEnded) break;

            await Task.Delay(1000); 

            await ExecuteTurn(second, first);
            if (isBattleEnded) break;

            await Task.Delay(1000); 

            activeAllyData.ReduceCooldowns();
            activeEnemyData.ReduceCooldowns();
        }
        Debug.Log("<color=cyan>TRẬN ĐẤU KẾT THÚC!</color>");
    }

    private async Task ExecuteTurn(BattlePet attacker, BattlePet defender)
    {
        if (isBattleEnded) return;

        PetSkillSO skill = ChooseActiveSkill(attacker);
        if (skill == null) return;

        GameObject attackerModel = (attacker == activeAllyData) ? allyModel : enemyModel;
        GameObject defenderModel = (defender == activeAllyData) ? allyModel : enemyModel;
        BattleUnit defenderUI = (defender == activeAllyData) ? allyUI : enemyUI;

        if (attackerModel != null)
        {
            var anim = attackerModel.GetComponentInChildren<Animator>();
            if (anim != null) anim.Play(skill.animationTrigger);
        }
        
        if (defenderModel != null && skill.vfxPrefab != null)
        {
            GameObject vfx = Instantiate(skill.vfxPrefab, defenderModel.transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        await Task.Delay(1000); 

        int damage = CalculateDamage(attacker, defender, skill);
        defender.currentHP -= damage;
        if (defender.currentHP < 0) defender.currentHP = 0;

        if (defenderUI != null)
        {
            defenderUI.UpdateHPUI();
            defenderUI.ShowDamageText(damage);
        }

        if (defenderModel != null) 
        {
            StartCoroutine(ShakeObject(defenderModel));
            var anim = defenderModel.GetComponentInChildren<Animator>();
            if (anim != null) anim.Play("Hurt");
        }

        await Task.Delay(500); 

        if (attackerModel != null)
        {
            var anim = attackerModel.GetComponentInChildren<Animator>();
            if (anim != null) anim.Play("Idle");
        }
        if (defenderModel != null && !defender.isDead)
        {
            var anim = defenderModel.GetComponentInChildren<Animator>();
            if (anim != null) anim.Play("Idle");
        }

        if (defender.isDead)
        {
            await HandlePetDeath(defender);
        }
    }

    private IEnumerator<WaitForSeconds> ShakeObject(GameObject obj)
    {
        Vector3 origPos = obj.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            obj.transform.localPosition = origPos + (Vector3)Random.insideUnitCircle * 0.1f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.transform.localPosition = origPos;
    }

    private PetSkillSO ChooseActiveSkill(BattlePet attacker)
    {
        if (attacker.baseData.skills == null || attacker.baseData.skills.Count == 0) return null;
        PetSkillSO bestSkill = attacker.baseData.skills[0]; 
        foreach (var skill in attacker.baseData.skills)
        {
            if (skill.skillType == SkillType.Active)
            {
                bool isOffCooldown = !attacker.cooldownDict.ContainsKey(skill.skillID) || attacker.cooldownDict[skill.skillID] <= 0;
                if (isOffCooldown && skill.priority > bestSkill.priority) bestSkill = skill;
            }
        }
        if (bestSkill.cooldownTurns > 0) attacker.cooldownDict[bestSkill.skillID] = bestSkill.cooldownTurns;
        return bestSkill;
    }

    private int CalculateDamage(BattlePet attacker, BattlePet defender, PetSkillSO skill)
    {
        float atk = (attacker.baseData.attackType == PetAttackType.Physical) ? attacker.stats.AtkPhy : attacker.stats.AtkMag;
        float def = (attacker.baseData.attackType == PetAttackType.Physical) ? defender.stats.DefPhy : defender.stats.DefMag;
        float rawDamage = atk * skill.valueScale;
        float finalDamage = rawDamage - (def * 0.5f);
        return Mathf.Max(1, Mathf.RoundToInt(finalDamage));
    }

    private async Task<bool> HandlePetDeath(BattlePet deadPet)
    {
        await Task.Delay(500); 
        bool isAlly = (deadPet == activeAllyData);
        List<BattlePet> team = isAlly ? allyTeam : enemyTeam;

        if (isAlly) Destroy(allyModel);
        else Destroy(enemyModel);

        BattlePet nextPetData = team.Find(p => !p.isDead);

        if (nextPetData != null)
        {
            if (isAlly)
            {
                activeAllyData = nextPetData;
                allyUI.Setup(activeAllyData);
                allyModel = SpawnPetModel(activeAllyData, allySpawnPoint, flipAlly);
            }
            else
            {
                activeEnemyData = nextPetData;
                enemyUI.Setup(activeEnemyData);
                enemyModel = SpawnPetModel(activeEnemyData, enemySpawnPoint, false);
            }
            await Task.Delay(1000); 
            return true;
        }
        else
        {
            isBattleEnded = true;
            return false;
        }
    }
}
