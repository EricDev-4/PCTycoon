using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms;

public class UsingPcState : IState
{
    private UnitFSM owner;
    private bool isRequesting = false;
    int requestTime;
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
                owner.textBubble.gameObject.SetActive(true);
                //TODO : 요청하기
                
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
