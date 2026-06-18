using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [Header("Ballistics")]
    public float initialVelocity = 800f;
    public float bulletMass = 0.004f;
    public float drag = 0.001f;
    public float gravityMultiplier = 1f;

    [Header("Damage")]
    public float baseDamage = 35f;
    public float maxDistance = 300f;
    public bool enableDrop = true;

    private Vector3 velocity;
    private float distanceTraveled;
    private Vector3 lastPosition;

    public void Initialize(Vector3 direction, float speedMultiplier = 1f)
    {
        Vector3 spread = transform.TransformDirection(new Vector3(
            Random.Range(-0.01f, 0.01f),
            Random.Range(-0.01f, 0.01f),
            0
        ));
        velocity = (direction + spread).normalized * initialVelocity * speedMultiplier;
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (enableDrop)
        {
            velocity += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
        }

        velocity -= velocity * drag * Time.fixedDeltaTime;

        Vector3 delta = velocity * Time.fixedDeltaTime;
        float stepDistance = delta.magnitude;
        distanceTraveled += stepDistance;

        RaycastHit hit;
        if (Physics.Raycast(lastPosition, delta.normalized, out hit, stepDistance))
        {
            Impact(hit);
            return;
        }

        transform.position += delta;
        lastPosition = transform.position;

        if (distanceTraveled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void Impact(RaycastHit hit)
    {
        HealthSystem health = hit.collider.GetComponentInParent<HealthSystem>();
        if (health != null)
        {
            DamageSystem.BodyPart part = DamageSystem.GetBodyPartFromRaycast(hit);
            health.TakeDamage(baseDamage, part);

            GameManager.Instance?.AddKill();
        }

        InstantiateBulletImpact(hit);

        Destroy(gameObject);
    }

    void InstantiateBulletImpact(RaycastHit hit)
    {
        GameObject impactPool = GameObject.Find("_BulletImpacts_Pool");
        if (impactPool == null)
        {
            impactPool = new GameObject("_BulletImpacts_Pool");
        }

        GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        impact.transform.localScale = Vector3.one * 0.05f;
        impact.transform.position = hit.point;
        impact.transform.rotation = Quaternion.LookRotation(hit.normal);
        impact.transform.SetParent(impactPool.transform);

        Renderer r = impact.GetComponent<Renderer>();
        if (r != null) r.material.color = Color.gray;

        Destroy(impact, 3f);
        Object.Destroy(GetComponent<Collider>());
    }
}
