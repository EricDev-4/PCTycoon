using UnityEngine;

[CreateAssetMenu(fileName = "PCUpgrade", menuName = "SO/PCUpgrade", order = int.MaxValue)]
public class PCUpgradeDataSO : ScriptableObject
{
    private const float GpuIncomeBonusPerLevel = 0.06f;
    private const float CpuSatisfactionBonusPerLevel = 1.7f;
    private const float SsdUsingTimeReductionPerLevel = 0.008f;

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
    public float incomeBonus; // 사용료보너스
    public float satisfactionBonus; // 만족도 보너스
    public float decresingUsingTimePercent; // 이용 시간 감소 %

    public static float CalculateGpuIncomeBonusRate(int level)
    {
        return Mathf.Max(0, level - 1) * GpuIncomeBonusPerLevel;
    }

    public static float CalculateCpuSatisfactionBonus(int level)
    {
        return Mathf.Max(0, level - 1) * CpuSatisfactionBonusPerLevel;
    }

    public static float CalculateSsdUsingTimeReductionRate(int level)
    {
        return Mathf.Max(0, level - 1) * SsdUsingTimeReductionPerLevel;
    }
}
