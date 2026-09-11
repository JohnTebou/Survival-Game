using UnityEngine;

public class CamLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private Transform _movementOrienter;

    [Header("Normal Look")]
    [SerializeField] private float _sensitivity = 2f;
    [SerializeField] private float _minimumPitch = -85f;
    [SerializeField] private float _maximumPitch = 85f;

    [Header("Alt Look")]
    [SerializeField] private float _returnSpeed = 12f;
    [SerializeField] private Transform altLookPivot;
    
    [Header("Aim Recoil")]
    [SerializeField] private float _recoilFollowSpeed = 18f;

    private float _pendingRecoilYaw;
    private float _pendingRecoilPitch;

    private float _normalYaw;
    private float _normalPitch;

    private float _altYaw;
    private float _altPitch;

    public float NormalPitch => _normalPitch;
    public float NormalYaw => _normalYaw;

    public Quaternion NormalLookRotation =>
        Quaternion.Euler(_normalPitch, _normalYaw, 0f);

    /// <summary>Raw mouse movement this frame. Displacement, not a rate — do not scale by deltaTime.</summary>
    public Vector2 NormalLookDelta { get; private set; }

    /// <summary>Raw alt-look mouse movement this frame.</summary>
    public Vector2 AltLookDelta { get; private set; }

    /// <summary>Look movement expressed per second. Framerate-invariant; use this to drive reactive effects like sway.</summary>
    public Vector2 NormalLookVelocity { get; private set; }

    /// <summary>Alt-look movement expressed per second.</summary>
    public Vector2 AltLookVelocity { get; private set; }

    public bool IsAltLooking => inputReader.GetBool(PlayerInputNames.FreeLook);

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _normalYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        HandleLook();
        HandleRecoil();
    }

    private void LateUpdate()
    {
        ApplyRotations();
    }

    private void HandleLook()
    {
        float deltaTime = Time.deltaTime;

        NormalLookDelta = Vector2.zero;
        AltLookDelta = Vector2.zero;
        NormalLookVelocity = Vector2.zero;
        AltLookVelocity = Vector2.zero;

        // Mouse axes are per-frame displacement, so they are NOT scaled by deltaTime here.
        Vector2 mouseInput = inputReader.GetVector2(PlayerInputNames.Look);
        float mouseX = mouseInput.x * _sensitivity;
        float mouseY = mouseInput.y * _sensitivity;

        if (IsAltLooking)
        {
            AltLookDelta = new Vector2(mouseX, mouseY);
            AltLookVelocity = ToVelocity(AltLookDelta, deltaTime);

            _altYaw += mouseX;
            _altPitch -= mouseY;

            _altPitch = Mathf.Clamp(_altPitch, _minimumPitch - _normalPitch, _maximumPitch - _normalPitch);

            return;
        }

        NormalLookDelta = new Vector2(mouseX, mouseY);
        NormalLookVelocity = ToVelocity(NormalLookDelta, deltaTime);

        _normalYaw += mouseX;
        _normalPitch -= mouseY;
        _normalPitch = Mathf.Clamp(_normalPitch, _minimumPitch, _maximumPitch);

        ReturnAltLook(deltaTime);
    }
    
    public void AddRecoil(float yaw, float pitch)
    {
        _pendingRecoilYaw += yaw;
        _pendingRecoilPitch -= pitch;
    }
    
    private void HandleRecoil()
    {
        float t = 1f - Mathf.Exp(-_recoilFollowSpeed * Time.deltaTime);

        // Smoothly consume yaw recoil into the real look angle.
        float yawStep = _pendingRecoilYaw * t;
        _normalYaw += yawStep;
        _pendingRecoilYaw -= yawStep;

        // Smoothly consume pitch recoil into the real look angle.
        float pitchStep = _pendingRecoilPitch * t;

        float oldPitch = _normalPitch;

        _normalPitch = Mathf.Clamp(
            _normalPitch + pitchStep,
            _minimumPitch,
            _maximumPitch
        );

        float actuallyApplied = _normalPitch - oldPitch;
        _pendingRecoilPitch -= actuallyApplied;

        // Don't store impossible recoil beyond the pitch limits.
        if ((_normalPitch <= _minimumPitch && _pendingRecoilPitch < 0f) ||
            (_normalPitch >= _maximumPitch && _pendingRecoilPitch > 0f))
        {
            _pendingRecoilPitch = 0f;
        }
    }

    private void ReturnAltLook(float deltaTime)
    {
        float returnT = 1f - Mathf.Exp(-_returnSpeed * deltaTime);

        _altYaw = Mathf.LerpAngle(_altYaw, 0f, returnT);
        _altPitch = Mathf.Lerp(_altPitch, 0f, returnT);

        if (Mathf.Abs(Mathf.DeltaAngle(_altYaw, 0f)) < 0.01f)
            _altYaw = 0f;

        if (Mathf.Abs(_altPitch) < 0.01f)
            _altPitch = 0f;
    }

    private static Vector2 ToVelocity(Vector2 delta, float deltaTime)
    {
        return deltaTime > 0f ? delta / deltaTime : Vector2.zero;
    }

    private void ApplyRotations()
    {
        // Normal aim affects camera AND viewmodel.
        transform.rotation =
            Quaternion.Euler(_normalPitch, _normalYaw, 0f);

        // Alt-look affects camera branch only.
        altLookPivot.localRotation =
            Quaternion.Euler(_altPitch, _altYaw, 0f);

        _movementOrienter.rotation =
            Quaternion.Euler(0f, _normalYaw, 0f);
    }
}