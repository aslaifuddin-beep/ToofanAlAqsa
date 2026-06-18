using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UpgradeSystem : MonoBehaviour
{
    [System.Serializable]
    public class PlayerUpgrade
    {
        public string upgradeName;
        public UpgradeType type;
        public int currentLevel;
        public int maxLevel = 5;
        public float baseValue;
        public float valuePerLevel;
        public int upgradePointsRequired = 1;
    }

    public enum UpgradeType
    {
        Health,
        Speed,
        Damage,
        FireRate,
        ReloadSpeed,
        MagSize,
        Armor
    }

    [Header("Player Upgrades")]
    public List<PlayerUpgrade> upgrades = new List<PlayerUpgrade>();

    [Header("References")]
    public FPSController fpsController;
    public WeaponManager weaponManager;
    public HealthSystem playerHealth;

    [Header("UI")]
    public GameObject upgradePanel;
    public Transform upgradeButtonContainer;
    public GameObject upgradeButtonPrefab;
    public Text availablePointsText;

    private int availablePoints;

    public static UpgradeSystem Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeUpgrades();

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleUpgradePanel();
        }
    }

    void InitializeUpgrades()
    {
        upgrades = new List<PlayerUpgrade>
        {
            new PlayerUpgrade { upgradeName = "زيادة الصحة", type = UpgradeType.Health, baseValue = 100f, valuePerLevel = 25f },
            new PlayerUpgrade { upgradeName = "زيادة السرعة", type = UpgradeType.Speed, baseValue = 5f, valuePerLevel = 0.5f },
            new PlayerUpgrade { upgradeName = "قوة الضرر", type = UpgradeType.Damage, baseValue = 35f, valuePerLevel = 5f },
            new PlayerUpgrade { upgradeName = "سرعة إطلاق النار", type = UpgradeType.FireRate, baseValue = 0.1f, valuePerLevel = -0.01f },
            new PlayerUpgrade { upgradeName = "سرعة التلقيم", type = UpgradeType.ReloadSpeed, baseValue = 2f, valuePerLevel = -0.15f },
            new PlayerUpgrade { upgradeName = "حجم المخزن", type = UpgradeType.MagSize, baseValue = 30f, valuePerLevel = 5f },
            new PlayerUpgrade { upgradeName = "الدرع الواقي", type = UpgradeType.Armor, baseValue = 0f, valuePerLevel = 10f }
        };
    }

    public void AddUpgradePoint()
    {
        availablePoints++;
        UpdateUI();
    }

    public void ToggleUpgradePanel()
    {
        if (upgradePanel != null)
        {
            bool isActive = upgradePanel.activeSelf;
            upgradePanel.SetActive(!isActive);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(isActive ?
                    GameManager.GameState.Playing : GameManager.GameState.Paused);
            }

            if (!isActive)
                PopulateUpgradeUI();
        }
    }

    public void PopulateUpgradeUI()
    {
        UpdateUI();

        foreach (Transform child in upgradeButtonContainer)
            Destroy(child.gameObject);

        foreach (PlayerUpgrade upgrade in upgrades)
        {
            if (upgrade.currentLevel >= upgrade.maxLevel) continue;

            GameObject btn = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
            UpgradeButton ui = btn.GetComponent<UpgradeButton>();
            if (ui != null)
            {
                ui.Setup(upgrade, this);
            }
        }
    }

    public void ApplyUpgrade(PlayerUpgrade upgrade)
    {
        if (availablePoints < upgrade.upgradePointsRequired) return;
        if (upgrade.currentLevel >= upgrade.maxLevel) return;

        availablePoints -= upgrade.upgradePointsRequired;
        upgrade.currentLevel++;

        ApplyUpgradeEffects(upgrade);
        PopulateUpgradeUI();
    }

    void ApplyUpgradeEffects(PlayerUpgrade upgrade)
    {
        float currentValue = upgrade.baseValue + (upgrade.valuePerLevel * upgrade.currentLevel);

        switch (upgrade.type)
        {
            case UpgradeType.Health:
                if (playerHealth != null)
                {
                    playerHealth.maxHealth = currentValue;
                    playerHealth.Heal(upgrade.valuePerLevel);
                }
                break;

            case UpgradeType.Speed:
                if (fpsController != null)
                {
                    fpsController.walkSpeed = currentValue;
                    fpsController.sprintSpeed = currentValue * 1.6f;
                }
                break;

            case UpgradeType.Armor:
                if (playerHealth != null)
                {
                    playerHealth.maxArmor = currentValue;
                    playerHealth.AddArmor(upgrade.valuePerLevel);
                }
                break;

            case UpgradeType.Damage:
                if (weaponManager != null && weaponManager.CurrentWeapon != null)
                {
                    weaponManager.CurrentWeapon.stats.damage = currentValue;
                }
                break;

            case UpgradeType.FireRate:
                if (weaponManager != null && weaponManager.CurrentWeapon != null)
                {
                    weaponManager.CurrentWeapon.stats.fireRate = Mathf.Max(0.03f, currentValue);
                }
                break;

            case UpgradeType.ReloadSpeed:
                if (weaponManager != null && weaponManager.CurrentWeapon != null)
                {
                    weaponManager.CurrentWeapon.stats.reloadTime = Mathf.Max(0.3f, currentValue);
                }
                break;

            case UpgradeType.MagSize:
                if (weaponManager != null && weaponManager.CurrentWeapon != null)
                {
                    weaponManager.CurrentWeapon.stats.magSize = Mathf.RoundToInt(currentValue);
                }
                break;
        }
    }

    void UpdateUI()
    {
        if (availablePointsText != null)
            availablePointsText.text = $"نقاط الترقية: {availablePoints}";
    }

    public int GetUpgradeLevel(UpgradeType type)
    {
        PlayerUpgrade up = upgrades.Find(u => u.type == type);
        return up != null ? up.currentLevel : 0;
    }
}

public class UpgradeButton : MonoBehaviour
{
    public Text nameText;
    public Text levelText;
    public Text costText;
    public UnityEngine.UI.Button button;

    private UpgradeSystem.PlayerUpgrade upgrade;
    private UpgradeSystem system;

    public void Setup(UpgradeSystem.PlayerUpgrade upgrade, UpgradeSystem system)
    {
        this.upgrade = upgrade;
        this.system = system;

        if (nameText != null) nameText.text = upgrade.upgradeName;
        if (levelText != null) levelText.text = $"مستوى {upgrade.currentLevel}/{upgrade.maxLevel}";
        if (costText != null) costText.text = $"التكلفة: {upgrade.upgradePointsRequired}";

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => system.ApplyUpgrade(upgrade));
        }
    }
}
