using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public delegate void NotifyDrift();
public class WheelScript : MonoBehaviour {
    public bool turningAllowed;
    public bool acceleratingAllowed;

    public WheelCollider wheelCollider;
    public VisualEffect driftEffect;
    public Skidmarks skidmarksController;
    public Transform wheelMesh;

    private Rigidbody carRB;

    private int lastSkid = -1;
    public bool isDrifting = false;
    public bool startedDrifting = false;

    public WheelFrictionCurve defaultSettings;
    public bool onGround = true;
    
    public event NotifyDrift ProcessCompleted;
    
    // Start is called before the first frame update
    void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
        driftEffect = GetComponent<VisualEffect>();
        carRB = GetComponentInParent<Rigidbody>();
        skidmarksController = FindObjectsOfType<Skidmarks>()[0];
        defaultSettings = wheelCollider.sidewaysFriction;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position;
        Quaternion rotation;
        //var wh = GetComponentsInChildren<MeshFilter>()[0].transform;
        wheelCollider.GetWorldPose(out position, out rotation);
        //wheel.transform.position = Vector3.Lerp(wheel.transform.position, position, 0.1f);
        //wheel.transform.rotation = rotation;
        wheelMesh.position = position;
        wheelMesh.rotation = rotation; //* Quaternion.Euler(Vector3.back * 90);
        
        
        WheelHit hit = new WheelHit();
        if (wheelCollider.GetGroundHit(out hit)) {
            //wheel.driftEffect.
            var slip = Mathf.Abs(hit.sidewaysSlip);
            if (slip > .3) {
                driftEffect.SendEvent("OnStartDrift");
                Vector3 skidPoint = hit.point + (carRB.velocity * Time.deltaTime);
                lastSkid = skidmarksController.AddSkidMark(skidPoint, hit.normal, 0.4f+slip, lastSkid);
                //Debug.Log(lastSkid);
                if (!isDrifting) startedDrifting = true;
                else startedDrifting = false;//ProcessCompleted?.Invoke(); 
                isDrifting = true;
            }
            else {
                driftEffect.SendEvent("OnStopDrift");
                lastSkid = -1;
                isDrifting = false;
                startedDrifting = false;
            }

            onGround = true;
        }
        else {
            lastSkid = -1;
            onGround = false;
        }

    }
}
