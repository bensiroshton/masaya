using UnityEngine;

namespace Siroshton.Masaya.Player
{

    public class PlayerDelegate : MonoBehaviour
    {

        [SerializeField] private Transform _lookAtTarget;
        [SerializeField, Range(0, 1)] private float _lookAtTargetWeight = 0.75f;

        private Player _player;

        public Transform lookAtTarget
        { 
            get => _lookAtTarget; 
            set
            {
                if (_lookAtTarget != null && value == null ) _player.lookAtTargetWeight = 0;
                _lookAtTarget = value as Transform;
            }
        }

        public float lookAtTargetWeight { get => _lookAtTargetWeight; set => _lookAtTargetWeight = value; }

        private void OnDisable()
        {
            if( _lookAtTarget != null ) _player.lookAtTargetWeight = 0;
        }

        private void Awake()
        {
            _player = Player.instance;
        }

        public void ClearLookAtTarget()
        {
            _lookAtTarget = null;
            _player.lookAtTargetWeight = 0;
        }

        public void PlayAudio(AudioClip clip)
        {
            _player.PlayAudio(clip);
        }

        public void Wait()
        {
            _player.Wait();
        }

        public void SetPlaying()
        {
            _player.SetPlaying();
        }

        public void PointAt(Transform transform)
        {
            Vector3 target = transform.position;
            target.y = _player.transform.position.y;
            _player.transform.LookAt(target, Vector3.up);
        }

        public void SetParent(Transform parent)
        {
            if( parent == null ) SetDefaultParent();
            else _player.transform.SetParent(parent);
        }

        public void SetDefaultParent()
        {
            _player.SetDefaultParent();
        }

        private void LateUpdate()
        {
            if( _lookAtTarget != null )
            {
                _player.lookAtTarget = _lookAtTarget.position;
                _player.lookAtTargetWeight = _lookAtTargetWeight;
            }
        }
    }

}