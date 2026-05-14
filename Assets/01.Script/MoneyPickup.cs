using DG.Tweening;
using TMPro;
using UnityEngine;

public class MoneyPickup : MonoBehaviour
{
    private const float EffectDuration = 0.5f;
    private const float EffectMoveDistance = 60f;

    private BoxCollider2D col;
    [SerializeField] private int price;
    [SerializeField] private TMP_Text MoneyEffectText;
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

        isPickedUp = true;
        if (col != null) col.enabled = false;

        GameManager.instance.money += price;
        PlayMoneyEffect();

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

    private void PlayMoneyEffect()
    {
        if (MoneyEffectText == null) return;

        Canvas targetCanvas = FindEffectCanvas();
        if (targetCanvas == null) return;

        RectTransform canvasRect = targetCanvas.transform as RectTransform;
        if (canvasRect == null) return;

        Camera worldCamera = GetWorldCamera(targetCanvas);
        Vector3 screenPoint = worldCamera != null
            ? worldCamera.WorldToScreenPoint(transform.position)
            : transform.position;

        if (screenPoint.z < 0f) return;

        Camera uiCamera = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : targetCanvas.worldCamera ?? worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCamera, out Vector2 anchoredPosition))
        {
            return;
        }

        TMP_Text effectText = Instantiate(MoneyEffectText, canvasRect);
        RectTransform effectRect = effectText.rectTransform;
        effectRect.anchoredPosition = anchoredPosition;
        effectText.raycastTarget = false;
        effectText.text = $"+{price:N0}$";

        Color color = effectText.color;
        color.a = 1f;
        effectText.color = color;

        Sequence sequence = DOTween.Sequence();
        sequence.Join(effectRect.DOAnchorPosY(anchoredPosition.y + EffectMoveDistance, EffectDuration).SetEase(Ease.OutQuad));
        sequence.Join(effectText.DOFade(0f, EffectDuration));
        sequence.OnComplete(() =>
        {
            if (effectText != null)
            {
                Destroy(effectText.gameObject);
            }
        });
    }

    private static Canvas FindEffectCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Canvas bestCanvas = null;

        foreach (Canvas canvas in canvases)
        {
            if (!canvas.isRootCanvas || !canvas.gameObject.activeInHierarchy) continue;

            if (bestCanvas == null || canvas.sortingOrder > bestCanvas.sortingOrder)
            {
                bestCanvas = canvas;
            }
        }

        return bestCanvas;
    }

    private static Camera GetWorldCamera(Canvas targetCanvas)
    {
        if (targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay && targetCanvas.worldCamera != null)
        {
            return targetCanvas.worldCamera;
        }

        if (Camera.main != null)
        {
            return Camera.main;
        }

        Camera[] cameras = Camera.allCameras;
        return cameras.Length > 0 ? cameras[0] : null;
    }
}
