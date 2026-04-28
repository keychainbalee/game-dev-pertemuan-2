using UnityEngine;
using System.Collections;
using TMPro;

public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject weaponObject;
    [SerializeField] private Transform firePoint;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shootForce = 30f;
    [SerializeField] private float fireRate = 0.2f;

    [Header("Ammo")]
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private int totalAmmo = 60;
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private TextMeshProUGUI ammoText;

    private int currentAmmo;
    private bool isReloading;

    private bool weaponActive;
    private float nextFireTime;

    void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoUI();
    }
    void Update()
    {
        HandleWeaponToggle();
        HandleShooting();
        HandleReload();
    }

    private void HandleReload()
    {
        if (isReloading) return;

        if (inputHandler.ReloadTriggered)
        {
            if (currentAmmo < magazineSize && totalAmmo > 0)
            {
                StartCoroutine(Reload());
            }
        }
    }

    private void HandleWeaponToggle()
    {
        // Consume the toggle atomically via the input handler
        if (inputHandler.ConsumeToggleWeaponTriggered())
        {
            weaponActive = !weaponActive;
            weaponObject.SetActive(weaponActive);
        }
    }

    private void HandleShooting()
    {
        if (!weaponActive) return;
        if (!inputHandler.FireTriggered) return;
        if (Time.time < nextFireTime) return;

        if (currentAmmo <= 0)
        {
            Debug.Log("Peluru habis!");
            return;
        }

        nextFireTime = Time.time + fireRate;
        currentAmmo--;
        Shoot();
        UpdateAmmoUI();
    }

    private void Shoot()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            targetPoint = hit.point;
            Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 100f;
        }

        // arah dari muzzle ke titik crosshair
        Vector3 direction = (targetPoint - firePoint.position).normalized;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * shootForce;
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        int neededAmmo = magazineSize - currentAmmo;
        int ammoToReload = Mathf.Min(neededAmmo, totalAmmo);

        currentAmmo += ammoToReload;
        totalAmmo -= ammoToReload;

        Debug.Log("Reload selesai");
        UpdateAmmoUI();

        isReloading = false;
    }

    private void UpdateAmmoUI()
    {
        ammoText.text = currentAmmo + " / " + totalAmmo;
    }


}