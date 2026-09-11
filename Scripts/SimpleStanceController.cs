using System;
using UnityEngine;

public class SimpleStanceController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform anchor;
    private CharacterController cc;

    [Header("Keybinds")]
    [SerializeField] private KeyCode crouchKey = KeyCode.C;
    [SerializeField] private KeyCode proneKey = KeyCode.X;
    [SerializeField] private HoldOrToggle crouchKeyMode = HoldOrToggle.Toggle;
    [SerializeField] private HoldOrToggle proneKeyMode = HoldOrToggle.Toggle;
    
    [Header("Parameters")]
    [SerializeField] private float anchorOffsetFromTop;
    [SerializeField] private float standHeight;
    [SerializeField] private float crouchHeight;
    [SerializeField] private float proneHeight;
    private float targetHeight;
    [SerializeField] private float oneLevelStanceChangeSpeed;
    [SerializeField] private float twoLevelStanceChangeSpeed;
    private float currentStanceChangeSpeed;

    private PlayerStance targetStance;
    
    public PlayerStance TargetStance => targetStance;

    void Awake()
    {
        cc = gameObject.GetComponent<CharacterController>();
        targetStance = PlayerStance.Stand;
        currentStanceChangeSpeed = oneLevelStanceChangeSpeed;
    }
    void Update()
    {
        HandleStanceState();
        HandleStanceFunctionality();
        HandleStanceChange();
    }
    void LateUpdate()
    {
        RecenterCharacterController();
        HandleAnchorPosition();
    }

    void HandleStanceState()
    {
        if (targetStance == PlayerStance.Stand)
        {
            if (crouchKeyMode == HoldOrToggle.Toggle ? Input.GetKeyDown(crouchKey) : Input.GetKey(crouchKey))
            {
                targetStance = PlayerStance.Crouch;
                currentStanceChangeSpeed = oneLevelStanceChangeSpeed;
            }
            
            if (proneKeyMode == HoldOrToggle.Toggle ? Input.GetKeyDown(proneKey) : Input.GetKey(proneKey))
            {
                targetStance = PlayerStance.Prone;
                currentStanceChangeSpeed = twoLevelStanceChangeSpeed;
            }
        }
        else if (targetStance == PlayerStance.Crouch)
        {
            if (crouchKeyMode == HoldOrToggle.Toggle ? Input.GetKeyDown(crouchKey) : Input.GetKeyUp(crouchKey))
            {
                targetStance = PlayerStance.Stand;
                currentStanceChangeSpeed = oneLevelStanceChangeSpeed;
            }
            
            if (proneKeyMode == HoldOrToggle.Toggle ? Input.GetKeyDown(proneKey) : Input.GetKey(proneKey))
            {
                targetStance = PlayerStance.Prone;
                currentStanceChangeSpeed = oneLevelStanceChangeSpeed;
            }
        }
        else
        {
            if (proneKeyMode == HoldOrToggle.Toggle ? Input.GetKeyDown(proneKey) : Input.GetKeyUp(proneKey))
            {
                targetStance = PlayerStance.Stand;
                currentStanceChangeSpeed = twoLevelStanceChangeSpeed;
            }
            
            if (crouchKeyMode == HoldOrToggle.Toggle ? Input.GetKeyDown(crouchKey) : Input.GetKeyUp(crouchKey))
            {
                targetStance = PlayerStance.Crouch;
                currentStanceChangeSpeed = oneLevelStanceChangeSpeed;
            }
        }
    }
    void HandleStanceFunctionality()
    {
        switch (targetStance)
        {
            case PlayerStance.Stand:
                targetHeight = standHeight;
                break;
            
            case PlayerStance.Crouch:
                targetHeight = crouchHeight;
                break;
            
            case PlayerStance.Prone:
                targetHeight = proneHeight;
                break;
        }
    }
    void HandleStanceChange()
    {
        cc.height = Mathf.MoveTowards(cc.height, targetHeight, Time.deltaTime * currentStanceChangeSpeed);
    }
    void HandleAnchorPosition()
    {
        float anchorYPosition = cc.height - anchorOffsetFromTop; 
        anchor.localPosition = new Vector3(0f, anchorYPosition, 0f);
    }
    void RecenterCharacterController()
    {
        cc.center = new Vector3(0f, cc.height * 0.5f, 0f); // makes Player located at base (easier intuition for repositioning children)
    }
}

public enum PlayerStance
{
    Stand,
    Crouch,
    Prone
}
public enum HoldOrToggle
{
    Toggle,
    Hold
}
