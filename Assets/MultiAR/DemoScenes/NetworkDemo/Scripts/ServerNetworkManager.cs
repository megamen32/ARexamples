using System;
using UnityEngine.Networking;

/// <summary>
/// ArServer's NetworkManager component
/// </summary>
public class ServerNetworkManager : NetworkManager
{
    public ArServerController arServer;
    public event Action<NetworkConnection>       OnClientConnected;

    /// <summary>
    /// Action which get called when the client disconnects from a server.
    /// </summary>
    public event Action<NetworkConnection> OnClientDisconnected;


    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);

        if (arServer != null)
        {
            int connId = conn.connectionId;
            arServer.LogToConsole("Connected client " + connId + ", IP: " + conn.address);
            arServer.numConnections++;
            OnClientConnected?.Invoke(conn);
            arServer.LogConnections();
        }
    }


    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (arServer != null)
        {
            int connId = conn.connectionId;
            arServer.LogToConsole("Disconnected client " + connId);

            arServer.numConnections--;

            OnClientDisconnected?.Invoke(conn);
            arServer.LogConnections();
        }

        base.OnServerDisconnect(conn);
    }


    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        base.OnServerError(conn, errorCode);

        if (arServer != null)
        {
            int connId = conn.connectionId;
            arServer.LogErrorToConsole("NetError " + connId + " detected: " + (NetworkError) errorCode);
        }
    }
}