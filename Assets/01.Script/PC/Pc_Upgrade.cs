using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Pc_Upgrade : MonoBehaviour
{
    [SerializeField] private PC selectedPC;
    [SerializeField] private GameObject upgradeShopPanel;
    [SerializeField] private Slider currentExpBar;
    [SerializeField] private TMP_Text currentLevelText;
    [SerializeField] private TMP_Text currentExpText;
    [SerializeField] private TMP_Text gpuLevelText;
    [SerializeField] private TMP_Text cpuLevelText;
    [SerializeField] private TMP_Text ssdLevelText;
    [SerializeField] private TMP_Text gpuCurrentEffectText;
    [SerializeField] private TMP_Text cpuCurrentEffectText;
    [SerializeField] private TMP_Text ssdCurrentEffectText;
    [SerializeField] private TMP_Text gpuEffectDescriptionText;
    [SerializeField] private TMP_Text cpuEffectDescriptionText;
    [SerializeField] private TMP_Text ssdEffectDescriptionText;
    [SerializeField] private GetUpgradeSO gpuUpgradeSource;
    [SerializeField] private GetUpgradeSO cpuUpgradeSource;
    [SerializeField] private GetUpgradeSO ssdUpgradeSource;

    public PC SelectedPC => selectedPC;

    private void Awake()
    {
        if (upgradeShopPanel == null)
        {
            upgradeShopPanel = gameObject;
        }

        if (currentExpBar == null)
        {
            Slider[] sliders = GetComponentsInChildren<Slider>(true);
            for (int i = 0; i < sliders.Length; i++)
            {
                if (sliders[i] == null || sliders[i].name != "CurrentEXP_Bar")
                {
                    continue;
                }

                currentExpBar = sliders[i];
                break;
            }
        }

        if (currentLevelText == null || currentExpText == null)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == null)
                {
                    continue;
                }

                if (currentLevelText == null && texts[i].name == "CurrentLevel_Txt")
                {
                    currentLevelText = texts[i];
                    continue;
                }

                if (currentExpText == null && texts[i].name == "Exp_Text")
                {
                    currentExpText = texts[i];
                }
            }
        }

        CacheUpgradeUiReferences();
        RefreshSelectedPcUI();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        RefreshSelectedPcUI();
    }

    public void OpenShop(PC pc)
    {
        if (pc == null)
        {
            return;
        }

        selectedPC = pc;

        if (upgradeShopPanel == null)
        {
            upgradeShopPanel = gameObject;
        }

        RefreshSelectedPcUI();
        upgradeShopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        if (upgradeShopPanel == null)
        {
            upgradeShopPanel = gameObject;
        }

        RefreshCurrentExpBar(0f);
        RefreshCurrentLevelText(null);
        RefreshCurrentExpText(null);
        RefreshUpgradeLevelTexts(null);
        RefreshUpgradeEffectTexts(null);
        upgradeShopPanel.SetActive(false);
    }

    public bool ApplyUpgrade(PCUpgradeDataSO upgradeData)
    {
        if (selectedPC == null)
        {
            Debug.LogWarning("No PC is selected. Open the upgrade shop from a PC before applying an upgrade.");
            return false;
        }

        if (upgradeData == null)
        {
            Debug.LogWarning("Upgrade data is missing. Cannot apply upgrade.");
            return false;
        }

        if (!selectedPC.ApplyUpgrade(upgradeData))
        {
            return false;
        }

        RefreshSelectedPcUI();
        return true;
    }

    private void RefreshSelectedPcUI()
    {
        RefreshCurrentExpBar();
        RefreshCurrentLevelText(selectedPC);
        RefreshCurrentExpText(selectedPC);
        RefreshUpgradeLevelTexts(selectedPC);
        RefreshUpgradeEffectTexts(selectedPC);
    }

    private void RefreshCurrentExpBar()
    {
        if (selectedPC == null)
        {
            RefreshCurrentExpBar(0f);
            return;
        }

        RefreshCurrentExpBar(selectedPC.GetLevelProgress());
    }

    private void RefreshCurrentExpBar(float value)
    {
        if (currentExpBar == null)
        {
            return;
        }

        currentExpBar.SetValueWithoutNotify(Mathf.Clamp01(value));
    }

    private void RefreshCurrentLevelText(PC pc)
    {
        if (currentLevelText == null)
        {
            return;
        }

        if (pc == null)
        {
            currentLevelText.text = "\uB808\uBCA8 -";
            return;
        }

        currentLevelText.text = $"\uB808\uBCA8 {pc.GetCurrentLevel()}";
    }

    private void RefreshCurrentExpText(PC pc)
    {
        if (currentExpText == null)
        {
            return;
        }

        if (pc == null)
        {
            currentExpText.text = "0Exp / 0EXP";
            return;
        }

        currentExpText.text = $"{pc.GetCurrentExperienceValue()}Exp / {pc.GetDisplayMaxExperience()}EXP";
    }

    private void CacheUpgradeUiReferences()
    {
        gpuLevelText = FindUpgradeLevelText("GPU", gpuLevelText);
        cpuLevelText = FindUpgradeLevelText("CPU", cpuLevelText);
        ssdLevelText = FindUpgradeLevelText("SSD", ssdLevelText);

        gpuCurrentEffectText = FindUpgradeText("GPU", "Current_Effect", gpuCurrentEffectText);
        cpuCurrentEffectText = FindUpgradeText("CPU", "Current_Effect", cpuCurrentEffectText);
        ssdCurrentEffectText = FindUpgradeText("SSD", "Current_Effect", ssdCurrentEffectText);

        gpuEffectDescriptionText = FindUpgradeText("GPU", "Effect_Text_1", gpuEffectDescriptionText);
        cpuEffectDescriptionText = FindUpgradeText("CPU", "Effect_Text_1", cpuEffectDescriptionText);
        ssdEffectDescriptionText = FindUpgradeText("SSD", "Effect_Text_1", ssdEffectDescriptionText);

        gpuUpgradeSource = FindUpgradeComponent("GPU", gpuUpgradeSource);
        cpuUpgradeSource = FindUpgradeComponent("CPU", cpuUpgradeSource);
        ssdUpgradeSource = FindUpgradeComponent("SSD", ssdUpgradeSource);
    }

    private TMP_Text FindUpgradeLevelText(string upgradeObjectName, TMP_Text existingText)
    {
        if (existingText != null)
        {
            return existingText;
        }

        Transform[] childTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < childTransforms.Length; i++)
        {
            Transform childTransform = childTransforms[i];
            if (childTransform == null || childTransform.name != upgradeObjectName)
            {
                continue;
            }

            TMP_Text[] texts = childTransform.GetComponentsInChildren<TMP_Text>(true);
            for (int j = 0; j < texts.Length; j++)
            {
                if (texts[j] != null && texts[j].name == "level_Text")
                {
                    return texts[j];
                }
            }
        }

        return null;
    }

    private TMP_Text FindUpgradeText(string upgradeObjectName, string textObjectName, TMP_Text existingText)
    {
        if (existingText != null)
        {
            return existingText;
        }

        Transform upgradeTransform = FindUpgradeTransform(upgradeObjectName);
        if (upgradeTransform == null)
        {
            return null;
        }

        Transform[] childTransforms = upgradeTransform.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < childTransforms.Length; i++)
        {
            Transform childTransform = childTransforms[i];
            if (childTransform == null || childTransform.name != textObjectName)
            {
                continue;
            }

            return childTransform.GetComponent<TMP_Text>();
        }

        return null;
    }

    private GetUpgradeSO FindUpgradeComponent(string upgradeObjectName, GetUpgradeSO existingComponent)
    {
        if (existingComponent != null)
        {
            return existingComponent;
        }

        Transform upgradeTransform = FindUpgradeTransform(upgradeObjectName);
        return upgradeTransform != null ? upgradeTransform.GetComponent<GetUpgradeSO>() : null;
    }

    private Transform FindUpgradeTransform(string upgradeObjectName)
    {
        Transform[] childTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < childTransforms.Length; i++)
        {
            Transform childTransform = childTransforms[i];
            if (childTransform != null && childTransform.name == upgradeObjectName)
            {
                return childTransform;
            }
        }

        return null;
    }

    private void RefreshUpgradeLevelTexts(PC pc)
    {
        RefreshUpgradeLevelText(gpuLevelText, pc, PCUpgradeDataSO.UpgradeType.GPU);
        RefreshUpgradeLevelText(cpuLevelText, pc, PCUpgradeDataSO.UpgradeType.CPU);
        RefreshUpgradeLevelText(ssdLevelText, pc, PCUpgradeDataSO.UpgradeType.SSD);
    }

    private void RefreshUpgradeLevelText(TMP_Text targetText, PC pc, PCUpgradeDataSO.UpgradeType upgradeType)
    {
        if (targetText == null)
        {
            return;
        }

        if (pc == null)
        {
            targetText.text = "Lv-";
            return;
        }

        targetText.text = $"Lv{pc.GetUpgradeLevel(upgradeType)}";
    }

    private void RefreshUpgradeEffectTexts(PC pc)
    {
        RefreshUpgradeEffectText(pc, gpuUpgradeSource, gpuCurrentEffectText, gpuEffectDescriptionText);
        RefreshUpgradeEffectText(pc, cpuUpgradeSource, cpuCurrentEffectText, cpuEffectDescriptionText);
        RefreshUpgradeEffectText(pc, ssdUpgradeSource, ssdCurrentEffectText, ssdEffectDescriptionText);
    }

    private void RefreshUpgradeEffectText(
        PC pc,
        GetUpgradeSO upgradeSource,
        TMP_Text currentEffectText,
        TMP_Text effectDescriptionText)
    {
        if (currentEffectText == null && effectDescriptionText == null)
        {
            return;
        }

        if (pc == null || upgradeSource == null || upgradeSource.upgradeDataSO == null)
        {
            if (currentEffectText != null)
            {
                currentEffectText.text = "-";
            }

            if (effectDescriptionText != null)
            {
                effectDescriptionText.text = "-";
            }

            return;
        }

        PCUpgradeDataSO upgradeData = upgradeSource.upgradeDataSO;
        int currentLevel = pc.GetUpgradeLevel(upgradeData.type);
        bool hasNextLevel = upgradeData.CanUpgradeFromLevel(currentLevel);
        int nextLevel = hasNextLevel ? currentLevel + 1 : currentLevel;

        switch (upgradeData.type)
        {
            case PCUpgradeDataSO.UpgradeType.GPU:
            {
                float currentValue = upgradeData.GetIncomeBonusDeltaPercent(currentLevel);
                float nextValue = upgradeData.GetIncomeBonusDeltaPercent(nextLevel);
                RefreshPercentEffectTexts(
                    currentEffectText,
                    effectDescriptionText,
                    "\uBE44\uC6A9",
                    "\uBE44\uC6A9 \uC99D\uAC00",
                    currentValue,
                    nextValue,
                    hasNextLevel);
                break;
            }
            case PCUpgradeDataSO.UpgradeType.CPU:
            {
                float currentValue = GameManager.GetDisplayedSatisfactionValue(upgradeData.GetSatisfactionPointGain(currentLevel));
                float nextValue = GameManager.GetDisplayedSatisfactionValue(upgradeData.GetSatisfactionPointGain(nextLevel));
                RefreshSatisfactionEffectTexts(currentEffectText, effectDescriptionText, currentValue, nextValue, hasNextLevel);
                break;
            }
            case PCUpgradeDataSO.UpgradeType.SSD:
            {
                float currentValue = upgradeData.GetUsingTimeReductionDeltaPercent(currentLevel);
                float nextValue = upgradeData.GetUsingTimeReductionDeltaPercent(nextLevel);
                RefreshPercentEffectTexts(
                    currentEffectText,
                    effectDescriptionText,
                    "\uC0AC\uC6A9 \uC2DC\uAC04 \uAC10\uC18C",
                    "\uC0AC\uC6A9 \uC2DC\uAC04 \uAC10\uC18C",
                    currentValue,
                    nextValue,
                    hasNextLevel);
                break;
            }
        }
    }

    private void RefreshPercentEffectTexts(
        TMP_Text currentEffectText,
        TMP_Text effectDescriptionText,
        string currentLabel,
        string descriptionLabel,
        float currentValue,
        float nextValue,
        bool hasNextLevel)
    {
        if (currentEffectText != null)
        {
            currentEffectText.text = hasNextLevel
                ? $"{currentLabel} {FormatPercent(currentValue)} -> {FormatPercent(nextValue)}"
                : $"{currentLabel} {FormatPercent(currentValue)} (MAX)";
        }

        if (effectDescriptionText != null)
        {
            effectDescriptionText.text = hasNextLevel
                ? $"{descriptionLabel} +{FormatPercent(nextValue - currentValue)}"
                : "\uCD5C\uB300 \uB808\uBCA8";
        }
    }

    private void RefreshSatisfactionEffectTexts(
        TMP_Text currentEffectText,
        TMP_Text effectDescriptionText,
        float currentValue,
        float nextValue,
        bool hasNextLevel)
    {
        if (currentEffectText != null)
        {
            currentEffectText.text = hasNextLevel
                ? $"\uB9CC\uC871\uB3C4 {FormatSatisfactionValue(currentValue)} -> {FormatSatisfactionValue(nextValue)}"
                : $"\uB9CC\uC871\uB3C4 {FormatSatisfactionValue(currentValue)} (MAX)";
        }

        if (effectDescriptionText != null)
        {
            effectDescriptionText.text = hasNextLevel
                ? $"\uC190\uB2D8 \uB9CC\uC871\uB3C4 \uC99D\uAC00 +{FormatSatisfactionValue(nextValue - currentValue)}"
                : "\uCD5C\uB300 \uB808\uBCA8";
        }
    }

    private static string FormatPercent(float value)
    {
        return $"{FormatNumericValue(value)}%";
    }

    private static string FormatNumericValue(float value)
    {
        float roundedValue = Mathf.Round(value);
        return Mathf.Approximately(value, roundedValue)
            ? roundedValue.ToString("0")
            : value.ToString("0.#");
    }

    private static string FormatSatisfactionValue(float value)
    {
        return value.ToString("0.0");
    }

    public static Pc_Upgrade FindInstance()
    {
        Pc_Upgrade[] upgrades = Resources.FindObjectsOfTypeAll<Pc_Upgrade>();
        for (int i = 0; i < upgrades.Length; i++)
        {
            Pc_Upgrade upgrade = upgrades[i];
            if (upgrade == null)
            {
                continue;
            }

            if (!upgrade.gameObject.scene.IsValid())
            {
                continue;
            }

            return upgrade;
        }

        GameObject[] sceneObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < sceneObjects.Length; i++)
        {
            GameObject sceneObject = sceneObjects[i];
            if (sceneObject == null)
            {
                continue;
            }

            if (!sceneObject.scene.IsValid())
            {
                continue;
            }

            if (sceneObject.name != "UpgradeShop")
            {
                continue;
            }

            Pc_Upgrade upgrade = sceneObject.GetComponent<Pc_Upgrade>();
            if (upgrade == null)
            {
                upgrade = sceneObject.AddComponent<Pc_Upgrade>();
            }

            return upgrade;
        }

        return null;
    }
}
