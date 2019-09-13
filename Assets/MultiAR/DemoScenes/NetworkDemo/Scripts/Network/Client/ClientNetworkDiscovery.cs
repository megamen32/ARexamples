using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ArClient's NetworkDiscovery component
/// </summary>
public class ClientNetworkDiscovery : NetworkDiscovery
{

    public ArClientBaseController arClient;


    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        if (string.IsNullOrEmpty(data))
            return;

        // split the data
        string[] items = data.Split(':');
        if (items == null || items.Length < 3)
            return;

        if (arClient != null && items[0] == arClient.gameName && 
            (arClient.serverHost == "0.0.0.0" || string.IsNullOrEmpty(arClient.serverHost)))
        {
            Debug.Log("GotBroadcast: " + data);

            arClient.serverHost = items[1];
            arClient.serverPort = int.Parse(items [2]);
            //this.StopBroadcast();

            arClient.ConnectToServer();
        }
    }

}