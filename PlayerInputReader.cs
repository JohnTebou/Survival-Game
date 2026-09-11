using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputReader : MonoBehaviour
{
    [SerializeField] private PlayerInputBufferConfig bufferConfig;

    private PlayerInput playerInput;

    private readonly Dictionary<string, Vector2> vector2Values = new();
    private readonly Dictionary<string, float> floatValues = new();
    private readonly Dictionary<string, bool> boolValues = new();

    private readonly Dictionary<string, bool> wasPressedThisFrame = new();
    private readonly Dictionary<string, bool> wasReleasedThisFrame = new();

    private readonly Dictionary<string, float> lastPressedTimes = new();
    private readonly Dictionary<string, float> lastReleasedTimes = new();
    private readonly Dictionary<string, float> startedHoldingTimes = new();

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        playerInput.actions
            .FindActionMap(PlayerInputNames.Player, true)
            .Enable();

        foreach (string name in PlayerInputNames.Vector2Actions)
            vector2Values[name] = Vector2.zero;

        foreach (string name in PlayerInputNames.FloatActions)
            floatValues[name] = 0f;

        foreach (string name in PlayerInputNames.BoolActions)
        {
            boolValues[name] = false;
            wasPressedThisFrame[name] = false;
            wasReleasedThisFrame[name] = false;
        }

        Bind();
    }

    void Bind()
    {
        foreach (string name in PlayerInputNames.Vector2Actions)
        {
            InputAction a = Action(name);
            a.performed += ReadVector2;
            a.canceled += ReadVector2;
        }

        foreach (string name in PlayerInputNames.FloatActions)
        {
            InputAction a = Action(name);
            a.performed += ReadFloat;
            a.canceled += ReadFloat;
        }

        foreach (string name in PlayerInputNames.BoolActions)
        {
            InputAction a = Action(name);
            a.started += ButtonPressed;
            a.canceled += ButtonReleased;
        }
    }

    void OnDestroy()
    {
        foreach (string name in PlayerInputNames.Vector2Actions)
        {
            InputAction a = Action(name);
            a.performed -= ReadVector2;
            a.canceled -= ReadVector2;
        }

        foreach (string name in PlayerInputNames.FloatActions)
        {
            InputAction a = Action(name);
            a.performed -= ReadFloat;
            a.canceled -= ReadFloat;
        }

        foreach (string name in PlayerInputNames.BoolActions)
        {
            InputAction a = Action(name);
            a.started -= ButtonPressed;
            a.canceled -= ButtonReleased;
        }
    }

    InputAction Action(string name) => playerInput.actions.FindAction(name, true);

    void ReadVector2(InputAction.CallbackContext ctx) =>
        vector2Values[ctx.action.name] = ctx.canceled ? Vector2.zero : ctx.ReadValue<Vector2>();

    void ReadFloat(InputAction.CallbackContext ctx) =>
        floatValues[ctx.action.name] = ctx.canceled ? 0f : ctx.ReadValue<float>();

    void ButtonPressed(InputAction.CallbackContext ctx)
    {
        string name = ctx.action.name;
        float now = Time.unscaledTime;

        boolValues[name] = true;
        wasPressedThisFrame[name] = true;
        lastPressedTimes[name] = now;
        startedHoldingTimes[name] = now;
    }

    void ButtonReleased(InputAction.CallbackContext ctx)
    {
        string name = ctx.action.name;

        boolValues[name] = false;
        wasReleasedThisFrame[name] = true;
        lastReleasedTimes[name] = Time.unscaledTime;
        startedHoldingTimes.Remove(name);
    }

    public Vector2 GetVector2(string name) => vector2Values[name];
    public float GetFloat(string name) => floatValues[name];
    public bool GetBool(string name) => boolValues[name];
    public bool WasPressedRecently(string name)
    {
        if (!lastPressedTimes.TryGetValue(name, out float time))
            return false;

        float age = Time.unscaledTime - time;

        return age <= bufferConfig.GetPressBuffer(name);
    }
    public bool WasPressedThisFrame(string name) => wasPressedThisFrame[name];
    public bool WasReleasedThisFrame(string name) => wasReleasedThisFrame[name];

    public float GetHeldDuration(string name) =>
        boolValues[name] && startedHoldingTimes.TryGetValue(name, out float start)
            ? Time.unscaledTime - start
            : 0f;

    public bool ConsumeBufferedPress(string name)
    {
        if (!lastPressedTimes.TryGetValue(name, out float time)) return false;

        float age = Time.unscaledTime - time;
        if (!wasPressedThisFrame[name] && age > bufferConfig.GetPressBuffer(name))
        {
            lastPressedTimes.Remove(name);
            return false;
        }

        lastPressedTimes.Remove(name);
        return true;
    }

    public bool ConsumeBufferedRelease(string name)
    {
        if (!lastReleasedTimes.TryGetValue(name, out float time)) return false;

        float age = Time.unscaledTime - time;
        if (!wasReleasedThisFrame[name] && age > bufferConfig.GetReleaseBuffer(name))
        {
            lastReleasedTimes.Remove(name);
            return false;
        }

        lastReleasedTimes.Remove(name);
        return true;
    }

    void LateUpdate()
    {
        foreach (string name in PlayerInputNames.BoolActions)
        {
            wasPressedThisFrame[name] = false;
            wasReleasedThisFrame[name] = false;
        }
    }
}