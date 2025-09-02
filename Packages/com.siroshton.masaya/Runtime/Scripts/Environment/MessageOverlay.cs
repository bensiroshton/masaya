
using Siroshton.Masaya.Core;
using Siroshton.Masaya.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Environment
{

    public class MessageOverlay : MonoBehaviour, IInteractible
    {
        [SerializeField, TextArea] private string _message;
        [SerializeField] private bool _showButton;
        [SerializeField] UnityEvent _onButton;
        [SerializeField, Range(0.1f, 2.0f)] float _scale = 1;

        private MessageOverlayUI _spawnedMessage;

        public string message 
        { 
            get => _message; 
            set
            {
                _message = value;
                if(_spawnedMessage != null) _spawnedMessage.message = value;
            }
        }

        public bool showButton
        {
            get => _showButton;
            set
            {
                _showButton = value;
                if (_spawnedMessage != null) _spawnedMessage.showButton = value;
            }
        }

        public float scale { get => _scale; set => _scale = value; }

        public bool isReadyForInteraction
        {
            get => enabled && _showButton;
        }

        public void TriggerInteraction()
        {
            if (_onButton != null) _onButton.Invoke();
        }

        private void OnEnable()
        {
            if( _spawnedMessage != null ) _spawnedMessage.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (_spawnedMessage != null) _spawnedMessage.gameObject.SetActive(false);
        }

        private void Start()
        {
            _spawnedMessage = GameManager.instance.ShowNPCMessage(this);
            _spawnedMessage.gameObject.SetActive(enabled);
        }

        private void OnDestroy()
        {
            Destroy(_spawnedMessage);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            GUIStyle style = GUI.skin.label;
            style.alignment = TextAnchor.MiddleCenter;
            UnityEditor.Handles.Label(transform.position, _message, style);
        }
#endif

    }

}