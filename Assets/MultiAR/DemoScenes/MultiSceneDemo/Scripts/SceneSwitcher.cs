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
}