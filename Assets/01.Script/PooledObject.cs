using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [SerializeField] private string poolTag;
    public string PoolTag => poolTag;

    public bool IsInPool { get; private set; } = true;

    public void MarkSpawned(string tag)
    {
        poolTag = tag;
        IsInPool = false;
    }

    public void MarkReturned()
    {
        IsInPool = true;
    }
}
