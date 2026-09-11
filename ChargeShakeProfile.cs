using UnityEngine;

[CreateAssetMenu(fileName = "ChargeShakeProfile", menuName = "Weapons/Charge Shake Profile")]
public class ChargeShakeProfile : ScriptableObject
{
    [Header("Noise")]
    public float positionAmount = 0.008f;
    public float rotationAmount = 1.5f;
    public float frequency = 15f;

    [Header("Position Spring")]
    public float positionOmega = 25f;
    public float positionDamping = 0.7f;

    [Header("Rotation Spring")]
    public float rotationOmega = 25f;
    public float rotationDamping = 0.7f;
}