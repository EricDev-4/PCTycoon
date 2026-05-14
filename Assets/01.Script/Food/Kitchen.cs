using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Kitchen : MonoBehaviour
{
    #region Component
    [SerializeField] private BoxCollider2D coll;
    #endregion

    #region memberVariable
    [SerializeField] private FoodSO currentOrderFood;
    private UnitFSM currentOrderUnit;
    [SerializeField] private bool isCooking = false;
    #endregion

    #region UI
    [SerializeField] private RectTransform foodMenuUI;
    [SerializeField] private RectTransform content;
    [SerializeField] private Image cookingIconImage;
    [SerializeField] private Transform cookingIconStartPos;
    [SerializeField] private Transform cookingIconEndPos;
    private Slider slider;
    private Sequence cookingIconSequence;
    // private SelectFoodSender[] foodSenders;
    #endregion

    private const float CookingIconShowDuration = 0.45f;
    private const float CookingIconHideDuration = 0.3f;

    private float time = 0;
    [SerializeField] [Min(0f)] private float nextCookDelay = 1f;
    private float nextOrderAvailableTime;

    public bool IsIdle => currentOrderFood == null || currentOrderUnit == null;

    private void Awake()
    {
        slider = GetComponentInChildren<Slider>(true);
        ResetCookingIconImmediate();
        ResetSliderUI();
    }

    private void Start()
    {
        // foodSenders = content.GetComponentsInChildren<SelectFoodSender>(true);

        // HideMenuButton(true);
        // slider.gameObject.SetActive(false);
        // HideFoodMenu();

        // menuOpenBtn.onClick.AddListener(OpenFoodMenu);

        // foreach (var sender in foodSenders)
        // {
        //     sender.Init(this);

        //     Button button = sender.GetComponent<Button>();
        //     if (button == null) continue;

        //     button.onClick.RemoveListener(sender.OnFoodSelected);
        //     button.onClick.AddListener(sender.OnFoodSelected);
        // }
    }

    private void ResetSliderUI()
    {
        if (slider == null)
        {
            return;
        }

        slider.interactable = false;
        slider.SetValueWithoutNotify(0f);
        slider.gameObject.SetActive(false);
    }

    private void ResetCookingIconImmediate()
    {
        if (cookingIconImage == null)
        {
            return;
        }

        RectTransform iconRect = cookingIconImage.rectTransform;
        KillCookingIconTween();

        if (cookingIconStartPos != null)
        {
            iconRect.localPosition = cookingIconStartPos.localPosition;
        }
        else if (cookingIconEndPos != null)
        {
            iconRect.localPosition = cookingIconEndPos.localPosition;
        }

        iconRect.localScale = Vector3.one;
        cookingIconImage.sprite = null;
        cookingIconImage.gameObject.SetActive(false);
    }

    private void ShowCookingIcon(FoodSO food)
    {
        if (cookingIconImage == null || food == null)
        {
            return;
        }

        RectTransform iconRect = cookingIconImage.rectTransform;
        KillCookingIconTween();

        if (cookingIconStartPos != null)
        {
            iconRect.localPosition = cookingIconStartPos.localPosition;
        }

        iconRect.localScale = Vector3.one * 0.8f;
        cookingIconImage.sprite = food.foodIcon;
        cookingIconImage.gameObject.SetActive(true);

        Vector3 targetLocalPosition = cookingIconEndPos != null
            ? cookingIconEndPos.localPosition
            : iconRect.localPosition;

        cookingIconSequence = DOTween.Sequence();
        cookingIconSequence.Append(iconRect.DOLocalMove(targetLocalPosition, CookingIconShowDuration).SetEase(Ease.OutBack));
        cookingIconSequence.Join(iconRect.DOScale(1.08f, CookingIconShowDuration * 0.6f).SetEase(Ease.OutBack));
        cookingIconSequence.Append(iconRect.DOScale(1f, CookingIconShowDuration * 0.25f).SetEase(Ease.OutQuad));
        cookingIconSequence.OnKill(() => cookingIconSequence = null);
    }

    private void HideCookingIcon()
    {
        if (cookingIconImage == null)
        {
            return;
        }

        RectTransform iconRect = cookingIconImage.rectTransform;
        KillCookingIconTween();
        cookingIconImage.gameObject.SetActive(true);

        Vector3 targetLocalPosition = cookingIconStartPos != null
            ? cookingIconStartPos.localPosition
            : iconRect.localPosition;

        cookingIconSequence = DOTween.Sequence();
        cookingIconSequence.Append(iconRect.DOLocalMove(targetLocalPosition, CookingIconHideDuration).SetEase(Ease.InBack));
        cookingIconSequence.Join(iconRect.DOScale(0.85f, CookingIconHideDuration).SetEase(Ease.InBack));
        cookingIconSequence.OnComplete(() =>
        {
            iconRect.localScale = Vector3.one;
            cookingIconImage.sprite = null;
            cookingIconImage.gameObject.SetActive(false);
        });
        cookingIconSequence.OnKill(() => cookingIconSequence = null);
    }

    public bool TryAssignOrder(FoodSO food, UnitFSM unit)
    {
        if (food == null || unit == null || !IsIdle || IsCookDelayActive())
        {
            return false;
        }

        currentOrderFood = food;
        currentOrderUnit = unit;
        time = 0f;
        isCooking = false;
        ResetSliderUI();
        ResetCookingIconImmediate();
        return true;
    }

    public bool TryCookAssignedOrder(out FoodSO cookedFood, out UnitFSM targetUnit)
    {
        cookedFood = null;
        targetUnit = null;

        if (currentOrderFood == null || currentOrderUnit == null)
        {
            ClearCurrentOrder(false);
            return false;
        }

        float cookTime = Mathf.Max(0.01f, currentOrderFood.cookTime);

        if (!isCooking)
        {
            ShowCookingIcon(currentOrderFood);
        }

        isCooking = true;
        time += Time.deltaTime;
        float value = time / cookTime;

        if (slider != null)
        {
            slider.interactable = false;
            slider.gameObject.SetActive(true);
            slider.SetValueWithoutNotify(value);
        }

        if (value < 1f)
        {
            return false;
        }

        cookedFood = currentOrderFood;
        targetUnit = currentOrderUnit;
        BeginNextCookDelay();
        ClearCurrentOrder(true);
        return true;
    }

    private void BeginNextCookDelay()
    {
        nextOrderAvailableTime = Time.time + nextCookDelay;
    }

    private bool IsCookDelayActive()
    {
        return Time.time < nextOrderAvailableTime;
    }

    private void KillCookingIconTween()
    {
        cookingIconSequence?.Kill();
        cookingIconSequence = null;

        if (cookingIconImage == null)
        {
            return;
        }

        cookingIconImage.rectTransform.DOKill();
    }

    private void ClearCurrentOrder(bool animateHideIcon)
    {
        currentOrderFood = null;
        currentOrderUnit = null;
        time = 0f;
        isCooking = false;
        ResetSliderUI();

        if (animateHideIcon)
        {
            HideCookingIcon();
            return;
        }

        ResetCookingIconImmediate();
    }
}
