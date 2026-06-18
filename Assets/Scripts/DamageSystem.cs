using UnityEngine;

public static class DamageSystem
{
    public enum BodyPart
    {
        Head,
        Torso,
        Arms,
        Legs
    }

    public static float GetBodyPartMultiplier(BodyPart part)
    {
        switch (part)
        {
            case BodyPart.Head:  return 2.5f;
            case BodyPart.Torso: return 1.0f;
            case BodyPart.Arms:  return 0.7f;
            case BodyPart.Legs:  return 0.5f;
            default:             return 1.0f;
        }
    }

    public static BodyPart GetBodyPartFromRaycast(RaycastHit hit)
    {
        string tag = hit.collider.tag;
        switch (tag)
        {
            case "Head": return BodyPart.Head;
            case "Arm":  return BodyPart.Arms;
            case "Leg":  return BodyPart.Legs;
            default:     return BodyPart.Torso;
        }
    }
}

public class Hitbox : MonoBehaviour
{
    public DamageSystem.BodyPart bodyPart = DamageSystem.BodyPart.Torso;
    public HealthSystem targetHealth;

    public void OnHit(float damage)
    {
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage, bodyPart);
        }
    }
}
