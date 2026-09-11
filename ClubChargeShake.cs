using UnityEngine;

public class ClubChargeShake : MonoBehaviour
{
    [SerializeField] private ClubSM club;

    [SerializeField] private float positionAmount = 0.008f;
    [SerializeField] private float rotationAmount = 1.5f;
    [SerializeField] private float frequency = 15f;
    [SerializeField] private float smoothing = 18f;

    private Vector3 restPosition;
    private Quaternion restRotation;

    private Vector3 currentPosition;
    private Vector3 currentRotation;

    private void Awake()
    {
        restPosition = transform.localPosition;
        restRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        float charge = club.Charge01;

        float time = Time.time * frequency;

        Vector3 noise = new Vector3(
            Mathf.PerlinNoise(time, 0f) * 2f - 1f,
            Mathf.PerlinNoise(0f, time) * 2f - 1f,
            Mathf.PerlinNoise(time, time) * 2f - 1f
        );

        Vector3 targetPosition =
            noise * positionAmount * charge;

        Vector3 targetRotation =
            noise * rotationAmount * charge;

        float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);

        currentPosition = Vector3.Lerp(currentPosition, targetPosition, t);
        currentRotation = Vector3.Lerp(currentRotation, targetRotation, t);

        transform.localPosition = restPosition + currentPosition;
        transform.localRotation =
            restRotation * Quaternion.Euler(currentRotation);
    }
}