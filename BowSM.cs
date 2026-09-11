using FishNet.Object;
using UnityEngine;

public class BowSM : NetworkBehaviour, IChargeSource
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerInputReader inputReader;

    [Header("Arrow")]
    [SerializeField] private GameObject viewmodelArrow;
    [SerializeField] private NetworkObject arrowPrefab;
    [SerializeField] private Transform arrowSpawn;

    [Header("Charge")]
    [SerializeField] private float fullChargeTime = 1.25f;
    [SerializeField] private float minArrowSpeed = 8f;
    [SerializeField] private float maxArrowSpeed = 35f;

    [Header("Release")]
    [SerializeField] private float releaseFadeTime = 0.08f;

    // When during the Release animation the real arrow appears.
    [SerializeField, Range(0f, 1f)]
    private float projectileReleaseTime = 0.1f;

    [Header("Return")]
    [SerializeField] private float idleFadeTime = 0.15f;

    private State state = State.Idle;

    private float charge;
    private float releasedCharge;

    private bool projectileReleased;

    public float Charge01 => charge;

    private enum State
    {
        Idle,
        Charging,
        Releasing
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        HandleInput();
        HandleAnimation();
    }

    private void HandleInput()
    {
        if (state == State.Idle &&
            inputReader.WasPressedRecently(PlayerInputNames.PrimaryAction))
        {
            StartCharging();
        }

        if (state != State.Charging)
            return;

        if (inputReader.GetBool(PlayerInputNames.PrimaryAction))
        {
            charge += Time.deltaTime / fullChargeTime;
            charge = Mathf.Clamp01(charge);
        }
        else
        {
            BeginRelease();
        }
    }

    private void StartCharging()
    {
        state = State.Charging;

        charge = 0f;
        releasedCharge = 0f;
        projectileReleased = false;

        if (viewmodelArrow != null)
            viewmodelArrow.SetActive(true);
    }

    private void BeginRelease()
    {
        state = State.Releasing;

        // Preserve the amount used for projectile speed.
        releasedCharge = charge;

        // ChargeShake now springs back toward zero.
        charge = 0f;

        animator.CrossFadeInFixedTime(
            "Release",
            releaseFadeTime,
            0,
            0f
        );
    }

    private void HandleAnimation()
    {
        if (state == State.Charging)
        {
            // Directly scrub through Draw according to charge.
            animator.Play(
                "Draw",
                0,
                charge
            );

            return;
        }

        if (state != State.Releasing)
            return;

        AnimatorStateInfo anim =
            animator.GetCurrentAnimatorStateInfo(0);

        // During the transition we haven't fully entered Release yet.
        if (!anim.IsName("Release"))
            return;

        // Spawn the physical arrow at the chosen point
        // in the Release animation.
        if (!projectileReleased &&
            anim.normalizedTime >= projectileReleaseTime)
        {
            FireArrow();
        }

        if (animator.IsInTransition(0))
            return;

        // Release finished -> return to idle.
        if (anim.normalizedTime >= 1f)
        {
            ReturnToIdle();
        }
    }

    private void FireArrow()
    {
        projectileReleased = true;

        if (viewmodelArrow != null)
            viewmodelArrow.SetActive(false);

        SpawnArrowServerRpc(
            arrowSpawn.position,
            arrowSpawn.rotation,
            releasedCharge
        );
    }

    [ServerRpc]
    private void SpawnArrowServerRpc(
        Vector3 position,
        Quaternion rotation,
        float charge01)
    {
        charge01 = Mathf.Clamp01(charge01);

        float speed = Mathf.Lerp(
            minArrowSpeed,
            maxArrowSpeed,
            charge01
        );

        NetworkObject arrow =
            Instantiate(
                arrowPrefab,
                position,
                rotation
            );

        Spawn(arrow);

        Rigidbody rb =
            arrow.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Assumes the arrow points along local +Z.
            rb.linearVelocity =
                rotation * Vector3.forward * speed;
        }
    }

    private void ReturnToIdle()
    {
        animator.CrossFadeInFixedTime(
            "Idle",
            idleFadeTime
        );

        state = State.Idle;

        charge = 0f;
        releasedCharge = 0f;
        projectileReleased = false;

        // Testing: magically give us another arrow.
        if (viewmodelArrow != null)
            viewmodelArrow.SetActive(true);
    }
}