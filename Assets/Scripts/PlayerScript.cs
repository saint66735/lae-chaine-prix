using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class PlayerScript : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public bool loaded = false;
    public VisualEffect exhaustEffect;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 movedirection;
    //NetworkVariable<WheelScript[]> wheels = new NetworkVariable<WheelScript[]>();
    private WheelScript[] wheels;
    private Rigidbody rigidBody;
    
    
    public float motorTorque = 3000;
    public float brakeTorque = 2000;
    public float maxSpeed = 50;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public float centreOfGravityOffset = -1f;
    
    void Start() {
        wheels = GetComponentsInChildren<WheelScript>();
        rigidBody = GetComponent<Rigidbody>();
        // Adjust center of mass vertically, to help prevent the car from rolling
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
    }

    public override void OnNetworkSpawn()
    {
        //if(IsServer && GetComponent<NetworkObject>().IsOwnedByServer) GetComponent<NetworkObject>().ChangeOwnership(GetComponent<NetworkObject>().NetworkManager.LocalClientId);
        if (IsOwner)
        {
            Move();
            
        }

        loaded = true;
    }

    public void Move()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var randomPosition = GetNextPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }
        else
        {
            SubmitPositionRequestServerRpc();
        }
        transform.eulerAngles = new Vector3(0,0,0);
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = GetNextPositionOnPlane();
        transform.position = Position.Value;
    }

    Vector3 GetNextPositionOnPlane() {
        return GameNetworkManager.instance.spawnPoints[NetworkManager.Singleton.ConnectedClients.Count-1].position;
    }

    void Update() {
        //transform.position = Position.Value;
        //if (loaded && IsServer)
        //{
        //    //Debug.Log(NetworkObject.NetworkObjectId);
        //    float theta = Time.frameCount / 10.0f + NetworkObject.NetworkObjectId;
        //    transform.position = new Vector3((float) Math.Cos(theta), 0.0f, (float) Math.Sin(theta));
        //}
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        if (IsOwner && Application.isFocused)
        ControlCarServerRpc(horizontalInput, verticalInput);
        //transform.Translate();
    }

  

    [ServerRpc]
    void ControlCarServerRpc(float horizontalInput, float verticalInput, ServerRpcParams rpcParams = default) {
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);


        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
        exhaustEffect.SetFloat("Exhaust", 10+verticalInput * 200);
        
        
        movedirection = new Vector3(horizontalInput, 0, verticalInput);
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);
        bool isAccelerating = Mathf.Sign(verticalInput) == Mathf.Sign(forwardSpeed);

        foreach (var wheel in wheels) {
            
            wheel.wheelCollider.motorTorque = verticalInput * currentMotorTorque;
            if (wheel.turningAllowed)
            {
                wheel.wheelCollider.steerAngle = horizontalInput * currentSteerRange;
            }
            
            if (isAccelerating)
            {
                // Apply torque to Wheel colliders that have "Motorized" enabled
                if (wheel.acceleratingAllowed)
                {
                    wheel.wheelCollider.motorTorque = verticalInput * currentMotorTorque;
                }
                wheel.wheelCollider.brakeTorque = 0;
            }
            else
            {
                // If the user is trying to go in the opposite direction
                // apply brakes to all wheels
                wheel.wheelCollider.brakeTorque = Mathf.Abs(verticalInput) * brakeTorque;
                wheel.wheelCollider.motorTorque = 0;
            }

        }
    }
}
