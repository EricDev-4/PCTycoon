using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class UnitFSM : MonoBehaviour
{
    #region States
    private StateMachine unitMachine = new StateMachine();
    private IState enteringState = new EnteringState();
    private IState usingPcState = new UsingPcState();
    private IState leavingState = new LeavingState();

    public IState UsingPCState => usingPcState;
    public IState LeavingState => leavingState;
    #endregion

    #region References
    public FoodSO requestedFood;
    public FoodSO receivedFood;
    public Image foodIcon;
    public PC targetPC;
    public LeavingDoor leavingdoor;

    #endregion

    #region Fields
    public BoxCollider2D interactiveColl;
    private NavMeshAgent navMeshAgent;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private FloatingCharacterMotion movementMotion;
    public Sprite upSprite, downSprite;
    public Canvas textBubble;
    public Image bubbleFillMask;
    [SerializeField] MoneySO moneySO;
    [SerializeField] private string moneyPoolTag = "Money";
    [SerializeField] private int moneyPoolPrewarmCount = 10;
    #endregion

    public bool isServed = false;

    public NavMeshAgent NavAgent => navMeshAgent;
    public SpriteRenderer SpriteRen => spriteRenderer;

    private void Awake()
    {
        CacheReferences();
    }

    private void OnEnable()
    {
        CacheReferences();
        ResetForSpawn();
    }

    private void OnDisable()
    {
        ResetInteractionState();
        ReleasePC();

        if (navMeshAgent != null)
        {
            navMeshAgent.ResetPath();
        }

        unitMachine.currentState = null;
    }

    private void CacheReferences()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (movementMotion == null)
        {
            movementMotion = GetComponent<FloatingCharacterMotion>();
        }

        if (interactiveColl == null)
        {
            interactiveColl = GetComponent<BoxCollider2D>();
        }

        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponentInParent<NavMeshAgent>();
        }

        if (textBubble == null)
        {
            Transform parent = transform.parent;
            if (parent != null)
            {
                textBubble = parent.GetComponentInChildren<Canvas>(true);
            }
            else
            {
                textBubble = GetComponentInChildren<Canvas>(true);
            }
        }

        if (bubbleFillMask == null && textBubble != null)
        {
            Transform maskTransform = textBubble.transform.Find("TextBubble_Image/TimerFill_Mask");
            if (maskTransform != null)
            {
                bubbleFillMask = maskTransform.GetComponent<Image>();
            }
        }

        if (leavingdoor == null)
        {
            leavingdoor = FindAnyObjectByType<LeavingDoor>();
        }

        if (navMeshAgent != null)
        {
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;
        }
    }

    private void Update()
    {
        unitMachine.currentState?.Execute();
        UpdateMovementPresentation();
    }

    private void UpdateMovementPresentation()
    {
        Vector2 moveInput = navMeshAgent != null ? (Vector2)navMeshAgent.velocity : Vector2.zero;
        if (moveInput.magnitude > 0.1f && spriteRenderer != null)
        {
            spriteRenderer.sprite = moveInput.y > 0 ? upSprite : downSprite;
            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                spriteRenderer.flipX = moveInput.x > 0f;
            }
        }
        movementMotion?.SetMovement(moveInput);
    }

    public void AssignToPC(PC targetPC)
    {
        CacheReferences();
        ResetInteractionState();

        if (targetPC == null)
        {
            Debug.LogWarning("UnitFSM: Setup called with null PC.");
            return;
        }

        targetPC.isTargeted = true;
        targetPC.isArrived = false;

        this.targetPC = targetPC;

        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.ResetPath();
        }

        unitMachine.ChangeState(enteringState, this);
    }

    public void ChangeState(IState newState)
    {
        unitMachine.ChangeState(newState, this);
    }

    public void ResetTextBubble()
    {
        if (bubbleFillMask != null)
        {
            bubbleFillMask.fillAmount = 0f;
        }

        if (textBubble != null)
        {
            textBubble.gameObject.SetActive(false);
        }
    }

    public bool TryServe(FoodSO food)
    {
        if (food == null || requestedFood == null)
        {
            return false;
        }

        receivedFood = food;
        if (requestedFood.foodName != receivedFood.foodName)
        {
            return false;
        }

        isServed = true;
        SpawnMoney(receivedFood.foodPrice);
        ResetTextBubble();

        if (interactiveColl != null)
        {
            interactiveColl.enabled = false;
        }

        return true;
    }

    public void ReturnToPool()
    {
        var pooledObject = GetComponentInParent<PooledObject>();
        GameObject root = pooledObject != null
            ? pooledObject.gameObject
            : (navMeshAgent != null ? navMeshAgent.gameObject : gameObject);

        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.Despawn(root);
            return;
        }

        root.SetActive(false);
    }

    public void ReleasePC()
    {
        if (targetPC == null) return;

        targetPC.isUsing = false;
        targetPC.isTargeted = false;
        targetPC.isArrived = false;

        if (targetPC.slider != null)
        {
            targetPC.slider.gameObject.SetActive(false);
        }

        targetPC.usingTime = 0f;
        targetPC = null;
    }

    private void ResetForSpawn()
    {
        ResetInteractionState();

        if (navMeshAgent != null)
        {
            navMeshAgent.ResetPath();
            navMeshAgent.isStopped = false;
        }

        unitMachine.currentState = null;
        targetPC = null;
    }

    // 손님의 음식 요청 UI/판정/서빙 플래그를 초기 상태로 되돌리는 함수
    public void ResetInteractionState()
    {
        isServed = false;
        requestedFood = null;
        receivedFood = null;

        if (foodIcon != null)
        {
            foodIcon.sprite = null;
            Color c = foodIcon.color;
            c.a = 0f;
            foodIcon.color = c;
        }

        ResetTextBubble();

        if (interactiveColl != null)
        {
            interactiveColl.enabled = false;
        }
    }

    public void SpawnMoney(int price)
    {
        if (moneySO == null || moneySO.prefab == null) return;

        int spawnPrice = price != 0 ? price : moneySO.price;

        float randomX = Random.Range(2f, 6f) * (Random.value > 0.5f ? 1f : -1f);
        float randomY = Random.Range(-4f, -4.5f);
        Vector3 targetPos = transform.position + new Vector3(randomX, randomY, 0f);

        GameObject ins = null;
        if (ObjectPool.Instance != null)
        {
            string poolTag = string.IsNullOrWhiteSpace(moneyPoolTag) ? "Money" : moneyPoolTag;
            int prewarmCount = Mathf.Max(0, moneyPoolPrewarmCount);
            ObjectPool.Instance.RegisterPool(poolTag, moneySO.prefab, prewarmCount);
            ins = ObjectPool.Instance.SpawnFormPool(poolTag, targetPos);
        }

        if (ins == null)
        {
            ins = Instantiate(moneySO.prefab, targetPos, Quaternion.identity);
        }

        MoneyPickup moneyPickup = ins.GetComponentInChildren<MoneyPickup>(true);
        if (moneyPickup != null)
        {
            moneyPickup.SetPrice(spawnPrice);
        }
    }


    #region  Serving
    void OnTriggerEnter2D(Collider2D collider)
    {
        Player player = collider.GetComponent<Player>();
        if (player == null) return;

        if (player.servingFood != null && requestedFood != null)
        {
            if (TryServe(player.servingFood))
            {
                player.ClearFood();
            }
            else
            {
                // ChangeState(AngryState);
            }
        }
    }
    #endregion
}
