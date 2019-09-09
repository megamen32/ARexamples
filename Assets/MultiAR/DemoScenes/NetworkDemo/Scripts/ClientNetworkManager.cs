using System;
using UnityEngine.Networking;

/// <summary>
/// ArClient's NetworkManager component
/// </summary>
/*
public class ClientNetworkManager : NetworkManager
{

    public ArClientBaseController arClient;
    public event Action           OnClientConnected;
    */

    /// <summary>
    /// Action which get called when the client disconnects from a server.
    /// </summary>
    

    /*public override void OnClientConnect(NetworkConnection conn)
    {
        //Debug.Log ("OnClientConnect");
        base.OnClientConnect(conn);

        if (arClient != null) 
        {
            arClient.OnClientConnect(conn);
        }
    }


    public override void OnClientDisconnect(NetworkConnection conn)
    {
        //Debug.Log ("OnClientDisconnect");

        if (arClient != null) 
        {
            arClient.OnClientDisconnect(conn);
        }

        base.OnClientDisconnect(conn);
    }


    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        base.OnClientError(conn, errorCode);

        if (arClient != null) 
        {
            arClient.OnNetworkError(conn, errorCode);
        }
    }

}*/