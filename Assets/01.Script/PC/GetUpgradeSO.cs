using UnityEngine;
using UnityEngine.UI;

public class GetUpgradeSO : MonoBehaviour
{
    public PCUpgradeDataSO upgradeDataSO;

    private Button cachedButton;

    private void Awake()
    {
        cachedButton = GetComponentInChildren<Button>(true);
        if (cachedButton == null)
        {
            Debug.LogWarning($"No Button found under {name}. GetUpgradeSO will not receive click events.");
            return;
        }

        cachedButton.onClick.AddListener(HandleUpgradeButtonClicked);
    }

    private void OnDestroy()
    {
        if (cachedButton != null)
        {
            cachedButton.onClick.RemoveListener(HandleUpgradeButtonClicked);
        }
    }

    private void HandleUpgradeButtonClicked()
    {
        if (upgradeDataSO == null)
        {
            Debug.LogWarning($"No upgrade data is assigned for {name}.");
            return;
        }

        Pc_Upgrade pcUpgrade = GetComponentInParent<Pc_Upgrade>(true);
        if (pcUpgrade == null)
        {
            pcUpgrade = Pc_Upgrade.FindInstance();
        }

        if (pcUpgrade == null)
        {
            Debug.LogWarning("Pc_Upgrade not found. Cannot apply upgrade.");
            return;
        }

        if (pcUpgrade.SelectedPC == null)
        {
            Debug.LogWarning("No PC is selected. Cannot apply upgrade.");
            return;
        }

        int cost = pcUpgrade.SelectedPC.GetUpgradeCost(upgradeDataSO);
        if (cost < 0)
        {
            Debug.Log($"{upgradeDataSO.type} upgrade is already at max level.");
            return;
        }

        if (GameManager.instance == null)
        {
            Debug.LogWarning("GameManager instance is missing. Cannot apply upgrade.");
            return;
        }

        if (GameManager.instance.money >= cost)
        {
            if (pcUpgrade.ApplyUpgrade(upgradeDataSO))
            {
                GameManager.instance.money -= cost;
            }
        }
        else
        {
            // 돈이 부족합니다!
            Debug.Log("츄르가 부족합니다!");
        }
    }
}
