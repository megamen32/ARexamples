using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] ArServerController ServerController;
    [SerializeField] ArClientCloudAnchorController ClientController;

    public void GotoMainScene()
        {
            SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex+1)%SceneManager.sceneCount);
        }

        public void StartServer()
        {
          ServerController.gameObject.SetActive(true);
        }

        public void StopServer()
        {
            ServerController.gameObject.SetActive(false);
        }

        public void ToggleServer()
        {
            ServerController.gameObject.SetActive( !  ServerController.gameObject.active);
        }

        public void StartClient()
        {
            ClientController.gameObject.SetActive(true);
        }

        public void StopClient()
        {
            ClientController.gameObject.SetActive(false);
        }

        public void ToggleClient()
        {
            ClientController.gameObject.SetActive( !  ClientController.gameObject.active);
        }
      
    
}
