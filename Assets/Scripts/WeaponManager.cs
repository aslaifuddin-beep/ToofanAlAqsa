using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    public enum WeaponSlot
    {
        AssaultRifle,
        SniperRifle,
        RPG
    }

    [System.Serializable]
    public class WeaponEntry
    {
        public WeaponSlot slot;
        public string weaponName;
        public GameObject weaponPrefab;
        public Transform weaponMountPoint;
        public WeaponStats stats;
        public int currentAmmo;
        public int reserveAmmo;
        public AudioClip fireSound;
        public AudioClip reloadSound;
        public AudioClip emptySound;
    }

    [System.Serializable]
    public class WeaponStats
    {
        public float fireRate = 0.1f;
        public float reloadTime = 2f;
        public int magSize = 30;
        public float damage = 35f;
        public float recoilAmount = 0.05f;
        public float recoilRecovery = 5f;
        public float spread = 0.01f;
        public float aimDownSightSpeed = 0.3f;
        public bool isAutomatic = true;
        public bool isProjectile = false;
        public GameObject projectilePrefab;
        public float projectileSpeed = 50f;
    }

    [Header("Weapons")]
    public List<WeaponEntry> weapons = new List<WeaponEntry>();
    public WeaponSlot startingWeapon = WeaponSlot.AssaultRifle;

    [Header("References")]
    public Transform weaponHolder;
    public Camera playerCamera;
    public AudioSource weaponAudioSource;
    public Animator weaponAnimator;

    [Header("Recoil")]
    public float recoilKickback = 0.1f;
    public float recoilRotation = 2f;

    private WeaponEntry currentWeapon;
    private WeaponSlot currentSlot;
    private float lastFireTime;
    private bool isReloading;
    private float reloadTimer;
    private bool isADS;
    private Vector3 originalWeaponPos;

    public bool IsFiring { get; private set; }
    public bool IsReloading => isReloading;
    public WeaponEntry CurrentWeapon => currentWeapon;
    public WeaponSlot CurrentSlot => currentSlot;

    void Start()
    {
        originalWeaponPos = weaponHolder.localPosition;
        SwitchWeapon(startingWeapon);
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleWeaponSwitching();
        HandleReload();
        HandleADS();

        if (currentWeapon == null) return;

        if (currentWeapon.stats.isAutomatic)
        {
            if (Input.GetButton("Fire1") && CanFire())
                Fire();
        }
        else
        {
            if (Input.GetButtonDown("Fire1") && CanFire())
                Fire();
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
            StartReload();
    }

    bool CanFire()
    {
        if (isReloading) return false;
        if (currentWeapon.currentAmmo <= 0)
        {
            PlayEmptySound();
            return false;
        }
        if (Time.time - lastFireTime < currentWeapon.stats.fireRate) return false;

        return true;
    }

    void Fire()
    {
        currentWeapon.currentAmmo--;
        lastFireTime = Time.time;
        IsFiring = true;

        PlayFireSound();

        ApplyRecoil();

        if (weaponAnimator != null)
            weaponAnimator.SetTrigger("Fire");

        if (currentWeapon.stats.isProjectile && currentWeapon.stats.projectilePrefab != null)
        {
            FireProjectile();
        }
        else
        {
            FireHitscan();
        }
    }

    void FireHitscan()
    {
        Vector3 spread = Random.insideUnitCircle * currentWeapon.stats.spread;
        Vector3 direction = playerCamera.transform.forward +
            playerCamera.transform.right * spread.x +
            playerCamera.transform.up * spread.y;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, direction.normalized, out hit, 300f))
        {
            HealthSystem health = hit.collider.GetComponentInParent<HealthSystem>();
            if (health != null)
            {
                DamageSystem.BodyPart part = DamageSystem.GetBodyPartFromRaycast(hit);
                health.TakeDamage(currentWeapon.stats.damage, part);
                GameManager.Instance?.AddKill();
            }
        }
    }

    void FireProjectile()
    {
        GameObject proj = Instantiate(currentWeapon.stats.projectilePrefab,
            weaponHolder.position, playerCamera.transform.rotation);

        BulletProjectile bullet = proj.GetComponent<BulletProjectile>();
        if (bullet != null)
        {
            bullet.Initialize(playerCamera.transform.forward);
        }

        YasinRPG_Projectile rpg = proj.GetComponent<YasinRPG_Projectile>();
        if (rpg != null)
        {
            rpg.Initialize(playerCamera.transform.forward, currentWeapon.stats.projectileSpeed);
        }
    }

    void ApplyRecoil()
    {
        float recoilX = Random.Range(-recoilRotation * 0.5f, recoilRotation * 0.5f);
        float recoilY = Random.Range(recoilRotation * 0.5f, recoilRotation);

        Vector3 kick = new Vector3(
            -currentWeapon.stats.recoilAmount * 10f,
            Random.Range(-currentWeapon.stats.recoilAmount * 5f, currentWeapon.stats.recoilAmount * 5f),
            0
        );

        weaponHolder.localPosition += kick;
    }

    void HandleWeaponSwitching()
    {
        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchWeapon(WeaponSlot.AssaultRifle);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchWeapon(WeaponSlot.SniperRifle);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchWeapon(WeaponSlot.RPG);
    }

    public void SwitchWeapon(WeaponSlot slot)
    {
        WeaponEntry entry = weapons.Find(w => w.slot == slot);
        if (entry == null || entry.weaponPrefab == null) return;

        foreach (Transform child in weaponHolder)
            Destroy(child.gameObject);

        currentSlot = slot;
        currentWeapon = entry;

        GameObject instance = Instantiate(entry.weaponPrefab, weaponHolder);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        isReloading = false;
        reloadTimer = 0f;
        weaponHolder.localPosition = originalWeaponPos;
    }

    void HandleReload()
    {
        if (!isReloading) return;

        reloadTimer -= Time.deltaTime;
        if (reloadTimer <= 0f)
        {
            int needed = currentWeapon.stats.magSize - currentWeapon.currentAmmo;
            int available = Mathf.Min(needed, currentWeapon.reserveAmmo);
            currentWeapon.currentAmmo += available;
            currentWeapon.reserveAmmo -= available;
            isReloading = false;
        }
    }

    public void StartReload()
    {
        if (isReloading) return;
        if (currentWeapon == null) return;
        if (currentWeapon.currentAmmo >= currentWeapon.stats.magSize) return;
        if (currentWeapon.reserveAmmo <= 0) return;

        isReloading = true;
        reloadTimer = currentWeapon.stats.reloadTime;

        if (weaponAnimator != null)
            weaponAnimator.SetTrigger("Reload");

        if (weaponAudioSource != null && currentWeapon.reloadSound != null)
            weaponAudioSource.PlayOneShot(currentWeapon.reloadSound);
    }

    public void AddAmmo(WeaponSlot slot, int amount)
    {
        WeaponEntry entry = weapons.Find(w => w.slot == slot);
        if (entry != null)
            entry.reserveAmmo += amount;
    }

    void PlayFireSound()
    {
        if (weaponAudioSource != null && currentWeapon.fireSound != null)
            weaponAudioSource.PlayOneShot(currentWeapon.fireSound);
    }

    void PlayEmptySound()
    {
        if (weaponAudioSource != null && currentWeapon.emptySound != null)
            weaponAudioSource.PlayOneShot(currentWeapon.emptySound);
    }

    void HandleADS()
    {
        if (Input.GetButton("Fire2"))
        {
            isADS = true;
            weaponHolder.localPosition = Vector3.Lerp(weaponHolder.localPosition,
                originalWeaponPos + Vector3.forward * 0.3f,
                currentWeapon.stats.aimDownSightSpeed * Time.deltaTime * 10f);

            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 50f,
                currentWeapon.stats.aimDownSightSpeed * Time.deltaTime * 10f);
        }
        else
        {
            isADS = false;
            weaponHolder.localPosition = Vector3.Lerp(weaponHolder.localPosition,
                originalWeaponPos, currentWeapon.stats.aimDownSightSpeed * Time.deltaTime * 10f);

            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 70f,
                currentWeapon.stats.aimDownSightSpeed * Time.deltaTime * 10f);
        }
    }

    public void SetADS(bool aiming)
    {
        isADS = aiming;
    }

    public void FireFromTouch()
    {
        if (CanFire() && currentWeapon != null)
            Fire();
    }
}
