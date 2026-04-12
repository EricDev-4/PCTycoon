using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnteringState : IState
{
    private UnitFSM owner;
    public void Enter(UnitFSM owner)
    {
        this.owner = owner;
    }
    public void Execute()
    {
        if (owner.TargetPC != null)
        {
            owner.NavAgent.SetDestination(owner.TargetPC.transform.position);
        }
        Vector2 moveInput = owner.NavAgent.velocity;
        if (moveInput.magnitude > 0.1f)
        {
            owner.SpriteRen.sprite = moveInput.y > 0 ? owner.upSprite : owner.downSprite;
        }
        if(owner.NavAgent.velocity.magnitude > 0.2f && owner.NavAgent.remainingDistance <= 0.5f)
        {
            owner.TargetPC.isArrived = true;
            Debug.Log(owner.name + "À¯´ÖÀÌ" + owner.TargetPC.name + "¿¡ µµÂøÇß½À´Ï´Ù!");
        }
    }
    public void Exit()
    {

    }
}
