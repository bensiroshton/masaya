using Siroshton.Masaya.Motion;
using UnityEngine;

namespace Siroshton.Masaya.Environment
{
    public class PathEmitter : Emitter
    {
        [Tooltip("Path to start objects on.  If not set then the emitters position is used.")]
        [SerializeField] private GameObjectPath _objectPath;

        [Tooltip("Max number of objects allowed on the path.  Set to < 0 to disable the check.  0 disables the emitter.")]
        [SerializeField] private int _maxObjectsOnPath = -1;

        public int maxObjectsOnPath { get => _maxObjectsOnPath; set => _maxObjectsOnPath = value; }

        override protected bool isEmissionOk 
        { 
            get
            {
                if( _objectPath == null ) return false;
                else if( _maxObjectsOnPath < 0 ) return true;
                else return _objectPath.objectCount < _maxObjectsOnPath;
            }
        }

        protected override void OnObjectEmitted(GameObject o)
        {
            if( _objectPath ) _objectPath.AddGameObject(o);
        }

    }
}
