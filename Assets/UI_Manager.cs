using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode.Transports.UTP;
public class UI_Manager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject additionalPanel;
    public TextMeshProUGUI ipInput;
    public TextMeshProUGUI portInput;
    UnityTransport transport;
    // Start is called before the first frame update
    void Start()
    {
       

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
    }
    public void OnQuit()
    {
        //Application.Quit();
        GameNetworkManager.instance.NetworkManager.Shutdown();
        Debug.Log("I WANNA QUIT");
    }
    void closePanels()
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
        updateAddress();
        GameNetworkManager.instance.NetworkManager.StartHost();
        closePanels();
    }
    public void OnJoin()
    {
        updateAddress();
        GameNetworkManager.instance.NetworkManager.StartClient();
        closePanels();
    }
}  
