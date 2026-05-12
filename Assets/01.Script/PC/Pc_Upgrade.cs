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
