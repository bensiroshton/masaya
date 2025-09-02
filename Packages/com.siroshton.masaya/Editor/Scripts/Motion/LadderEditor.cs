using UnityEngine;
using UnityEditor;
using Siroshton.Masaya.Motion;

namespace Siroshton.Masaya.Editor.Motion
{
    [CustomEditor(typeof(Ladder))]
    public class LadderEditor : UnityEditor.Editor
    {

        private void OnSceneGUI()
        {
            UnityEngine.Event e = UnityEngine.Event.current;
            if( e.type == EventType.MouseDown && e.button == 0 )
            {
                Vector3 pos;
                RaycastHit hit;
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if(UnityEngine.Physics.Raycast(ray, out hit))
                {
                    if( Vector3.Angle(Vector3.up, hit.normal) > 60 )
                    {
                        Ladder ladder = target as Ladder;
                        pos = hit.point;
                        pos.y = hit.collider.bounds.min.y;
                        ladder.transform.position = pos;
                        Quaternion rotation = Quaternion.identity;
                        rotation.SetLookRotation(-hit.normal);
                        ladder.transform.rotation = rotation;
                        ladder.height = hit.collider.bounds.extents.y * 2;
                        e.Use();
                    }
                }

            }
        }

    }
}
