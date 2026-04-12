using UnityEngine;

public class StateMachine
{
    public IState currentState;
    public void ChangeState(IState state , UnitFSM owner)
    {
        if (state == null)
        {
            Debug.LogWarning("바꾸려는 상태가 null이다");
            return;
        }

        // 현재 상태와 바꿀려는 상태가 같으면 return
        if (currentState == state)
        {
            Debug.LogError($"이미 {currentState?.GetType().Name} 상태입니다.");
            return;
        }

        currentState?.Exit();
        currentState = state;
        currentState?.Enter(owner);
    }
}
