using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [Header("Head Bob")]
    public bool enableBob = true;
    public float bobFrequency = 8f;
    public float bobAmplitude = 0.06f;

    [Header("Camera Shake")]
    public float shakeDecay = 2f;
    public float explosionShakeMagnitude = 0.5f;

    [Header("Sway")]
    public float swayAmount = 0.02f;
    public float swaySmoothness = 4f;

    private FPSController fpsController;
    private Camera cam;
    private Vector3 originalPosition;
    private float bobTimer;
    private float currentShakeMagnitude;
    private Vector3 shakeOffset;
    private Vector3 swayOffset;

    void Start()
    {
        fpsController = GetComponentInParent<FPSController>();
        cam = GetComponent<Camera>();
        originalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        if (fpsController == null) return;

        Vector3 finalOffset = Vector3.zero;

        if (enableBob && fpsController.IsMoving && fpsController.IsGrounded)
        {
            finalOffset += CalculateHeadBob();
        }

        finalOffset += CalculateShake();

        finalOffset += CalculateSway();

        transform.localPosition = originalPosition + finalOffset;
    }

    Vector3 CalculateHeadBob()
    {
        float speed = fpsController.IsSprinting ? 1.5f : 1f;
        bobTimer += Time.deltaTime * bobFrequency * speed;

        float x = Mathf.Sin(bobTimer) * bobAmplitude * 0.5f;
        float y = Mathf.Sin(bobTimer * 2f) * bobAmplitude;

        return new Vector3(x, y, 0);
    }

    Vector3 CalculateShake()
    {
        if (currentShakeMagnitude > 0)
        {
            shakeOffset = Random.insideUnitSphere * currentShakeMagnitude;
            currentShakeMagnitude = Mathf.Lerp(currentShakeMagnitude, 0, shakeDecay * Time.deltaTime);

            if (currentShakeMagnitude < 0.01f)
                currentShakeMagnitude = 0;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }

        return shakeOffset;
    }

    Vector3 CalculateSway()
    {
        float mx = -Input.GetAxis("Mouse X") * swayAmount;
        float my = -Input.GetAxis("Mouse Y") * swayAmount;

        swayOffset = Vector3.Lerp(swayOffset, new Vector3(mx, my, 0), swaySmoothness * Time.deltaTime);
        return swayOffset;
    }

    public void TriggerShake(float magnitude)
    {
        currentShakeMagnitude = Mathf.Max(currentShakeMagnitude, magnitude);
    }

    public void TriggerExplosionShake()
    {
        TriggerShake(explosionShakeMagnitude);
    }
}
