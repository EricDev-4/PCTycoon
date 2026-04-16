using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms;

public class UsingPcState : IState
{
    private UnitFSM owner;
    private bool isRequesting = false;
    int requestTime;
    [SerializeField] private float requestWaitTime = 0f;
    [SerializeField] private float maxWaitTime = 10f; // 말풍선 타이머 최대 시간
    public void Enter(UnitFSM owner)
    {
        this.owner = owner;
        owner.targetPC.slider.gameObject.SetActive(true);
        owner.targetPC.isOccupied = true;
        // owner.usingPC = true;

            int minTime = 1;
            int maxTime = (int)owner.targetPC.earningTime - 5;

            requestTime = Random.Range(minTime , maxTime);
    }
    public void Execute()
    {
        /*
        Execute()에서 UpdateUsingTimer() 완료 감지
        → owner.ChangeState(LeavingState) 호출
        → StateMachine 내부에서 currentState.Exit(owner) 자동 호출
        → Exit()에서 SpawnMoney, 슬라이더 끄기 등 정리
        → LeavingState.Enter() 실행 
        */
        if(owner.targetPC.UpdateUsingTimer())
        {
            owner.ChangeState(owner.LeavingState);
        }

        if(!isRequesting)
        {
            if((int)owner.targetPC.usingTime == requestTime)
            {
                isRequesting = true;
                
                //TODO : 요청하기
                // var foods = GameManager.instance.FoodMenu;
                owner.textBubble.gameObject.SetActive(true);

                // int n  = GameManager.instance.FoodMenu.foods.Count;
                // owner.requestedFood = foods[Random.Range(0, )];
                var food = GameManager.instance.foodMenu.foods;
                int n = Random.Range(0 , food.Count);
                owner.requestedFood = food[n];
                owner.foodIcon.sprite = food[n].icon;
            }
        }
        else
        {
            // 아래서 위로 차오름
            requestWaitTime += Time.deltaTime;
            owner.bubbleFillMask.fillAmount = requestWaitTime / maxWaitTime;

            if(requestWaitTime >= maxWaitTime)
            {
                // TODO : Angry 상태 전환
            }
        }
    }
    public void Exit(UnitFSM owner)
    {
        // owner.usingPC = false;
        owner.SpawnMoney();
        owner.targetPC.isOccupied = false;
        owner.targetPC.slider.gameObject.SetActive(false);
        // owner.ChangeState(owner.LeavingState);
    }
}
