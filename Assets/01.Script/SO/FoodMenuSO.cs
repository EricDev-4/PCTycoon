using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO", menuName = "SO/FoodMenu")]

public class FoodMenuSO : ScriptableObject
{
    public List<FoodSO> foods;
}
