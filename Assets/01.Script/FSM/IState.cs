using UnityEngine;

public interface IState
{
    public void Enter(UnitFSM owner);
    public void Execute();
    public void Exit(UnitFSM owner);
}

