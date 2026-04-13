using UnityEngine;

public class StateMachine
{
    public IState currentState;
    public void ChangeState(IState state , UnitFSM owner)
    {
        if (state == null)
        {
            Debug.LogWarning("�ٲٷ��� ���°� null�̴�");
            return;
        }

        // ���� ���¿� �ٲܷ��� ���°� ������ return
        if (currentState == state)
        {
            Debug.LogError($"�̹� {currentState?.GetType().Name} �����Դϴ�.");
            return;
        }

        currentState?.Exit(owner);
        currentState = state;
        currentState?.Enter(owner);
    }
}
