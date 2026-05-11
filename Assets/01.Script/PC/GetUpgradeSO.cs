using UnityEngine;
using UnityEngine.UI;

public class GetUpgradeSO : MonoBehaviour
{
    public PCUpgradeDataSO UpgradeDataSO;

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
        if (UpgradeDataSO == null)
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

        pcUpgrade.ApplyUpgrade(UpgradeDataSO);
    }
}
