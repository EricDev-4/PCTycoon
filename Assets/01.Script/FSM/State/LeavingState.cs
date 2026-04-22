using UnityEngine;
using UnityEngine.AI;

public class LeavingState : IState
{
    private UnitFSM owner;
    private bool hasReturned = false;

    public void Enter(UnitFSM owner)
    {
        this.owner = owner;
        hasReturned = false;

        owner.ResetInteractionState();
        owner.ReleasePC();

        if (owner.NavAgent != null && owner.leavingdoor != null)
        {
            owner.NavAgent.isStopped = false;
            owner.NavAgent.SetDestination(owner.leavingdoor.transform.position);
        }
    }

    public void Execute()
    {
        if (owner == null) return;

        NavMeshAgent agent = owner.NavAgent;
        if (agent == null) return;

        Vector2 moveInput = agent.velocity;
        if (moveInput.magnitude > 0.1f && owner.SpriteRen != null)
        {
            owner.SpriteRen.sprite = moveInput.y > 0 ? owner.upSprite : owner.downSprite;
        }

        if (hasReturned) return;

        if (HasReachedExit(agent))
        {
            hasReturned = true;
            owner.ReturnToPool();
        }
    }

    public void Exit(UnitFSM owner)
    {
    }

    private bool HasReachedExit(NavMeshAgent agent)
    {
        if (owner != null && owner.leavingdoor != null)
        {
            float dist = Vector3.Distance(agent.transform.position, owner.leavingdoor.transform.position);
            if (dist <= 0.35f) return true;
        }

        if (agent.pathPending) return false;
        if (agent.remainingDistance == Mathf.Infinity) return false;
        if (agent.remainingDistance > agent.stoppingDistance) return false;

        return !agent.hasPath || agent.velocity.sqrMagnitude < 0.01f;
    }
}
