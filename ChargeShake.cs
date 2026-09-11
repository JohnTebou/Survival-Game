using UnityEngine;

public class ChargeShake : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MonoBehaviour chargeSourceBehaviour;
    [SerializeField] private ChargeShakeProfile profile;

    [Header("Profile Transition")]
    [SerializeField] private float profileTransitionSpeed = 8f;

    private IChargeSource chargeSource;

    private Vector3 restPosition;
    private Quaternion restRotation;

    private Vector3 position;
    private Vector3 positionVelocity;

    private Vector3 rotation;
    private Vector3 rotationVelocity;

    private float positionAmount;
    private float rotationAmount;
    private float frequency;

    private float positionOmega;
    private float positionDamping;
    private float rotationOmega;
    private float rotationDamping;

    private float noisePhase;

    private void Awake()
    {
        chargeSource = chargeSourceBehaviour as IChargeSource;

        if (chargeSource == null)
        {
            Debug.LogError(
                $"{chargeSourceBehaviour} must implement IChargeSource.",
                this
            );

            enabled = false;
            return;
        }

        restPosition = transform.localPosition;
        restRotation = transform.localRotation;

        if (profile != null)
            SnapToProfile(profile);
    }

    private void LateUpdate()
    {
        if (profile == null)
            return;

        float dt = Time.deltaTime;

        BlendProfile(dt);

        noisePhase += frequency * dt;

        float charge = chargeSource.Charge01;

        Vector3 noise = new Vector3(
            Mathf.PerlinNoise(noisePhase, 0f) * 2f - 1f,
            Mathf.PerlinNoise(0f, noisePhase) * 2f - 1f,
            Mathf.PerlinNoise(noisePhase, noisePhase) * 2f - 1f
        );

        Vector3 targetPosition =
            noise * positionAmount * charge;

        Vector3 targetRotation =
            noise * rotationAmount * charge;

        position = Spring(
            position,
            targetPosition,
            positionOmega,
            positionDamping,
            ref positionVelocity,
            dt
        );

        rotation = Spring(
            rotation,
            targetRotation,
            rotationOmega,
            rotationDamping,
            ref rotationVelocity,
            dt
        );

        transform.localPosition =
            restPosition + position;

        transform.localRotation =
            restRotation * Quaternion.Euler(rotation);
    }

    private void BlendProfile(float dt)
    {
        float t =
            1f - Mathf.Exp(-profileTransitionSpeed * dt);

        positionAmount =
            Mathf.Lerp(positionAmount, profile.positionAmount, t);

        rotationAmount =
            Mathf.Lerp(rotationAmount, profile.rotationAmount, t);

        frequency =
            Mathf.Lerp(frequency, profile.frequency, t);

        positionOmega =
            Mathf.Lerp(positionOmega, profile.positionOmega, t);

        positionDamping =
            Mathf.Lerp(positionDamping, profile.positionDamping, t);

        rotationOmega =
            Mathf.Lerp(rotationOmega, profile.rotationOmega, t);

        rotationDamping =
            Mathf.Lerp(rotationDamping, profile.rotationDamping, t);
    }

    private void SnapToProfile(ChargeShakeProfile p)
    {
        positionAmount = p.positionAmount;
        rotationAmount = p.rotationAmount;
        frequency = p.frequency;

        positionOmega = p.positionOmega;
        positionDamping = p.positionDamping;

        rotationOmega = p.rotationOmega;
        rotationDamping = p.rotationDamping;
    }

    public void SetProfile(ChargeShakeProfile newProfile)
    {
        profile = newProfile;
    }

    // Exact analytic second-order spring.
    // Stable across different frame rates and large dt values.
    private static Vector3 Spring(
        Vector3 input,
        Vector3 target,
        float omega,
        float damping,
        ref Vector3 velocity,
        float dt)
    {
        if (dt <= 0f || omega <= 0f)
            return input;

        Vector3 x = input - target;
        Vector3 oldX = x;
        Vector3 oldV = velocity;

        const float epsilon = 0.0001f;

        // Underdamped.
        if (damping < 1f - epsilon)
        {
            float alpha = damping * omega;
            float dampedOmega =
                omega * Mathf.Sqrt(1f - damping * damping);

            float e = Mathf.Exp(-alpha * dt);
            float c = Mathf.Cos(dampedOmega * dt);
            float s = Mathf.Sin(dampedOmega * dt);

            Vector3 a = oldX;
            Vector3 b =
                (oldV + alpha * oldX) / dampedOmega;

            x = e * (a * c + b * s);

            velocity = e * (
                (-alpha * a + dampedOmega * b) * c +
                (-alpha * b - dampedOmega * a) * s
            );
        }

        // Critically damped.
        else if (damping <= 1f + epsilon)
        {
            float e = Mathf.Exp(-omega * dt);

            Vector3 c =
                oldV + omega * oldX;

            x = e * (
                oldX + c * dt
            );

            velocity = e * (
                oldV - c * (omega * dt)
            );
        }

        // Overdamped.
        else
        {
            float z =
                Mathf.Sqrt(damping * damping - 1f);

            float r1 =
                -omega * (damping - z);

            float r2 =
                -omega * (damping + z);

            Vector3 c1 =
                (oldV - r2 * oldX) / (r1 - r2);

            Vector3 c2 =
                oldX - c1;

            float e1 = Mathf.Exp(r1 * dt);
            float e2 = Mathf.Exp(r2 * dt);

            x =
                c1 * e1 +
                c2 * e2;

            velocity =
                r1 * c1 * e1 +
                r2 * c2 * e2;
        }

        return target + x;
    }
}