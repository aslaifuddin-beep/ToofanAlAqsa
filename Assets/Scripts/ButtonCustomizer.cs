using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ButtonCustomizer : MonoBehaviour
{
    [System.Serializable]
    public class CustomizableButton
    {
        public string buttonName;
        public RectTransform buttonRect;
        public Vector2 defaultPosition;
        public Vector2 defaultSize;
        public float defaultScale = 1f;
        public bool isDraggable = true;
        public bool isResizable = true;

        [HideInInspector] public Vector2 dragOffset;
        [HideInInspector] public bool isDragging;
    }

    [Header("Customizable Buttons")]
    public List<CustomizableButton> buttons = new List<CustomizableButton>();

    [Header("Settings")]
    public Canvas canvas;
    public GameObject customizationPanel;
    public bool enableCustomization = true;

    [Header("UI")]
    public Button saveLayoutButton;
    public Button resetLayoutButton;
    public Button closeButton;

    private bool isCustomizing;
    private CustomizableButton selectedButton;
    private Vector2 resizeStartPos;
    private Vector2 resizeStartSize;
    private bool isResizing;

    const string LAYOUT_PREFS_KEY = "TouchLayout_";

    void Start()
    {
        if (!enableCustomization) return;

        LoadLayout();

        if (customizationPanel != null)
            customizationPanel.SetActive(false);

        if (saveLayoutButton != null)
            saveLayoutButton.onClick.AddListener(SaveLayout);

        if (resetLayoutButton != null)
            resetLayoutButton.onClick.AddListener(ResetLayout);

        if (closeButton != null)
            closeButton.onClick.AddListener(ToggleCustomization);
    }

    void Update()
    {
        if (!isCustomizing) return;

        HandleDrag();
        HandleResize();
    }

    public void ToggleCustomization()
    {
        isCustomizing = !isCustomizing;

        if (customizationPanel != null)
            customizationPanel.SetActive(isCustomizing);

        GameManager.Instance?.SetState(isCustomizing ?
            GameManager.GameState.Paused : GameManager.GameState.Playing);
    }

    void HandleDrag()
    {
        if (Input.touchCount > 0 || Input.GetMouseButton(0))
        {
            Vector2 inputPos = Input.touchCount > 0 ?
                Input.GetTouch(0).position : (Vector2)Input.mousePosition;

            foreach (var btn in buttons)
            {
                if (!btn.isDraggable) continue;
                if (!RectTransformUtility.RectangleContainsScreenPoint(btn.buttonRect, inputPos, canvas.worldCamera))
                    continue;

                if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                {
                    btn.isDragging = true;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        btn.buttonRect.parent as RectTransform,
                        inputPos, canvas.worldCamera, out btn.dragOffset);
                    btn.dragOffset -= btn.buttonRect.anchoredPosition;
                    selectedButton = btn;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            foreach (var btn in buttons)
            {
                btn.isDragging = false;
            }
            selectedButton = null;
            isResizing = false;
        }

        if (selectedButton != null && selectedButton.isDragging)
        {
            Vector2 inputPos = Input.touchCount > 0 ?
                Input.GetTouch(0).position : (Vector2)Input.mousePosition;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                selectedButton.buttonRect.parent as RectTransform,
                inputPos, canvas.worldCamera, out localPoint);

            selectedButton.buttonRect.anchoredPosition = localPoint - selectedButton.dragOffset;

            ClampToCanvas(selectedButton.buttonRect);
        }
    }

    void HandleResize()
    {
        if (selectedButton == null || !selectedButton.isResizable) return;

        if (Input.GetKey(KeyCode.LeftControl) || Input.touchCount >= 2)
        {
            float scaleDelta = 0;

            if (Input.touchCount >= 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                Vector2 prevPos0 = t0.position - t0.deltaPosition;
                Vector2 prevPos1 = t1.position - t1.deltaPosition;

                float prevMag = (prevPos0 - prevPos1).magnitude;
                float curMag = (t0.position - t1.position).magnitude;

                scaleDelta = (curMag - prevMag) * 0.01f;
            }
            else
            {
                scaleDelta = Input.GetAxis("Mouse ScrollWheel");
            }

            Vector3 scale = selectedButton.buttonRect.localScale;
            scale = Vector3.one * Mathf.Clamp(scale.x + scaleDelta, 0.5f, 2.5f);
            selectedButton.buttonRect.localScale = scale;
            selectedButton.defaultScale = scale.x;
        }
    }

    void ClampToCanvas(RectTransform rect)
    {
        if (canvas == null) return;

        Rect canvasRect = (canvas.transform as RectTransform).rect;

        Vector2 pos = rect.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x, canvasRect.xMin + rect.sizeDelta.x * 0.5f,
            canvasRect.xMax - rect.sizeDelta.x * 0.5f);
        pos.y = Mathf.Clamp(pos.y, canvasRect.yMin + rect.sizeDelta.y * 0.5f,
            canvasRect.yMax - rect.sizeDelta.y * 0.5f);

        rect.anchoredPosition = pos;
    }

    public void SaveLayout()
    {
        foreach (var btn in buttons)
        {
            string prefix = LAYOUT_PREFS_KEY + btn.buttonName;
            PlayerPrefs.SetFloat(prefix + "_posX", btn.buttonRect.anchoredPosition.x);
            PlayerPrefs.SetFloat(prefix + "_posY", btn.buttonRect.anchoredPosition.y);
            PlayerPrefs.SetFloat(prefix + "_scale", btn.buttonRect.localScale.x);
        }
        PlayerPrefs.Save();

        if (customizationPanel != null)
            customizationPanel.SetActive(false);

        isCustomizing = false;
        GameManager.Instance?.SetState(GameManager.GameState.Playing);
    }

    public void LoadLayout()
    {
        foreach (var btn in buttons)
        {
            string prefix = LAYOUT_PREFS_KEY + btn.buttonName;

            if (PlayerPrefs.HasKey(prefix + "_posX"))
            {
                float x = PlayerPrefs.GetFloat(prefix + "_posX");
                float y = PlayerPrefs.GetFloat(prefix + "_posY");
                btn.buttonRect.anchoredPosition = new Vector2(x, y);
            }
            else
            {
                btn.buttonRect.anchoredPosition = btn.defaultPosition;
            }

            if (PlayerPrefs.HasKey(prefix + "_scale"))
            {
                float scale = PlayerPrefs.GetFloat(prefix + "_scale");
                btn.buttonRect.localScale = Vector3.one * scale;
            }
            else
            {
                btn.buttonRect.localScale = Vector3.one * btn.defaultScale;
            }

            btn.buttonRect.sizeDelta = btn.defaultSize;
        }
    }

    public void ResetLayout()
    {
        foreach (var btn in buttons)
        {
            string prefix = LAYOUT_PREFS_KEY + btn.buttonName;
            PlayerPrefs.DeleteKey(prefix + "_posX");
            PlayerPrefs.DeleteKey(prefix + "_posY");
            PlayerPrefs.DeleteKey(prefix + "_scale");
        }

        LoadLayout();
    }
}
