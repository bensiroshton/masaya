
using System;
using UnityEngine;
using static Siroshton.Masaya.Entity.AttributeModifiers;

namespace Siroshton.Masaya.Entity
{
    [Serializable]
    public class Attributes
    {
        [SerializeField] private float _speed = 1;
        [SerializeField] private int _maxHealth = 1;
        [SerializeField] private int _damage = 1;

        public float speed { get => _speed; set => _speed = value; }
        public int maxHealth { get => _maxHealth; set => _maxHealth = value; }
        public int damage { get => _damage; set => _damage = value; }

        public float GetSpeed(AttributeModifiers modifier)
        {
            return _speed * modifier.speedModifier;
        }

        public int GetDamage(AttributeModifiers modifier, out bool isCritcal)
        {
            isCritcal = modifier.criticalChance > 0 && UnityEngine.Random.Range(0.0f, 1.0f) <= modifier.criticalChance;
            float mod = isCritcal ? modifier.criticalDamageModifier : 1.0f;

            return Mathf.CeilToInt((float)_damage * modifier.damageModifier * mod);
        }



    }
}