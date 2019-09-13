using UnityEngine;

public class AbilityLauncher : LaunchProjectile
{
    Rigidbody                  rigid;
    public           Transform target;
    [SerializeField] float     TorqueSpeed;

    void Start()
    {
    }


    void Update()
    {
        if (!target)
        {
            return;
        }

        if (projectile != null)
        {
            rigid = projectile.GetComponent<Rigidbody>();
            var relativeSpeed = rigid.GetRelativePointVelocity(target.position);
            rigid.AddTorque(Quaternion.AngleAxis(Time.deltaTime * TorqueSpeed / relativeSpeed.magnitude, Vector3.Cross(rigid.velocity, relativeSpeed)) *
                            rigid.velocity.normalized);
        }
    }
}