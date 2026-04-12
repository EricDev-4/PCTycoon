using UnityEngine;
using UnityEngine.UIElements.Experimental;


[CreateAssetMenu(fileName ="MoneySO", menuName ="SO/Money", order = int.MaxValue)]
public class MoneySO : ScriptableObject
{
    public int price;

    public GameObject prefab;
    public Sprite sprite;

}
