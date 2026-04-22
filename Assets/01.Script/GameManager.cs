using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
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




    [SerializeField] Transform door;
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

        for (int i =0; i < unitList.Count; i++)
        {
            if (unitList[i] == null) continue;
            if (i >= pcList.Count) break;

            PC pc = pcList[i];
            if (pc == null) continue;

            pc.isTargeted = true;
            pc.isArrived = false;
            unitList[i].Setup(pc);
        }
    }

    void Update()
    {
        if (ObjectPool.Instance == null) return;
        if (door == null) return;

        // 비어있는 pc가 있으면 반복
        spawnCooldown -= Time.deltaTime;
        if (spawnCooldown > 0f) return;

        // 비어있는 pc가 있으면 스폰 (스폰 텀 적용)
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

                // Pc 후보가 사용중이거나 타겟이 아니면 return
            if (!candidate.isUsing && !candidate.isTargeted)
            {
                return candidate;
            }
        }
        return null;
    }

    private bool TrySpawnAndAssignPc(string poolTag, Vector3 spawnPosition, PC pc)
    {
        GameObject spawned = ObjectPool.Instance.SpawnFormPool(poolTag, spawnPosition);
        if (spawned == null) return false;

        UnitFSM unit = spawned.GetComponentInChildren<UnitFSM>();
        if (unit == null)
        {
            Debug.LogWarning($"GameManager: Spawned '{poolTag}' doesn't have UnitFSM. Despawning.");
            ObjectPool.Instance.Despawn(spawned);
            return false;
        }

        pc.isTargeted = true;
        pc.isArrived = false;
        unit.Setup(pc);
        return true;
    }
}
