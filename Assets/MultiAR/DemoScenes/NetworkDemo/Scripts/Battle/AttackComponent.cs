using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class AttackComponent : NetworkBehaviour
{
    private AbilityLauncher abilityLauncher;

    // Ability Settings
    public  GameObject                    abilityRoot;
    private ArClientCloudAnchorController arClient     ;

    private Camera arMainCamera  ;

    // reference to the Multi-AR manager
    private MultiARManager arManager     ;

    // Attack Settings
    private bool             canAttack;
    private float            coolDown = 1.0f;
    private bool             isPressed;
    private float            longPressCutoff = 0.4f;
    private LaunchProjectile projectileLauncher;
    private bool             shieldActivated;

    // Shield Settings
    public                   GameObject shieldRoot;
    [HideInInspector] public Team       team;
    private                  float      timer;

    [SerializeField] GameObject wand  ;

    // Player Settings
    [HideInInspector] public bool isUser => isLocalPlayer;

    public void Setup ()
    {
        // Default values
        canAttack       = true;
        shieldActivated = false;
        LauncherBase[] Weapons = new [] {projectileLauncher, abilityLauncher};
        // Set up projectile launcher
        for (var index = 0; index < Weapons.Length; index++)
        {
            var type = Weapons[index].GetType();
            Weapons[index]                      = GetComponentInChildren(type) as LauncherBase;
            Weapons[index].projectileTrailColor = team.color;
            Weapons[index].projectileOwnerPeer  = GetComponent<ClientPlayerController>();
        }

        arManager = MultiARManager.Instance;
        arClient  = FindObjectOfType<ArClientCloudAnchorController>();

        // Attach a random wand based on the team that this player is in 
        AttachRandomWand();
    }

    public void AttachRandomWand ()
    {
        wand                            = Instantiate(team.wands[Random.Range(0, team.wands.Length)], abilityRoot.transform) as GameObject;
        wand.transform.localPosition    = new Vector3 (0,    -0.5f, -0.7f);
        wand.transform.localEulerAngles = new Vector3 (45,   0,     0);
        wand.transform.localScale       = new Vector3 (2.5f, 2.5f,  2.5f);
    }

    [Command]
    void CmdRequestTriggerBlast()
    {
        StartCoroutine(AbilityCooldown());
    }

    [Command]
    void CmdRequestCastingShield(bool status)
    {
        arClient.netClient.Send(NetMsgType.AttackRequest,
            new AttackMSG
            {
                attackMode = status ? AttackMSG.AttackMode.Primary : AttackMSG.AttackMode.Secondary,
                attackType = AttackMSG.AttackType.Shield,
                netId      = netId.Value
            });
    }

    // This function ensures that the player can only attack after a certain cooldown

    private IEnumerator AbilityCooldown()
    {
        canAttack = false;

        // Send a data packet to GameSparks to notify opponent that player has attacked

        // Data doesn't need to contain anything since we are just notifying the other peers that players has attacked
        //    MultiplayerNetworkingManager.Instance().GetRTSession().
        //                               SendData(3, GameSparks.RT.GameSparksRT.DeliveryIntent.UNRELIABLE, data); // send the data at op-code 3


        arClient.netClient.Send(NetMsgType.AttackRequest,
            new AttackMSG {attackMode = AttackMSG.AttackMode.Primary, attackType = AttackMSG.AttackType.Blast, netId = netId.Value});
        yield return new WaitForSeconds (coolDown / 5);
        arClient.netClient.Send(NetMsgType.AttackRequest,
            new AttackMSG {attackMode = AttackMSG.AttackMode.Secondary, attackType = AttackMSG.AttackType.Blast, netId = netId.Value});


        // Wait for the cooldown period before letting the player attack again
        yield return new WaitForSeconds (coolDown);
        canAttack = true;
    }


    [ClientRpc]
    public void RpcDoShield (AttackMSG.AttackMode _isActivated)
    {
        var status = _isActivated == AttackMSG.AttackMode.Primary;
        if (status != shieldActivated)
        {
            shieldRoot.SetActive(status);
            shieldActivated = status; //TODO change to response
        }
    }

    [ClientRpc]
    public void RpcDoBlast(AttackMSG.AttackMode attackAttackMode)
    {
        switch (attackAttackMode)
        {
            case AttackMSG.AttackMode.Primary:
                abilityLauncher.LaunchPrimary();

                break;
            case AttackMSG.AttackMode.Secondary:
                abilityLauncher.LaunchSecondary();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(attackAttackMode), attackAttackMode, null);
        }
    }

    [ClientRpc]
    public void RpcDoBullet(AttackMSG.AttackMode attackAttackMode)
    {
        switch (attackAttackMode)
        {
            case AttackMSG.AttackMode.Primary:
                projectileLauncher.LaunchPrimary();

                break;
            case AttackMSG.AttackMode.Secondary:
                projectileLauncher.LaunchSecondary();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(attackAttackMode), attackAttackMode, null);
        }
    }

    void Update ()
    {
        // Only the user can provide input to this Attack script
        if (!isUser)
        {
            return;
        }

        if (!gameObject.activeSelf) return;


    #region Magic

        if (canAttack)
        {
        #region Touch Input

            if (Input.touchCount > 0)
            {
                Touch touch = Input.touches[0];

                // Reset the timer once the user touches the screen
                if (touch.phase == TouchPhase.Began)
                {
                    timer     = 0f;
                    isPressed = true;
                }

                if (isPressed)
                {
                    timer += Time.deltaTime;

                    // If the timer is a long press, then activate the shield
                    if (timer > longPressCutoff)
                    {
                        CmdRequestCastingShield(true);
                    }
                }

                // Check the timer amount when the user lifts his finger off the screen
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    if (timer < longPressCutoff)
                    {
                        // Tap ended
                        CmdRequestTriggerBlast();
                    } else
                    {
                        // Long press ended
                        CmdRequestCastingShield(false);
                    }

                    isPressed = false;
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    var cursorPosition = arManager.GetCursorPosition();
                    var direction      = cursorPosition - wand.transform.position;
                    if (cursorPosition != null && direction.z > 0)
                        wand.transform.LookAt(Vector3.Slerp(wand.transform.forward * direction.magnitude, cursorPosition, Time.deltaTime),
                            arMainCamera.transform.up);
                }
            }

        #endregion

        #region Keyboard Input

            // Reset the timer once the user touches the screen
            if (Input.GetKeyDown("space") || Input.GetKeyDown(KeyCode.Space))
            {
                timer     = 0f;
                isPressed = true;
            }

            if (isPressed)
            {
                timer += Time.deltaTime;

                // If the timer is a long press, then activate the shield
                if (timer > longPressCutoff)
                {
                    CmdRequestCastingShield(true);
                }
            }

            // Check the timer amount when the user lifts his finger off the screen
            if (Input.GetKeyUp("space"))
            {
                if (timer < longPressCutoff)
                {
                    // Tap ended
                    CmdRequestTriggerBlast();
                } else
                {
                    // Long press ended
                    if (shieldActivated)
                    {
                        CmdRequestCastingShield(false);
                    }
                }

                isPressed = false;
            }

        #endregion
        }

    #endregion

    #region BlackMagick

        if (!arMainCamera && arManager != null && arManager.IsInitialized())
        {
            arMainCamera = arManager.GetMainCamera();
        }

        if (countDown < 0)
        {
            MultiARInterop.InputAction action = arManager.GetInputAction();
            switch (action)
            {
                case MultiARInterop.InputAction.Click:

                    CmdFire();
                    countDown = FireRate;
                    break;
                case MultiARInterop.InputAction.Grip:

                    CmdSpawnStar(arManager.GetCursorPosition(), Quaternion.LookRotation(arManager.GetInputNavCoordinates(), Vector3.up));

                    break;
                case MultiARInterop.InputAction.None:

                    break;
                case MultiARInterop.InputAction.Release:
                    CmdFire();
                    Portal();
                    countDown = FireRate;
                    break;
                case MultiARInterop.InputAction.SpeechCommand:
                    break;
                case MultiARInterop.InputAction.CustomCommand:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

    GameObject        prevShield;
    public GameObject bulletPrefab;

    public Transform bulletSpawn;

    [Command]
    void CmdFire()
    {
        // Create the Bullet from the Bullet Prefab
        var bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);

        // Set the player-owner
        bullet.GetComponent<BulletScript>().playerOwner = gameObject;

        var cursorPosition = arManager.GetCursorPosition();
        // Add velocity to the bullet
        var rigid = bullet.GetComponent<Rigidbody>();
        rigid.velocity = bullet.transform.forward * 3;
        if (cursorPosition != Vector3.zero)
        {
            var distance = Vector3.Distance(cursorPosition, bulletSpawn.position);
            if (distance > bulletPrefab.transform.lossyScale.magnitude)
            {
                rigid.velocity *= distance / ProjectilePool.GetProjectilePool()[0].projectileSettings.lifetime;
            } else
            {
                // Destroy the bullet after 2 seconds
                Destroy(bullet, 4.0f);
            }
        }

        // Spawn the bullet on the Clients
        NetworkServer.Spawn(bullet);
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

#endregion
}