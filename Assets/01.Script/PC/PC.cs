using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PC : MonoBehaviour
{
    private const string DefaultLevelTableResourcePath = "PcLevelTable";
    private const string GpuUpgradeResourcePath = "03.SO/GPU";
    private const string CpuUpgradeResourcePath = "03.SO/CPU";
    private const string SsdUpgradeResourcePath = "03.SO/SSD";
    private const float MinimumUsingDuration = 0.1f;
    private const float DefaultPercentBonus = 100f;

    private static PcLevelTableSO cachedDefaultLevelTable;
    private static bool hasLoggedMissingLevelTable;
    private static PCUpgradeDataSO cachedGpuUpgradeData;
    private static PCUpgradeDataSO cachedCpuUpgradeData;
    private static PCUpgradeDataSO cachedSsdUpgradeData;
    private static bool hasLoggedMissingGpuUpgradeData;
    private static bool hasLoggedMissingCpuUpgradeData;
    private static bool hasLoggedMissingSsdUpgradeData;

    public bool isTargeted = false;
    public bool isArrived = false;
    public Slider slider;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private PcLevelTableSO levelTable;
    public float usingTime;
    public float earningTime = 20f;
    public Transform interactionPos;
    [SerializeField, Min(1)] private int gpuLevel = 1;
    [SerializeField, Min(1)] private int cpuLevel = 1;
    [SerializeField, Min(1)] private int ssdLevel = 1;

    [Min(0)] public int currentExperience;
    [Min(1)] public int expPerCompletedSession = 10;

    public bool isUsing = false;
    public bool isUsagePaused = false;

    private void Start()
    {
        slider = GetComponentInChildren<Slider>(true);
        if (slider != null)
        {
            slider.gameObject.SetActive(false);
        }

        if (levelText == null)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                if (text.name != "LevelText")
                {
                    continue;
                }

                levelText = text;
                break;
            }
        }

        UpdateLevelText();
    }

    public bool UpdateUsingTimer()
    {
        float effectiveEarningTime = GetEffectiveUsingTime();

        if (!isUsagePaused)
        {
            usingTime += Time.deltaTime;
        }

        float value = usingTime / effectiveEarningTime;

        if (slider != null)
        {
            slider.SetValueWithoutNotify(value);
        }

        if (!isUsagePaused && value >= 1f)
        {
            usingTime = 0f;
            return true;
        }

        return false;
    }

    public void SetUsagePaused(bool paused)
    {
        isUsagePaused = paused;
    }

    public void AddSessionExp()
    {
        AddExp(expPerCompletedSession);
    }

    public bool ApplyUpgrade(PCUpgradeDataSO upgradeData)
    {
        if (upgradeData == null)
        {
            return false;
        }

        int currentUpgradeLevel = GetUpgradeLevel(upgradeData.type);
        int nextUpgradeLevel = currentUpgradeLevel + 1;
        if (!upgradeData.TryGetTier(nextUpgradeLevel, out _))
        {
            Debug.LogWarning($"{name} {upgradeData.type} upgrade is already at max level.");
            return false;
        }

        SetUpgradeLevel(upgradeData.type, nextUpgradeLevel);

        int expBonus = Mathf.RoundToInt(upgradeData.GetExpBonus(nextUpgradeLevel));
        if (expBonus > 0)
        {
            AddExp(expBonus);
        }

        return true;
    }

    public int GetUpgradeCost(PCUpgradeDataSO upgradeData)
    {
        if (upgradeData == null)
        {
            return -1;
        }

        int nextUpgradeLevel = GetUpgradeLevel(upgradeData.type) + 1;
        return upgradeData.GetUpgradeCost(nextUpgradeLevel);
    }

    public int GetUpgradeLevel(PCUpgradeDataSO.UpgradeType upgradeType)
    {
        return upgradeType switch
        {
            PCUpgradeDataSO.UpgradeType.GPU => Mathf.Max(1, gpuLevel),
            PCUpgradeDataSO.UpgradeType.CPU => Mathf.Max(1, cpuLevel),
            PCUpgradeDataSO.UpgradeType.SSD => Mathf.Max(1, ssdLevel),
            _ => 1
        };
    }

    public void AddExp(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int previousLevel = ResolveLevel(Mathf.Max(0, currentExperience));
        currentExperience = Mathf.Max(0, currentExperience) + amount;

        int newLevel = ResolveLevel(currentExperience);
        if (newLevel > previousLevel)
        {
            Debug.Log($"{name} leveled up: Lv.{previousLevel} -> Lv.{newLevel} (EXP: {currentExperience})");
        }

        UpdateLevelText();
    }

    private int ResolveLevel(int totalExperience)
    {
        PcLevelTableSO table = GetLevelTable();
        if (table != null)
        {
            return table.ResolveLevel(totalExperience);
        }

        return 1;
    }

    public int GetRequiredExp(int level)
    {
        PcLevelTableSO table = GetLevelTable();
        if (table != null)
        {
            return table.GetRequiredExp(level);
        }

        return 0;
    }

    private int GetMaxLevel()
    {
        PcLevelTableSO table = GetLevelTable();
        if (table != null)
        {
            return table.MaxLevel;
        }

        return 1;
    }

    private PcLevelTableSO GetLevelTable()
    {
        if (levelTable != null)
        {
            return levelTable;
        }

        if (cachedDefaultLevelTable == null)
        {
            cachedDefaultLevelTable = Resources.Load<PcLevelTableSO>(DefaultLevelTableResourcePath);
        }

        if (cachedDefaultLevelTable == null && !hasLoggedMissingLevelTable)
        {
            hasLoggedMissingLevelTable = true;
            Debug.LogError($"PcLevelTableSO not found at Resources/{DefaultLevelTableResourcePath}. Assign a level table asset to the PC or create the default resource asset.");
        }

        return cachedDefaultLevelTable;
    }

    private void UpdateLevelText()
    {
        if (levelText == null)
        {
            return;
        }

        levelText.text = $"Lv: {GetCurrentLevel()}";
    }

    public void OpenUpgradeShop()
    {
        Pc_Upgrade pcUpgrade = Pc_Upgrade.FindInstance();
        if (pcUpgrade == null)
        {
            Debug.LogWarning("Pc_Upgrade not found. Add Pc_Upgrade to the shop panel or name the panel UpgradeShop.");
            return;
        }

        pcUpgrade.OpenShop(this);
    }

    public float GetLevelProgress()
    {
        int totalExp = Mathf.Max(0, currentExperience);
        int currentLevel = GetCurrentLevel();
        int maxLevel = GetMaxLevel();

        if (currentLevel >= maxLevel)
        {
            return 1f;
        }

        int minExp = GetRequiredExp(currentLevel);
        int maxExp = GetRequiredExp(currentLevel + 1);
        if (maxExp <= minExp)
        {
            return 1f;
        }

        return Mathf.InverseLerp(minExp, maxExp, totalExp);
    }

    public int GetCurrentLevel()
    {
        return ResolveLevel(Mathf.Max(0, currentExperience));
    }

    public int GetCurrentExperienceValue()
    {
        return Mathf.Max(0, currentExperience);
    }

    public int GetDisplayMaxExperience()
    {
        int currentLevel = GetCurrentLevel();
        int maxLevel = GetMaxLevel();

        if (currentLevel >= maxLevel)
        {
            return Mathf.Max(GetCurrentExperienceValue(), GetRequiredExp(maxLevel));
        }

        return GetRequiredExp(currentLevel + 1);
    }

    public float GetEffectiveUsingTime()
    {
        PCUpgradeDataSO ssdUpgradeData = GetUpgradeData(PCUpgradeDataSO.UpgradeType.SSD);
        float effectivePercent = ssdUpgradeData != null
            ? ssdUpgradeData.GetUsingTimePercent(ssdLevel)
            : DefaultPercentBonus;

        return Mathf.Max(MinimumUsingDuration, earningTime * (DefaultPercentBonus / effectivePercent));
    }

    public int GetUsageFee(int basePrice)
    {
        PCUpgradeDataSO gpuUpgradeData = GetUpgradeData(PCUpgradeDataSO.UpgradeType.GPU);
        float bonusPercent = gpuUpgradeData != null
            ? gpuUpgradeData.GetIncomeBonusPercent(gpuLevel)
            : DefaultPercentBonus;

        return Mathf.Max(0, Mathf.RoundToInt(basePrice * (bonusPercent / DefaultPercentBonus)));
    }

    public int GetSatisfactionPointGain()
    {
        PCUpgradeDataSO cpuUpgradeData = GetUpgradeData(PCUpgradeDataSO.UpgradeType.CPU);
        return cpuUpgradeData != null
            ? cpuUpgradeData.GetSatisfactionPointGain(cpuLevel)
            : 0;
    }

    private void SetUpgradeLevel(PCUpgradeDataSO.UpgradeType upgradeType, int level)
    {
        switch (upgradeType)
        {
            case PCUpgradeDataSO.UpgradeType.GPU:
                gpuLevel = Mathf.Max(1, level);
                break;
            case PCUpgradeDataSO.UpgradeType.CPU:
                cpuLevel = Mathf.Max(1, level);
                break;
            case PCUpgradeDataSO.UpgradeType.SSD:
                ssdLevel = Mathf.Max(1, level);
                break;
        }
    }

    private PCUpgradeDataSO GetUpgradeData(PCUpgradeDataSO.UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case PCUpgradeDataSO.UpgradeType.GPU:
                return LoadUpgradeData(ref cachedGpuUpgradeData, ref hasLoggedMissingGpuUpgradeData, GpuUpgradeResourcePath, upgradeType);
            case PCUpgradeDataSO.UpgradeType.CPU:
                return LoadUpgradeData(ref cachedCpuUpgradeData, ref hasLoggedMissingCpuUpgradeData, CpuUpgradeResourcePath, upgradeType);
            case PCUpgradeDataSO.UpgradeType.SSD:
                return LoadUpgradeData(ref cachedSsdUpgradeData, ref hasLoggedMissingSsdUpgradeData, SsdUpgradeResourcePath, upgradeType);
            default:
                return null;
        }
    }

    private static PCUpgradeDataSO LoadUpgradeData(
        ref PCUpgradeDataSO cachedUpgradeData,
        ref bool hasLoggedMissingUpgradeData,
        string resourcePath,
        PCUpgradeDataSO.UpgradeType upgradeType)
    {
        if (cachedUpgradeData != null)
        {
            return cachedUpgradeData;
        }

        cachedUpgradeData = Resources.Load<PCUpgradeDataSO>(resourcePath);
        if (cachedUpgradeData == null && !hasLoggedMissingUpgradeData)
        {
            hasLoggedMissingUpgradeData = true;
            Debug.LogError($"PCUpgradeDataSO for {upgradeType} not found at Resources/{resourcePath}.");
        }

        return cachedUpgradeData;
    }
}
