using UnityEngine;

[CreateAssetMenu(fileName = "SO", menuName = "SO/Food")]
public class FoodSO : ScriptableObject
{
    public GameObject prefab;
    public string foodName;
    public Sprite foodIcon;
    public float cookTime;
    public int foodPrice;
}
