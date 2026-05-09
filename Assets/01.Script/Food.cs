using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] private FoodSO foodSO;
    private UnitFSM targetUnit;

    public FoodSO FoodData => foodSO;
    public UnitFSM TargetUnit => targetUnit;

    public void Init(FoodSO food, UnitFSM unit)
    {
        foodSO = food;
        targetUnit = unit;
    }

    public bool TryServeTarget()
    {
        if (foodSO == null || targetUnit == null)
        {
            return false;
        }

        return targetUnit.TryServe(foodSO);
    }
}
