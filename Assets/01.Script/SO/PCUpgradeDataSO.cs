using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PCUpgrade", menuName = "SO/PCUpgrade", order = int.MaxValue)]
public class PCUpgradeDataSO : ScriptableObject
{
    private const float BasePercentValue = 100f;
    private const float DefaultPercentDelta = 0f;

    public enum UpgradeType
    {
        GPU,
        CPU,
        SSD
    }

    [System.Serializable]
    public class UpgradeTier
    {
        public int cost;
        public float ExpBonus;
        public float incomeBonus = DefaultPercentDelta;
        [FormerlySerializedAs("satisfactionBonus")]
        public int satisfactionPointGain;
        public float decresingUsingTimePercent = DefaultPercentDelta;
    }

    public UpgradeType type;
    public List<UpgradeTier> tiers = new List<UpgradeTier>();

    public int MaxLevel => Mathf.Max(1, tiers.Count);

    public bool CanUpgradeFromLevel(int currentLevel)
    {
        return HasLevel(currentLevel + 1);
    }

    public int GetUpgradeCost(int nextLevel)
    {
        return TryGetTier(nextLevel, out UpgradeTier tier)
            ? Mathf.Max(0, tier.cost)
            : -1;
    }

    public float GetExpBonus(int level)
    {
        return TryGetTier(level, out UpgradeTier tier)
            ? Mathf.Max(0f, tier.ExpBonus)
            : 0f;
    }

    public float GetIncomeBonusPercent(int level)
    {
        return BasePercentValue + GetIncomeBonusDeltaPercent(level);
    }

    public float GetIncomeBonusDeltaPercent(int level)
    {
        return TryGetTier(level, out UpgradeTier tier)
            ? Mathf.Max(0f, tier.incomeBonus)
            : DefaultPercentDelta;
    }

    public int GetSatisfactionPointGain(int level)
    {
        return TryGetTier(level, out UpgradeTier tier)
            ? Mathf.Max(0, tier.satisfactionPointGain)
            : 0;
    }

    public int GetSatisfactionPointGainDelta(int level)
    {
        int currentGain = GetSatisfactionPointGain(level);
        int baseGain = GetSatisfactionPointGain(1);
        return Mathf.Max(0, currentGain - baseGain);
    }

    public float GetSatisfactionPointGainPercentFromBase(int level)
    {
        int baseGain = GetSatisfactionPointGain(1);
        if (baseGain <= 0)
        {
            return 0f;
        }

        return (GetSatisfactionPointGainDelta(level) / (float)baseGain) * BasePercentValue;
    }

    public float GetUsingTimePercent(int level)
    {
        return BasePercentValue + GetUsingTimeReductionDeltaPercent(level);
    }

    public float GetUsingTimeReductionDeltaPercent(int level)
    {
        return TryGetTier(level, out UpgradeTier tier)
            ? Mathf.Max(0f, tier.decresingUsingTimePercent)
            : DefaultPercentDelta;
    }

    public bool TryGetTier(int level, out UpgradeTier tier)
    {
        if (HasLevel(level))
        {
            tier = tiers[level - 1];
            return true;
        }

        tier = null;
        return false;
    }

    private bool HasLevel(int level)
    {
        return level >= 1 && level <= tiers.Count;
    }
}
