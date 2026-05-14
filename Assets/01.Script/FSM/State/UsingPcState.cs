using UnityEngine;

public class UsingPcState : IState
{
    private const float FoodOrderChance = 0.5f;

    private UnitFSM owner;
    private bool isRequesting = false;
    private bool hasProcessedOrderChance = false;
    private int requestTime;
    private float requestWaitTime = 0f;
    [SerializeField] private float maxWaitTime = 10f;

    public void Enter(UnitFSM owner)
    {
        this.owner = owner;
        isRequesting = false;
        hasProcessedOrderChance = false;
        requestWaitTime = 0f;

        owner.ResetInteractionState();

        if (owner.targetPC == null) return;

        if (owner.targetPC.slider != null)
        {
            owner.targetPC.slider.gameObject.SetActive(true);
        }
        owner.targetPC.SetUsagePaused(false);
        owner.targetPC.isUsing = true;
        owner.targetPC.isTargeted = false;

        int minTime = 3;
        int maxTime = Mathf.FloorToInt(owner.targetPC.GetEffectiveUsingTime()) - 5;
        requestTime = maxTime <= minTime ? minTime : Random.Range(minTime, maxTime);
    }

    public void Execute()
    {
        if (owner.targetPC == null) return;

        if (owner.targetPC.UpdateUsingTimer())
        {
            owner.ChangeState(owner.LeavingState);
            return;
        }

        if (!isRequesting)
        {
            if (!hasProcessedOrderChance && (int)owner.targetPC.usingTime == requestTime)
            {
                hasProcessedOrderChance = true;

                if (Random.value > FoodOrderChance)
                {
                    return;
                }

                isRequesting = true;
                owner.targetPC.SetUsagePaused(true);

                if (owner.textBubble != null)
                {
                    owner.textBubble.gameObject.SetActive(true); // 요청 말풍선 on
                }

                var foods = GameManager.instance != null ? GameManager.instance.foodMenu?.foods : null; // Food List 가져옴 
                if (foods != null && foods.Count > 0)
                {
                    int n = Random.Range(0, foods.Count); // FoodList의 랜덤 food
                    owner.requestedFood = foods[n];

                    if (owner.foodIcon != null)
                    {
                        owner.foodIcon.sprite = foods[n].foodIcon;
                        owner.foodIcon.color = new Color(1, 1, 1, 1);
                    }
                }

                if (owner.interactiveColl != null)
                {
                    owner.interactiveColl.enabled = true;
                }
            }
            return;
        }

        if (owner.isServed)
        {
            owner.targetPC.SetUsagePaused(false);
            owner.ResetInteractionState();
            requestWaitTime = 0f;
            isRequesting = false;
            return;
        }

        bool isAwaitingOrderAcceptance = owner.interactiveColl != null
            ? owner.interactiveColl.enabled
            : owner.textBubble != null && owner.textBubble.gameObject.activeSelf;

        if (!isAwaitingOrderAcceptance)
        {
            return;
        }

        requestWaitTime += Time.deltaTime;
        if (owner.bubbleFillMask != null)
        {
            owner.bubbleFillMask.fillAmount = requestWaitTime / maxWaitTime;
        }

        if (requestWaitTime >= maxWaitTime)
        {
            owner.targetPC.SetUsagePaused(false);
            owner.ResetInteractionState();
            requestWaitTime = 0f;
            isRequesting = false;
        }
    }

    public void Exit(UnitFSM owner)
    {
        Debug.Log("Exit");
        if (owner.targetPC != null)
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.satisfactionPoint += owner.targetPC.GetSatisfactionPointGain();
            }
            owner.targetPC.AddSessionExp();
        }
        // price 가 0이 아니면 price 0이면 기본값
        owner.SpawnMoney(0);
        owner.ResetInteractionState();
        owner.ReleasePC();
    }
}
