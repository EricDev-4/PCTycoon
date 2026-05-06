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
    private Slider slider;
    private RectTransform cookingButtonRoot;
    private Button cookingButton;
    private Image cookingIconImage;
    // private SelectFoodSender[] foodSenders;
    #endregion

    private const float CookingButtonRiseDistance = 40f;
    private const float CookingButtonRiseDuration = 0.4f;

    private float time = 0;
    private Vector2 cookingButtonDefaultAnchoredPosition;

    private void Awake()
    {
        slider = GetComponentInChildren<Slider>(true);
        Transform cookingButtonTransform = transform.Find("Btn_Canvas/Cooking_Btn");
        if (cookingButtonTransform != null)
        {
            cookingButton = cookingButtonTransform.GetComponent<Button>();
            cookingButtonRoot = cookingButtonTransform.parent as RectTransform;
        }

        Transform cookingIconTransform = transform.Find("Btn_Canvas/Cooking_Btn/Cooking_Icon");
        if (cookingIconTransform != null)
        {
            cookingIconImage = cookingIconTransform.GetComponent<Image>();
        }

        if (cookingButtonRoot != null)
        {
            cookingButtonDefaultAnchoredPosition = cookingButtonRoot.anchoredPosition;
        }

        ResetSliderUI();
        ResetCookingButtonUI();
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

    private void ShowCookingButton(FoodSO food)
    {
        if (cookingButtonRoot == null)
        {
            return;
        }

        if (cookingButton != null)
        {
            cookingButton.interactable = false;
        }

        if (cookingIconImage != null)
        {
            cookingIconImage.sprite = food.foodIcon;
        }

        cookingButtonRoot.DOKill();
        cookingButtonRoot.gameObject.SetActive(true);
        cookingButtonRoot.anchoredPosition = cookingButtonDefaultAnchoredPosition + Vector2.down * CookingButtonRiseDistance;
        cookingButtonRoot.DOAnchorPos(cookingButtonDefaultAnchoredPosition, CookingButtonRiseDuration)
            .SetEase(Ease.OutBack);
    }

    private void ResetCookingButtonUI()
    {
        if (cookingButtonRoot == null)
        {
            return;
        }

        cookingButtonRoot.DOKill();
        cookingButtonRoot.anchoredPosition = cookingButtonDefaultAnchoredPosition;
        cookingButtonRoot.gameObject.SetActive(false);

        if (cookingButton != null)
        {
            cookingButton.interactable = false;
        }
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
            ShowCookingButton(food);
        }

        isCooking = true;
        time += Time.deltaTime;
        float cookTime = food.cookTime;
        float value = time / cookTime;

        slider.interactable = false;
        slider.gameObject.SetActive(true);
        slider.SetValueWithoutNotify(value);
        if(value < 1) // 조리가 아직 다 안 끝났으면
            return null;
        
        // 조리가 끝나면 완료 처리
        time = 0;
        isCooking = false;
        ResetSliderUI();
        ResetCookingButtonUI();
        return food;
    }

    // private void Cooking()
    // {
    //     time += Time.deltaTime;
    //     float cookTime = selectedFood.cookTime;
    //     float value = time / cookTime;

    //     slider.gameObject.SetActive(true);
    //     slider.value = value;

    //     if (value < 1)
    //     {
    //         return;
    //     }

    //     time = 0;
    //     isCooking = false;
    //     player.InitFood(selectedFood);
    //     selectedFood = null;
    //     slider.gameObject.SetActive(false);
    //     ShowMenuButton();
    // }

    // public void SetSelectedFood(FoodSO food)
    // {
    //     selectedFood = food;
    //     HideFoodMenu();
    //     HideMenuButton(true);
    // }

    // private void OnTriggerStay2D(Collider2D collider)
    // {
    //     Player currentPlayer = collider.GetComponent<Player>();
    //     if (currentPlayer == null) return;

    //     if (selectedFood != null)
    //     {
    //         isCooking = true;
    //     }
    // }

    // private void OnTriggerEnter2D(Collider2D collider)
    // {
    //     player = collider.GetComponent<Player>();
    //     if (player == null) return;

    //     if (selectedFood == null)
    //     {
    //         ShowMenuButton();
    //     }
    // }

    // private void OnTriggerExit2D(Collider2D collider)
    // {
    //     if (collider.GetComponent<Player>() == null) return;

    //     player = null;
    //     isCooking = false;
    //     HideFoodMenu();
    //     HideMenuButton();
    // }

    // private void OpenFoodMenu()
    // {
    //     if (foodMenuUI == null) return;

    //     foodMenuUI.gameObject.SetActive(true);
    // }

    // private void HideFoodMenu()
    // {
    //     if (foodMenuUI == null) return;

    //     foodMenuUI.gameObject.SetActive(false);
    // }

    // private void HideMenuButton(bool immediate = false)
    // {
    //     if (menuOpenBtn == null) return;

    //     menuOpenBtn.transform.DOKill();

    //     if (immediate)
    //     {
    //         menuOpenBtn.transform.localScale = Vector3.zero;
    //         menuOpenBtn.gameObject.SetActive(false);
    //         return;
    //     }

    //     menuOpenBtn.transform.DOScale(Vector3.zero, 0.3f)
    //         .SetEase(Ease.InBack)
    //         .OnComplete(() => menuOpenBtn.gameObject.SetActive(false));
    // }

    // private void ShowMenuButton()
    // {
    //     if (menuOpenBtn == null) return;

    //     menuOpenBtn.gameObject.SetActive(true);
    //     menuOpenBtn.transform.DOKill();
    //     menuOpenBtn.transform.localScale = Vector3.zero;
    //     menuOpenBtn.transform.DOScale(Vector3.one, 1f)
    //         .SetEase(Ease.OutBack)
    //         .OnComplete(() =>
    //         {
    //             menuOpenBtn.transform.DOScale(Vector3.one * 1.15f, 0.4f)
    //                 .SetEase(Ease.InOutSine)
    //                 .SetLoops(-1, LoopType.Yoyo);
    //         });
    // }
}
