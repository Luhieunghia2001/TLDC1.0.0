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
    [SerializeField] private bool flipAlly = true; 

    [Header("Teams")]
    public List<BattlePet> allyTeam = new List<BattlePet>();
    public List<BattlePet> enemyTeam = new List<BattlePet>();

    private BattlePet activeAllyData;
    private BattlePet activeEnemyData;
    private GameObject allyModel;
    private GameObject enemyModel;
    private bool isBattleEnded = false;

    private void OnDestroy()
    {
        // Khi Scene bị đóng hoặc Object bị hủy, dừng ngay lập tức mọi logic chiến đấu
        isBattleEnded = true;
    }

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
            if (allyUI != null) allyUI.Setup(activeAllyData, allyTeam);
            if (enemyUI != null) enemyUI.Setup(activeEnemyData, enemyTeam);
            
            allyModel = SpawnPetModel(activeAllyData, allySpawnPoint, flipAlly);
            enemyModel = SpawnPetModel(activeEnemyData, enemySpawnPoint, false);
            
            // Kích hoạt kỹ năng lúc vào trận (OnWaveStart)
            await TriggerSkills(activeAllyData, activeEnemyData, SkillTrigger.OnWaveStart);
            await TriggerSkills(activeEnemyData, activeAllyData, SkillTrigger.OnWaveStart);

            await WaitForSecondsScaled(1.0f); 
            await BattleLoop();
        }
    }

    private GameObject SpawnPetModel(BattlePet pet, Transform spawnPoint, bool flipX)
    {
        if (pet.baseData.petPrefab == null || spawnPoint == null) return null;
        GameObject go = Instantiate(pet.baseData.petPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
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
            await WaitForSecondsScaled(1.0f); 

            BattlePet first = activeAllyData.stats.Speed >= activeEnemyData.stats.Speed ? activeAllyData : activeEnemyData;
            BattlePet second = (first == activeAllyData) ? activeEnemyData : activeAllyData;

            // 1. Kích hoạt OnTurnStart cho cả 2 bên (hoặc bên nhanh hơn trước)
            await TriggerSkills(first, second, SkillTrigger.OnTurnStart);
            if (isBattleEnded) break;

            await ExecuteTurn(first, second);
            if (isBattleEnded) break;

            await WaitForSecondsScaled(1.0f); 

            // 2. Kích hoạt OnTurnStart cho bên thứ hai
            await TriggerSkills(second, first, SkillTrigger.OnTurnStart);
            if (isBattleEnded) break;

            await ExecuteTurn(second, first);
            if (isBattleEnded) break;

            await WaitForSecondsScaled(1.0f); 

            activeAllyData.ReduceCooldowns();
            activeEnemyData.ReduceCooldowns();
        }

        Debug.Log("<color=cyan>TRẬN ĐẤU KẾT THÚC!</color>");
        await HandleBattleEnd();
    }

    private async Task HandleBattleEnd()
    {
        bool playerWon = enemyTeam.TrueForAll(p => p.isDead);
        
        if (playerWon)
        {
            Debug.Log("<color=green>BẠN ĐÃ CHIẾN THẮNG!</color>");
            // Nhận thưởng bảo mật từ Server
            if (ResourceManager.Instance != null && !string.IsNullOrEmpty(BattleDataStore.currentBattleLogId))
            {
                await ResourceManager.Instance.ClaimBattleReward(BattleDataStore.currentBattleLogId);
            }
        }
        else
        {
            Debug.Log("<color=red>BẠN ĐÃ THẤT BẠI!</color>");
        }

        await WaitForSecondsScaled(2.0f); // Chờ 2 giây để xem kết quả
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
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

        await WaitForSecondsScaled(1.0f); 

        // 1. Kích hoạt OnAttack (Trước khi thực hiện đòn đánh)
        await TriggerSkills(attacker, defender, SkillTrigger.OnAttack);
        
        // 2. GIAI ĐOẠN THỰC THI LOGIC (Skill Effects)
        Debug.Log($"<color=yellow>[BATTLE]</color> {attacker.petData.petName} sử dụng kỹ năng: <b>{skill.skillName}</b>");

        if (skill.effects != null && skill.effects.Count > 0)
        {
            foreach (var effect in skill.effects)
            {
                if (effect == null) continue;
                Debug.Log($"   <color=grey>-> Thực thi Effect:</color> {effect.name} ({effect.GetType().Name})");
                effect.Execute(attacker, defender);
            }
        }
        else
        {
            // FALLBACK: Nếu chưa gán Effect mới thì dùng logic Damage cũ
            Debug.Log($"   <color=grey>-> Không có Effect, sử dụng đòn đánh thường dự phòng.</color>");
            int damage = CalculateDamage(attacker, defender, skill);
            defender.TakeDamage(damage);
        }

        // 3. Kích hoạt OnAttacked (Khi bị trúng đòn)
        await TriggerSkills(defender, attacker, SkillTrigger.OnAttacked);

        // Cập nhật UI sau khi thực thi logic
        if (defenderUI != null)
        {
            defenderUI.UpdateHPUI();
            // Lấy damage từ logic (đây là phần demo, bạn có thể truyền damage ra từ Effect nếu muốn chi tiết hơn)
            // Hiện tại tôi vẫn dùng CalculateDamage để hiện số cho đẹp
            defenderUI.ShowDamageText(CalculateDamage(attacker, defender, skill)); 
        }

        if (defenderModel != null) 
        {
            StartCoroutine(ShakeObject(defenderModel));
            var anim = defenderModel.GetComponentInChildren<Animator>();
            if (anim != null) anim.Play("Hurt");
        }

        await WaitForSecondsScaled(0.5f); 

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
        await WaitForSecondsScaled(0.5f); 
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
                allyUI.Setup(activeAllyData, allyTeam);
                allyModel = SpawnPetModel(activeAllyData, allySpawnPoint, flipAlly);
            }
            else
            {
                activeEnemyData = nextPetData;
                enemyUI.Setup(activeEnemyData, enemyTeam);
                enemyModel = SpawnPetModel(activeEnemyData, enemySpawnPoint, false);
            }

            // Kích hoạt OnWaveStart khi Pet mới xuất hiện
            await TriggerSkills(isAlly ? activeAllyData : activeEnemyData, isAlly ? activeEnemyData : activeAllyData, SkillTrigger.OnWaveStart);

            await WaitForSecondsScaled(1.0f); 
            return true;
        }
        else
        {
            isBattleEnded = true;
            return false;
        }
    }

    private async Task TriggerSkills(BattlePet attacker, BattlePet defender, SkillTrigger triggerType)
    {
        if (attacker == null || attacker.baseData.skills == null) return;

        foreach (var skill in attacker.baseData.skills)
        {
            // CHỈ kích hoạt các kỹ năng Bị động (Passive) dựa trên Trigger
            if (skill.skillType == SkillType.Passive && skill.trigger == triggerType)
            {
                Debug.Log($"<color=orange>[TRIGGER]</color> {attacker.petData.petName} kích hoạt nội tại: <b>{skill.skillName}</b> ({triggerType})");
                
                // Thực thi các hiệu ứng của skill nội tại
                if (skill.effects != null)
                {
                    foreach (var effect in skill.effects)
                    {
                        if (effect != null) effect.Execute(attacker, defender);
                    }
                }

                // Cập nhật UI nếu có thay đổi máu (ví dụ từ hiệu ứng phản đam hoặc hồi máu)
                if (allyUI != null) allyUI.UpdateHPUI();
                if (enemyUI != null) enemyUI.UpdateHPUI();
            }
        }
        await Task.Yield();
    }

    // HÀM CHỜ THEO TỐC ĐỘ GAME
    private async Task WaitForSecondsScaled(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds && !isBattleEnded)
        {
            if (Time.timeScale > 0)
            {
                elapsed += Time.deltaTime;
            }
            await Task.Yield();
        }
    }
}
