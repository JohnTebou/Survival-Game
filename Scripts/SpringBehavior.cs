using UnityEngine;

public static class SpringBehavior
{
    // analytic spring behavior
    public static Vector3 Spring(Vector3 x, Vector3 target, float omega, float damping, ref Vector3 velocity)
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
    
    // converts into shortest signed form; e.g. 720 => 0, 539 => 179
    public static Vector3 SignedEuler(Vector3 e) => new(
        Mathf.DeltaAngle(0f, e.x),
        Mathf.DeltaAngle(0f, e.y),
        Mathf.DeltaAngle(0f, e.z)
    );
}