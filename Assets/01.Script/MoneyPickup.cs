using UnityEngine;

public class MoneyPickup : MonoBehaviour
{
    private BoxCollider2D col;
    [SerializeField] private int price;


    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("Something touched me!");
        if (collider.gameObject.CompareTag("Player"))
        {
            GameManager.instance.money += price;
            Destroy(gameObject);
        }
    }

    public void SetPrice(int price)
    {
        this.price = price;
        Debug.Log("Price : " + price);
    }
}
