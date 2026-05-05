using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset;
    [SerializeField] [Min(0f)] private float smoothTime = 0.15f;
    [SerializeField] private bool autoFindPlayer = true;

    [Header("Map Bounds")]
    [SerializeField] private Collider2D mapBounds;

    private Camera cameraComponent;
    private Vector3 followVelocity;
    private float fixedZ;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        fixedZ = transform.position.z;
        ResolveTarget();
    }

    private void LateUpdate()
    {
        ResolveTarget();

        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            fixedZ);

        desiredPosition = ClampToBounds(desiredPosition);

        if (smoothTime <= 0f)
        {
            transform.position = desiredPosition;
            return;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            smoothTime);
    }

    private void ResolveTarget()
    {
        if (target != null || !autoFindPlayer)
        {
            return;
        }

        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            target = player.transform;
        }
    }

    private Vector3 ClampToBounds(Vector3 desiredPosition)
    {
        if (mapBounds == null || !cameraComponent.orthographic)
        {
            return desiredPosition;
        }

        Bounds bounds = mapBounds.bounds;
        float halfHeight = cameraComponent.orthographicSize;
        float halfWidth = halfHeight * cameraComponent.aspect;

        desiredPosition.x = ClampAxis(desiredPosition.x, bounds.min.x, bounds.max.x, halfWidth);
        desiredPosition.y = ClampAxis(desiredPosition.y, bounds.min.y, bounds.max.y, halfHeight);

        return desiredPosition;
    }

    private static float ClampAxis(float desired, float min, float max, float extent)
    {
        float size = max - min;
        if (size <= extent * 2f)
        {
            return (min + max) * 0.5f;
        }

        return Mathf.Clamp(desired, min + extent, max - extent);
    }
}
