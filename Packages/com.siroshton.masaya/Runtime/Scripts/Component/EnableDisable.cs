
using UnityEngine;
using UnityEngine.AI;

namespace Siroshton.Masaya.Component
{

    public class EnableDisable : MonoBehaviour
    {
        [SerializeField] private GameObject[] _objects;
        [SerializeField] private Behaviour[] _components;

        public void Enable()
        {
            SetEnabled(true);
        }

        public void Disable()
        {
            SetEnabled(false);
        }

        public void SetEnabled(bool enabled)
        {
            if (_objects != null)
            {
                foreach (GameObject o in _objects)
                {
                    if (o != null) o.SetActive(enabled);
                }
            }

            if (_components != null)
            {
                foreach (Behaviour b in _components)
                {
                    if (b != null) b.enabled = enabled;
                }
            }
        }

    }

}