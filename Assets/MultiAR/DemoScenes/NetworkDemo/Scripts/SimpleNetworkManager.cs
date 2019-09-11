using System;
using UnityEngine;
using UnityEngine.Networking;

public class SimpleNetworkManager:NetworkManager
{

    /// <summary>
    /// Action which get called when the client connects to a server.
    /// </summary>
    public event Action OnClientConnected;

    /// <summary>
    /// Action which get called when the client disconnects from a server.
    /// </summary>
    public event Action OnClientDisconnected;

    /// <summary>
    /// Called on the client when connected to a server.
    /// </summary>
    /// <param name="conn">Connection to the server.</param>
#pragma warning disable 618
    public override void OnClientConnect(NetworkConnection conn)
#pragma warning restore 618
    {
        base.OnClientConnect(conn);
        Debug.Log("Successfully connected to server: " + conn.lastError);
        if (OnClientConnected != null)
        {
            OnClientConnected();
        }
    }

    /// <summary>
    /// Called on the client when disconnected from a server.
    /// </summary>
    /// <param name="conn">Connection to the server.</param>
#pragma warning disable 618
    public override void OnClientDisconnect(NetworkConnection conn)
#pragma warning restore 618
    {
        base.OnClientDisconnect(conn);
        Debug.Log("Disconnected from the server: " + conn.lastError);
        if (OnClientDisconnected != null)
        {
            OnClientDisconnected();
        }
    }
	
}