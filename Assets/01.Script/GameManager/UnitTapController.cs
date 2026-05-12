using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

    // Stores cooking order in first-in, first-out order.
    private readonly Queue<OrderRequest> pendingOrders = new Queue<OrderRequest>();
    // Tracks units that already have an accepted or cooked order.
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
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (TryServeFood()) return;
            if (TryGetMoney()) return;
            if (TryEnqueueUnitOrder()) return;
            if (TryOpenPcUpgrade()) return;
        }

        if (Input.GetMouseButton(0))
        {
            TryCookAtKitchen();
        }
    }

    private bool TryEnqueueUnitOrder()
    {
        if (!TryGetComponentAtPointer(out UnitFSM unit)) return false;
        if (unit == null || unit.requestedFood == null) return false;
        if (queuedUnits.Contains(unit)) return false;

        pendingOrders.Enqueue(new OrderRequest(unit, unit.requestedFood));
        queuedUnits.Add(unit);
        orderListUI?.AddOrder(unit, unit.requestedFood);
        unit.ResetTextBubble();

        if (unit.targetPC != null)
        {
            unit.targetPC.SetUsagePaused(true);
        }

        if (unit.interactiveColl != null)
        {
            unit.interactiveColl.enabled = false;
        }

        return true;
    }

    private void TryCookAtKitchen()
    {
        if (!TryGetComponentAtPointer(out Kitchen kitchen)) return;
        if (kitchen == null) return;

        if (kitchen.IsIdle && !TryAssignNextOrderToKitchen(kitchen)) return;
        if (!TryGetEmptyServingLineSlot(out Transform servingSlot)) return;

        if (!kitchen.TryCookAssignedOrder(out FoodSO cookedFood, out UnitFSM targetUnit)) return;

        SpawnCookedFood(cookedFood, servingSlot, targetUnit);
    }

    private bool TryGetMoney()
    {
        if (!TryGetComponentAtPointer(out MoneyPickup money)) return false;
        if (money == null) return false;

        money.GetMoney();
        return true;
    }

    private bool TryOpenPcUpgrade()
    {
        if (!TryGetComponentAtPointer(out PC pc)) return false;
        if (pc == null) return false;

        pc.OpenUpgradeShop();
        return true;
    }

    private bool TryServeFood()
    {
        if (!TryGetComponentAtPointer(out Food food)) return false;
        if (food == null) return false;

        UnitFSM targetUnit = food.TargetUnit;
        bool served = food.TryServeTarget();

        if (targetUnit != null && (served || targetUnit.requestedFood == null))
        {
            CleanupAcceptedOrder(targetUnit);
        }

        if (served || targetUnit == null || targetUnit.requestedFood == null)
        {
            Destroy(food.gameObject);
        }

        return true;
    }

    private void CleanupAcceptedOrder(UnitFSM unit)
    {
        if (unit == null)
        {
            return;
        }

        queuedUnits.Remove(unit);
        orderListUI?.RemoveOrder(unit);
    }

    private bool TryAssignNextOrderToKitchen(Kitchen kitchen)
    {
        if (kitchen == null || !kitchen.IsIdle)
        {
            return false;
        }

        while (pendingOrders.Count > 0)
        {
            OrderRequest nextOrder = pendingOrders.Peek();
            if (nextOrder == null || nextOrder.Unit == null || nextOrder.Food == null)
            {
                pendingOrders.Dequeue();

                if (nextOrder != null && nextOrder.Unit != null)
                {
                    CleanupAcceptedOrder(nextOrder.Unit);
                }

                continue;
            }

            if (!kitchen.TryAssignOrder(nextOrder.Food, nextOrder.Unit))
            {
                return false;
            }

            pendingOrders.Dequeue();
            orderListUI?.RemoveOrder(nextOrder.Unit);
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

    private void SpawnCookedFood(FoodSO cookedFood, Transform servingSlot, UnitFSM targetUnit)
    {
        GameObject spawnedFood = Instantiate(cookedFood.prefab, servingSlot);
        Vector3 parentScale = servingSlot.lossyScale;
        Vector3 localScale = spawnedFood.transform.localScale;

        spawnedFood.transform.localScale = new Vector3(
            parentScale.x != 0f ? localScale.x / parentScale.x : localScale.x,
            parentScale.y != 0f ? localScale.y / parentScale.y : localScale.y,
            parentScale.z != 0f ? localScale.z / parentScale.z : localScale.z);

        Food food = spawnedFood.GetComponent<Food>();
        if (food == null)
        {
            food = spawnedFood.AddComponent<Food>();
        }

        food.Init(cookedFood, targetUnit);
    }

    // T is the component type to search for under the current pointer.
    private bool TryGetComponentAtPointer<T>(out T target) where T : Component
    {
        target = null;

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

            target = colliders[i].GetComponentInParent<T>();
            if (target != null)
            {
                return true;
            }
        }

        return false;
    }
}
