using Siroshton.Masaya.Player;
using UnityEditor;

namespace Siroshton.Masaya.Editor.Player
{
    [CustomEditor(typeof(PlayerCamera))]
    public class PlayerCameraEditor : UnityEditor.Editor
    {
        private SerializedProperty _equipment;

        private void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            PlayerCamera cam = target as PlayerCamera;
            DrawDefaultInspector();

            cam.Editor_Update();
        }
    }
}

