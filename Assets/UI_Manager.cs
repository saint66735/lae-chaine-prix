using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public class UI_Manager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject additionalPanel;
    public TextMeshProUGUI ipInput;
    public TextMeshProUGUI portInput;
    UnityTransport transport;
    // Start is called before the first frame update
    void Start() {
        loadDefaults();
    }

    public void loadDefaults() {
        var tp = GameNetworkManager.instance.NetworkManager.GetComponent<UnityTransport>();
        ipInput.SetText(tp.ConnectionData.Address);
        portInput.SetText(Convert.ToString((int)tp.ConnectionData.Port));
        //Debug.Log(tp.ConnectionData.Port);
    }
    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (additionalPanel.activeInHierarchy)
            {
                additionalPanel.SetActive(false);
                Cursor.visible = false;
            }
            else
            {
                additionalPanel.SetActive(true);
                Cursor.visible = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            GameNetworkManager.instance.isFreeroam = true;
            OnJoin();
        }
        if (Input.GetKeyDown(KeyCode.O)) {
            GameNetworkManager.instance.isFreeroam = true;
            OnHost();
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            GameNetworkManager.instance.isFreeroam = !GameNetworkManager.instance.isFreeroam;
            Camera.main.GetComponent<FreeCamera>().enabled = GameNetworkManager.instance.isFreeroam;
            Camera.main.GetComponent<CameraScript>().enabled = !GameNetworkManager.instance.isFreeroam;
        }
    }
    public void OnQuit()
    {
        //Application.Quit();
        GameNetworkManager.instance.NetworkManager.Shutdown();
        Debug.Log("I WANNA QUIT");
    }
    public void closePanels()
    {
        mainPanel.SetActive(false);
        additionalPanel.SetActive(false);
        Cursor.visible = false;
    }
    void updateAddress()
    {
        transport = GameNetworkManager.instance.NetworkManager.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ipInput.text;
        string temp = portInput.text.Substring(0, portInput.text.Length - 2);
        transport.ConnectionData.Port = ushort.Parse(temp);
    }
    public void OnHost()
    {
        //updateAddress();
        GameNetworkManager.instance.NetworkManager.StartHost();
        closePanels();
    }
    public void OnJoin()
    {
        //updateAddress();
        GameNetworkManager.instance.NetworkManager.StartClient();
        closePanels();
    }
}  
