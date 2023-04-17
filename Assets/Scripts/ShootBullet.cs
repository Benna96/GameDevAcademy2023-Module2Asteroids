using UnityEngine;
using UnityEngine.InputSystem;

public class ShootBullet : MonoBehaviour
{
    [field: SerializeField] private GameObject bullet { get; set; }
    [field: SerializeField] private Transform shotPosition { get; set; }
    [field: SerializeField] private float shotForce { get; set; }
    [field: SerializeField] private float shotPeriod { get; set; }
    [field: SerializeField] private float bulletLifetime { get; set; }

    private AudioSource audioSource { get; set; }

    private bool shooting { get; set; } = false;
    private float shotCooldown { get; set; } = 0;

    private void Start() {
        audioSource = shotPosition.GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (shotCooldown <= 0 && shooting)
            Shoot();
    }

    private void Update()
    {
        if (shotCooldown > 0)
            shotCooldown -= Time.deltaTime;
    }

    private void Shoot()
    {
        GameObject newBullet = Instantiate(bullet, shotPosition.position, Quaternion.identity, transform.parent);
        PlaySound();
        Rigidbody2D newBulletRb = newBullet.GetComponent<Rigidbody2D>();
        newBulletRb.AddForce(transform.up * shotForce);
        Destroy(newBullet, bulletLifetime);
        shotCooldown = shotPeriod;
        shooting = false;
    }

    private void PlaySound()
    {
        audioSource.PlayOneShot(audioSource.clip);
    }

#pragma warning disable IDE0051 // Remove unused private members
    private void OnFire(InputValue value)
#pragma warning restore IDE0051 // Remove unused private members
    {
        // Allow faster shooting by spamming the button.
        shooting = value.Get<float>() > 0.5;
        if (value.Get<float>() == 1)
            shotCooldown = 0;

        if (shooting)
            GetComponent<PlayerController>().isInvincible = false;
    }
}
