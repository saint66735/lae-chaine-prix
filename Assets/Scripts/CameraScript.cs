using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    public float cameraRotateSpeed = 5f;
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
            var lookAt = playerTransform.position - transform.position;
            transform.LookAt(playerTransform);
           // transform.rotation =Quaternion.Euler(lookAt.normalized); //Quaternion.Lerp(transform.rotation, Quaternion.Euler(lookAt),Time.deltaTime*cameraRotateSpeed);
            //Vector3.RotateTowards(transform.eulerAngles, lookAt, )
            
        }

    }
}
