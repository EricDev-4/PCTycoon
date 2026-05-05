using System.Collections.Generic;
using UnityEngine;

public class UnitTapController : MonoBehaviour
{
    private sealed class OrderRequest
    {
        public UnitFSM Unit { get; }
        public FoodSO Food { get; }

        public OrderRequest(UnitFSM unit, FoodSO food)
        {
            Unit = unit;
            Food = food;
        }
    }

    [SerializeField] private OrderListUI orderListUI;
    [SerializeField] private Transform[] foodServingLinePos;

    private readonly Queue<OrderRequest> pendingOrders = new Queue<OrderRequest>();
    private readonly HashSet<UnitFSM> queuedUnits = new HashSet<UnitFSM>();
    private Camera mainCamera;

    private void Awake()
    {
        if (orderListUI == null)
        {
            orderListUI = FindAnyObjectByType<OrderListUI>();
        }

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryEnqueueUnitOrder();
        }

        if (Input.GetMouseButton(0))
        {
            TryCookAtKitchen();
        }
    }

    private void TryEnqueueUnitOrder()
    {
        if (!TryGetUnitAtPointer(out UnitFSM unit)) return;
        if (unit == null || unit.requestedFood == null) return;
        if (queuedUnits.Contains(unit)) return;

        pendingOrders.Enqueue(new OrderRequest(unit, unit.requestedFood));
        queuedUnits.Add(unit);
        orderListUI?.AddOrder(unit, unit.requestedFood);
        unit.ResetTextBubble();

        if (unit.interactiveColl != null)
        {
            unit.interactiveColl.enabled = false;
        }
    }

    private void TryCookAtKitchen()
    {
        if (!TryGetKitchenAtPointer(out Kitchen kitchen)) return;
        if (!TryGetCurrentOrder(out OrderRequest order)) return;
        if (!TryGetEmptyServingLineSlot(out Transform servingSlot)) return;

        FoodSO cookedFood = kitchen.Cooking(order.Food);
        if (cookedFood == null) return;

        SpawnCookedFood(cookedFood, servingSlot);

        pendingOrders.Dequeue();
        queuedUnits.Remove(order.Unit);
        orderListUI?.RemoveOrder(order.Unit);
    }

    private bool TryGetCurrentOrder(out OrderRequest order)
    {
        order = null;

        while (pendingOrders.Count > 0)
        {
            OrderRequest nextOrder = pendingOrders.Peek();
            if (nextOrder == null || nextOrder.Unit == null || nextOrder.Food == null)
            {
                pendingOrders.Dequeue();

                if (nextOrder != null && nextOrder.Unit != null)
                {
                    queuedUnits.Remove(nextOrder.Unit);
                    orderListUI?.RemoveOrder(nextOrder.Unit);
                }

                continue;
            }

            order = nextOrder;
            return true;
        }

        return false;
    }

    private bool TryGetEmptyServingLineSlot(out Transform servingSlot)
    {
        servingSlot = null;

        if (foodServingLinePos == null || foodServingLinePos.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < foodServingLinePos.Length; i++)
        {
            Transform slot = foodServingLinePos[i];
            if (slot == null) continue;
            if (slot.childCount > 0) continue;

            servingSlot = slot;
            return true;
        }

        return false;
    }

    private void SpawnCookedFood(FoodSO cookedFood, Transform servingSlot)
    {
        GameObject spawnedFood = Instantiate(cookedFood.prefab, servingSlot);
        Vector3 parentScale = servingSlot.lossyScale;
        Vector3 localScale = spawnedFood.transform.localScale;

        spawnedFood.transform.localScale = new Vector3(
            parentScale.x != 0f ? localScale.x / parentScale.x : localScale.x,
            parentScale.y != 0f ? localScale.y / parentScale.y : localScale.y,
            parentScale.z != 0f ? localScale.z / parentScale.z : localScale.z);
    }

    private bool TryGetUnitAtPointer(out UnitFSM unit)
    {
        unit = null;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null) return false;

        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] colliders = Physics2D.OverlapPointAll(mousePos);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null) continue;

            unit = colliders[i].GetComponentInParent<UnitFSM>();
            if (unit != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetKitchenAtPointer(out Kitchen kitchen)
    {
        kitchen = null;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null) return false;

        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] colliders = Physics2D.OverlapPointAll(mousePos);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null) continue;

            kitchen = colliders[i].GetComponentInParent<Kitchen>();
            if (kitchen != null)
            {
                return true;
            }
        }

        return false;
    }
}
