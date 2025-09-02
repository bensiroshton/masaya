using Siroshton.Masaya.Event;
using UnityEditor;

namespace Siroshton.Masaya.Editor.Event
{
    [CustomEditor(typeof(TimedEventSequence))]
    public class TimedEventSequenceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}

