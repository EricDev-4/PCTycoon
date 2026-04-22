using UnityEngine;

public class UsingPcState : IState
{
    private UnitFSM owner;
    private bool isRequesting = false;
    private int requestTime;
    private float requestWaitTime = 0f;
    [SerializeField] private float maxWaitTime = 10f;

    public void Enter(UnitFSM owner)
    {
        this.owner = owner;
        isRequesting = false;
        requestWaitTime = 0f;

        owner.ResetInteractionState();

        if (owner.targetPC == null) return;

        if (owner.targetPC.slider != null)
        {
            owner.targetPC.slider.gameObject.SetActive(true);
        }
        owner.targetPC.isUsing = true;
        owner.targetPC.isTargeted = false;

        int minTime = 1;
        int maxTime = (int)owner.targetPC.earningTime - 5;
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
            if ((int)owner.targetPC.usingTime == requestTime)
            {
                isRequesting = true;

                if (owner.textBubble != null)
                {
                    owner.textBubble.gameObject.SetActive(true);
                }

                var foods = GameManager.instance != null ? GameManager.instance.foodMenu?.foods : null;
                if (foods != null && foods.Count > 0)
                {
                    int n = Random.Range(0, foods.Count);
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
            owner.ResetInteractionState();
            requestWaitTime = 0f;
            isRequesting = false;
            return;
        }

        requestWaitTime += Time.deltaTime;
        if (owner.bubbleFillMask != null)
        {
            owner.bubbleFillMask.fillAmount = requestWaitTime / maxWaitTime;
        }

        if (requestWaitTime >= maxWaitTime)
        {
            if (owner.interactiveColl != null)
            {
                owner.interactiveColl.enabled = false;
            }
        }
    }

    public void Exit(UnitFSM owner)
    {
        Debug.Log("Exit");
        owner.SpawnMoney(0);
        owner.ResetInteractionState();
        owner.ReleasePC();
    }
}
