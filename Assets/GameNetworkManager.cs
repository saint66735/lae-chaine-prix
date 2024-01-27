using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;

using UnityEngine;

public class GameNetworkManager : NetworkBehaviour {
  // Start is called before the first frame update
  public GameObject playerPrefab;
  public GameObject playerClientInstance;
  public static GameNetworkManager instance;
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
  }
  
  [ServerRpc(RequireOwnership = false)]
  void SubmitSpawnRequestServerRpc(ServerRpcParams rpcParams = default) {
    Debug.Log("got request");
    SpawnPlayers(rpcParams.Receive.SenderClientId);
  }

  void SpawnPlayers(ulong newClientId) {
    GameObject go = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
    go.GetComponent<NetworkObject>().SpawnAsPlayerObject(newClientId, true);
    
  }


  // Update is called once per frame
  void Update() {
    if (IsClient && playerClientInstance == null && NetworkManager.LocalClient.PlayerObject!=null) {
      playerClientInstance = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
    }
  }
}