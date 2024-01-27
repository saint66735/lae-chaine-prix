using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WheelScript : MonoBehaviour {
    public bool turningAllowed;
    public bool acceleratingAllowed;

    public WheelCollider wheelCollider;

    public Transform wheelMesh;
    // Start is called before the first frame update
    void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
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
        wheelMesh.rotation = rotation; //* Quaternion.Euler(Vector3.back * 90);;
    }
}
