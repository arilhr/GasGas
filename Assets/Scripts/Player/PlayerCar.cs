using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum Axel
{
    Front,
    Rear
}

[System.Serializable]
public struct Wheel
{
    public GameObject model;
    public WheelCollider collider;
    public Axel axel;
}

public class PlayerCar : NetworkBehaviour
{

    private Rigidbody playerRb;
    public PlayerController playerController;
    public BarGauge barGauge;
    [SerializeField] private GameObject cameraObject;

    [Header("Player Force / Speed")]
    public float defaultForce = 1000;
    private float maxForceNow;

    [Header("Player Hold Time")]
    public float defaultMaxHoldTime = 3f;
    private float maxHoldTime;
    private float holdTime = 0;

    [Header("Player Move Cooldown")]
    public float defaultChargingCooldown = 2f;
    private float currentChargingCooldown;
    private float chargingCooldown = 0;
    private bool isChargingCooldown = false;

    [Header("Player Turn")]
    [SerializeField] private float maxTurnAngle = 45f;
    [SerializeField] private float turnSensitivity = 1.0f;
    private float turnInput = 0;
    
    [Header("Wheel Coliders")]
    [SerializeField] private List<Wheel> wheels;

    [Header("Mass")]
    [SerializeField] private Transform centerOfMass = null;

    [Header("Is Grounded")]
    private bool isGrounded;
    [SerializeField] private float distToGround = 1f;

    private Vector3 savedPosition;

    private Quaternion savedRotation;

    public override void OnStartAuthority()
    {
        cameraObject.SetActive(true);

        enabled = true;
    }

    private void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        barGauge = ControllerUI.controller.GetComponentInChildren<BarGauge>();
        playerController = ControllerUI.controller.GetComponent<PlayerController>();
        maxForceNow = defaultForce;
        maxHoldTime = defaultMaxHoldTime;
        currentChargingCooldown = defaultChargingCooldown;
        playerRb.centerOfMass = playerRb.transform.InverseTransformPoint(centerOfMass.position);

        playerRb.velocity = transform.forward * 1;
    }

    [ClientCallback]
    private void Update()
    {
        GetTurnInput();
        GasInput();
        GasCooldown();
        GasBarPercentage();
        AnimateWheel();
        CheckGrounded();
    }

    [ClientCallback]
    private void LateUpdate()
    {
        Turn();
    }

    [Client]
    private void GasInput()
    {

        if (!isChargingCooldown)
        {

            // GAS INPUT
            if (playerController.gasButton.IsPressed())
            {
                if (holdTime >= maxHoldTime)
                {
                    holdTime = maxHoldTime;
                }
                else
                {
                    holdTime += Time.deltaTime;
                }
            }

            if (!playerController.gasButton.IsPressed())
            {
                if (holdTime != 0)
                {
                    if (isGrounded)
                        SetSavedPosition();

                    GasMove();
                }
            }
        }
    }

    [Client]
    private void GasMove()
    {
        playerRb.AddForce(transform.forward * (holdTime / maxHoldTime) * maxForceNow);
        
        isChargingCooldown = true;
        chargingCooldown = currentChargingCooldown;
        
        // reset force
        holdTime = 0;
    }

    [Client]
    private void GasCooldown()
    {
        if (isChargingCooldown)
        {
            chargingCooldown -= Time.deltaTime;
            if (chargingCooldown <= 0)
            {
                isChargingCooldown = false;
            }
        }
    }

    [Client]
    private void GasBarPercentage() => barGauge.UpdateBar(holdTime / maxHoldTime);

    [Client]
    private void Turn()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = maxTurnAngle * turnInput * turnSensitivity;
                wheel.collider.steerAngle = Mathf.Lerp(wheel.collider.steerAngle, _steerAngle, 1f);
            } 
        }

    }

    [Client]
    private void AnimateWheel()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.collider.GetWorldPose(out pos, out rot);
            wheel.model.transform.position = pos;
            wheel.model.transform.rotation = rot;
        }
    }

    [Client]
    private void GetTurnInput()
    {
        if (playerController.leftButton.IsPressed())
        {
            turnInput -= Time.deltaTime;
            if (turnInput <= -1)
                turnInput = -1;
        } 
        else if (playerController.rightButton.IsPressed())
        {
            turnInput += Time.deltaTime;
            if (turnInput >= 1)
                turnInput = 1;
        }
        else
        {
            if (turnInput > 0.1)
                turnInput -= Time.deltaTime;
            else if (turnInput < -0.1)
                turnInput += Time.deltaTime;
            else
                turnInput = 0;
        }
    }

    [Client]
    public void RespawnPlayer()
    {
        transform.position = savedPosition;
        transform.localRotation = savedRotation;
    }

    [Client]
    private void SetSavedPosition()
    {
        savedPosition = transform.position;
        savedRotation = transform.localRotation;
    }

    [Client]
    private void CheckGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, distToGround + 0.1f))
            isGrounded = true;
        else
            isGrounded = false;
    }

    [TargetRpc]
    public void ActivatePower(NetworkConnection conn, GameObject player, int powerType, float duration)
    {

        IPowerup[] powers = player.gameObject.GetComponentsInChildren<IPowerup>();

        powers[powerType].PowerStart(duration);

    }


    /* SETTER */
    [Client]
    public void SetMaxForceNow(float _force) => maxForceNow = _force;
    [Client]
    public void SetMaxHoldTime(float _time) => maxHoldTime = _time;
    [Client]
    public void SetChargingCooldown(float _time) => currentChargingCooldown = _time;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);
    }
}
