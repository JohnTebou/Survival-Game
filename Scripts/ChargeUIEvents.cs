using System;
using UnityEngine;

namespace John
{
    public static class ChargeUIEvents
    {
        public static event Action ChargeStarted;
        public static event Action<float> ChargeProgressChanged;
        public static event Action ChargeEnded;

        public static void StartCharge() => ChargeStarted?.Invoke();

        public static void SetProgress(float progress) =>
            ChargeProgressChanged?.Invoke(Mathf.Clamp01(progress));

        public static void EndCharge() =>
            ChargeEnded?.Invoke();
    }
}