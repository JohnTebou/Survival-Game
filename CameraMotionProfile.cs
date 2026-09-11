using UnityEngine;

[CreateAssetMenu(menuName = "Camera/Camera Motion Profile")]
public class CameraMotionProfile : ScriptableObject
{
    [Min(0.01f)] public float duration = 0.3f;

    [Header("Position — meters")]
    public AnimationCurve posX = AnimationCurve.Linear(0, 0, 1, 0);
    public AnimationCurve posY = AnimationCurve.Linear(0, 0, 1, 0);
    public AnimationCurve posZ = AnimationCurve.Linear(0, 0, 1, 0);

    [Header("Rotation — degrees")]
    public AnimationCurve rotX = AnimationCurve.Linear(0, 0, 1, 0);
    public AnimationCurve rotY = AnimationCurve.Linear(0, 0, 1, 0);
    public AnimationCurve rotZ = AnimationCurve.Linear(0, 0, 1, 0);

    public Vector3 EvaluatePosition(float t) => new(posX.Evaluate(t), posY.Evaluate(t), posZ.Evaluate(t));
    public Vector3 EvaluateRotation(float t) => new(rotX.Evaluate(t), rotY.Evaluate(t), rotZ.Evaluate(t));
}