using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnteringState : IState
{
    private UnitFSM owner;
    public void Enter(UnitFSM owner)
    {
        this.owner = owner;
        if (owner.targetPC != null)
        {
            owner.NavAgent.SetDestination(owner.targetPC.interactionPos.transform.position);
        }
    }
    public void Execute()
    {
        Vector2 moveInput = owner.NavAgent.velocity;
        if (moveInput.magnitude > 0.1f)
        {
            owner.SpriteRen.sprite = moveInput.y > 0 ? owner.upSprite : owner.downSprite;
        }
        
        if(owner.NavAgent.velocity.magnitude > 0.2f && owner.NavAgent.remainingDistance <= 0.5f)
        {
            owner.targetPC.isArrived = true;
        }


        // remainingDistance 는 목적지까지 남은 거리를 반환
        // 남은 거리가 stoppingDistance 보다 작을 때
        if(owner.NavAgent.remainingDistance <= owner.NavAgent.stoppingDistance)
        {
            owner.ChangeState(owner.UsingPCState);
        }
    }
    public void Exit(UnitFSM owner)
    {

    }
}
