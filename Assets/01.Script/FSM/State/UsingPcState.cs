using UnityEngine;

public class UsingPcState : IState
{
    private UnitFSM owner;

    public void Enter(UnitFSM owner)
    {
        this.owner = owner;
        owner.TargetPC.slider.gameObject.SetActive(true);
    }
    public void Execute()
    {
        if(owner.TargetPC.UsingPc())
        {
            owner.SpawnMoney();
        }
    }
    public void Exit()
    {
        owner.TargetPC.slider.gameObject.SetActive(false);
    }
}
