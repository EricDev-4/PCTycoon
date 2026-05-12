using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PCUpgrade", menuName = "SO/PCUpgrade", order = int.MaxValue)]
public class PCUpgradeDataSO : ScriptableObject
{
    private const float DefaultPercentBonus = 100f;

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
        public float incomeBonus = DefaultPercentBonus;
        [FormerlySerializedAs("satisfactionBonus")]
        public int satisfactionPointGain;
        public float decresingUsingTimePercent = DefaultPercentBonus;
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
        return TryGetTier(level, out UpgradeTier tier) && tier.incomeBonus > 0f
            ? tier.incomeBonus
            : DefaultPercentBonus;
    }

    public int GetSatisfactionPointGain(int level)
    {
        return TryGetTier(level, out UpgradeTier tier)
            ? Mathf.Max(0, tier.satisfactionPointGain)
            : 0;
    }

    public float GetUsingTimePercent(int level)
    {
        return TryGetTier(level, out UpgradeTier tier) && tier.decresingUsingTimePercent > 0f
            ? tier.decresingUsingTimePercent
            : DefaultPercentBonus;
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
