using UnityEngine;

namespace Siroshton.Masaya.Component
{

    public class DestroyObjects : MonoBehaviour
    {
        [SerializeField] private GameObject[] _objects;
        [SerializeField] private Behaviour[] _behaviours;

        public GameObject[] objects { get => _objects; set => _objects = value; }
        public Behaviour[] behaviours { get => _behaviours; set => _behaviours = value; }

        public void Destroy()
        {
            if(_behaviours != null )
            {
                foreach(Behaviour b in _behaviours) Destroy(b);
            }

            if (_objects != null)
            {
                foreach (GameObject o in _objects) Destroy(o);
            }
        }
    }

}