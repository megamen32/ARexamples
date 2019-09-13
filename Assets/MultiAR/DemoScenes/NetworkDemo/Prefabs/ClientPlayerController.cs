using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public static class Utility
{
    public static IEnumerable<TOut> ForEach< TOut>(this IEnumerable x, Func<object, TOut> func)
    {
        for (var e = x.GetEnumerator(); e.MoveNext(); )
        {
            yield return func(e.Current);
        }
    }
}

public class ClientPlayerController : NetworkBehaviour // MonoBehaviour
{
    // reference to the ar-client
    private ArClientBaseController arClient  ;
    private Camera                 arMainCamera  ;


    // reference to the Multi-AR manager
    private MultiARManager arManager     ;
    public  GameObject     bulletPrefab;

    public AttackComponent Attack => GetComponent<AttackComponent>();

    public Transform bodyRoot => transform.ForEach(x => (Transform) x).FirstOrDefault(x => x.name == "bodyRoot") ?? transform.GetChild(0) ?? transform;


    void Start()
    {
        arManager = MultiARManager.Instance;
        arClient  = FindObjectOfType<ArClientCloudAnchorController>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // A Name is provided to the Game Object so it can be found by other Scripts, since this
        // is instantiated as a prefab in the scene.
        gameObject.name = "LocalPlayer";
    }

    void Update()
    {
        // check for local & active player
        if (!isLocalPlayer) return;
        if (!gameObject.activeSelf) return;

        if (!arMainCamera && arManager != null && arManager.IsInitialized())
        {
            arMainCamera = arManager.GetMainCamera();
        }

        if (arMainCamera)
        {
            transform.position = arMainCamera.transform.position;
            transform.rotation = arMainCamera.transform.rotation;
        }

        // fire when clicked (world anchor must be present)

        if (arClient == null || arClient.WorldAnchorObj == null || !arManager || !arManager.IsInitialized() || !arManager.IsInputAvailable(true))
        {
            return;
        }
    }
}