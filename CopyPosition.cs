using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CopyPosition : MonoBehaviour
{
    [SerializeField] private List<TransformPair> pairings;
    
    void Update()
    {
        foreach (TransformPair pairing in pairings)
            pairing.follower.position = pairing.target.position;
    }
}

[System.Serializable]
public struct TransformPair
{
    public Transform follower;
    public Transform target;
}
