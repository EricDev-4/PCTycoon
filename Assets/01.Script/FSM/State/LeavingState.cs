using UnityEngine;

public class LeavingState : IState
{
    private UnitFSM owner;
    public void Enter(UnitFSM owner)
    {
        this.owner = owner;
        owner.targetPC = null;
    }
    public void Execute()
    {
        if (owner.leavingdoor != null)
        {
            owner.NavAgent.SetDestination(owner.leavingdoor.transform.position);
        }
        Vector2 moveInput = owner.NavAgent.velocity;
        if (moveInput.magnitude > 0.1f)
        {
            owner.SpriteRen.sprite = moveInput.y > 0 ? owner.upSprite : owner.downSprite;
        }

        
    }
    public void Exit(UnitFSM owner)
    {

    }
}
