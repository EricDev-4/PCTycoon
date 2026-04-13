using UnityEngine;

[CreateAssetMenu(fileName = "SO", menuName = "SO/Food")]
public class FoodSO : ScriptableObject
{
    public string foodName;
    public Sprite icon;
    public float cookTime;
    public int price;
}
