using UnityEngine;

public class UnitTapController : MonoBehaviour
{
    [SerializeField] private FoodSO requestedFood;
    [SerializeField] private UnitFSM selectedUnit;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectUnit();
        }

        if (Input.GetMouseButton(0))
        {
            TryCookAtKitchen();
        }
    }

    private void TrySelectUnit()
    {
        if (!TryGetUnitAtPointer(out UnitFSM unit)) return;
        if (unit == null || unit.requestedFood == null) return;

        selectedUnit = unit;
        unit.ResetTextBubble();
        requestedFood = unit.requestedFood;
    }

    private void TryCookAtKitchen()
    {
        if (requestedFood == null) return;
        if (!TryGetKitchenAtPointer(out Kitchen kitchen)) return;

        FoodSO cookedFood = kitchen.Cooking(requestedFood);
        if (cookedFood == null) return;

        if (selectedUnit != null && selectedUnit.TryServe(cookedFood))
        {
            ClearSelection();
        }
    }

    private void ClearSelection()
    {
        requestedFood = null;
        selectedUnit = null;
    }

    private bool TryGetUnitAtPointer(out UnitFSM unit)
    {
        unit = null;

        if (!TryGetPointerWorldPosition(out Vector2 mousePos)) return false;

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

        if (!TryGetPointerWorldPosition(out Vector2 mousePos)) return false;

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

    private bool TryGetPointerWorldPosition(out Vector2 mousePos)
    {
        mousePos = default;

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return false;

        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return true;
    }
}
