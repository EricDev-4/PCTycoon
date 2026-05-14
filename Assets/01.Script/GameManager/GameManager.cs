using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private const float SatisfactionDisplayDivisor = 10f;

    public static GameManager instance;
    public static event System.Action<int> OnMoneyChanged;

    public List<PC> pcList = new List<PC>();
    public List<UnitFSM> unitList = new List<UnitFSM>();

    public FoodMenuSO foodMenu;
    public int _money;
    public int money
    {
        get => _money;
        set
        {
            _money = value;
            OnMoneyChanged?.Invoke(_money);
        }
    }

    [FormerlySerializedAs("satisfaction")]
    public int satisfactionPoint;
    [SerializeField] private TMP_Text satisfactionText;
    private int lastSatisfactionPoint = int.MinValue;

    [SerializeField] private Transform door;
    [SerializeField] private float spawnInterval = 1f;
    private float spawnCooldown;

    private void FindPC()
    {
        PC[] found = FindObjectsByType<PC>(FindObjectsSortMode.None);
        foreach (PC pc in found)
        {
            pcList.Add(pc);
        }
    }

    private void FindUnit()
    {
        UnitFSM[] found = FindObjectsByType<UnitFSM>(FindObjectsSortMode.None);
        foreach (UnitFSM unit in found)
        {
            unitList.Add(unit);
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FindPC();
        FindUnit();
        RefreshSatisfactionText();
    }

    private void Update()
    {
        RefreshSatisfactionText();

        if (ObjectPool.Instance == null) return;
        if (door == null) return;

        spawnCooldown -= Time.deltaTime;
        if (spawnCooldown > 0f) return;

        PC pc = GetAvailablePC();
        if (pc == null) return;

        TrySpawnAndAssignPc("Gamer", door.position, pc);
        spawnCooldown = Mathf.Max(0.01f, spawnInterval);
    }

    private PC GetAvailablePC()
    {
        for (int i = 0; i < pcList.Count; i++)
        {
            PC candidate = pcList[i];
            if (candidate == null) continue;

            if (!candidate.isUsing && !candidate.isTargeted)
            {
                return candidate;
            }
        }

        return null;
    }

    private void TrySpawnAndAssignPc(string poolTag, Vector3 spawnPosition, PC pc)
    {
        GameObject spawned = ObjectPool.Instance.SpawnFormPool(poolTag, spawnPosition);
        if (spawned == null) return;

        UnitFSM unit = spawned.GetComponentInChildren<UnitFSM>();
        if (unit == null)
        {
            Debug.LogWarning($"GameManager: Spawned '{poolTag}' doesn't have UnitFSM. Despawning.");
            ObjectPool.Instance.Despawn(spawned);
            return;
        }

        unit.AssignToPC(pc);
    }

    private void RefreshSatisfactionText()
    {
        if (lastSatisfactionPoint == satisfactionPoint) return;

        lastSatisfactionPoint = satisfactionPoint;

        if (satisfactionText == null) return;
        satisfactionText.text = $"\uB9CC\uC871\uB3C4 : {FormatDisplayedSatisfaction(satisfactionPoint)}";
    }

    public static float GetDisplayedSatisfactionValue(int satisfactionPoints)
    {
        return Mathf.Max(0, satisfactionPoints) / SatisfactionDisplayDivisor;
    }

    public static string FormatDisplayedSatisfaction(int satisfactionPoints)
    {
        return GetDisplayedSatisfactionValue(satisfactionPoints).ToString("0.0");
    }
}
