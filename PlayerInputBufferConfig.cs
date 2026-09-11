using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInputBufferConfig", menuName = "You Will Die/Input/Buffer Config")]
public class PlayerInputBufferConfig : ScriptableObject
{
    [Min(0)] public float defaultPressBuffer = .12f;
    [Min(0)] public float defaultReleaseBuffer = .12f;

    public List<BufferOverride> overrides = new();

    public float GetPressBuffer(string action) => Get(action, true);
    public float GetReleaseBuffer(string action) => Get(action, false);

    float Get(string action, bool press)
    {
        foreach (BufferOverride setting in overrides)
            if (setting.actionName == action)
                return press ? setting.pressBuffer : setting.releaseBuffer;

        return press ? defaultPressBuffer : defaultReleaseBuffer;
    }

    [Serializable]
    public class BufferOverride
    {
        public string actionName;
        [Min(0)] public float pressBuffer = .12f;
        [Min(0)] public float releaseBuffer = .12f;
    }
}