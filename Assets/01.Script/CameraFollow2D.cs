using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Swipe Paging")]
    [SerializeField] private bool enableSwipePaging = true;
    [SerializeField] private bool detachFromParentOnAwake = true;
    [SerializeField] [Min(8f)] private float swipeThresholdPixels = 80f;
    [SerializeField] [Min(0f)] private float pageSmoothTime = 0.2f;
    [SerializeField] private Transform[] swipePageAnchors;

    private Vector3 followVelocity;
    private float fixedZ;
    private Vector3[] swipePages = System.Array.Empty<Vector3>();
    private int currentPageIndex;
    private bool pointerPressed;
    private Vector2 pointerStartScreenPosition;

    public int CurrentPageIndex => currentPageIndex;
    public int PageCount => swipePages.Length;

    private void Awake()
    {
        DetachFromParent();
        fixedZ = transform.position.z;
        InitializeSwipePages();
    }

    private void LateUpdate()
    {
        TryUpdateSwipePaging();
    }

    private bool TryUpdateSwipePaging()
    {
        if (!enableSwipePaging)
        {
            return false;
        }

        if (swipePages.Length <= 1)
        {
            InitializeSwipePages();
        }

        if (swipePages.Length <= 1)
        {
            return false;
        }

        UpdateSwipeInput();

        Vector3 desiredPosition = swipePages[currentPageIndex];
        if (pageSmoothTime <= 0f)
        {
            transform.position = desiredPosition;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref followVelocity,
                pageSmoothTime);
        }

        return true;
    }

    private void InitializeSwipePages()
    {
        if (!enableSwipePaging)
        {
            return;
        }

        if (TryBuildPagesFromAnchors(out Vector3[] anchorPages))
        {
            swipePages = anchorPages;
            currentPageIndex = GetClosestPageIndex(transform.position);
            return;
        }

        if (TryBuildPagesFromScene(out Vector3[] scenePages))
        {
            swipePages = scenePages;
            currentPageIndex = 0;
        }
    }

    private bool TryBuildPagesFromAnchors(out Vector3[] pages)
    {
        pages = System.Array.Empty<Vector3>();

        if (swipePageAnchors == null || swipePageAnchors.Length == 0)
        {
            return false;
        }

        int validCount = 0;
        for (int i = 0; i < swipePageAnchors.Length; i++)
        {
            if (swipePageAnchors[i] != null)
            {
                validCount++;
            }
        }

        if (validCount == 0)
        {
            return false;
        }

        pages = new Vector3[validCount];
        int index = 0;
        for (int i = 0; i < swipePageAnchors.Length; i++)
        {
            if (swipePageAnchors[i] == null)
            {
                continue;
            }

            Vector3 pagePosition = swipePageAnchors[i].position;
            pagePosition.z = fixedZ;
            pages[index++] = pagePosition;
        }

        System.Array.Sort(pages, (a, b) => a.x.CompareTo(b.x));
        return pages.Length > 1;
    }

    private bool TryBuildPagesFromScene(out Vector3[] pages)
    {
        pages = System.Array.Empty<Vector3>();

        Kitchen[] kitchens = FindObjectsByType<Kitchen>(FindObjectsSortMode.None);
        if (kitchens == null || kitchens.Length == 0)
        {
            return false;
        }

        float leftMostKitchenX = float.MaxValue;
        float rightMostKitchenX = float.MinValue;
        for (int i = 0; i < kitchens.Length; i++)
        {
            if (kitchens[i] == null)
            {
                continue;
            }

            if (TryGetKitchenBounds(kitchens[i], out Bounds kitchenBounds))
            {
                leftMostKitchenX = Mathf.Min(leftMostKitchenX, kitchenBounds.min.x);
                rightMostKitchenX = Mathf.Max(rightMostKitchenX, kitchenBounds.max.x);
                continue;
            }

            float kitchenX = kitchens[i].transform.position.x;
            leftMostKitchenX = Mathf.Min(leftMostKitchenX, kitchenX);
            rightMostKitchenX = Mathf.Max(rightMostKitchenX, kitchenX);
        }

        if (leftMostKitchenX == float.MaxValue || rightMostKitchenX == float.MinValue)
        {
            return false;
        }

        Vector3 currentPosition = transform.position;
        float kitchenCenterX = (leftMostKitchenX + rightMostKitchenX) * 0.5f;
        Vector3 kitchenPosition = new Vector3(kitchenCenterX, currentPosition.y, fixedZ);

        if (Mathf.Abs(kitchenPosition.x - currentPosition.x) < 0.01f)
        {
            return false;
        }

        pages = currentPosition.x <= kitchenPosition.x
            ? new[] { currentPosition, kitchenPosition }
            : new[] { kitchenPosition, currentPosition };

        return true;
    }

    private void UpdateSwipeInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    pointerPressed = true;
                    pointerStartScreenPosition = touch.position;
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (pointerPressed)
                    {
                        TryCommitSwipe(touch.position - pointerStartScreenPosition);
                    }
                    pointerPressed = false;
                    break;
            }

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            pointerPressed = true;
            pointerStartScreenPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (pointerPressed)
            {
                Vector2 pointerEndPosition = Input.mousePosition;
                TryCommitSwipe(pointerEndPosition - pointerStartScreenPosition);
            }

            pointerPressed = false;
        }
    }

    private void TryCommitSwipe(Vector2 swipeDelta)
    {
        if (Mathf.Abs(swipeDelta.x) < swipeThresholdPixels || Mathf.Abs(swipeDelta.x) <= Mathf.Abs(swipeDelta.y))
        {
            return;
        }

        if (swipeDelta.x < 0f)
        {
            currentPageIndex = Mathf.Min(currentPageIndex + 1, swipePages.Length - 1);
        }
        else
        {
            currentPageIndex = Mathf.Max(currentPageIndex - 1, 0);
        }
    }

    private int GetClosestPageIndex(Vector3 position)
    {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < swipePages.Length; i++)
        {
            float distance = Mathf.Abs(position.x - swipePages[i].x);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    public bool IsAtPage(int pageIndex, float tolerance = 0.05f)
    {
        if (pageIndex < 0 || pageIndex >= swipePages.Length)
        {
            return false;
        }

        Vector3 pagePosition = swipePages[pageIndex];
        return Mathf.Abs(transform.position.x - pagePosition.x) <= tolerance
            && Mathf.Abs(transform.position.y - pagePosition.y) <= tolerance;
    }

    public bool IsOnPage(int pageIndex, float tolerance = 0.05f)
    {
        return currentPageIndex == pageIndex && IsAtPage(pageIndex, tolerance);
    }

    private void DetachFromParent()
    {
        if (!detachFromParentOnAwake || transform.parent == null)
        {
            return;
        }

        transform.SetParent(null, true);
    }

    private static bool TryGetKitchenBounds(Kitchen kitchen, out Bounds bounds)
    {
        bounds = default;

        Collider2D[] colliders = kitchen.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null || !colliders[i].enabled)
            {
                continue;
            }

            if (bounds.size == Vector3.zero)
            {
                bounds = colliders[i].bounds;
            }
            else
            {
                bounds.Encapsulate(colliders[i].bounds);
            }
        }

        return bounds.size != Vector3.zero;
    }
}
