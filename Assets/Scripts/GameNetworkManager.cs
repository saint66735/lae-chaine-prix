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
  public List<NetworkObject> playerInstances;
  public GameObject chain;
  public List<Transform> spawnPoints;

  private Action<NetworkManager.ConnectionApprovalRequest, NetworkManager.ConnectionApprovalResponse> defaultAprovalCallback;
  void Start() {
    instance = this;
    var mppmTag = CurrentPlayer.ReadOnlyTags().First();
    Debug.Log(mppmTag);
    var networkManager = NetworkManager.Singleton;
    
    if (mppmTag.Contains("Server")) {
      networkManager.StartServer();
    }
    else if (mppmTag.Contains("Host")) {
      networkManager.StartHost();
    }
    else if (mppmTag.Contains("Client")) {
      networkManager.StartClient();
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
    hingeA.spring = 80f;
    hingeA.damper = 0;
    var hingeB = playerB.gameObject.GetComponent<SpringJoint>();//chain.AddComponent<ConfigurableJoint>();
    hingeB.connectedBody = playerA.gameObject.GetComponent<Rigidbody>();
    hingeB.spring = 80f;
    hingeB.damper = 0;

    //hingeA.linearLimitSpring.
  }
  // Update is called once per frame
  void Update() {
    if (IsServer) {
      var clientObjects = NetworkManager.Singleton.ConnectedClients.Select(x => x.Value.PlayerObject).AsReadOnlyList();
      if (playerInstances == null || playerInstances.Count != clientObjects.Count) playerInstances = new List<NetworkObject>(clientObjects);
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