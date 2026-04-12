using NUnit.Framework;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class UnitFSM : MonoBehaviour
{
    #region States
    private StateMachine unitMachine = new StateMachine();
    private IState enteringState = new EnteringState();
    private IState usingPcState = new UsingPcState();
    #endregion  
    [SerializeField] private PC targetPC;
    private NavMeshAgent navMeshAgent;
    private SpriteRenderer spriteRenderer;
    public Sprite upSprite, downSprite;
    [SerializeField] MoneySO moneySO;

    public NavMeshAgent NavAgent => navMeshAgent;
    public SpriteRenderer SpriteRen => spriteRenderer;
    public PC TargetPC { get; private set; } // 쓰기는 나만, 읽기는 모두

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        navMeshAgent = GetComponent<NavMeshAgent>();
        // 회전이 자동으로 설정되면 3D인 xz 평면 기준으로 회전하기 때문에 false
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
    }
    public void Setup(PC targetPC)
    {
        this.targetPC = targetPC;
        TargetPC = targetPC;
        unitMachine.ChangeState(enteringState, this);
    }

    private void Update()
    {
        unitMachine.currentState?.Execute();
        if(targetPC != null && targetPC.isArrived && unitMachine.currentState != usingPcState)
        {
            unitMachine.ChangeState(usingPcState, this);
        }
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
