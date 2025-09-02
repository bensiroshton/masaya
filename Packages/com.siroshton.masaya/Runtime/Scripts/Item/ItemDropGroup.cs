using Siroshton.Masaya.Math;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Item
{
    [Serializable]
    public class ItemDropGroup
    {
        [Tooltip("Chance these items will drop, between 1 and 100.")]
        [Range(1, 100)]
        [SerializeField] private int _chance = 1;
        [Tooltip("Min/Max number of these items that will drop.")]
        [SerializeField] private IntervalInt _dropCount = new IntervalInt(1, 1);
        [Tooltip("Prefab list.")]
        [SerializeField] private GameObject[] _items;
        [Tooltip("Gaurantee a drop if none are achieved after X failed attempts.  Set to 0 to disable.")]
        [SerializeField] private int _gauranteeDropAfterXFails = 0;
        [SerializeField] private int _disableGauranteeWhenChanceIsBelow = 0;
        [SerializeField] private int _chanceReductionAfterDrop = 0;
        [SerializeField] private int _mininumChanceAfterReductions = 0;

        private int _failedDrops;

        public int chance { get => _chance; set => _chance = value; }
        public IntervalInt dropCount { get => _dropCount; set => _dropCount = value; }
        public GameObject[] items { get => _items; set => _items = value; }
        public int gauranteeDropAfterXFails { get => _gauranteeDropAfterXFails; set => _gauranteeDropAfterXFails = value; }
        public int disableGauranteeWhenChanceIsBelow { get => _disableGauranteeWhenChanceIsBelow; set => _disableGauranteeWhenChanceIsBelow = value; }
        public int failedDrops { get => _failedDrops; set => _failedDrops = value; }
        public int chanceReductionAfterDrop { get => _chanceReductionAfterDrop; set => _chanceReductionAfterDrop = value; }
        public int mininumChanceAfterReductions { get => _mininumChanceAfterReductions; set => _mininumChanceAfterReductions = value; }

    }
}