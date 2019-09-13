﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Net;
using GoogleARCore.Examples.CloudAnchors;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using PlayerController = UnityEngine.Networking.PlayerController;

public partial class ArServerController : MonoBehaviour
{
    [Tooltip("The name of the AR-game (used by client-server and broadcast messages).")]
    public string gameName = "ArGame";

    [Tooltip("Port to be used for incoming connections.")]
    public int listenOnPort = 7777;

    //	[Tooltip("Port used for server broadcast discovery.")]
    //	public int broadcastPort = 7779;

    //  [Tooltip("Maximum number of allowed connections.")]
    // public int maxConnections = 8;

    // [Tooltip("Whether the server should use websockets or not.")]
    //public bool useWebSockets = false;

    // [Tooltip("Registered player prefab.")] public GameObject playerPrefab;

    [Tooltip("UI-Text to display connection status messages.")]
    public Text connStatusText;

    [Tooltip("UI-Text to display anchor status messages.")]
    public Text anchorStatusText;

    [Tooltip("UI-Text to display server status messages.")]
    public Text serverStatusText;

    [Tooltip("UI-Text to display server console.")]
    public Text consoleMessages;

    [Tooltip("Whether to show debug messages.")]
    public bool showDebugMessages;

    // number of connections
    [HideInInspector] public int numConnections = 0;

    // reference to the network components
    private ServerNetworkManager netManager = null;
    //private NetworkDiscovery netDiscovery = null;


    // api key needed for cloud anchor hosting or resolving
    //private string cloudApiKey = string.Empty;

    // timeout constants in seconds
    private const float GameAnchorTimeout    = 24 * 3600f; // how long to keep the anchor
    private const float AnchorHostingTimeout = 60f;        // how long to wait for anchor hosting

    // cloud anchor Id
    private string    gameCloudAnchorId   = string.Empty;
    private Transform gameAnchorTransform = null;
    private float     gameAnchorTimestamp = 0f;

    // game anchor data, if any
    private byte[] gameAnchorData = null;

    // id of the hosting-requesting cloud-anchor client 
    private int   hostingClientId        = -1;
    private float hostingClientTimestamp = 0f;


    /// <summary>
    /// Gets the anchor transform.
    /// </summary>
    /// <returns>The anchor transform.</returns>
    public Transform GetAnchorTransform()
    {
        if (gameAnchorTransform)
        {
            return gameAnchorTransform.parent ? gameAnchorTransform.parent : gameAnchorTransform;
        }

        return null;
    }

    NetworkManager                                manager;
    string                                        m_CurrentRoomNumber;
    CloudAnchorsExampleController.ApplicationMode m_CurrentMode = CloudAnchorsExampleController.ApplicationMode.Ready;
    bool                                          m_IsQuitting  = false;

    public void OnCreateRoomClicked()
    {
        m_CurrentMode = CloudAnchorsExampleController.ApplicationMode.Hosting;
        netManager.matchMaker.CreateMatch(netManager.matchName, netManager.matchSize, true, string.Empty, string.Empty, string.Empty, 0, 0, _OnMatchCreate);

     
    }

    private void _OnConnectedToServer()
    {
        if (m_CurrentMode == CloudAnchorsExampleController.ApplicationMode.Hosting)
        {
            LogToConsole("Find a plane, tap to create a Cloud Anchor.");
        } else if (m_CurrentMode == CloudAnchorsExampleController.ApplicationMode.Resolving)
        {
            LogToConsole("Waiting for Cloud Anchor to be hosted...");
        } else
        {
            LogToConsole("Connected to server with neither Hosting nor Resolving mod\n Please start the app again.");
        }
    }

    private void _OnDisconnectedFromServer()
    {
        _QuitWithReason("Network session disconnected! " + "Please start the app again and try another room.");
    }

    /// <summary>                                                            
    /// Quits the application after 5 seconds for the toast to appear.       
    /// </summary>                                                           
    /// <param name="reason">The reason of quitting the application.</param> 
    private void _QuitWithReason(string reason)
    {
        if (m_IsQuitting)
        {
            return;
        }

        LogToConsole(reason);
        m_IsQuitting = true;
        Invoke("_DoQuit", 5.0f);
    }

    /// <summary>                        
    /// Actually quit the application.   
    /// </summary>                       
    private void _DoQuit()
    {
        Application.Quit();
    }

    NetworkDiscovery            netDiscovery  ;
    [SerializeField] bool       useWebSockets;
    [SerializeField] GameObject playerPrefab;

    void Start ()
    {
        gameObjects = new List<GameObject>(MaxObjectCount);
        try
        {
            // setup network manager component
            netManager = GetComponent<ServerNetworkManager>();
            if (netManager == null)
            {
                netManager                      =  gameObject.AddComponent<ServerNetworkManager>();
                netManager.OnClientConnected    += _OnConnectedToServer;
                netManager.OnClientDisconnected += _OnDisconnectedFromServer;
            }

            // start the server
            if (netManager == null)
            {
                LogErrorToConsole("Server disapear");
                return;

            }

            netManager.arServer = this;

            netManager.networkPort   = listenOnPort;
            netManager.useWebSockets = useWebSockets;

            if (playerPrefab != null)
            {
                netManager.playerPrefab = playerPrefab;
            }

            // configure the network server
#pragma warning disable 618
            var config = new ConnectionConfig();
#pragma warning restore 618
            config.AddChannel(QosType.ReliableSequenced);
            config.AddChannel(QosType.Unreliable);


            /// <summary>
            /// Handles the user intent to create a new room.
            /// </summary>


            NetworkServer.RegisterHandler(NetMsgType.GetGameAnchorRequest,   OnGetGameAnchorRequest);
            NetworkServer.RegisterHandler(NetMsgType.CheckHostAnchorRequest, OnCheckHostAnchorRequest);
            NetworkServer.RegisterHandler(NetMsgType.SetGameAnchorRequest,   OnSetGameAnchorRequest);
            NetworkServer.RegisterHandler(NetMsgType.HandleSyncTransform,    ArSyncTransform.HandleSyncTransform);
            NetworkServer.RegisterHandler(NetMsgType.AttackRequest,          OnAttackRequest);
            NetworkServer.RegisterHandler(NetMsgType.SetGameObjectRequest,   OnSpawnObject);

            // get server ip address
#if !UNITY_WSA
            string serverHost = GetDeviceIpAddress();
#else
			string serverHost = "127.0.0.1";
#endif




            netManager.StartMatchMaker();
            netManager.matchMaker.ListMatches(startPageNumber: 0, resultPageSize: 5, matchNameFilter: string.Empty, filterOutPrivateMatchesFromResults: false,
                eloScoreTarget: 0, requestDomain: 0, callback: _OnMatchList);



             string sMessage = gameName + "-Server started on " + serverHost + ":" + listenOnPort;
           LogToConsole(sMessage);

            if (serverStatusText)
            {
                serverStatusText.text = sMessage;
            }

            // show current connections
            LogConnections();
        } catch (System.Exception ex)
        {
             LogErrorToConsole(ex.Message + "\n" + ex.StackTrace);

            if (serverStatusText)
            {
                serverStatusText.text = ex.Message;
            }
        }
    }




    void _OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> responseData)
    {
        if (success && responseData.Count > 0)
        {
            netManager.OnMatchList(success, extendedInfo, responseData);

            _OnJoinRoomClicked(responseData.OrderByDescending(snapshot => snapshot.currentSize).
                                            First(delegate (MatchInfoSnapshot snapshot) { return snapshot.currentSize < snapshot.maxSize ; }));
        } else
        {
            OnCreateRoomClicked();
        }
    }

    /// <summary>
    /// Handles the user intent to join the room associated with the button clicked.
    /// </summary>
    /// <param name="match">The information about the match that the user intents to
    /// join.</param>
#pragma warning disable 618
    private void _OnJoinRoomClicked(MatchInfoSnapshot match)
#pragma warning restore 618
    {
        m_CurrentMode        = CloudAnchorsExampleController.ApplicationMode.Resolving;
        netManager.matchName = match.name;
        netManager.matchMaker.JoinMatch(match.networkId, string.Empty, string.Empty, string.Empty, 0, 0, _OnMatchJoined);
        LogToConsole( "\n" + _GetRoomNumberFromNetworkId(match.networkId) + " " + match);
        LogToConsole( "Connecting to server...");
    }

    void _OnMatchJoined(bool success, string extendedinfo, MatchInfo matchInfo)
    {
        netManager.OnMatchJoined(success, extendedinfo, matchInfo);
        if (!success)
        {
            LogToConsole(  "Could not join to match: " + extendedinfo);
            return;
        }


        m_CurrentRoomNumber = _GetRoomNumberFromNetworkId(matchInfo.networkId);
        LogToConsole( "Connecting to server...");

        LogToConsole( "Room: " + m_CurrentRoomNumber);
    }

    void _OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        netManager.OnMatchCreate(success, extendedInfo, matchInfo);

        if (!success)
        {
            LogErrorToConsole( "Could not create match: " + extendedInfo);
            Start();
            return;
        }


#pragma warning restore 618

        if (netManager.matches != null)
        {
            //  var matchInfoSnapshot = netManager.matches.First(snapshot => snapshot.networkId == matchInfo.networkId);
            //  LogToConsole( "Room " + _GetRoomNumberFromNetworkId(matchInfoSnapshot.networkId));
            //netManager.matchName = matchInfo.address;
            // netManager.matchMaker.JoinMatch(matchInfo.networkId, string.Empty, string.Empty, string.Empty, 0, matchInfo.domain, _OnMatchJoined);
            LogErrorToConsole("NO ROOMS ANYWHERE");
        } else
        {
        }


#if !UNITY_EDITOR_64
ToggleServerUI(false);
#endif
    }

    void ToggleServerUI(bool visible)
    {
        //   consoleMessages.gameObject.SetActive(visible);
    }

    private string _GetRoomNumberFromNetworkId(NetworkID networkID)
    {
        return (System.Convert.ToInt64(networkID.ToString()) % 10000).ToString();
    }


    void OnDestroy()
    {
        // shutdown the server and disconnect all clients
        if (netManager!=null&&NetworkServer.active && netManager.isNetworkActive)
        {
            netManager.StopServer();
            if (netDiscovery != null && netDiscovery.hostId != -1) netDiscovery.StopBroadcast();
        }

        string sMessage = gameName + "-Server stopped.";
         LogDebugToConsole(sMessage);
    }


    void Update ()
    {
        // check for waiting too long for anchor hosting
        if (hostingClientId >= 0 && hostingClientTimestamp > 0f && Time.realtimeSinceStartup > (hostingClientTimestamp + AnchorHostingTimeout))
        {
            //hostingClientId = -1;
            hostingClientTimestamp = 0f;

            LogDebugToConsole("Hosting client timed out.");
        }

        // check for anchor timeout
        if (!string.IsNullOrEmpty(gameCloudAnchorId) && Time.time > (gameAnchorTimestamp + GameAnchorTimeout))
        {
            gameCloudAnchorId   = string.Empty;
            gameAnchorTransform = null;
            gameAnchorTimestamp = 0f;

            LogToConsole("Game anchor timed out.");

            if (anchorStatusText)
            {
                anchorStatusText.text = string.Empty;
            }
        }
    }

#if !UNITY_WSA
    // Gets the device IP address.
    private string GetDeviceIpAddress()
    {
        string ipAddress = "127.0.0.1";

#if UNITY_2018_2_OR_NEWER
        string      hostName  = Dns.GetHostName();
        IPAddress[] addresses = Dns.GetHostAddresses(hostName);

        foreach (IPAddress address in addresses)
        {
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ipAddress = address.ToString();
                break;
            }
        }
#else
        ipAddress = Network.player.ipAddress;
#endif

        return ipAddress;
    }
#endif

#region Battle

    void OnAttackRequest(NetworkMessage netMsg)
    {
        //TODO some checks for cheating
        UpdateAttack(netMsg);
    }

    public IEnumerable<UnityEngine.Networking.PlayerController> playerListDynamic
    {
        get
        {
            var numPlayers = 0;

            foreach (var conn in NetworkServer.connections)
            {
                if (conn == null) continue;

                foreach (var t in conn.playerControllers.Where(t => t.IsValid))
                {
                    yield return t;
                }
            }
        }
    }

    public UnityEngine.Networking.PlayerController[] NetPlayerList => playerListDynamic.ToArray();

    public global::ClientPlayerController[] PlayerList => netToPlayerControllers.
                                                    Where(n => playerListDynamic.Contains<UnityEngine.Networking.PlayerController>(n.Key)).Select(x => x.Value).
                                                    ToArray();

    Dictionary<PlayerController, ClientPlayerController> netToPlayerControllers;
    IEnumerable<GameObject> gameObjects;
    int ObjCount=>gameObjects.Count();
    [SerializeField] int MaxObjectCount=300;
    [SerializeField] float maxDenisty = .5f;

    public global::ClientPlayerController[] GetPlayerList => NetPlayerList.
                                                       Where(x => netToPlayerControllers != null && (x.IsValid && (  !(netToPlayerControllers[x] is null)))).
                                                       Select(x => netToPlayerControllers[x]).ToArray();


    public void UpdateAttack (NetworkMessage _packet)
    {
        // Loop through all the players
       // var       NetPlayerList = this.GetPlayerList;
        AttackMSG attack        = _packet.ReadMessage<AttackMSG>();

        var networkInstanceId = new NetworkInstanceId(attack.netId);//TODO add dictinary init register clients controllers

        if (netToPlayerControllers == null)
        {
            netToPlayerControllers = _packet.conn.playerControllers.ToDictionary(
                x => x,controller => controller.gameObject.GetComponent<ClientPlayerController>());

        }

        var player = netToPlayerControllers[ _packet.conn.playerControllers.Find(x => x.unetView.netId == networkInstanceId)];
       
        switch (attack.attackType)
        {
            case        AttackMSG.AttackType.Blast:
                player.Attack.RpcDoBlast(attack.attackMode);
                break;

            case AttackMSG.AttackType.Bullet:
                player.Attack.RpcDoBullet(attack.attackMode);
                break;
            case AttackMSG.AttackType.Shield:
                player.Attack.RpcDoShield(attack.attackMode);
                break;
            default:
                LogErrorToConsole("Can't response message'"+attack.attackMode+" , "+attack.attackType);
                break;
        }

        NetworkServer.SendToClient(_packet.conn.connectionId, NetMsgType.AttackResponse , new ReadyMessage());
    }

#endregion

#region Buildings

    private void OnSpawnObject(NetworkMessage netMsg)
    {
        var request = netMsg.ReadMessage<SetGameObjectRequestMsg>();

        if (request == null || request.gameName != gameName) return;

        bool requestGranted = !string.IsNullOrEmpty(gameCloudAnchorId);
        SetGameObjectResponseMsg response     ;
        bool denistyisOk=false   ;
        if (requestGranted&& ObjCount < MaxObjectCount)
        {
            gameObjects = gameObjects.OrderBy((obj =>
                                               {
                                                   var distance = Vector3.Distance(request.anchorPos, obj.transform.position);
                                                   return distance > obj.transform.lossyScale.sqrMagnitude ? distance : 2 * distance;
                                               }));

            denistyisOk = gameObjects.Take((int) Math.Ceiling(1 + ObjCount * .3f)).Average(x => x.transform.lossyScale.magnitude) < maxDenisty;
        }

        response = new SetGameObjectResponseMsg {confirmed = requestGranted&&denistyisOk};

       

        if (response.confirmed)
        {

            var spawnPrefab = manager.spawnPrefabs[Math.Abs((int) (uint) request.model % (manager.spawnPrefabs.Count - 1))];
            if (anchorStatusText)
            {
                anchorStatusText.text = "Shared world anchor: " + gameCloudAnchorId;
            }

            var go =Instantiate(spawnPrefab);

            go.transform.SetParent(gameAnchorTransform);
            go.transform.localPosition =request.anchorPos;
            go.transform.localRotation = request.anchorRot;
            NetworkServer.SpawnWithClientAuthority(spawnPrefab , netMsg.conn);
        }

        NetworkServer.SendToClient(netMsg.conn.connectionId, NetMsgType.SetGameObjectResponse , response);
        
        int connId = netMsg.conn.connectionId;
        LogDebugToConsole($"Get request to build {request.model.ToString()} received from client " + connId + "");

        if (!string.IsNullOrEmpty(gameCloudAnchorId))
        {
            LogToConsole("  Got anchor by client " + connId);
        }
    }

#endregion
#region ResolveAnchors

    void OnGetGameAnchorRequest(NetworkMessage netMsg)
    {
        var request = netMsg.ReadMessage<GetGameAnchorRequestMsg>();
        if (request == null || request.gameName != gameName) return;

        GetGameAnchorResponseMsg response = new GetGameAnchorResponseMsg
        {
            found    = !string.IsNullOrEmpty(gameCloudAnchorId),
            anchorId = gameCloudAnchorId,
            anchorData = gameAnchorData
        };

        NetworkServer.SendToClient(netMsg.conn.connectionId, NetMsgType.GetGameAnchorResponse, response);

        int connId = netMsg.conn.connectionId;
        LogDebugToConsole("GetGameAnchor received from client " + connId + ", anchorId: " + gameCloudAnchorId);

        if (!string.IsNullOrEmpty(gameCloudAnchorId))
        {
            LogToConsole("  Got anchor by client " + connId);
        }
    }
    


    // handles CheckHostAnchorRequest
    private void OnCheckHostAnchorRequest(NetworkMessage netMsg)
    {
        var request = netMsg.ReadMessage<CheckHostAnchorRequestMsg>();
        if (request == null || request.gameName != gameName) return;

        bool requestGranted = string.IsNullOrEmpty(gameCloudAnchorId) && (hostingClientTimestamp == 0f ||
                                                                          Time.realtimeSinceStartup > (hostingClientTimestamp + AnchorHostingTimeout));

        if (!requestGranted)
        {
            // check for last-granted client
            if (string.IsNullOrEmpty (gameCloudAnchorId) && (hostingClientId == netMsg.conn.connectionId))
            {
                requestGranted = true;
            }
        }

        if (requestGranted)
        {
            // save last-granted client & time
            hostingClientId        = netMsg.conn.connectionId;
            hostingClientTimestamp = Time.realtimeSinceStartup;
        }

        CheckHostAnchorResponseMsg response = new CheckHostAnchorResponseMsg
        {
            granted = requestGranted,
            //apiKey = cloudApiKey
        };

        NetworkServer.SendToClient(netMsg.conn.connectionId, NetMsgType.CheckHostAnchorResponse, response);

        int connId = netMsg.conn.connectionId;
        LogDebugToConsole("CheckHostAnchor received from client " + connId);
    }


    // handles SetGameAnchorRequest
    void OnSetGameAnchorRequest(NetworkMessage netMsg)
    {
        var request = netMsg.ReadMessage<SetGameAnchorRequestMsg>();
        if (request == null || request.gameName != gameName) return;

        bool requestConfirmed = !string.IsNullOrEmpty(request.anchorId) && hostingClientId == netMsg.conn.connectionId;

        if (requestConfirmed)
        {
            hostingClientId = -1;

            gameCloudAnchorId   = request.anchorId;
            gameAnchorTimestamp = Time.realtimeSinceStartup;

            gameAnchorData = request.anchorData;

            if (anchorStatusText)
            {
                anchorStatusText.text = "Shared world anchor: " + gameCloudAnchorId;
            }

            GameObject gameAnchorGo = new GameObject("GameAnchor-" + gameCloudAnchorId);
            gameAnchorTransform = gameAnchorGo.transform;

            gameAnchorTransform.position = request.anchorPos;
            gameAnchorTransform.rotation = request.anchorRot;

            GameObject anchoredCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            anchoredCube.transform.SetParent(gameAnchorTransform);

            anchoredCube.transform.localPosition = Vector3.zero;
            anchoredCube.transform.localRotation = Quaternion.identity;
            anchoredCube.transform.localScale    = new Vector3(0.1f, 0.1f, 0.1f);
        }

        SetGameAnchorResponseMsg response = new SetGameAnchorResponseMsg {confirmed = requestConfirmed,};

        NetworkServer.SendToClient(netMsg.conn.connectionId, NetMsgType.SetGameAnchorResponse, response);

        int connId = netMsg.conn.connectionId;
        LogDebugToConsole("SetGameAnchor received from client " + connId + ", anchorId: " + request.anchorId);

        LogToConsole("  Set anchor by client " + connId);
    }

#endregion


#region Logging

    // updates the connection message
    public void LogConnections()
    {
        if (connStatusText)
        {
            connStatusText.text = numConnections.ToString() + " player(s) connected.";
        }
    }

    // adds the message to console
    private void AddToConsole(string sMessage)
    {
        if (consoleMessages)
        {
            consoleMessages.text += "\r\n" + sMessage;

            // scroll to end
            ScrollRect scrollRect = consoleMessages.gameObject.GetComponentInParent<ScrollRect>();
            if (scrollRect)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalScrollbar.value = 0f;
                Canvas.ForceUpdateCanvases();
            }
        }
    }

    // logs message to console
    public void LogToConsole(string sMessage)
    {
        Debug.Log(sMessage);
        AddToConsole(sMessage);
    }

    // logs error message to console
    public void LogErrorToConsole(string sMessage)
    {
        Debug.LogError(sMessage);
        AddToConsole(sMessage);
    }

    // logs error message to console
    public void LogErrorToConsole(Exception ex)
    {
        Debug.LogError(ex.Message + "\n" + ex.StackTrace);
        AddToConsole(ex.Message);
    }

    // logs debug message to console
    public void LogDebugToConsole(string sMessage)
    {
        Debug.Log(sMessage);

        if (showDebugMessages)
        {
            AddToConsole(sMessage);
        }
    }

#endregion
}

