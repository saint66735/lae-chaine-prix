using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class audioTest : MonoBehaviour
{
    public AudioSource audioSource;
    public float defaultPitch = 1.0f;
    public float pitchChange = 0.02f;
    public float maxPitch = 3.0f;
    float input;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        input = Input.GetAxisRaw("Vertical");
        if (input != 0 && audioSource.pitch<maxPitch)
        {
            audioSource.pitch += pitchChange;
        }
        if (input == 0)
        {
            if (audioSource.pitch > defaultPitch)
            {
                audioSource.pitch -= pitchChange;
            }
            else if (audioSource.pitch < defaultPitch){
                audioSource.pitch = defaultPitch;
            }
        }
    }
}
