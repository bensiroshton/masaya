
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Component
{
    public class Reparent : MonoBehaviour
    {
        [SerializeField] private Transform _newParent;

        public void SetParent()
        {
            transform.SetParent(_newParent, true);
        }

    }
}
