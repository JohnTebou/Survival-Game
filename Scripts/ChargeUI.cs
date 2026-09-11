using UnityEngine;
using UnityEngine.UI;

namespace John
{
    public sealed class ChargeUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject chargeRoot;
        [SerializeField] private Image radialImage;

        [Header("Fill Range")]
        [SerializeField] private float minFill = 0.07f;
        [SerializeField] private float maxFill = 0.43f;

        [Header("Smoothing")]
        [SerializeField] private float followSpeed = 15f;
        [SerializeField] private float hideThreshold = 0.001f;

        private float displayedProgress;
        private float targetProgress;
        private bool ending;

        void Awake()
        {
            displayedProgress = 0f;
            targetProgress = 0f;

            SetFill(0f);
            chargeRoot.SetActive(false);
        }

        void OnEnable()
        {
            ChargeUIEvents.ChargeStarted += HandleChargeStarted;
            ChargeUIEvents.ChargeProgressChanged += HandleChargeProgressChanged;
            ChargeUIEvents.ChargeEnded += HandleChargeEnded;
        }

        void OnDisable()
        {
            ChargeUIEvents.ChargeStarted -= HandleChargeStarted;
            ChargeUIEvents.ChargeProgressChanged -= HandleChargeProgressChanged;
            ChargeUIEvents.ChargeEnded -= HandleChargeEnded;
        }

        void Update()
        {
            float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);

            displayedProgress = Mathf.Lerp(
                displayedProgress,
                targetProgress,
                t
            );

            SetFill(displayedProgress);

            if (ending && displayedProgress <= hideThreshold)
            {
                displayedProgress = 0f;
                targetProgress = 0f;
                SetFill(0f);
                chargeRoot.SetActive(false);
            }
        }

        void HandleChargeStarted()
        {
            if (!chargeRoot.activeSelf)
                chargeRoot.SetActive(true);

            ending = false;
        }

        void HandleChargeProgressChanged(float progress)
        {
            if (!chargeRoot.activeSelf)
                chargeRoot.SetActive(true);

            ending = false;
            targetProgress = Mathf.Clamp01(progress);
        }

        void HandleChargeEnded()
        {
            targetProgress = 0f;
            ending = true;
        }

        void SetFill(float progress)
        {
            radialImage.fillAmount =
                Mathf.Lerp(minFill, maxFill, Mathf.Clamp01(progress));
        }
    }
}