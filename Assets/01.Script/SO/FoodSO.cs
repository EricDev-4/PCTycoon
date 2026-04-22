using UnityEngine;

[CreateAssetMenu(fileName = "SO", menuName = "SO/Food")]
public class FoodSO : ScriptableObject
{
    public string foodName;
    public Sprite foodIcon;
    public float cookTime;
    public int foodPrice;
}
