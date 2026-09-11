using UnityEngine;
using UnityEngine.Serialization;

namespace John
{
    public sealed class Door : Interactable
    {
        [Header("References")]
        [SerializeField] private Transform translationPivot;
        [SerializeField] private Transform rotationPivot;

        [Header("Parameters")]
        [SerializeField] private float interactionDistance;
        [SerializeField] private float interactionTime;

        [Space(5)]
        [SerializeField] private Vector3 openPosition = Vector3.zero;
        [SerializeField] private Vector3 closedPosition = Vector3.zero;
        [SerializeField] private float openCloseTranslationSpeed = 5f;
        [SerializeField] private float openCloseTranslationError = 0.015f;

        [Space(5)]
        [SerializeField] private Vector3 openRotation = new Vector3(0f, 90f, 0f);
        [SerializeField] private Vector3 closedRotation = Vector3.zero;
        [SerializeField] private float openCloseRotationSpeed = 180f;
        [SerializeField] private float openCloseRotationError = 0.1f;

        private bool open;
        private bool isMoving;

        private Vector3 targetPosition;
        private Vector3 currentRotation;
        private Vector3 targetRotation;

        public override float InteractionDistance => interactionDistance;
        public override float InteractionTime => interactionTime;

        private void Awake()
        {
            currentRotation = closedRotation;
            targetRotation = closedRotation;
            targetPosition = closedPosition;

            translationPivot.localPosition = closedPosition;
            rotationPivot.localRotation = Quaternion.Euler(closedRotation);
        }

        private void Update()
        {
            HandleOpenClose();
        }

        public override void TryInteract()
        {
            open = !open;

            targetPosition = open
                ? openPosition
                : closedPosition;

            targetRotation = open
                ? openRotation
                : closedRotation;

            isMoving = true;
        }

        private void HandleOpenClose()
        {
            if (!isMoving)
                return;

            currentRotation = Vector3.MoveTowards(
                currentRotation,
                targetRotation,
                openCloseRotationSpeed * Time.deltaTime
            );

            rotationPivot.localRotation = Quaternion.Euler(currentRotation);

            translationPivot.localPosition = Vector3.MoveTowards(
                translationPivot.localPosition,
                targetPosition,
                openCloseTranslationSpeed * Time.deltaTime
            );

            bool rotationFinished =
                Vector3.Distance(currentRotation, targetRotation)
                <= openCloseRotationError;

            bool translationFinished =
                Vector3.Distance(translationPivot.localPosition, targetPosition)
                <= openCloseTranslationError;

            if (!rotationFinished || !translationFinished)
                return;

            currentRotation = targetRotation;

            translationPivot.localPosition = targetPosition;
            rotationPivot.localRotation = Quaternion.Euler(targetRotation);

            isMoving = false;
        }
    }
}