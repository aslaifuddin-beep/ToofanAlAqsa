using UnityEngine;

public class YasinRPG_Projectile : MonoBehaviour
{
    public enum YasinType
    {
        Yasin100,
        Yasin5
    }

    [Header("Yasin Rocket Type")]
    public YasinType rocketType = YasinType.Yasin100;

    [Header("Flight Physics")]
    public float initialSpeed = 40f;
    public float maxSpeed = 80f;
    public float acceleration = 10f;
    public float drag = 0.5f;
    public bool enableGravity = true;
    public float gravityScale = 0.3f;

    [Header("Guidance")]
    public bool hasGuidance = true;
    public float guidanceStrength = 2f;
    public float maxTurnRate = 30f;

    [Header("Explosion")]
    public float explosionRadius = 10f;
    public float explosionForce = 1000f;
    public float explosionDamage = 200f;
    public float destructionDelay = 0.1f;
    public GameObject explosionEffectPrefab;

    [Header("Audio")]
    public AudioSource flightAudioSource;
    public AudioClip flightSound;
    public AudioClip impactSound;

    private Vector3 velocity;
    private float currentSpeed;
    private float lifetime;
    private bool hasExploded;
    private Transform target;

    void Start()
    {
        currentSpeed = initialSpeed;
        velocity = transform.forward * currentSpeed;

        if (flightAudioSource != null && flightSound != null)
        {
            flightAudioSource.clip = flightSound;
            flightAudioSource.loop = true;
            flightAudioSource.Play();
        }

        Destroy(gameObject, 15f);
    }

    public void Initialize(Vector3 direction, float speedMultiplier = 1f)
    {
        velocity = direction.normalized * initialSpeed * speedMultiplier;
        transform.forward = direction.normalized;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Update()
    {
        if (hasExploded) return;

        lifetime += Time.deltaTime;

        UpdateFlight();

        transform.position += velocity * Time.deltaTime;
        transform.forward = velocity.normalized;
    }

    void UpdateFlight()
    {
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);

        velocity = velocity.normalized * currentSpeed;

        if (enableGravity)
        {
            velocity += Physics.gravity * gravityScale * Time.deltaTime;
        }

        if (hasGuidance && target != null)
        {
            Vector3 targetDir = (target.position - transform.position).normalized;
            Vector3 newDir = Vector3.RotateTowards(velocity.normalized, targetDir,
                maxTurnRate * Mathf.Deg2Rad * Time.deltaTime, 0f);

            velocity = newDir * currentSpeed;
        }

        velocity -= velocity * drag * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        Explode();
    }

    void Explode()
    {
        hasExploded = true;

        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }
        else
        {
            SpawnPlaceholderExplosion();
        }

        CameraEffects camFx = FindObjectOfType<CameraEffects>();
        if (camFx != null)
        {
            float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
            float shakeMag = Mathf.Lerp(0.8f, 0.1f, dist / 50f);
            camFx.TriggerShake(shakeMag);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders)
        {
            HealthSystem health = col.GetComponentInParent<HealthSystem>();
            if (health != null)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                float damageFalloff = 1f - (dist / explosionRadius);
                float finalDamage = explosionDamage * Mathf.Max(0.1f, damageFalloff);
                health.TakeDamage(finalDamage);
            }

            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);
            }
        }

        AudioSource.PlayClipAtPoint(impactSound, transform.position, 1f);

        Destroy(gameObject, destructionDelay);
    }

    void SpawnPlaceholderExplosion()
    {
        // TEMPORARY PLACEHOLDER: Replace with your particle effect prefab
        // --- INSTRUCTION FOR ARTIST ---
        // Replace this code with a proper ParticleSystem explosion.
        // Create a prefab with: ParticleSystem (smoke, fire, sparks), light flash.
        // Assign to explosionEffectPrefab in the Inspector.
        // Recommended: Use Unity's VFX Graph or Shuriken Particle System.
        GameObject expSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        expSphere.transform.position = transform.position;
        expSphere.transform.localScale = Vector3.one * explosionRadius * 0.5f;
        Renderer r = expSphere.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = new Color(1f, 0.3f, 0f, 0.5f);
            r.material.SetFloat("_Mode", 3);
        }
        Destroy(expSphere, 1.5f);

        GameObject lightObj = new GameObject("ExplosionLight");
        Light explLight = lightObj.AddComponent<Light>();
        explLight.color = new Color(1f, 0.5f, 0f);
        explLight.range = explosionRadius * 2f;
        explLight.intensity = 5f;
        lightObj.transform.position = transform.position;
        Destroy(lightObj, 0.5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
