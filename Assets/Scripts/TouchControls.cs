using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TouchControls : MonoBehaviour
{
    [Header("Control References")]
    public RectTransform leftJoystickArea;
    public RectTransform rightJoystickArea;
    public Image leftJoystickKnob;
    public Image rightJoystickKnob;

    [Header("Control Settings")]
    public float joystickRange = 80f;
    public float deadZone = 10f;
    public float swipeSensitivity = 0.2f;
    public bool invertY = false;

    [Header("Buttons")]
    public Button fireButton;
    public Button reloadButton;
    public Button switchWeaponButton;
    public Button crouchButton;
    public Button sprintButton;
    public Button jumpButton;
    public Button zoomButton;

    [Header("Button Layout")]
    public Canvas canvas;
    public RectTransform buttonContainer;

    private FPSController fpsController;
    private WeaponManager weaponManager;

    private Vector2 leftJoystickInput;
    private Vector2 rightJoystickInput;
    private int leftTouchId = -1;
    private int rightTouchId = -1;
    private Vector2 leftStartPos;
    private Vector2 rightStartPos;

    public Vector2 MoveInput => leftJoystickInput;
    public Vector2 LookInput => rightJoystickInput;

    public bool IsUsingTouchControls { get; private set; }

    void Start()
    {
        fpsController = FindObjectOfType<FPSController>();
        weaponManager = FindObjectOfType<WeaponManager>();

        IsUsingTouchControls = Application.isMobilePlatform || Input.touchSupported;

        if (!IsUsingTouchControls)
        {
            if (leftJoystickArea != null) leftJoystickArea.gameObject.SetActive(false);
            if (rightJoystickArea != null) rightJoystickArea.gameObject.SetActive(false);
            return;
        }

        SetupButtons();
    }

    void Update()
    {
        if (!IsUsingTouchControls) return;
        if (fpsController == null) return;

        HandleTouchInput();
        UpdateMovement();
    }

    void HandleTouchInput()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (IsTouchInRect(touch.position, leftJoystickArea))
                    {
                        leftTouchId = touch.fingerId;
                        leftStartPos = touch.position;
                    }
                    else if (IsTouchInRect(touch.position, rightJoystickArea))
                    {
                        rightTouchId = touch.fingerId;
                        rightStartPos = touch.position;
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (touch.fingerId == leftTouchId)
                    {
                        Vector2 delta = touch.position - leftStartPos;
                        float dist = Mathf.Clamp(delta.magnitude, 0, joystickRange);
                        leftJoystickInput = delta.normalized * (dist / joystickRange);

                        if (leftJoystickInput.magnitude < deadZone / joystickRange)
                            leftJoystickInput = Vector2.zero;

                        if (leftJoystickKnob != null)
                            leftJoystickKnob.rectTransform.anchoredPosition = leftJoystickInput * joystickRange;
                    }
                    else if (touch.fingerId == rightTouchId)
                    {
                        Vector2 delta = touch.position - rightStartPos;
                        float dist = Mathf.Clamp(delta.magnitude, 0, joystickRange);
                        rightJoystickInput = delta.normalized * (dist / joystickRange);

                        if (rightJoystickInput.magnitude < deadZone / joystickRange)
                            rightJoystickInput = Vector2.zero;

                        if (rightJoystickKnob != null)
                            rightJoystickKnob.rectTransform.anchoredPosition = rightJoystickInput * joystickRange;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (touch.fingerId == leftTouchId)
                    {
                        leftTouchId = -1;
                        leftJoystickInput = Vector2.zero;
                        if (leftJoystickKnob != null)
                            leftJoystickKnob.rectTransform.anchoredPosition = Vector2.zero;
                    }
                    else if (touch.fingerId == rightTouchId)
                    {
                        rightTouchId = -1;
                        rightJoystickInput = Vector2.zero;
                        if (rightJoystickKnob != null)
                            rightJoystickKnob.rectTransform.anchoredPosition = Vector2.zero;
                    }
                    break;
            }
        }
    }

    void UpdateMovement()
    {
        if (fpsController != null)
        {
            fpsController.MoveWithInput(leftJoystickInput);

            Vector2 lookDelta = rightJoystickInput * swipeSensitivity;
            if (invertY) lookDelta.y = -lookDelta.y;
            fpsController.ApplyLookDelta(lookDelta);
        }
    }

    bool IsTouchInRect(Vector2 touchPos, RectTransform rect)
    {
        if (rect == null) return false;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect, touchPos, canvas?.worldCamera, out localPoint);

        return rect.rect.Contains(localPoint);
    }

    void SetupButtons()
    {
        if (fireButton != null)
            fireButton.onClick.AddListener(() => weaponManager?.FireFromTouch());

        if (reloadButton != null)
            reloadButton.onClick.AddListener(() => weaponManager?.StartReload());

        if (switchWeaponButton != null)
        {
            switchWeaponButton.onClick.AddListener(() =>
            {
                if (weaponManager == null) return;
                int nextSlot = ((int)weaponManager.CurrentSlot + 1) % 3;
                weaponManager.SwitchWeapon((WeaponManager.WeaponSlot)nextSlot);
            });
        }

        if (crouchButton != null)
        {
            crouchButton.onClick.AddListener(() =>
            {
                if (fpsController != null)
                    fpsController.SetCrouch(!fpsController.IsCrouching);
            });
        }

        if (sprintButton != null)
        {
            EventTrigger sprintTrigger = sprintButton.gameObject.GetComponent<EventTrigger>();
            if (sprintTrigger == null)
                sprintTrigger = sprintButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pressEntry = new EventTrigger.Entry();
            pressEntry.eventID = EventTriggerType.PointerDown;
            pressEntry.callback.AddListener((data) => { if (fpsController != null) fpsController.SetSprint(true); });

            EventTrigger.Entry releaseEntry = new EventTrigger.Entry();
            releaseEntry.eventID = EventTriggerType.PointerUp;
            releaseEntry.callback.AddListener((data) => { if (fpsController != null) fpsController.SetSprint(false); });

            sprintTrigger.triggers.Add(pressEntry);
            sprintTrigger.triggers.Add(releaseEntry);
        }

        if (jumpButton != null)
            jumpButton.onClick.AddListener(() => fpsController?.Jump());

        if (zoomButton != null)
        {
            EventTrigger zoomTrigger = zoomButton.gameObject.GetComponent<EventTrigger>();
            if (zoomTrigger == null)
                zoomTrigger = zoomButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pressZoom = new EventTrigger.Entry();
            pressZoom.eventID = EventTriggerType.PointerDown;
            pressZoom.callback.AddListener((data) => { if (weaponManager != null) weaponManager.SetADS(true); });

            EventTrigger.Entry releaseZoom = new EventTrigger.Entry();
            releaseZoom.eventID = EventTriggerType.PointerUp;
            releaseZoom.callback.AddListener((data) => { if (weaponManager != null) weaponManager.SetADS(false); });

            zoomTrigger.triggers.Add(pressZoom);
            zoomTrigger.triggers.Add(releaseZoom);
        }
    }
}
