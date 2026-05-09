using UnityEngine;

public class MoneyPickup : MonoBehaviour
{
    private BoxCollider2D col;
    [SerializeField] private int price;
    private bool isPickedUp;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        isPickedUp = false;
        if (col == null) col = GetComponent<BoxCollider2D>();
        if (col != null) col.enabled = true;
    }

    // private void OnTriggerEnter2D(Collider2D collider)
    // {
    //     if (isPickedUp) return;
    //     // if (!collider.gameObject.CompareTag("Player")) return;

    //     // isPickedUp = true;
    //     // if (col != null) col.enabled = false;

    //     GameManager.instance.money += price;

    //     var pooledObject = GetComponent<PooledObject>() ?? GetComponentInParent<PooledObject>();
    //     if (ObjectPool.Instance != null && pooledObject != null && !string.IsNullOrWhiteSpace(pooledObject.PoolTag))
    //     {
    //         ObjectPool.Instance.Despawn(pooledObject.gameObject);
    //         return;
    //     }
    //     Destroy(gameObject);
    // }
    public void GetMoney()
    {
        if (isPickedUp) return;
        // if (!collider.gameObject.CompareTag("Player")) return;

        // isPickedUp = true;
        // if (col != null) col.enabled = false;

        GameManager.instance.money += price;

        var pooledObject = GetComponent<PooledObject>() ?? GetComponentInParent<PooledObject>();
        if (ObjectPool.Instance != null && pooledObject != null && !string.IsNullOrWhiteSpace(pooledObject.PoolTag))
        {
            ObjectPool.Instance.Despawn(pooledObject.gameObject);
            return;
        }
        Destroy(gameObject);
    }

    public void SetPrice(int price)
    {
        this.price = price;
    }
}
