using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameNetworkManager : NetworkBehaviour {
  // Start is called before the first frame update
  public GameObject playerPrefab;
  public GameObject chainPrefab;
  public GameObject playerClientInstance;
  public static GameNetworkManager instance;
  //public List<NetworkObject> playerInstances;
  public List<GameObject> playerInstances;
  public GameObject chain;
  public List<Transform> spawnPoints;
  public UI_Manager UIManagerScript;

  private Action<NetworkManager.ConnectionApprovalRequest, NetworkManager.ConnectionApprovalResponse> defaultAprovalCallback;
  void Start() {
    instance = this;
    UIManagerScript.loadDefaults();
    var mppmTag = "";
    if (CurrentPlayer.ReadOnlyTags().Count>0)mppmTag = CurrentPlayer.ReadOnlyTags().First();
    Debug.Log(mppmTag);
    var networkManager = NetworkManager.Singleton;
    
    if (mppmTag.Contains("Server")) {
      networkManager.StartServer();
    }
    else if (mppmTag.Contains("Host")) {
      //UIManagerScript.OnHost();
      networkManager.StartHost();
      UIManagerScript.closePanels();
    }
    else if (mppmTag.Contains("Client")) {
      //UIManagerScript.OnJoin();
      networkManager.StartClient();
      UIManagerScript.closePanels();
    }
    
  }
  
  public override void OnNetworkSpawn()
  {
    /*
    if (networkManager.IsServer || networkManager.IsHost) {
      
    }*/
    /*
    if (IsServer)
    {
      SpawnPlayers(NetworkManager.Singleton.LocalClientId);
      //NetworkManager.ConnectionApprovalCallback = ConnectionApprovalCallback;
    }
    else SubmitSpawnRequestServerRpc();
    */
    if (IsServer) {
      defaultAprovalCallback = NetworkManager.ConnectionApprovalCallback;
      NetworkManager.ConnectionApprovalCallback = ConnectionApprovalCallback;
    }
  }
  private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
  {
    Debug.Log("conection aproved");
    GameObject go = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
    go.GetComponent<NetworkObject>().SpawnAsPlayerObject(request.ClientNetworkId, true);
    if (defaultAprovalCallback!=null) defaultAprovalCallback.Invoke(request,response);
    //playerInstances.Append(NetworkManager.Singleton.ConnectedClients[request.ClientNetworkId].PlayerObject);
    //foreach (var client in NetworkManager.Singleton.ConnectedClients.AsReadOnlyList()){
    //  client.Value.PlayerObject
    //}
  }
  /*
  [ServerRpc(RequireOwnership = false)]
  void SubmitSpawnRequestServerRpc(ServerRpcParams rpcParams = default) {
    Debug.Log("got request");
    SpawnPlayers(rpcParams.Receive.SenderClientId);
  }

  void SpawnPlayers(ulong newClientId) {
    GameObject go = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
    go.GetComponent<NetworkObject>().SpawnAsPlayerObject(newClientId, true);
    
  }
 */

  void SpawnChain(GameObject playerA, GameObject playerB) {
    chain = (GameObject)Instantiate(chainPrefab, Vector3.zero, Quaternion.identity);
    //var hingeA = chain.GetComponents<ConfigurableJoint>()[0];//chain.AddComponent<ConfigurableJoint>();
    //var hingeB = chain.GetComponents<ConfigurableJoint>()[1];//chain.AddComponent<ConfigurableJoint>();
    //hingeA.connectedBody = playerA.gameObject.GetComponent<Rigidbody>();
    //hingeB.connectedBody = playerB.gameObject.GetComponent<Rigidbody>();
    //hingeA.connectedAnchor = playerA.transform.position;
    var chainScript = chain.GetComponent<RopeVerletScript>();
    chainScript.startAnchor = playerA.transform;
    chainScript.endAnchor = playerB.transform;
    
    var hingeA = playerA.gameObject.GetComponent<SpringJoint>();//chain.AddComponent<ConfigurableJoint>();
    hingeA.connectedBody = playerB.GetComponent<Rigidbody>();
    hingeA.spring = 150f;
    hingeA.damper = 0.1f;
    var hingeB = playerB.gameObject.GetComponent<SpringJoint>();//chain.AddComponent<ConfigurableJoint>();
    hingeB.connectedBody = playerA.gameObject.GetComponent<Rigidbody>();
    hingeB.spring = 150f;
    hingeB.damper = 0.1f;

    //hingeA.linearLimitSpring.
  }
  // Update is called once per frame
  void Update() {
    if (true || IsServer) {
      var clientObjects = FindObjectsOfType<PlayerScript>().Select(x=>x.gameObject);
      if (playerInstances == null || playerInstances.Count != clientObjects.Count()) playerInstances = new List<GameObject>(clientObjects);
    }
    //Debug.Log(playerInstances.Count);
    /*
    if (IsServer && NetworkManager.Singleton.ConnectedClients.Count >= 2) {
      
      SpawnChain(l[0], l[1]);
    }
    */
    
    if (playerInstances.Count >= 2 && chain == null) {
      SpawnChain(playerInstances[0], playerInstances[1]);
    }

    if (IsClient && playerClientInstance == null && NetworkManager.LocalClient.PlayerObject!=null) {
      playerClientInstance = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
    }
  }
}