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

            unitList[i].Setup(pcList[i]);
        }


    }
}
