using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] ArServerController            ServerController;
    [SerializeField] ArClientCloudAnchorController ClientController;

    public void GotoMainScene()
    {
      Application.Quit(0);
    }

    public void StartServer()
    {
        if (ClientController != null && ClientController.isActiveAndEnabled)
        {
            DeStopClient();
            Invoke("StartServer", Time.deltaTime * 3);
            return;
        }


        if (!ServerController.isActiveAndEnabled)
        {
            ServerController.gameObject.SetActive(true);
        }
    }

    public void StopServer()
    {
        ServerController.gameObject.SetActive(false);
    }

    public void ToggleServer()
    {
        if (ServerController != null && !  ServerController.isActiveAndEnabled)
        {
            DeStopClient();
        }

        ServerController.gameObject.SetActive( !  ServerController.gameObject.active);
    }

    void DeStopClient()
    {
        //  SceneManager.LoadSceneAsync(0).allowSceneActivation = true;
        if (ClientController != null)
        {
            Destroy(ClientController.gameObject);
        }
    }

    void DeStopServer()
    {
        // SceneManager.LoadSceneAsync(0).allowSceneActivation=true;
        if (ServerController != null)
        {
            Destroy(ServerController.gameObject);
        }
    }

    public void StartClient()
    {
        if (ClientController != null && !  ClientController.isActiveAndEnabled)
        {
            DeStopServer();
            Invoke("StartClient", Time.deltaTime * 3);
        } else
        {
            if (ClientController != null)
            {
                ClientController.gameObject.SetActive(true);
            }
        }
    }

    public void StopClient()
    {
        if (ClientController != null)
        {
            ClientController.gameObject.SetActive(false);
        }
    }

    public void ToggleClient()
    {
        if (ClientController != null && !  ClientController.gameObject.active)
        {
            DeStopServer();
            Invoke("StartClient", Time.deltaTime * 3);
        } else
        {
            if (ClientController != null)
            {
                ClientController.gameObject.SetActive(!  ClientController.gameObject.active);
            }
        }
    }
}