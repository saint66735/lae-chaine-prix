using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class GameNetworkManager : NetworkBehaviour {
  // Start is called before the first frame update
  public List<GameObject> playerPrefabs;
  public GameObject chainPrefab;
  public GameObject playerClientInstance;
  public static GameNetworkManager instance;
  //public List<NetworkObject> playerInstances;
  public List<GameObject> otherPlayerObjects;
  public List<NetworkObject> playerInstances;
  public GameObject chain;
  public List<Transform> spawnPoints;
  public UI_Manager UIManagerScript;
  public float raceTime = 0;
  public NetworkVariable<bool> raceStarted = new NetworkVariable<bool>(false);
  public bool isFreeroam = false;

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
      UIManagerScript.OnHost();
      //networkManager.StartHost();
      //UIManagerScript.closePanels();
    }
    else if (mppmTag.Contains("Client")) {
      UIManagerScript.OnJoin();
      //networkManager.StartClient();
      //UIManagerScript.closePanels();
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
      defaultAprovalCallback = NetworkManager.Singleton.ConnectionApprovalCallback;
      NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;
    }
    if (IsClient) {
      if (isFreeroam) {
        Camera.main.GetComponent<FreeCamera>().enabled = true;
        Camera.main.GetComponent<CameraScript>().enabled = false;
      }
      else {
        SpawnServerRpc(NetworkManager.LocalClient.ClientId);
      }

      
    }
  }

  public void RaceWin(ulong winnerID) {
    //NetworkManager.ConnectedClients[winnerID]
    WinClientRpc(winnerID);
  }
  
  
  [ClientRpc]
  void WinClientRpc(ulong winnerID) {
    var txt = winnerID == NetworkManager.LocalClientId ? "You won!" : "You lost!";
    UIManagerScript.ShowWinner(txt);
  }

  void StartRace() {
    raceStarted.Value = true;
    
    StartClientRpc();
  }
  [ClientRpc]
  void StartClientRpc() {
    UIManagerScript.timeLeft = 6f;
  }

  [ServerRpc(RequireOwnership = false)]
  void SpawnServerRpc(ulong id, ServerRpcParams rpcParams = default) {
    var playerPrefab = playerPrefabs[Random.Range(0, playerPrefabs.Count)];
    GameObject go = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
    go.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
  }
  private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
  {
    Debug.Log("conection aproved");
    
    
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

  void SpawnChain(NetworkObject playerA, NetworkObject playerB) {
   
    //var hingeA = chain.GetComponents<ConfigurableJoint>()[0];//chain.AddComponent<ConfigurableJoint>();
    //var hingeB = chain.GetComponents<ConfigurableJoint>()[1];//chain.AddComponent<ConfigurableJoint>();
    //hingeA.connectedBody = playerA.gameObject.GetComponent<Rigidbody>();
    //hingeB.connectedBody = playerB.gameObject.GetComponent<Rigidbody>();
    //hingeA.connectedAnchor = playerA.transform.position;
    
    
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

  void SpawnVisualChain(GameObject playerA, GameObject playerB) {
    chain = (GameObject)Instantiate(chainPrefab, Vector3.zero, Quaternion.identity);
    var chainScript = chain.GetComponent<RopeVerletScript>();
    chainScript.startAnchor = playerA.transform;
    chainScript.endAnchor = playerB.transform;
  }
  // Update is called once per frame
  void Update() {
    if (true || IsServer) {
      var _otherPlayerObjects = FindObjectsOfType<PlayerScript>().Select(x=>x.gameObject);
      if (otherPlayerObjects == null || _otherPlayerObjects.Count() != otherPlayerObjects.Count()) otherPlayerObjects = new List<GameObject>(_otherPlayerObjects);
    }
    if (IsServer) {
      var _playerInstances = NetworkManager.Singleton.ConnectedClients
        .Select(x => x.Value.PlayerObject).Where(x=>x!=null).AsReadOnlyList();
      if (playerInstances == null || playerInstances.Count != _playerInstances.Count()) playerInstances = new List<NetworkObject>(_playerInstances);
    }
    //Debug.Log(playerInstances.Count);
    /*
    if (IsServer && NetworkManager.Singleton.ConnectedClients.Count >= 2) {
      
      SpawnChain(l[0], l[1]);
    }
    */
    
    if (playerInstances.Count >= 2) {
      if(chain == null)SpawnChain(playerInstances[0], playerInstances[1]);
      if(!raceStarted.Value) StartRace();
    }
    if (otherPlayerObjects.Count >= 2 && chain == null) {
      SpawnVisualChain(otherPlayerObjects[0], otherPlayerObjects[1]);
    }
    

    
    if (raceStarted.Value == true) raceTime += Time.deltaTime;

    if (IsClient && playerClientInstance == null && NetworkManager.LocalClient.PlayerObject!=null) {
      playerClientInstance = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
    }
  }
  
}