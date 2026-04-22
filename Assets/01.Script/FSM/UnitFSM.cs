using UnityEngine.UI;
using UnityEngine;
using UnityEngine.AI;
using Unity.VisualScripting;
using System.Security.Cryptography;

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
    public Sprite upSprite, downSprite;
    public Canvas textBubble;
    public Image bubbleFillMask;
    // public 
    [SerializeField] MoneySO moneySO;
    #endregion

    public bool isServed = false;

    public NavMeshAgent NavAgent => navMeshAgent;
    public SpriteRenderer SpriteRen => spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        textBubble = transform.parent.GetComponentInChildren<Canvas>();
        bubbleFillMask = textBubble.transform.Find("TextBubble_Image/TimerFill_Mask").GetComponent<Image>();
        navMeshAgent = GetComponentInParent<NavMeshAgent>();
        leavingdoor = FindAnyObjectByType<LeavingDoor>();
        interactiveColl = GetComponent<BoxCollider2D>();
        
        // ȸ���� �ڵ����� �����Ǹ� 3D�� xz ��� �������� ȸ���ϱ� ������ false
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;

        textBubble.gameObject.SetActive(false);
        // interactiveColl.gameObject.SetActive(false);
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

    public void SpawnMoney(int price)
    {
        float randomX = Random.Range(2f, 6f) * (Random.value > 0.5f ? 1f : -1f);
        float randomY = Random.Range(-4f, -4.5f);
        Vector3 targetPos = transform.position + new Vector3(randomX, randomY, 0f);

        GameObject ins = Instantiate(moneySO.prefab, targetPos, Quaternion.identity);
        MoneyPickup moneyPickup = ins.GetComponent<MoneyPickup>();
        if(moneyPickup != null)
        {
            if(price != 0)
            {
                moneyPickup.SetPrice(price);
                return;
            }
            else
            {
                moneyPickup.SetPrice(moneySO.price);
                return;
            }
        }
    }

    // Serving
    void OnTriggerEnter2D(Collider2D collider)
    {
            Player player = collider.GetComponent<Player>();
            if(player == null) return; // Player �ƴ� ������Ʈ ����

            if(player.servingFood != null && requestedFood != null)
            {
                receivedFood = player.servingFood;
                isServed = true;

                if(requestedFood.foodName == receivedFood.foodName)
                {
                    SpawnMoney(receivedFood.foodPrice);
                    ResetTextBubble();
                    player.ClearFood();
                }
                else
                {
                    // ChangeState(AngryState);
                }
            }
    }
}
