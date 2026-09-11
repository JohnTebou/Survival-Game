using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BobProfile", menuName = "You Will Die/Procedural Animation/Bob Profile")]
public class BobProfile : ScriptableObject
{
    public StateProfile grounded;
    public StateProfile jump;
    public StateProfile fall;
    public StateProfile underwater;

    public List<StateProfile> Profiles => new() { grounded, jump, fall, underwater };
}

[System.Serializable]
public class Profile
{
    public Vector3 basePos;
    public Vector3 baseRot;

    public enum BobFunction
    {
        Sine,
        Cosine
    }

    public BobFunction[] posFunctions = new BobFunction[3];
    public Vector3 posAmps;
    public Vector3 posFPC;

    public BobFunction[] rotFunctions = new BobFunction[3];
    public Vector3 rotAmps;
    public Vector3 rotFPC;
}

[System.Serializable]
public class StateProfile
{
    public Profile idleProfile;
    public Profile walkProfile;
    public Profile runProfile;
}