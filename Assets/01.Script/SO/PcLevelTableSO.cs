using UnityEngine;

[CreateAssetMenu(fileName = "PcLevelTable", menuName = "SO/PC Level Table", order = int.MaxValue)]
public class PcLevelTableSO : ScriptableObject
{
    private static readonly int[] EmptyLevelTable = { 0 };

    [SerializeField] private int[] requiredTotalExpByLevel =
    {
        0, 100, 130, 170, 220, 290, 380, 500, 650, 850,
        1100, 1400, 1800, 2300, 3000, 3900, 5100, 6600, 8600, 11000,
        14000, 18000, 23000, 30000, 39000, 51000, 66000, 86000, 110000, 140000
    };

    public int MaxLevel => GetSafeTable().Length;

    public int ResolveLevel(int totalExperience)
    {
        int[] table = GetSafeTable();
        totalExperience = Mathf.Max(0, totalExperience);

        for (int i = table.Length - 1; i >= 0; i--)
        {
            if (totalExperience < table[i])
            {
                continue;
            }

            return i + 1;
        }

        return 1;
    }

    public int GetRequiredExp(int level)
    {
        int[] table = GetSafeTable();
        int clampedLevel = Mathf.Clamp(level, 1, table.Length);
        return table[clampedLevel - 1];
    }

    private int[] GetSafeTable()
    {
        if (requiredTotalExpByLevel == null || requiredTotalExpByLevel.Length == 0)
        {
            return EmptyLevelTable;
        }

        return requiredTotalExpByLevel;
    }

    private void OnValidate()
    {
        if (requiredTotalExpByLevel == null || requiredTotalExpByLevel.Length == 0)
        {
            requiredTotalExpByLevel = new[] { 0 };
            return;
        }

        requiredTotalExpByLevel[0] = 0;

        for (int i = 1; i < requiredTotalExpByLevel.Length; i++)
        {
            requiredTotalExpByLevel[i] = Mathf.Max(0, requiredTotalExpByLevel[i]);
            if (requiredTotalExpByLevel[i] < requiredTotalExpByLevel[i - 1])
            {
                requiredTotalExpByLevel[i] = requiredTotalExpByLevel[i - 1];
            }
        }
    }
}
