using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public SpriteRenderer foodSprite;

    public FoodSO servingFood;

    void Start()
    {
    }

    public void InitFood(FoodSO foodSO)
    {
        servingFood = foodSO;
        foodSprite.sprite = foodSO.foodIcon;
        foodSprite.gameObject.SetActive(true);
    }
    public void ClearFood()
    {
        servingFood = null;
        foodSprite.sprite = null;
        foodSprite.gameObject.SetActive(false);
    }
}
