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
    [HideInInspector]
    public float verticalInput;
    private Vector3 movedirection;
    //NetworkVariable<WheelScript[]> wheels = new NetworkVariable<WheelScript[]>();
    private WheelScript[] wheels;
    private Rigidbody rigidBody;
    
    
    public float motorTorque = 3000;
    public float brakeTorque = 2000;
    public float maxSpeed = 50;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 15;
    public float centreOfGravityOffset = -1f;

    [HideInInspector]
    public float currentSpeed = 0f;
    [HideInInspector]
    public float currentSpeedFactor = 0f;
    
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
        var handbrakeInput = Input.GetButton("Jump");
        if (IsOwner && Application.isFocused)
        ControlCarServerRpc(new UserInputStruct(horizontalInput, verticalInput, handbrakeInput));
        //transform.Translate();
    }

    struct UserInputStruct : INetworkSerializeByMemcpy {
        public float horizontalInput;
        public float verticalInput;
        public bool handbrakeInput;

        public UserInputStruct(float horizontalInput, float verticalInput, bool handbrakeInput) {
            this.horizontalInput = horizontalInput;
            this.verticalInput = verticalInput;
            this.handbrakeInput = handbrakeInput;
        }
    }
  

    [ServerRpc]
    void ControlCarServerRpc(UserInputStruct t, ServerRpcParams rpcParams = default) {
        currentSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);

        
        // Calculate how close the car is to top speed
        // as a number from zero to one
        currentSpeedFactor = Mathf.InverseLerp(0, maxSpeed, currentSpeed);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, currentSpeedFactor);
        exhaustEffect.SetFloat("Exhaust", 10+t.verticalInput * 200);
        
        
        movedirection = new Vector3(t.horizontalInput, 0, t.verticalInput);
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, currentSpeedFactor);
        bool isAccelerating = Mathf.Sign(t.verticalInput) == Mathf.Sign(currentSpeed);
        //Debug.Log(t.handbrakeInput);
        foreach (var wheel in wheels) {
            if (wheel.acceleratingAllowed) {
                wheel.wheelCollider.motorTorque = t.verticalInput * currentMotorTorque;
            }

            if (wheel.turningAllowed)
            {
                wheel.wheelCollider.steerAngle = t.horizontalInput * currentSteerRange;
            }
            /*
            if (isAccelerating)
            {
                // Apply torque to Wheel colliders that have "Motorized" enabled
                if (wheel.acceleratingAllowed)
                {
                    wheel.wheelCollider.motorTorque = t.verticalInput * currentMotorTorque;
                }
                wheel.wheelCollider.brakeTorque = 0;
            }
            else
            {
                // If the user is trying to go in the opposite direction
                // apply brakes to all wheels
                wheel.wheelCollider.brakeTorque = Mathf.Abs(t.verticalInput) * brakeTorque;
                wheel.wheelCollider.motorTorque = 0;
            }*/

            if (t.handbrakeInput && wheel.acceleratingAllowed) {
                wheel.wheelCollider.brakeTorque = brakeTorque;
                wheel.wheelCollider.motorTorque = Mathf.Lerp(wheel.wheelCollider.motorTorque, 0, 2f*Time.deltaTime);
            }
            else {
                wheel.wheelCollider.brakeTorque = 0;
            }

            
        }
    }
}
