using UnityEngine;

public class AudioReverbZone : MonoBehaviour
{
    public enum EnvironmentType
    {
        OpenStreet,
        Tunnel,
        BuildingInterior,
        RubbleZone,
        Courtyard
    }

    [Header("Environment Settings")]
    public EnvironmentType environmentType = EnvironmentType.OpenStreet;
    public float zoneRadius = 20f;
    public float transitionZone = 5f;

    [Header("Reverb Parameters")]
    [Range(0f, 1f)] public float reverbMix = 0.3f;
    [Range(0f, 1f)] public float reflectionsLevel = 0.2f;
    [Range(100f, 5000f)] public float reflectionsDelay = 500f;
    [Range(0f, 1f)] public float reverbLevel = 0.3f;
    [Range(0.1f, 10f)] public float reverbTime = 2f;
    [Range(100f, 5000f)] public float reverbDelay = 100f;

    [Header("Audio Filter")]
    public AudioReverbFilter reverbFilter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
        {
            ApplyReverb();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
        {
            RemoveReverb();
        }
    }

    void ApplyReverb()
    {
        if (reverbFilter == null)
        {
            GameObject mainCam = Camera.main?.gameObject;
            if (mainCam != null)
            {
                reverbFilter = mainCam.GetComponent<AudioReverbFilter>();
                if (reverbFilter == null)
                    reverbFilter = mainCam.AddComponent<AudioReverbFilter>();
            }
        }

        if (reverbFilter != null)
        {
            switch (environmentType)
            {
                case EnvironmentType.Tunnel:
                    reverbFilter.reverbPreset = AudioReverbPreset.Cave;
                    reverbFilter.reverbLevel = reverbLevel;
                    reverbFilter.reverbTime = reverbTime * 1.5f;
                    break;

                case EnvironmentType.BuildingInterior:
                    reverbFilter.reverbPreset = AudioReverbPreset.Room;
                    reverbFilter.reverbLevel = reverbLevel * 0.8f;
                    reverbFilter.reverbTime = reverbTime * 0.7f;
                    break;

                case EnvironmentType.RubbleZone:
                    reverbFilter.reverbPreset = AudioReverbPreset.Concerthall;
                    reverbFilter.reverbLevel = reverbLevel * 0.6f;
                    reverbFilter.reverbTime = reverbTime * 1.2f;
                    break;

                case EnvironmentType.Courtyard:
                    reverbFilter.reverbPreset = AudioReverbPreset.LivingRoom;
                    reverbFilter.reverbLevel = reverbLevel * 0.4f;
                    reverbFilter.reverbTime = reverbTime * 0.5f;
                    break;

                default:
                    reverbFilter.reverbPreset = AudioReverbPreset.Off;
                    break;
            }

            reverbFilter.reflectionsLevel = reflectionsLevel;
            reverbFilter.reflectionsDelay = reflectionsDelay / 1000f;
            reverbFilter.reverbDelay = reverbDelay / 1000f;
            reverbFilter.dryLevel = -reverbMix * 10f;
            reverbFilter.room = Mathf.Lerp(-1000f, 0f, 1f - reverbMix);
        }
    }

    void RemoveReverb()
    {
        if (reverbFilter != null)
        {
            reverbFilter.reverbPreset = AudioReverbPreset.Off;
        }
    }

    void OnDrawGizmosSelected()
    {
        Color envColor = Color.blue;
        switch (environmentType)
        {
            case EnvironmentType.Tunnel:          envColor = Color.black; break;
            case EnvironmentType.BuildingInterior: envColor = Color.gray; break;
            case EnvironmentType.RubbleZone:      envColor = new Color(0.5f, 0.3f, 0f); break;
            case EnvironmentType.Courtyard:       envColor = Color.green; break;
        }

        Gizmos.color = new Color(envColor.r, envColor.g, envColor.b, 0.2f);
        Gizmos.DrawSphere(transform.position, zoneRadius);

        Gizmos.color = envColor;
        Gizmos.DrawWireSphere(transform.position, zoneRadius);
        Gizmos.DrawWireSphere(transform.position, zoneRadius + transitionZone);
    }
}
