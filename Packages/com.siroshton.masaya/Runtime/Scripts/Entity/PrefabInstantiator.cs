using Siroshton.Masaya.Core;
using Siroshton.Masaya.Util;
using UnityEngine;
using static Siroshton.Masaya.Core.Types;

namespace Siroshton.Masaya.Entity
{
    public class PrefabInstantiator : MonoBehaviour, IInstantiator<GameObject>
    {
        [SerializeField] private NextSelectionType _selectionType;
        [SerializeField] private GameObject[] _prefabs;

        private int _lastSelected = -1;

        public GameObject Instantiate()
        {
            return Instantiate(transform.position, transform.rotation);
        }

        public GameObject Instantiate(Vector3 position, Quaternion rotation)
        {
            if (_prefabs == null || _prefabs.Length == 0) return null;
            _lastSelected = SelectionUtil.SelectNext(_selectionType, _lastSelected, _prefabs.Length);
            if (_prefabs[_lastSelected] == null) return null;

            return GameObject.Instantiate(_prefabs[_lastSelected], position, rotation);
        }

    }
}