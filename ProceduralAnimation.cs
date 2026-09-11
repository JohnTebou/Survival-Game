using UnityEngine;
using UnityEngine.Serialization;

public class ProceduralAnimation : MonoBehaviour
{
    [Header("Base Bob")]
    [SerializeField] Transform baseBobber;
    [SerializeField] float baseOmega = 10f, baseDamping = 1f;

    [Header("Main Bob")]
    [SerializeField] Transform mainBobber;
    [SerializeField] float mainOmega = 10f, mainDamping = 1f;

    [Header("Profiles")]
    [SerializeField] BobProfile clubProfile;

    [Header("Locomotion")]
    [SerializeField] CharacterController cc;
    [SerializeField] Movement m;
    [SerializeField] float lerpSpeed = 10f;

    StateProfile state;
    bool underwater;

    float walkSpeed, runSpeed, bobTime;
    Vector3 weights;

    Vector3 basePosVel, baseRotVel, baseRot;
    Vector3 mainPosVel, mainRotVel, mainPos, mainRot;
    Vector3 mainRestPos, mainRestRot;

    const float ReferenceFPS = 60f;

    void Awake()
    {
        walkSpeed = m.WalkSpeed;
        runSpeed = m.RunSpeed;

        weights = Vector3.right;

        baseRot = SignedEuler(baseBobber.localEulerAngles);

        mainRestPos = mainPos = mainBobber.localPosition;
        mainRestRot = mainRot = SignedEuler(mainBobber.localEulerAngles);

        UpdateState();
    }

    void Update()
    {
        bobTime += Time.deltaTime;

        UpdateState();
        UpdateWeights();
        HandleBaseBob();
        HandleMainBob();
    }

    void UpdateState()
    {
        if (underwater)
            state = clubProfile.underwater;
        else if (cc.isGrounded)
            state = clubProfile.grounded;
        else
            state = m.VerticalVelocity > 0f ? clubProfile.jump : clubProfile.fall;
    }

    public void SetUnderwater(bool value) => underwater = value;

    void UpdateWeights()
    {
        float speed = m.FlatSpeed;

        Vector3 target = new(
            1f - Mathf.InverseLerp(0f, walkSpeed, speed),
            0f,
            Mathf.InverseLerp(walkSpeed, runSpeed, speed)
        );

        target.y = 1f - target.x - target.z;

        float t = 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime);
        weights = Vector3.Lerp(weights, target, t);
    }

    void HandleBaseBob()
    {
        Vector3 targetPos =
            state.idleProfile.basePos * weights.x +
            state.walkProfile.basePos * weights.y +
            state.runProfile.basePos * weights.z;

        Vector3 targetRot =
            state.idleProfile.baseRot * weights.x +
            state.walkProfile.baseRot * weights.y +
            state.runProfile.baseRot * weights.z;

        baseBobber.localPosition = Spring(
            baseBobber.localPosition, targetPos,
            baseOmega, baseDamping, ref basePosVel);

        baseRot = Spring(baseRot, targetRot, baseOmega, baseDamping, ref baseRotVel);
        baseBobber.localRotation = Quaternion.Euler(baseRot);
    }

    void HandleMainBob()
    {
        Vector3 pos =
            EvaluatePos(state.idleProfile) * weights.x +
            EvaluatePos(state.walkProfile) * weights.y +
            EvaluatePos(state.runProfile) * weights.z;

        Vector3 rot =
            EvaluateRot(state.idleProfile) * weights.x +
            EvaluateRot(state.walkProfile) * weights.y +
            EvaluateRot(state.runProfile) * weights.z;

        mainPos = Spring(mainPos, mainRestPos + pos, mainOmega, mainDamping, ref mainPosVel);
        mainRot = Spring(mainRot, mainRestRot + rot, mainOmega, mainDamping, ref mainRotVel);

        mainBobber.localPosition = mainPos;
        mainBobber.localRotation = Quaternion.Euler(mainRot);
    }

    Vector3 EvaluatePos(Profile p) => Evaluate(p.posFunctions, p.posAmps, p.posFPC);
    Vector3 EvaluateRot(Profile p) => Evaluate(p.rotFunctions, p.rotAmps, p.rotFPC);

    Vector3 Evaluate(Profile.BobFunction[] f, Vector3 amp, Vector3 fpc) => new(
        Axis(f[0], amp.x, fpc.x),
        Axis(f[1], amp.y, fpc.y),
        Axis(f[2], amp.z, fpc.z)
    );

    float Axis(Profile.BobFunction function, float amp, float fpc)
    {
        if (fpc <= 0f || amp == 0f) return 0f;

        float phase = bobTime * ReferenceFPS / fpc * 2f * Mathf.PI;
        return amp * (function == Profile.BobFunction.Sine ? Mathf.Sin(phase) : Mathf.Cos(phase));
    }

    Vector3 Spring(Vector3 x, Vector3 target, float omega, float damping, ref Vector3 velocity)
    {
        float dt = Time.deltaTime;
        if (dt <= 0f || omega <= 0f) return x;

        Vector3 offset = x - target;
        float z = Mathf.Max(0f, damping);

        if (z < .9999f)
        {
            float a = z * omega;
            float wd = omega * Mathf.Sqrt(1f - z * z);
            float e = Mathf.Exp(-a * dt);
            float c = Mathf.Cos(wd * dt);
            float s = Mathf.Sin(wd * dt);

            Vector3 oldX = offset, oldV = velocity;
            offset = e * (oldX * c + (oldV + a * oldX) / wd * s);
            velocity = e * (oldV * c - (a * oldV + omega * omega * oldX) / wd * s);
        }
        else if (z <= 1.0001f)
        {
            float e = Mathf.Exp(-omega * dt);
            Vector3 c = velocity + omega * offset;

            offset = e * (offset + c * dt);
            velocity = e * (velocity - omega * c * dt);
        }
        else
        {
            float s = Mathf.Sqrt(z * z - 1f);
            float r1 = -omega * (z - s), r2 = -omega * (z + s);

            Vector3 c1 = (velocity - r2 * offset) / (r1 - r2);
            Vector3 c2 = offset - c1;

            float e1 = Mathf.Exp(r1 * dt), e2 = Mathf.Exp(r2 * dt);

            offset = c1 * e1 + c2 * e2;
            velocity = c1 * r1 * e1 + c2 * r2 * e2;
        }

        return target + offset;
    }

    Vector3 SignedEuler(Vector3 e) => new(
        Mathf.DeltaAngle(0f, e.x),
        Mathf.DeltaAngle(0f, e.y),
        Mathf.DeltaAngle(0f, e.z)
    );
}