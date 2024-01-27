using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    public float cameraRotateSpeed = 0.3f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameNetworkManager.instance.playerClientInstance != null) {
            Transform playerTransform = GameNetworkManager.instance.NetworkManager.LocalClient.PlayerObject.transform;//GameNetworkManager.instance.playerClientInstance.transform;
            //GameNetworkManager.instance.NetworkManager.
            var carspeed = playerTransform.GetComponent<Rigidbody>().velocity.magnitude;
            var distanceMulti = 5 + carspeed*0.01f;
            //var positionTarget = playerTransform.position + playerTransform.forward * -10 + playerTransform.up * 5;
            var positionTarget = playerTransform.position + new Vector3(0,0,playerTransform.forward.z * -1.5f * distanceMulti) + Vector3.up * distanceMulti;
            //Debug.Log(positionTarget);
            transform.position = Vector3.Lerp(transform.position, positionTarget, Mathf.Clamp(cameraRotateSpeed*(1+carspeed*0.01f),0,0.9f)*Time.deltaTime);//new Vector3(playerTransform.position.x, playerTransform.position.y+4, playerTransform.position.z-6);
            //var lookAt = (playerTransform.position - transform.position).normalized;
            //Quaternion toRotation = Quaternion.FromToRotation(transform.forward, lookAt);
            //transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, cameraRotateSpeed * Time.time);
            transform.LookAt(playerTransform);
            //Debug.DrawLine(transform.position, transform.position+lookAt.normalized, new Color(0, 0, 1.0f));
            //transform.rotation = Quaternion.Euler(Vector3.down);
            //transform.rotation = Quaternion.Euler(-lookAt); //Quaternion.Lerp(transform.rotation, Quaternion.Euler(lookAt),Time.deltaTime*cameraRotateSpeed);
            //Vector3.RotateTowards(transform.eulerAngles, lookAt, )
            
        }

    }
}
