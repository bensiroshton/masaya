using Siroshton.Masaya.Path;
using UnityEditor;
using UnityEngine;

namespace Siroshton.Masaya.Editor.Path
{

    [CustomEditor(typeof(FixedPointProvider)), CanEditMultipleObjects]
    public class FixedPointProviderEditor : UnityEditor.Editor
    {

        protected virtual void OnSceneGUI()
        {
            FixedPointProvider p = (FixedPointProvider)target;

            if( p.type == FixedPointProvider.PointType.Manual )
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newPoint = Handles.PositionHandle(p.point, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(p, "Moved point");
                    p.point = newPoint;
                }
            }
        }

    }
}