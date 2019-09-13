using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static ProjectilePool;

public class LaunchProjectile : LauncherBase
{
    private float launchForce = 1000f;

    protected Projectile projectile  ;
    public Transform bulletSpawn;
    private Camera         arMainCamera  ;

    // reference to the ar-client
    private ArClientBaseController arClient  ;

    void Awake()
    {
        
    }

    void Start()
    {
        
        arClient  = FindObjectOfType<ArClientCloudAnchorController>();
    }
    
    public override void LaunchPrimary ()
    {
        // Loop through all the projectiles in the projectile pool
        for (var i = 0; i < GetProjectilePool().Length; i++)
        {
            // Find one that is currently inactive
            projectile = GetProjectilePool()[i];
            if (!projectile.gameObject.activeSelf)
            {
                // Assign values to the projectile
                projectile.spawnPos    = bulletSpawn?? gameObject.transform;
                projectile.trailColor  = projectileTrailColor;
                projectile.ClientPlayerOwner = projectileOwnerPeer;
                Physics.IgnoreCollision(projectile.gameObject.GetComponent<Collider>(),
                    GetComponentInParent<ClientPlayerController>().bodyRoot.GetComponentInChildren<Collider>());

                // Initialize the projectile
                projectile.Reset();
                var rigid = projectile.GetComponent<Rigidbody>();
                var cursorPosition = MultiARManager.Instance.GetCursorPosition();
                if (cursorPosition != Vector3.zero)
                {
                    var distance = Vector3.Distance(cursorPosition, bulletSpawn.position);
                    if (distance > projectile.transform.lossyScale.magnitude)
                    {
                        rigid.velocity = transform.forward * distance / projectile.projectileSettings.lifetime;

                    }
                } else
                {

                    // Launch the projectile
                    rigid.AddForce(transform.forward * launchForce);
                }

                NetworkServer.Spawn(projectile.gameObject);
                break;
            }

            projectile = null;
        }
    }

    public override void LaunchSecondary()
    {
        LaunchPrimary();
        Invoke(nameof(LaunchPrimary),.1f);
        Invoke(nameof(LaunchPrimary),.3f);
        Invoke(nameof(LaunchPrimary),.5f);
    }
}