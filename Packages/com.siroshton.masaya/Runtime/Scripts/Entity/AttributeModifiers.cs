using System;
using System.Collections.Generic;
using UnityEngine;

namespace Siroshton.Masaya.Entity
{
    [Serializable]
    public class AttributeModifiers
    {
        [Flags]
        public enum Modifier
        {
            Speed = 1,
            Damage = 2,
            FireRate = 4,
            BulletSpeed = 8,
            BulletRange = 16,
            CriticalChance = 32,
            CriticalDamage = 64,
            BulletSize = 128,
        }

        [SerializeField] private float _speedModifier = 1;
        [SerializeField] private float _damageModifier = 1;
        [SerializeField] private float _fireRateModifier = 1;
        [SerializeField] private float _bulletSpeedModifier = 1;
        [SerializeField] private float _bulletRangeModifier = 1;
        [SerializeField] private float _bulletSizeModifier = 1;
        [SerializeField, Range(0, 1)] private float _criticalChance = 0;
        [SerializeField] private float _criticalDamageModifier = 1;

        public float speedModifier { get => _speedModifier; set => _speedModifier = value; }
        public float damageModifier { get => _damageModifier; set => _damageModifier = value; }
        public float fireRateModifier { get => _fireRateModifier; set => _fireRateModifier = value; }
        public float bulletSpeedModifier { get => _bulletSpeedModifier; set => _bulletSpeedModifier = value; }
        public float bulletRangeModifier { get => _bulletRangeModifier; set => _bulletRangeModifier = value; }
        public float bulletSizeModifier { get => _bulletSizeModifier; set => _bulletSizeModifier = value; }
        public float criticalChance { get => _criticalChance; set => _criticalChance = value; }
        public float criticalDamageModifier { get => _criticalDamageModifier; set => _criticalDamageModifier = value; }

        public void Reset()
        {
            _speedModifier = 1;
            _damageModifier = 1;
            _fireRateModifier = 1;
            _bulletSpeedModifier = 1;
            _bulletRangeModifier = 1;
            _bulletSizeModifier = 1;
            _criticalChance = 0;
            _criticalDamageModifier = 1;
        }

        public void AddModifier(Modifier modifier, float value)
        {
            if ((modifier & Modifier.Speed) > 0 ) _speedModifier += value;
            if ((modifier & Modifier.Damage) > 0) _damageModifier += value;
            if ((modifier & Modifier.FireRate) > 0) _fireRateModifier += value;
            if ((modifier & Modifier.BulletSpeed) > 0) _bulletSpeedModifier += value;
            if ((modifier & Modifier.BulletRange) > 0) _bulletRangeModifier += value;
            if ((modifier & Modifier.BulletSize) > 0) _bulletSizeModifier += value;
            if ((modifier & Modifier.CriticalChance) > 0) _criticalChance += value;
            if ((modifier & Modifier.CriticalDamage) > 0) _criticalDamageModifier += value;
        }

        private void AddStat(List<string> stats, string name, float value, Color32 color, bool useDirect = false)
        {
            string rgb = "#" + ColorUtility.ToHtmlStringRGB(color);

            if ( useDirect ) 
            {
                if( value > 0 ) stats.Add($"<color=white>{name} <color={rgb}>+{Mathf.RoundToInt(value * 100.0f)}%");
            }
            else if (value > 1) stats.Add($"<color=white>{name} <color={rgb}>+{Mathf.RoundToInt((value - 1.0f) * 100.0f)}%");
            else if (value < 1) stats.Add($"<color=white>{name} <color={rgb}>-{Mathf.RoundToInt((1.0f - value) * 100.0f)}%");
        }

        public List<string> GetStats()
        {
            List<string> stats = new List<string>();

            Color32 red = new Color32(251, 24, 45, 255);
            Color32 green = new Color32(131, 251, 75, 255);
            Color32 blue = new Color32(28, 244, 251, 255);
            Color32 yellow = new Color32(251, 236, 24, 255);
            Color32 orange = new Color32(251, 146, 28, 255);
            Color32 pink = new Color32(251, 108, 205, 255);

            AddStat(stats, "Speed", _speedModifier, green);
            AddStat(stats, "Damage", _damageModifier, red);
            AddStat(stats, "Fire Rate", _fireRateModifier, blue);
            AddStat(stats, "Missle Speed", _bulletSpeedModifier, orange);
            AddStat(stats, "Missle Range", _bulletRangeModifier, orange);
            AddStat(stats, "Missle Size", _bulletSizeModifier, pink);
            AddStat(stats, "Critical Chance", _criticalChance, yellow, true);
            AddStat(stats, "Critical Damage", _criticalDamageModifier, yellow);

            return stats;
        }
    }
}