using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Kitchen : MonoBehaviour
{
    #region Component
    private Player player;
    [SerializeField] private BoxCollider2D coll;
    #endregion

    #region memberVariable
    [SerializeField] private FoodSO selectedFood;
    [SerializeField] private bool isCooking = false;
    #endregion

    #region UI
    [SerializeField] private RectTransform foodMenuUI;
    [SerializeField] private RectTransform content;
    [SerializeField] private Image cookingIconImage;
    [SerializeField] private Transform cookingIconStartPos;
    [SerializeField] private Transform cookingIconEndPos;
    private Slider slider;
    // private SelectFoodSender[] foodSenders;
    #endregion

    private const float CookingIconShowDuration = 0.45f;
    private const float CookingIconHideDuration = 0.3f;

    private float time = 0;

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
        iconRect.DOKill();

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
        if (cookingIconImage == null)
        {
            return;
        }

        RectTransform iconRect = cookingIconImage.rectTransform;
        iconRect.DOKill();

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

        Sequence sequence = DOTween.Sequence();
        sequence.Append(iconRect.DOLocalMove(targetLocalPosition, CookingIconShowDuration).SetEase(Ease.OutBack));
        sequence.Join(iconRect.DOScale(1.08f, CookingIconShowDuration * 0.6f).SetEase(Ease.OutBack));
        sequence.Append(iconRect.DOScale(1f, CookingIconShowDuration * 0.25f).SetEase(Ease.OutQuad));
    }

    private void HideCookingIcon()
    {
        if (cookingIconImage == null)
        {
            return;
        }

        RectTransform iconRect = cookingIconImage.rectTransform;
        iconRect.DOKill();

        Vector3 targetLocalPosition = cookingIconStartPos != null
            ? cookingIconStartPos.localPosition
            : iconRect.localPosition;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(iconRect.DOLocalMove(targetLocalPosition, CookingIconHideDuration).SetEase(Ease.InBack));
        sequence.Join(iconRect.DOScale(0.85f, CookingIconHideDuration).SetEase(Ease.InBack));
        sequence.OnComplete(() =>
        {
            iconRect.localScale = Vector3.one;
            cookingIconImage.sprite = null;
            cookingIconImage.gameObject.SetActive(false);
        });
    }

    private void Update()
    {
        if (player == null) return;

        // if (isCooking)
        // {
        //     Cooking();
        // }
    }

    public FoodSO Cooking(FoodSO food)
    {
        if (food == null || slider == null)
        {
            return null;
        }

        if (!isCooking)
        {
            ShowCookingIcon(food);
        }

        isCooking = true;
        time += Time.deltaTime;
        float cookTime = food.cookTime;
        float value = time / cookTime;

        slider.interactable = false;
        slider.gameObject.SetActive(true);
        slider.SetValueWithoutNotify(value);

        if (value < 1)
        {
            return null;
        }

        time = 0;
        isCooking = false;
        ResetSliderUI();
        HideCookingIcon();
        return food;
    }
}
