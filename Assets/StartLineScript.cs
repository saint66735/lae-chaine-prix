using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StartLineScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter(Collider other) {
        if (GameNetworkManager.instance.raceTime>15f && GameNetworkManager.instance.IsServer && other.CompareTag("Player")) {
            var id = other.gameObject.GetComponent<NetworkObject>().OwnerClientId;
            GameNetworkManager.instance.RaceWin(id);
        }

        
    }
}
