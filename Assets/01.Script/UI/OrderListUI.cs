using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class OrderListUI : MonoBehaviour
{
    [SerializeField] private TMP_Text orderListText;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private CameraFollow2D cameraFollow;
    [SerializeField] private bool showOnlyOnKitchenPage = true;
    [SerializeField] private int kitchenPageIndex = 1;
    [SerializeField] [Min(0f)] private float pageArrivalTolerance = 0.05f;
    [SerializeField] [Min(0f)] private float slideDistance = 260f;
    [SerializeField] [Min(0f)] private float showDuration = 0.35f;
    [SerializeField] [Min(0f)] private float hideDuration = 0.2f;
    public List<FoodSO> pendingOrders = new List<FoodSO>();

    private readonly Dictionary<UnitFSM, FoodSO> unitOrders = new Dictionary<UnitFSM, FoodSO>();
    private Tween panelMoveTween;
    private Tween panelFadeTween;
    private Vector2 shownAnchoredPosition;
    private Vector2 hiddenAnchoredPosition;
    private bool hasPanelPositions;
    private bool isPanelVisible;
    private bool hasInitializedVisibility;

    private void Awake()
    {
        ResolveReferences();
        RefreshText();
        UpdateVisibility();
    }

    private void OnEnable()
    {
        ResolveReferences();
        UpdateVisibility();
    }

    private void OnDisable()
    {
        panelMoveTween?.Kill();
        panelFadeTween?.Kill();
    }

    private void LateUpdate()
    {
        UpdateVisibility();
    }

    public bool AddOrder(UnitFSM unit, FoodSO food)
    {
        if (unit == null || food == null)
        {
            return false;
        }

        if (unitOrders.TryGetValue(unit, out FoodSO existingFood))
        {
            if (existingFood == food)
            {
                return false;
            }

            pendingOrders.Remove(existingFood);
        }

        unitOrders[unit] = food;
        pendingOrders.Add(food);
        RefreshText();
        return true;
    }

    public bool RemoveOrder(UnitFSM unit)
    {
        if (unit == null)
        {
            return false;
        }

        if (!unitOrders.TryGetValue(unit, out FoodSO food))
        {
            return false;
        }

        unitOrders.Remove(unit);
        pendingOrders.Remove(food);
        RefreshText();
        return true;
    }

    private void RefreshText()
    {
        if (orderListText == null)
        {
            return;
        }

        if (pendingOrders.Count == 0)
        {
            orderListText.text = string.Empty;
            return;
        }

        Dictionary<FoodSO, int> counts = new Dictionary<FoodSO, int>();
        List<FoodSO> orderSequence = new List<FoodSO>();

        for (int i = 0; i < pendingOrders.Count; i++)
        {
            FoodSO food = pendingOrders[i];
            if (food == null)
            {
                continue;
            }

            if (!counts.ContainsKey(food))
            {
                counts.Add(food, 0);
                orderSequence.Add(food);
            }

            counts[food]++;
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < orderSequence.Count; i++)
        {
            FoodSO food = orderSequence[i];
            if (i > 0)
            {
                builder.AppendLine();
            }

            builder.Append(food.foodName);
            builder.Append(" : ");
            builder.Append(counts[food]);
            builder.Append("\uAC1C");
        }

        orderListText.text = builder.ToString();
    }

    private void UpdateVisibility()
    {
        if (showOnlyOnKitchenPage && cameraFollow == null)
        {
            cameraFollow = FindAnyObjectByType<CameraFollow2D>();
        }

        if (orderListText == null && panelCanvasGroup == null)
        {
            return;
        }

        bool shouldShow = !showOnlyOnKitchenPage;

        if (showOnlyOnKitchenPage)
        {
            shouldShow = cameraFollow != null
                && cameraFollow.IsOnPage(kitchenPageIndex, pageArrivalTolerance);
        }

        ApplyPanelVisibility(shouldShow);
    }

    private void ApplyPanelVisibility(bool shouldShow)
    {
        if (!hasInitializedVisibility)
        {
            SetPanelState(shouldShow, true);
            isPanelVisible = shouldShow;
            hasInitializedVisibility = true;
            return;
        }

        if (isPanelVisible == shouldShow)
        {
            return;
        }

        isPanelVisible = shouldShow;
        SetPanelState(shouldShow, false);
    }

    private void SetPanelState(bool shouldShow, bool instant)
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.interactable = shouldShow;
            panelCanvasGroup.blocksRaycasts = shouldShow;
        }

        if (panelRoot != null && hasPanelPositions)
        {
            Vector2 targetPosition = shouldShow ? shownAnchoredPosition : hiddenAnchoredPosition;
            float moveDuration = shouldShow ? showDuration : hideDuration;

            panelMoveTween?.Kill();
            if (instant || moveDuration <= 0f)
            {
                panelRoot.anchoredPosition = targetPosition;
            }
            else
            {
                panelMoveTween = panelRoot
                    .DOAnchorPos(targetPosition, moveDuration)
                    .SetEase(shouldShow ? Ease.OutBack : Ease.InCubic);
            }
        }

        if (panelCanvasGroup != null)
        {
            float targetAlpha = shouldShow ? 1f : 0f;
            float fadeDuration = shouldShow ? showDuration : hideDuration;

            panelFadeTween?.Kill();
            if (instant || fadeDuration <= 0f)
            {
                panelCanvasGroup.alpha = targetAlpha;
            }
            else
            {
                panelFadeTween = panelCanvasGroup
                    .DOFade(targetAlpha, fadeDuration)
                    .SetEase(shouldShow ? Ease.OutQuad : Ease.InQuad);
            }
        }
        else if (orderListText != null)
        {
            orderListText.enabled = shouldShow;
        }
    }

    private void ResolveReferences()
    {
        if (orderListText == null)
        {
            orderListText = GetComponent<TMP_Text>();
        }

        if (panelRoot == null && orderListText != null)
        {
            panelRoot = orderListText.transform.parent as RectTransform;
        }

        if (panelCanvasGroup == null && panelRoot != null)
        {
            panelCanvasGroup = panelRoot.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = panelRoot.gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (!hasPanelPositions && panelRoot != null)
        {
            shownAnchoredPosition = panelRoot.anchoredPosition;
            hiddenAnchoredPosition = shownAnchoredPosition + Vector2.right * slideDistance;
            hasPanelPositions = true;
        }

        if (cameraFollow == null)
        {
            cameraFollow = FindAnyObjectByType<CameraFollow2D>();
        }
    }
}
