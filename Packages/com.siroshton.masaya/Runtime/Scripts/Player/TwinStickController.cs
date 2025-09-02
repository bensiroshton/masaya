using Siroshton.Masaya.Core;
using Siroshton.Masaya.Weapon;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Siroshton.Masaya.Player
{

    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(PlayerInput))]
    public class TwinStickController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _inputDeadSpace = 0.1f;

        private Player _player;
        private Vector3 _moveDir;
        private Vector3 _aimDir;

        private void Start()
        {
            if(_camera == null) _camera = Camera.main;
            _player = GetComponent<Player>();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            float mag = input.magnitude;

            if ( mag >= _inputDeadSpace )
            {
                input = input.normalized;
                float rotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + _camera.transform.eulerAngles.y;
                _moveDir = Quaternion.Euler(0.0f, rotation, 0.0f) * Vector3.forward * mag;
            }
            else
            {
                _moveDir = Vector3.zero;
            }
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            float mag = input.magnitude;

            if (mag >= _inputDeadSpace)
            {
                input = input.normalized;
                float rotation = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + _camera.transform.eulerAngles.y;
                _aimDir = Quaternion.Euler(0.0f, rotation, 0.0f) * Vector3.forward * mag;
            }
            else
            {
                _aimDir = Vector3.zero;
            }
        }

        public void OnAction1(InputAction.CallbackContext context)
        {
            if (GameState.isPaused) return;

            if( context.started )
            {
                // context.ReadValueAsButton();
            }
        }

        public void OnAction2(InputAction.CallbackContext context)
        {
            if (GameState.isPaused) return;

            if (context.started)
            {
                // context.ReadValueAsButton();
            }
        }

        private void Update()
        {
            _player.Move(_moveDir);
            _player.Aim(_aimDir);
        }


    }

}
