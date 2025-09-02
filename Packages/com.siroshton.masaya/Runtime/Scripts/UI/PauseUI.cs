using Siroshton.Masaya.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Siroshton.Masaya.UI
{
    public class PauseUI : MonoBehaviour
    {
        [SerializeField] private float _stickButtonThreshold = 0.5f;
        [SerializeField] private TMP_Text _sectionTitle;
        [SerializeField] private List<GameObject> _sectionObjects = new List<GameObject>();
        [SerializeField] private RawImage _backgroundImage;

        private GameObject _currentSectionObject;
        private IUISection _currentSection;
        private int _currentSectionIndex = 0;

        private struct MoveDirection
        {
            public bool up;
            public bool down;
            public bool left;
            public bool right;
            public bool any;
        }

        private MoveDirection _move;

        public UnityEngine.Texture backgroundImage
        {
            set
            {
                _backgroundImage.texture = value;
            }
        }

        private void Awake()
        {
            foreach(GameObject o in _sectionObjects) o.SetActive(false);
        }

        private void OnEnable()
        {
            if( _currentSectionIndex < _sectionObjects.Count )
            {
                SetSection(_sectionObjects[_currentSectionIndex]);
            }
        }

        private void OnDisable()
        {
            SetSection(null);
        }

        private void SetSection(GameObject o)
        {
            if(_currentSectionObject != null )
            {
                _currentSectionObject.SetActive(false);
                _currentSectionObject = null;
                _currentSection = null;
            }

            if (o != null)
            {
                _currentSectionObject = o;
                _currentSection = o.GetComponent<IUISection>();
                if(_currentSection != null ) _sectionTitle.text = _currentSection.title;
                else _sectionTitle.text = o.name;
                _currentSectionObject.SetActive(true);
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();

            if (input.y > -_stickButtonThreshold && input.y < _stickButtonThreshold)
            {
                _move.up = _move.down = false;
            }
            else if (input.y < -_stickButtonThreshold && !_move.down)
            {
                _move.down = true;
                OnMoveDown();
            }
            else if (input.y > _stickButtonThreshold && !_move.up)
            {
                _move.up = true;
                OnMoveUp();
            }

            if ( input.x > -_stickButtonThreshold && input.x < _stickButtonThreshold )
            {
                _move.left = _move.right = false;
            }
            else if ( input.x < -_stickButtonThreshold && !_move.left)
            {
                _move.left = true;
                OnMoveLeft();
            }
            else if (input.x > _stickButtonThreshold && !_move.right)
            {
                _move.right = true;
                OnMoveRight();
            }

            if (!_move.up && !_move.down && !_move.right && !_move.left)
            {
                _move.any = false;
            }
            else if( !_move.any && (_move.up || _move.down || _move.left || _move.right) )
            {
                _move.any = true;
                OnMove(input);
            }

        }

        private void OnMove(Vector2 direction)
        {
            _currentSection?.OnMovePushed(direction);
        }

        private void OnMoveUp()
        {
            _currentSection?.OnMoveUpPushed();
        }

        private void OnMoveDown()
        {
            _currentSection?.OnMoveDownPushed();
        }

        private void OnMoveLeft()
        {
            _currentSection?.OnMoveLeftPushed();
        }

        private void OnMoveRight()
        {
            _currentSection?.OnMoveRightPushed();
        }

        public void OnButton1(InputAction.CallbackContext context)
        {
            if (!context.started) return;

            _currentSection?.OnButton1Pushed();
        }

        public void OnButton2(InputAction.CallbackContext context)
        {
            if (!context.started) return;

            _currentSection?.OnButton2Pushed();
        }

        public void OnButton3(InputAction.CallbackContext context)
        {
            if (!context.started) return;

            _currentSection?.OnButton3Pushed();
        }

        public void OnButton4(InputAction.CallbackContext context)
        {
            if (!context.started) return;

            GameManager.instance.SetPaused(false);
        }

    }
}
