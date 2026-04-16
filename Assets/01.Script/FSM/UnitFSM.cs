using UnityEngine.UI;
using UnityEngine;
using UnityEngine.AI;

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
    public Image foodIcon;
    public PC targetPC;
    public LeavingDoor leavingdoor;
    #endregion

    #region Fields
    private NavMeshAgent navMeshAgent;
    private SpriteRenderer spriteRenderer;
    public Sprite upSprite, downSprite;
    public Canvas textBubble;
    public Image bubbleFillMask;
    // public 
    [SerializeField] MoneySO moneySO;
    #endregion

    public NavMeshAgent NavAgent => navMeshAgent;
    public SpriteRenderer SpriteRen => spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        textBubble = transform.parent.GetComponentInChildren<Canvas>();
        bubbleFillMask = textBubble.transform.Find("TextBubble_Image/TimerFill_Mask").GetComponent<Image>();
        navMeshAgent = GetComponentInParent<NavMeshAgent>();
        leavingdoor = FindAnyObjectByType<LeavingDoor>();
        // 회전이 자동으로 설정되면 3D인 xz 평면 기준으로 회전하기 때문에 false
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;

        textBubble.gameObject.SetActive(false);
    }

    private void Update()
    {
        unitMachine.currentState?.Execute();
    }
    public void Setup(PC targetPC)
    {
        this.targetPC = targetPC;
        unitMachine.ChangeState(enteringState, this);
    }

    public void ChangeState(IState newState)
    {
        unitMachine.ChangeState(newState , this);
    }

    public void SpawnMoney()
    {
        float randomX = Random.Range(2f, 6f) * (Random.value > 0.5f ? 1f : -1f);
        float randomY = Random.Range(-4f, -4.5f);
        Vector3 targetPos = transform.position + new Vector3(randomX, randomY, 0f);

        GameObject ins = Instantiate(moneySO.prefab, targetPos, Quaternion.identity);
        MoneyPickup moneyPickup = ins.GetComponent<MoneyPickup>();
        if(moneyPickup != null)
            moneyPickup.SetPrice(moneySO.price);
    }
}
