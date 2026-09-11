using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class ClubSM : NetworkBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Camera Motion")]
    [SerializeField] private CameraMotionPlayer cameraMotion;
    [SerializeField] private CameraMotionProfile raiseCamera;
    [SerializeField] private CameraMotionProfile hitCamera;

    [Header("Charge")]
    [SerializeField] private Slider chargeSlider;
    [SerializeField] private float maxChargeTime = 1.5f;

    [Header("Throw")]
    [SerializeField] private NetworkObject thrownClubPrefab;
    [SerializeField] private Transform throwOrigin;
    [SerializeField] private GameObject heldClubVisual;

    [SerializeField, Range(0f, 1f)]
    private float throwReleaseTime = 0.45f;

    [SerializeField] private float minThrowSpeed = 6f;
    [SerializeField] private float maxThrowSpeed = 22f;

    [SerializeField] private float minSpinSpeed = 5f;
    [SerializeField] private float maxSpinSpeed = 25f;

    [Header("Testing")]
    [SerializeField] private float testRespawnDelay = 3f;

    private State clubState = State.Idle;

    private float charge;
    private bool hasThrown;

    private float respawnTimer;
    private bool waitingForRespawn;

    public float Charge01 => charge;

    private enum State
    {
        Idle,
        SwingRaise,
        SwingHit,
        Raised,
        Throw
    }

    private void Awake()
    {
        ResetCharge();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        HandleInput();
        HandleCharge();
        HandleAnimation();
        HandleTestRespawn();
    }

    private void HandleInput()
    {
        if (waitingForRespawn)
            return;

        if (Input.GetKey(KeyCode.Mouse0) &&
            clubState == State.Idle)
        {
            StartSwing();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1) &&
            clubState == State.Idle)
        {
            StartCharging();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) &&
            clubState == State.Raised)
        {
            ResetCharge();
            StartSwing();
            return;
        }

        if (Input.GetKeyUp(KeyCode.Mouse1) &&
            clubState == State.Raised)
        {
            BeginThrow();
        }
    }

    private void StartSwing()
    {
        clubState = State.SwingRaise;

        animator.Play("SwingRaise1", 0, 0f);
        cameraMotion.Play(raiseCamera);
    }

    private void StartCharging()
    {
        clubState = State.Raised;
        charge = 0f;

        if (chargeSlider != null)
        {
            chargeSlider.value = 0f;
            chargeSlider.gameObject.SetActive(true);
        }

        animator.Play("ThrowRaise", 0, 0f);
        cameraMotion.Play(raiseCamera);
    }

    private void HandleCharge()
    {
        if (clubState != State.Raised)
            return;

        if (!Input.GetKey(KeyCode.Mouse1))
            return;

        charge += Time.deltaTime / maxChargeTime;
        charge = Mathf.Clamp01(charge);

        if (chargeSlider != null)
            chargeSlider.value = charge;
    }

    private void BeginThrow()
    {
        clubState = State.Throw;
        hasThrown = false;

        if (chargeSlider != null)
            chargeSlider.gameObject.SetActive(false);

        animator.Play("Throw", 0, 0f);
    }

    private void HandleAnimation()
    {
        if (animator.IsInTransition(0))
            return;

        AnimatorStateInfo anim =
            animator.GetCurrentAnimatorStateInfo(0);

        if (clubState == State.SwingRaise &&
            anim.IsName("SwingRaise1") &&
            anim.normalizedTime >= 1f)
        {
            clubState = State.SwingHit;

            animator.Play("SwingHit1", 0, 0f);
            cameraMotion.Play(hitCamera);

            return;
        }

        if (clubState == State.SwingHit &&
            anim.IsName("SwingHit1") &&
            anim.normalizedTime >= 1f)
        {
            HardReturnToIdle();
            return;
        }

        if (clubState == State.Throw &&
            anim.IsName("Throw") &&
            !hasThrown &&
            anim.normalizedTime >= throwReleaseTime)
        {
            ReleaseThrow();
        }
    }

    private void ReleaseThrow()
    {
        hasThrown = true;

        float finalCharge = charge;

        SetHeldClubVisible(false);

        SpawnThrownClubServerRpc(
            throwOrigin.position,
            throwOrigin.rotation,
            finalCharge
        );

        ResetCharge();

        respawnTimer = testRespawnDelay;
        waitingForRespawn = true;
    }

    private void HandleTestRespawn()
    {
        if (!waitingForRespawn)
            return;

        respawnTimer -= Time.deltaTime;

        if (respawnTimer > 0f)
            return;

        waitingForRespawn = false;
        hasThrown = false;

        clubState = State.Idle;
        animator.Play("Idle", 0, 0f);

        SetHeldClubVisible(true);
        SetHeldClubVisibleServerRpc(true);
    }

    private void ResetCharge()
    {
        charge = 0f;

        if (chargeSlider == null)
            return;

        chargeSlider.value = 0f;
        chargeSlider.gameObject.SetActive(false);
    }

    [ServerRpc]
    private void SpawnThrownClubServerRpc(
        Vector3 position,
        Quaternion rotation,
        float charge01)
    {
        charge01 = Mathf.Clamp01(charge01);

        float throwSpeed =
            Mathf.Lerp(minThrowSpeed, maxThrowSpeed, charge01);

        float spinSpeed =
            Mathf.Lerp(minSpinSpeed, maxSpinSpeed, charge01);

        NetworkObject thrownClub =
            Instantiate(thrownClubPrefab, position, rotation);

        Spawn(thrownClub);

        Rigidbody rb = thrownClub.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity =
                rotation * Vector3.forward * throwSpeed;

            rb.angularVelocity =
                rotation * Vector3.right * spinSpeed;
        }

        SetHeldClubVisibleObserversRpc(false);
    }

    [ServerRpc]
    private void SetHeldClubVisibleServerRpc(bool visible)
    {
        SetHeldClubVisibleObserversRpc(visible);
    }

    [ObserversRpc(ExcludeOwner = true)]
    private void SetHeldClubVisibleObserversRpc(bool visible)
    {
        SetHeldClubVisible(visible);
    }

    private void SetHeldClubVisible(bool visible)
    {
        if (heldClubVisual != null)
            heldClubVisual.SetActive(visible);
    }

    public void HardReturnToIdle()
    {
        animator.Play("Idle", 0, 0f);
        clubState = State.Idle;

        ResetCharge();
    }

    public void SoftReturnToIdle()
    {
        animator.CrossFadeInFixedTime("Idle", 0.15f);
        clubState = State.Idle;

        ResetCharge();
    }
}