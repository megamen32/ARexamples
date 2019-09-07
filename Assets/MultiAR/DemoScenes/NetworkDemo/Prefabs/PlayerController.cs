using UnityEngine;
using UnityEngine.Networking;


public class PlayerController : NetworkBehaviour // MonoBehaviour
{
    public GameObject bulletPrefab;

    public Transform bulletSpawn;

    // reference to the Multi-AR manager
    private MultiARManager arManager    = null;
    private Camera         arMainCamera = null;

    // reference to the ar-client
    private ArClientBaseController arClient = null;


    void Start()
    {
        arManager = MultiARManager.Instance;
        arClient  = ArClientBaseController.Instance;
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

        if (!arMainCamera && arManager && arManager.IsInitialized())
        {
            arMainCamera = arManager.GetMainCamera();
        }

        if (arMainCamera)
        {
            transform.position = arMainCamera.transform.position;
            transform.rotation = arMainCamera.transform.rotation;
        }

        // fire when clicked (world anchor must be present)
        countDown -= Time.deltaTime;
        if (arClient && arClient.WorldAnchorObj != null && arManager && arManager.IsInitialized() && arManager.IsInputAvailable(true))
        {
            MultiARInterop.InputAction action = arManager.GetInputAction();

            if (countDown < 0)
            {
                if (action == MultiARInterop.InputAction.Click)
                {
                    CmdFire();
                    countDown = FireRate;
                } else if (action == MultiARInterop.InputAction.Grip)
                {
                    
                        Portal();
                        CmdSpawnStar(bulletSpawn.position, Quaternion.LookRotation(bulletSpawn.forward, Vector3.up));
                   

                    countDown = FireRate;
                }
            }
        }
    }

    void Portal()
    {
        if (arManager != null && arManager.RaycastToWorld(true, out var hit))
        {
            CmdPortal( hit.point, hit.rotation);
        }
    }

    [Command]
    void CmdPortal( Vector3 transformPosition, Quaternion rotation)
    {
        if (portalObj != null)
        {
            Destroy(portalObj, 5);
            portalObj = null;
        }

        if (portalObj == null)
        {
            portalObj = Instantiate(portalPrefab);
            NetworkServer.Spawn(portalObj);
           
        }

        portalObj.transform.position = transformPosition;
        portalObj.transform.rotation = rotation;
        //portalObj.transform.localScale *= ( 10/ hit.distance);
        //if (turnPortal) MultiARInterop.TurnObjectToCamera(portalObj, arMainCamera, hit.point, hit.normal);
    }

    GameObject prevShield;

    [Command]
    void CmdFire()
    {
        // Create the Bullet from the Bullet Prefab
        var bullet = (GameObject) Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);

        // Set the player-owner
        bullet.GetComponent<BulletScript>().playerOwner = gameObject;

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 3;

        // Spawn the bullet on the Clients
        NetworkServer.Spawn(bullet);

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 4.0f);
    }

    public GameObject      ShieldPrefab;
    public GameObject      portalPrefab;
    float                  countDown = -1;
    [SerializeField] float FireRate  = 1;
    GameObject             portalObj;
    [SerializeField] bool  turnPortal = true;
    bool                   oneClick;

    [Command]
    public void CmdSpawnStar(Vector3 position, Quaternion rotation)
    {
        if (prevShield == null)
        {
            // Instantiate Star model at the hit pose.
            prevShield = Instantiate(ShieldPrefab, position, rotation);
            //  prevShield.transform.parent = arClient.GetAnchorTransform();
            NetworkServer.Spawn(prevShield);
        }

        prevShield.transform.SetPositionAndRotation(position, rotation);
        // Spawn the object in all clients.
#pragma warning disable 618

#pragma warning restore 618
    }
}