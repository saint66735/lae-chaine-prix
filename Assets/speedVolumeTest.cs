using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speedVolumeTest : MonoBehaviour
{
    public AudioSource windSound;
    public Rigidbody rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        windSound.volume = 0;
    }

    // Update is called once per frame
    void Update()
    {
        float velocity = Mathf.Abs(rigidbody.velocity.x) + Mathf.Abs(rigidbody.velocity.y) + Mathf.Abs(rigidbody.velocity.z);
        Debug.Log(velocity);
        windSound.volume = velocity / 60 ;
    }
}
