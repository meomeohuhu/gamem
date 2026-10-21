using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace SpaceExplore
{
    public sealed class PlayerShooter : MonoBehaviour
    {
        [SerializeField] private LaserProjectile laserPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireCooldown = 0.22f;
        [SerializeField] private float rapidFireMultiplier = 2.5f;
        [SerializeField] private float spreadAngle = 13f;
        [SerializeField] private float shotSpacing = .18f;
        [SerializeField] private float damageMultiplier = 2f;
        private float nextFireTime;
        private bool rapidFireFromManager;
        private float rapidFireUntil;
        private float doubleShotUntil;
        private float spreadShotUntil;
        private float piercingShotUntil;
        private float damageUpUntil;

        public bool RapidFireActive => rapidFireFromManager || Time.time < rapidFireUntil;
        public float CurrentFireCooldown => RapidFireActive ? fireCooldown / Mathf.Max(1f, rapidFireMultiplier) : fireCooldown;
        public bool DoubleShotActive => Time.time < doubleShotUntil;
        public bool SpreadShotActive => Time.time < spreadShotUntil;
        public bool PiercingShotActive => Time.time < piercingShotUntil;
        public bool DamageUpActive => Time.time < damageUpUntil;
        public float ActiveUpgradeRemaining
        {
            get
            {
                float remaining = 0f;
                if (RapidFireActive && !rapidFireFromManager) remaining = Mathf.Max(remaining, rapidFireUntil - Time.time);
                if (DoubleShotActive) remaining = Mathf.Max(remaining, doubleShotUntil - Time.time);
                if (SpreadShotActive) remaining = Mathf.Max(remaining, spreadShotUntil - Time.time);
                if (PiercingShotActive) remaining = Mathf.Max(remaining, piercingShotUntil - Time.time);
                if (DamageUpActive) remaining = Mathf.Max(remaining, damageUpUntil - Time.time);
                return remaining;
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed && Time.time >= nextFireTime) Fire();
        }

        public void SetRapidFire(bool enabled) => rapidFireFromManager = enabled;

        public void ActivateWeaponUpgrade(WeaponUpgradeType type, float duration)
        {
            float expiresAt = Mathf.Max(Time.time, GetExpiry(type)) + Mathf.Max(.1f, duration);
            switch (type)
            {
                case WeaponUpgradeType.RapidFire: rapidFireUntil = expiresAt; break;
                case WeaponUpgradeType.DoubleShot: doubleShotUntil = expiresAt; break;
                case WeaponUpgradeType.SpreadShot: spreadShotUntil = expiresAt; break;
                case WeaponUpgradeType.PiercingShot: piercingShotUntil = expiresAt; break;
                case WeaponUpgradeType.DamageUp: damageUpUntil = expiresAt; break;
            }
        }

        public void ResetWeaponUpgrades()
        {
            rapidFireFromManager = false;
            rapidFireUntil = 0f;
            doubleShotUntil = 0f;
            spreadShotUntil = 0f;
            piercingShotUntil = 0f;
            damageUpUntil = 0f;
            nextFireTime = 0f;
        }

        public string GetActiveUpgradeHudText()
        {
            List<string> active = new();
            if (RapidFireActive) active.Add("RAPID");
            if (DoubleShotActive) active.Add("DOUBLE");
            if (SpreadShotActive) active.Add("SPREAD");
            if (PiercingShotActive) active.Add("PIERCE");
            if (DamageUpActive) active.Add("DAMAGE");
            return active.Count == 0 ? string.Empty : string.Join(" + ", active);
        }

        public void Fire()
        {
            if (laserPrefab == null) return;
            nextFireTime = Time.time + CurrentFireCooldown;
            Transform origin = firePoint != null ? firePoint : transform;
            bool spread = SpreadShotActive;
            bool doubleShot = DoubleShotActive && !spread;
            int damage = DamageUpActive ? Mathf.CeilToInt(damageMultiplier) : 1;
            if (spread)
            {
                SpawnLaser(origin.position + Vector3.left * shotSpacing, Quaternion.Euler(0f, 0f, spreadAngle), damage);
                SpawnLaser(origin.position, Quaternion.identity, damage);
                SpawnLaser(origin.position + Vector3.right * shotSpacing, Quaternion.Euler(0f, 0f, -spreadAngle), damage);
            }
            else if (doubleShot)
            {
                SpawnLaser(origin.position + Vector3.left * shotSpacing * .55f, Quaternion.identity, damage);
                SpawnLaser(origin.position + Vector3.right * shotSpacing * .55f, Quaternion.identity, damage);
            }
            else
            {
                SpawnLaser(origin.position, Quaternion.identity, damage);
            }

            SpriteRenderer laserRenderer = laserPrefab.GetComponent<SpriteRenderer>();
            if (laserRenderer != null && laserRenderer.sprite != null)
            {
                GameObject flash = new("Muzzle Flash");
                flash.transform.position = origin.position;
                flash.transform.localScale = Vector3.one * .75f;
                SpriteRenderer flashRenderer = flash.AddComponent<SpriteRenderer>();
                flashRenderer.sprite = laserRenderer.sprite;
                flashRenderer.color = new Color(.75f, 1f, 1f, .9f);
                flashRenderer.sortingOrder = 8;
                Destroy(flash, .06f);
            }
        }

        private void SpawnLaser(Vector3 position, Quaternion rotation, int damage)
        {
            LaserProjectile projectile = Instantiate(laserPrefab, position, rotation);
            Vector2 direction = rotation * Vector2.up;
            projectile.Initialize(direction, damage, PiercingShotActive);
        }

        private float GetExpiry(WeaponUpgradeType type)
        {
            return type switch
            {
                WeaponUpgradeType.RapidFire => rapidFireUntil,
                WeaponUpgradeType.DoubleShot => doubleShotUntil,
                WeaponUpgradeType.SpreadShot => spreadShotUntil,
                WeaponUpgradeType.PiercingShot => piercingShotUntil,
                WeaponUpgradeType.DamageUp => damageUpUntil,
                _ => 0f
            };
        }
    }
}
