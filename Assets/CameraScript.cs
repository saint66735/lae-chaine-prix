using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameNetworkManager.instance.playerClientInstance) {
            Transform playerTransform = GameNetworkManager.instance.playerClientInstance.transform;
            transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y+4, playerTransform.position.z-6);
            transform.LookAt(playerTransform);
        }

    }
}
