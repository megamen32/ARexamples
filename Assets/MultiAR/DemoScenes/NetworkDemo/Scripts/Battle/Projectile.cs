using UnityEngine;

[CreateAssetMenu]
public class ProjectileSettings : ScriptableObject
{
    public GameObject collidedEffectsPrefab;
    public int        damage;
    public float      lifetime;
}

public class Projectile : MonoBehaviour
{
    [HideInInspector] public ClientPlayerController ClientPlayerOwner;

    private ParticleSystem collidedEffect; // Particle effects to play when colliding with anything
    private float          countdownTimer; // Timer to count each projectile's lifetime from the moment it was shot

    AttackMSG.AttackMode       Mode;
    public  ProjectileSettings projectileSettings;
    private Rigidbody          rigidbodyComponent; // Reference to the rigidbody component


    [HideInInspector] public Transform spawnPos; // Where the projectile is spawned from. 
    [HideInInspector] public Color     trailColor;

    public ParticleSystem
        trailParticles; // Reference to the projectile's particle trail. Needed to change the color when a player fires the projectile

    [HideInInspector] public int ownerPeerId => ClientPlayerOwner.playerControllerId;

    void Start ()
    {
        collidedEffect     = Instantiate(projectileSettings.collidedEffectsPrefab, this.gameObject.transform).GetComponentInChildren<ParticleSystem>();
        rigidbodyComponent = this.gameObject.GetComponent<Rigidbody>();
    }

    // Called each time the project is launched
    public void Reset ()
    {
        rigidbodyComponent.velocity        = Vector3.zero; // Get rid of the velocity from previous shot
        rigidbodyComponent.angularVelocity = Vector3.zero; // Get rid of the angular velocity from previous shot	
        rigidbodyComponent.isKinematic     = false;
        transform.position                 = spawnPos.position; // Set the spawn position
        transform.eulerAngles              = spawnPos.eulerAngles;
        countdownTimer                     = 0; // Reset the countdown timer for its inactivate itself

        Mode = AttackMSG.AttackMode.Primary;
        // Set the color of the trail particles
        var trailParticleSystemRef = trailParticles.main;
        trailParticleSystemRef.startColor = trailColor;

        gameObject.SetActive(true);

        trailParticles.Play();
    }

    void Update ()
    {
        // Automatically destroy self after certain lifetime
        if (gameObject.activeSelf)
        {
            countdownTimer += Time.deltaTime;

            if (countdownTimer >= projectileSettings.lifetime)
            {
                this.gameObject.SetActive(false);
                return;
            }


            if (Mode == AttackMSG.AttackMode.Primary)
            {
                rigidbodyComponent.AddTorque(Quaternion.AngleAxis(Time.deltaTime * rigidbodyComponent.velocity.magnitude,
                                                 Vector3.Cross(rigidbodyComponent.velocity, rigidbodyComponent.transform.forward).normalized) *
                                             rigidbodyComponent.velocity.normalized);
            }
        }
    }

    void OnCollisionEnter (Collision _col)
    {
        Debug.Log(_col.gameObject.name);

        var hitObject = _col.gameObject;

        if (hitObject == null)
        {
            return;
        }

        if (hitObject == ClientPlayerOwner)
        {
            Debug.Log("Collision with the same object: " + hitObject.name);
            return;
        }

        var health = hitObject.GetComponent<PlayerHealth>();

        if (health  != null)
        {
            var damage = projectileSettings.damage;
            if (Mode == AttackMSG.AttackMode.Secondary)
            {
                damage *= Mathf.RoundToInt(Mathf.Clamp(Vector3.Distance(spawnPos.position, transform.position) / 5f, damage, damage * damage));
            }

            Debug.Log(hitObject.name + " takes damage " + damage);

            health.TakeDamage(damage);
            Collide (_col.GetContact(0).normal);


            var mainDuration = collidedEffect.main.duration;
            if (collidedEffect != null && mainDuration > Mathf.Epsilon)
            {
                countdownTimer = projectileSettings.lifetime - mainDuration;
            }
        }
    }


    public void Collide (Vector3 normal)
    {
        if (rigidbodyComponent != null)
        {
            var rigidbodyComponentVelocity = Vector3.Reflect(rigidbodyComponent.velocity, normal);
            rigidbodyComponent.velocity = rigidbodyComponentVelocity; // Get rid of the velocity from previous shot
            rigidbodyComponent.angularVelocity =
                Quaternion.FromToRotation(rigidbodyComponentVelocity, rigidbodyComponent.velocity).
                           eulerAngles; // Get rid of the angular velocity from previous shot	
            //  rigidbodyComponent.isKinematic     = true;
        }

        if (collidedEffect != null)
        {
            collidedEffect.Play();
        }
    }
}