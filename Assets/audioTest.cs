using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PlayerScript))]
public class audioTest : NetworkBehaviour
{
    public AudioSource audioSource;
    public AudioSource windSound;
    public AudioSource driftSound;
    public AudioSource collisionSound;
    public List<AudioClip> collisionClips;
    public float defaultPitch = 0.5f;
    public float pitchChange = 0.2f;
    public float maxPitch = 2.0f;
    float input;
    
    public NetworkVariable<float> pitch = new NetworkVariable<float>();

    private PlayerScript playerScript;
    // Start is called before the first frame update
    void Start() {
        playerScript = GetComponent<PlayerScript>();
    }

    public override void OnNetworkSpawn() {
        if (IsServer) {
            pitch.Value = defaultPitch;
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Player")) {
            collisionSound.PlayOneShot(collisionClips[Random.Range(0,collisionClips.Count)]);
        }
    }

// Update is called once per frame
    public void handleDriftStart() {
        driftSound.PlayOneShot(driftSound.clip);
    }

    void Update() {

        audioSource.pitch = pitch.Value;
        if (IsOwner) {
            var targetPitch = defaultPitch +
                              (playerScript.verticalInput * 1.5f + playerScript.currentSpeedFactor * 0.5f) *
                              (maxPitch - defaultPitch);
            targetPitch = Mathf.Lerp(audioSource.pitch, targetPitch, pitchChange * Time.deltaTime);
            UpdatePitchServerRpc(targetPitch);
            
            windSound.volume = playerScript.currentSpeedFactor/4f;
            if(playerScript.wheels.Where(x=>x.startedDrifting).Count()>0)driftSound.Play();//.PlayOneShot(driftSound.clip);
            
        }

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
    [ServerRpc]
    void UpdatePitchServerRpc(float targetPitch, ServerRpcParams rpcParams = default) {
        pitch.Value = targetPitch;
    }
}
