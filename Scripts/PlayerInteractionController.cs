using UnityEngine;

namespace John
{
    public sealed class PlayerInteractionController : MonoBehaviour
    {
        [SerializeField] private Transform interactionOrigin;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;

        private Interactable currentInteractable;
        private Vector3 interactionHitPoint;

        private float holdStartTime;
        private float heldTime;

        void Update()
        {
            HandleInteraction();
        }

        void HandleInteraction()
        {
            if (currentInteractable != null)
            {
                HandleHeldInteraction();
                return;
            }

            TryStartInteraction();
        }

        void TryStartInteraction()
        {
            if (!Physics.Raycast(
                    interactionOrigin.position,
                    interactionOrigin.forward,
                    out RaycastHit hit))
                return;

            Interactable interactable =
                hit.collider.GetComponentInParent<Interactable>();

            if (interactable == null)
                return;

            if (hit.distance >= interactable.InteractionDistance)
                return;

            if (!Input.GetKeyDown(interactionKey))
                return;

            if (interactable.InteractionTime <= 0f)
            {
                interactable.TryInteract();
                return;
            }

            currentInteractable = interactable;
            interactionHitPoint = hit.point;

            holdStartTime = Time.time;
            heldTime = 0f;

            ChargeUIEvents.StartCharge();
        }

        void HandleHeldInteraction()
        {
            if (!Input.GetKey(interactionKey))
            {
                ClearHeldInteraction();
                return;
            }

            float distance = Vector3.Distance(
                interactionOrigin.position,
                interactionHitPoint
            );

            if (distance >= currentInteractable.InteractionDistance)
            {
                ClearHeldInteraction();
                return;
            }

            heldTime = Time.time - holdStartTime;

            float progress =
                heldTime / currentInteractable.InteractionTime;

            ChargeUIEvents.SetProgress(progress);

            if (heldTime >= currentInteractable.InteractionTime)
            {
                currentInteractable.TryInteract();
                ClearHeldInteraction();
            }
        }

        void ClearHeldInteraction()
        {
            if (currentInteractable != null)
                ChargeUIEvents.EndCharge();

            currentInteractable = null;
            interactionHitPoint = Vector3.zero;

            heldTime = 0f;
            holdStartTime = 0f;
        }
    }
}