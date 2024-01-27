using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerScript))]
public class audioTest : MonoBehaviour
{
    public AudioSource audioSource;
    public float defaultPitch = 0.5f;
    public float pitchChange = 0.2f;
    public float maxPitch = 2.0f;
    float input;

    private PlayerScript playerScript;
    // Start is called before the first frame update
    void Start() {
        playerScript = GetComponent<PlayerScript>();
    }

    // Update is called once per frame
    void Update() { 
        var targetPitch = defaultPitch+(playerScript.verticalInput*1.5f + playerScript.currentSpeedFactor*0.5f)*(maxPitch-defaultPitch);
        audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, pitchChange*Time.deltaTime);
        /*
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
        */
    }
}