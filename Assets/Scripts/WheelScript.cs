using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WheelScript : MonoBehaviour {
    public bool turningAllowed;
    public bool acceleratingAllowed;

    public WheelCollider collider;
    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position;
        Quaternion rotation;
        var wh = GetComponentsInChildren<MeshFilter>()[0].transform;
        collider.GetWorldPose(out position, out rotation);
        //wheel.transform.position = Vector3.Lerp(wheel.transform.position, position, 0.1f);
        //wheel.transform.rotation = rotation;
        wh.position = position;
        wh.rotation = rotation * Quaternion.Euler(Vector3.back * 90);;
    }
}
