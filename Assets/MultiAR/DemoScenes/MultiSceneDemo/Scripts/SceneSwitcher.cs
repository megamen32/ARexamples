using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
   
        public void GotoMainScene()
        {
            SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex+1)%SceneManager.sceneCount);
        }

        public void GotoMenuScene()
        {
            SceneManager.LoadScene(Mathf.Max(0,SceneManager.GetActiveScene().buildIndex - 1) % SceneManager.sceneCount);
        }
    
}
