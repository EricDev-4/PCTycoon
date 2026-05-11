using UnityEngine;

[CreateAssetMenu(fileName = "PCUpgrade", menuName = "SO/PCUpgrade", order = int.MaxValue)]
public class PCUpgradeDataSO : ScriptableObject
{
    public enum UpgradeType
    {
        GPU,
        CPU,
        SSD
    };

    public UpgradeType type;

    public int level = 1;

    public int cost;

    public float ExpBonus;
    public float incomeBonus; // 비용보너스
    public float satisfactionBonus; // 만족도 보너스

}
