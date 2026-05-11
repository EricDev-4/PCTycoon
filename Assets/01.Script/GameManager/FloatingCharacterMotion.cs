using DG.Tweening;
using UnityEngine;

public class FloatingCharacterMotion : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visualRoot;

    [Header("Motion Detection")]
    [SerializeField, Min(0f)] private float movementThreshold = 0.05f;

    [Header("Pixel Conversion")]
    [SerializeField, Min(1f)] private float pixelsPerUnit = 100f;

    [Header("Float Loop")]
    [SerializeField, Min(0f)] private float bouncePixels = 3f;
    [SerializeField, Min(0.05f)] private float bounceDuration = 0.72f;
    [SerializeField, Min(0f)] private float swayPixels = 1.5f;
    [SerializeField, Min(0.05f)] private float swayDuration = 0.95f;
    [SerializeField, Min(0f)] private float swayRotationDegrees = 1.75f;
    [SerializeField] private Ease loopEase = Ease.InOutSine;

    [Header("Body Squish")]
    [SerializeField] private Vector2 movingScale = new Vector2(1.03f, 0.97f);
    [SerializeField, Min(0.05f)] private float squishInDuration = 0.16f;
    [SerializeField, Min(0.05f)] private float squishOutDuration = 0.3f;
    [SerializeField] private Ease squishInEase = Ease.OutSine;
    [SerializeField] private Ease squishOutEase = Ease.OutSine;

    [Header("Directional Response")]
    [SerializeField, Min(0f)] private float trailingPixels = 1.2f;
    [SerializeField, Range(0f, 1f)] private float verticalTrailingMultiplier = 0.65f;
    [SerializeField, Min(0.05f)] private float trailingResponse = 0.18f;
    [SerializeField, Min(0f)] private float tiltDegrees = 3.5f;
    [SerializeField, Min(0.05f)] private float tiltResponse = 0.18f;
    [SerializeField, Min(0f)] private float directionChangeThreshold = 0.18f;
    [SerializeField, Range(0f, 180f)] private float turnMinAngle = 18f;
    [SerializeField, Min(0.05f)] private float turnPulseCooldown = 0.18f;
    [SerializeField, Min(0f)] private float turnBackStretchPixels = 2.5f;
    [SerializeField] private Vector2 turnScale = new Vector2(1.02f, 0.985f);
    [SerializeField, Min(0f)] private float turnTiltDegrees = 1.2f;
    [SerializeField, Min(0.05f)] private float turnOutDuration = 0.12f;
    [SerializeField, Min(0.05f)] private float turnRecoverDuration = 0.22f;

    [Header("Settling")]
    [SerializeField, Min(0.05f)] private float settleDuration = 0.25f;
    [SerializeField] private Ease settleEase = Ease.OutSine;

    private Vector3 baseLocalPosition;
    private Vector3 baseLocalScale;
    private Vector3 baseLocalEulerAngles;

    private Vector2 lastDirection;
    private bool isMoving;
    private float nextTurnTime;

    private float bobOffsetY;
    private float swayOffsetX;
    private float swayRotationZ;
    private Vector2 bodyScaleMultiplier = Vector2.one;
    private Vector3 trailingOffset;
    private float moveTiltZ;
    private Vector3 turnOffset;
    private Vector2 turnScaleMultiplier = Vector2.one;
    private float turnTiltZ;

    private Tween bounceTween;
    private Tween swayTween;
    private Tween swayRotationTween;
    private Sequence bodySquishSequence;
    private Tween trailingTween;
    private Tween tiltTween;
    private Sequence turnSequence;
    private Tween settleBobTween;
    private Tween settleSwayTween;
    private Tween settleSwayRotationTween;
    private Tween settleScaleTween;
    private Tween settleTrailingTween;
    private Tween settleTiltTween;
    private Tween settleTurnOffsetTween;
    private Tween settleTurnScaleTween;
    private Tween settleTurnTiltTween;

    private void Awake()
    {
        EnsureVisualRoot();
        CacheBasePose();
        ApplyPose();
    }

    private void OnEnable()
    {
        EnsureVisualRoot();
        CacheBasePose();
        ResetAnimationState();
        ApplyPose();
    }

    private void LateUpdate()
    {
        ApplyPose();
    }

    private void OnDisable()
    {
        KillAllTweens();
        ResetAnimationState();
        ApplyPose();
    }

    public void SetMovement(Vector2 velocity)
    {
        bool shouldMove = velocity.sqrMagnitude > movementThreshold * movementThreshold;
        if (!shouldMove)
        {
            if (isMoving)
            {
                StopMotion();
            }

            return;
        }

        Vector2 direction = velocity.normalized;

        if (!isMoving)
        {
            StartMotion(direction);
        }
        else if (lastDirection == Vector2.zero || Vector2.Distance(lastDirection, direction) >= 0.01f)
        {
            UpdateDirectionalResponse(direction);
        }

        if (lastDirection != Vector2.zero)
        {
            float angleDelta = Vector2.Angle(lastDirection, direction);
            float directionDelta = Vector2.Distance(lastDirection, direction);
            if (angleDelta >= turnMinAngle &&
                directionDelta >= directionChangeThreshold &&
                Time.time >= nextTurnTime)
            {
                PlayTurnPulse(direction, angleDelta);
                nextTurnTime = Time.time + turnPulseCooldown;
            }
        }

        lastDirection = direction;
    }

    private void StartMotion(Vector2 direction)
    {
        isMoving = true;
        KillLoopTweens();
        KillSettleTweens();

        float bounceUnits = PixelsToUnits(bouncePixels) * 0.5f;
        bobOffsetY = -bounceUnits;
        bounceTween = DOTween.To(() => bobOffsetY, value => bobOffsetY = value, bounceUnits, bounceDuration)
            .SetEase(loopEase)
            .SetLoops(-1, LoopType.Yoyo);

        float swayUnits = PixelsToUnits(swayPixels) * 0.5f;
        swayOffsetX = -swayUnits;
        swayTween = DOTween.To(() => swayOffsetX, value => swayOffsetX = value, swayUnits, swayDuration)
            .SetEase(loopEase)
            .SetLoops(-1, LoopType.Yoyo);

        swayRotationZ = -swayRotationDegrees;
        swayRotationTween = DOTween.To(() => swayRotationZ, value => swayRotationZ = value, swayRotationDegrees, swayDuration)
            .SetEase(loopEase)
            .SetLoops(-1, LoopType.Yoyo);

        bodySquishSequence = DOTween.Sequence();
        bodySquishSequence.Append(
            DOTween.To(() => bodyScaleMultiplier, value => bodyScaleMultiplier = value, movingScale, squishInDuration)
                .SetEase(squishInEase));
        bodySquishSequence.Append(
            DOTween.To(() => bodyScaleMultiplier, value => bodyScaleMultiplier = value, Vector2.one, squishOutDuration)
                .SetEase(squishOutEase));
        bodySquishSequence.SetLoops(-1, LoopType.Restart);

        UpdateDirectionalResponse(direction);
    }

    private void StopMotion()
    {
        isMoving = false;
        lastDirection = Vector2.zero;
        KillLoopTweens();
        KillDirectionalTweens();
        KillSettleTweens();

        settleBobTween = DOTween.To(() => bobOffsetY, value => bobOffsetY = value, 0f, settleDuration).SetEase(settleEase);
        settleSwayTween = DOTween.To(() => swayOffsetX, value => swayOffsetX = value, 0f, settleDuration).SetEase(settleEase);
        settleSwayRotationTween = DOTween.To(() => swayRotationZ, value => swayRotationZ = value, 0f, settleDuration).SetEase(settleEase);
        settleScaleTween = DOTween.To(() => bodyScaleMultiplier, value => bodyScaleMultiplier = value, Vector2.one, settleDuration).SetEase(settleEase);
        settleTrailingTween = DOTween.To(() => trailingOffset, value => trailingOffset = value, Vector3.zero, settleDuration).SetEase(settleEase);
        settleTiltTween = DOTween.To(() => moveTiltZ, value => moveTiltZ = value, 0f, settleDuration).SetEase(settleEase);
        settleTurnOffsetTween = DOTween.To(() => turnOffset, value => turnOffset = value, Vector3.zero, settleDuration).SetEase(settleEase);
        settleTurnScaleTween = DOTween.To(() => turnScaleMultiplier, value => turnScaleMultiplier = value, Vector2.one, settleDuration).SetEase(settleEase);
        settleTurnTiltTween = DOTween.To(() => turnTiltZ, value => turnTiltZ = value, 0f, settleDuration).SetEase(settleEase);
    }

    private void UpdateDirectionalResponse(Vector2 direction)
    {
        KillDirectionalTweens();

        Vector3 targetTrailingOffset = new Vector3(
            -direction.x * PixelsToUnits(trailingPixels),
            -direction.y * PixelsToUnits(trailingPixels) * verticalTrailingMultiplier,
            0f);

        trailingTween = DOTween.To(() => trailingOffset, value => trailingOffset = value, targetTrailingOffset, trailingResponse)
            .SetEase(Ease.OutSine);

        float targetTilt = -direction.x * tiltDegrees;
        tiltTween = DOTween.To(() => moveTiltZ, value => moveTiltZ = value, targetTilt, tiltResponse)
            .SetEase(Ease.OutSine);
    }

    private void PlayTurnPulse(Vector2 direction, float angleDelta)
    {
        if (turnSequence != null && turnSequence.IsActive())
        {
            turnSequence.Kill();
        }

        float intensity = Mathf.InverseLerp(turnMinAngle, 180f, angleDelta);
        Vector3 peakOffset = new Vector3(
            -direction.x * PixelsToUnits(turnBackStretchPixels) * intensity,
            -direction.y * PixelsToUnits(turnBackStretchPixels) * verticalTrailingMultiplier * intensity,
            0f);
        Vector2 peakScale = Vector2.Lerp(Vector2.one, turnScale, intensity);
        float peakTilt = -direction.x * turnTiltDegrees * intensity;

        turnSequence = DOTween.Sequence();
        turnSequence.Append(
            DOTween.To(() => turnOffset, value => turnOffset = value, peakOffset, turnOutDuration)
                .SetEase(Ease.OutQuad));
        turnSequence.Join(
            DOTween.To(() => turnScaleMultiplier, value => turnScaleMultiplier = value, peakScale, turnOutDuration)
                .SetEase(Ease.OutQuad));
        turnSequence.Join(
            DOTween.To(() => turnTiltZ, value => turnTiltZ = value, peakTilt, turnOutDuration)
                .SetEase(Ease.OutQuad));
        turnSequence.Append(
            DOTween.To(() => turnOffset, value => turnOffset = value, Vector3.zero, turnRecoverDuration)
                .SetEase(Ease.OutSine));
        turnSequence.Join(
            DOTween.To(() => turnScaleMultiplier, value => turnScaleMultiplier = value, Vector2.one, turnRecoverDuration)
                .SetEase(Ease.OutSine));
        turnSequence.Join(
            DOTween.To(() => turnTiltZ, value => turnTiltZ = value, 0f, turnRecoverDuration)
                .SetEase(Ease.OutSine));
    }

    private void ApplyPose()
    {
        if (visualRoot == null)
        {
            return;
        }

        Vector3 composedPosition = baseLocalPosition + trailingOffset + turnOffset + new Vector3(swayOffsetX, bobOffsetY, 0f);
        Vector2 composedScale = Vector2.Scale(bodyScaleMultiplier, turnScaleMultiplier);
        float composedRotation = baseLocalEulerAngles.z + swayRotationZ + moveTiltZ + turnTiltZ;

        visualRoot.localPosition = composedPosition;
        visualRoot.localScale = new Vector3(
            baseLocalScale.x * composedScale.x,
            baseLocalScale.y * composedScale.y,
            baseLocalScale.z);
        visualRoot.localRotation = Quaternion.Euler(baseLocalEulerAngles.x, baseLocalEulerAngles.y, composedRotation);
    }

    private void EnsureVisualRoot()
    {
        if (visualRoot == null)
        {
            visualRoot = transform;
        }
    }

    private void CacheBasePose()
    {
        baseLocalPosition = visualRoot.localPosition;
        baseLocalScale = visualRoot.localScale;
        baseLocalEulerAngles = visualRoot.localEulerAngles;
    }

    private void ResetAnimationState()
    {
        isMoving = false;
        nextTurnTime = 0f;
        lastDirection = Vector2.zero;
        bobOffsetY = 0f;
        swayOffsetX = 0f;
        swayRotationZ = 0f;
        bodyScaleMultiplier = Vector2.one;
        trailingOffset = Vector3.zero;
        moveTiltZ = 0f;
        turnOffset = Vector3.zero;
        turnScaleMultiplier = Vector2.one;
        turnTiltZ = 0f;
    }

    private void KillAllTweens()
    {
        KillLoopTweens();
        KillDirectionalTweens();

        if (turnSequence != null && turnSequence.IsActive())
        {
            turnSequence.Kill();
        }

        KillSettleTweens();
    }

    private void KillLoopTweens()
    {
        if (bounceTween != null && bounceTween.IsActive())
        {
            bounceTween.Kill();
        }

        if (swayTween != null && swayTween.IsActive())
        {
            swayTween.Kill();
        }

        if (swayRotationTween != null && swayRotationTween.IsActive())
        {
            swayRotationTween.Kill();
        }

        if (bodySquishSequence != null && bodySquishSequence.IsActive())
        {
            bodySquishSequence.Kill();
        }
    }

    private void KillDirectionalTweens()
    {
        if (trailingTween != null && trailingTween.IsActive())
        {
            trailingTween.Kill();
        }

        if (tiltTween != null && tiltTween.IsActive())
        {
            tiltTween.Kill();
        }
    }

    private void KillSettleTweens()
    {
        if (settleBobTween != null && settleBobTween.IsActive())
        {
            settleBobTween.Kill();
        }

        if (settleSwayTween != null && settleSwayTween.IsActive())
        {
            settleSwayTween.Kill();
        }

        if (settleSwayRotationTween != null && settleSwayRotationTween.IsActive())
        {
            settleSwayRotationTween.Kill();
        }

        if (settleScaleTween != null && settleScaleTween.IsActive())
        {
            settleScaleTween.Kill();
        }

        if (settleTrailingTween != null && settleTrailingTween.IsActive())
        {
            settleTrailingTween.Kill();
        }

        if (settleTiltTween != null && settleTiltTween.IsActive())
        {
            settleTiltTween.Kill();
        }

        if (settleTurnOffsetTween != null && settleTurnOffsetTween.IsActive())
        {
            settleTurnOffsetTween.Kill();
        }

        if (settleTurnScaleTween != null && settleTurnScaleTween.IsActive())
        {
            settleTurnScaleTween.Kill();
        }

        if (settleTurnTiltTween != null && settleTurnTiltTween.IsActive())
        {
            settleTurnTiltTween.Kill();
        }
    }

    private float PixelsToUnits(float pixelValue)
    {
        return pixelValue / pixelsPerUnit;
    }
}
