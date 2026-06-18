using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public GameObject hudPanel;
    public Text healthText;
    public Image healthBar;
    public Text armorText;
    public Image armorBar;
    public Text ammoText;
    public Text reserveAmmoText;
    public Text weaponNameText;
    public GameObject crosshair;
    public Image hitMarker;

    [Header("Damage Overlay")]
    public Image damageOverlay;
    public float damageOverlayFadeSpeed = 2f;

    [Header("Kill Feed")]
    public Transform killFeedContainer;
    public GameObject killFeedPrefab;

    [Header("Compass")]
    public RectTransform compassBar;
    public GameObject compassMarkerPrefab;

    [Header("Pause Menu")]
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Text gameOverStatsText;

    [Header("Inventory")]
    public GameObject inventoryPanel;
    public Image[] weaponIcons;
    public Color selectedWeaponColor = Color.white;
    public Color unselectedWeaponColor = Color.gray;

    private FPSController fpsController;
    private WeaponManager weaponManager;
    private HealthSystem playerHealth;
    private float currentDamageAlpha;

    void Start()
    {
        fpsController = FindObjectOfType<FPSController>();
        weaponManager = FindObjectOfType<WeaponManager>();
        playerHealth = FindObjectOfType<HealthSystem>();

        if (damageOverlay != null)
            damageOverlay.color = new Color(1, 0, 0, 0);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        UpdateHUD();
        UpdateDamageOverlay();
        HandlePause();
        UpdateInventroySelection();
    }

    void UpdateHUD()
    {
        if (playerHealth != null)
        {
            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(playerHealth.currentHealth)}";
            if (healthBar != null)
                healthBar.fillAmount = playerHealth.HealthPercent;
            if (armorText != null)
                armorText.text = $"{Mathf.CeilToInt(playerHealth.armor)}";
            if (armorBar != null)
                armorBar.fillAmount = playerHealth.ArmorPercent;
        }

        if (weaponManager != null && weaponManager.CurrentWeapon != null)
        {
            var w = weaponManager.CurrentWeapon;
            if (ammoText != null)
                ammoText.text = $"{w.currentAmmo}";
            if (reserveAmmoText != null)
                reserveAmmoText.text = $"/ {w.reserveAmmo}";
            if (weaponNameText != null)
                weaponNameText.text = w.weaponName;
        }

        if (crosshair != null)
        {
            crosshair.SetActive(weaponManager == null || !weaponManager.IsReloading);
        }
    }

    void UpdateDamageOverlay()
    {
        if (damageOverlay == null) return;

        if (currentDamageAlpha > 0)
        {
            currentDamageAlpha = Mathf.Lerp(currentDamageAlpha, 0, damageOverlayFadeSpeed * Time.deltaTime);
            Color c = damageOverlay.color;
            c.a = currentDamageAlpha;
            damageOverlay.color = c;
        }
    }

    public void ShowDamage()
    {
        currentDamageAlpha = 0.6f;
    }

    public void ShowHitMarker()
    {
        if (hitMarker != null)
        {
            hitMarker.gameObject.SetActive(true);
            CancelInvoke(nameof(HideHitMarker));
            Invoke(nameof(HideHitMarker), 0.2f);
        }
    }

    void HideHitMarker()
    {
        if (hitMarker != null)
            hitMarker.gameObject.SetActive(false);
    }

    void HandlePause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel != null)
            {
                bool isPaused = pausePanel.activeSelf;
                pausePanel.SetActive(!isPaused);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetState(isPaused ?
                        GameManager.GameState.Playing : GameManager.GameState.Paused);
                }
            }
        }
    }

    void UpdateInventroySelection()
    {
        if (weaponManager == null) return;

        for (int i = 0; i < weaponIcons.Length; i++)
        {
            if (weaponIcons[i] != null)
            {
                weaponIcons[i].color = (i == (int)weaponManager.CurrentSlot) ?
                    selectedWeaponColor : unselectedWeaponColor;
            }
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverStatsText != null && GameManager.Instance != null)
            {
                gameOverStatsText.text = $"القضاء: {GameManager.Instance.TotalKills}\n" +
                    $"المهام المنجزة: {GameManager.Instance.MissionsCompleted}\n" +
                    $"المستوى: {GameManager.Instance.PlayerLevel}";
            }

            GameManager.Instance?.SetState(GameManager.GameState.GameOver);
        }
    }

    public void AddKillFeedEntry(string enemyName)
    {
        if (killFeedPrefab == null || killFeedContainer == null) return;

        GameObject entry = Instantiate(killFeedPrefab, killFeedContainer);
        Text text = entry.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = $"قضيت على {enemyName}";
        }

        Destroy(entry, 4f);
    }

    public void ResumeGame()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        GameManager.Instance?.SetState(GameManager.GameState.Playing);
    }

    public void QuitToMenu()
    {
        GameManager.Instance?.LoadScene("MainMenu");
    }
}
