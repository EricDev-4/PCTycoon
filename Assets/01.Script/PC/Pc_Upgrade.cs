using UnityEngine;

public class Pc_Upgrade : MonoBehaviour
{
    [SerializeField] private PC selectedPC;
    [SerializeField] private GameObject upgradeShopPanel;

    public PC SelectedPC => selectedPC;

    private void Awake()
    {
        if (upgradeShopPanel == null)
        {
            upgradeShopPanel = gameObject;
        }
        gameObject.SetActive(false);
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

        upgradeShopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        if (upgradeShopPanel == null)
        {
            upgradeShopPanel = gameObject;
        }

        upgradeShopPanel.SetActive(false);
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
