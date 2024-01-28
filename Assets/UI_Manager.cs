using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UI_Manager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject additionalPanel;
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
        Debug.Log("I WANNA QUIT");
    }
    void closePanels()
    {
        mainPanel.SetActive(false);
        additionalPanel.SetActive(false);
        Cursor.visible = false;

    }
    public void OnHost()
    {
        GameNetworkManager.instance.NetworkManager.StartHost();
        closePanels();
    }
    public void OnJoin()
    {
        GameNetworkManager.instance.NetworkManager.StartClient();
        closePanels();
    }
}  
