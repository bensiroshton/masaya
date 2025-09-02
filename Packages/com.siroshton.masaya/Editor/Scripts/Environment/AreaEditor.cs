using Siroshton.Masaya.Core;
using Siroshton.Masaya.Environment;
using Siroshton.Masaya.Math;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using static UnityEditor.Handles;

namespace Siroshton.Masaya.Editor.Environment
{
    [CustomEditor(typeof(Area))]
    public class AreaEditor : UnityEditor.Editor
    {
        private SerializedProperty _radiusExtension;
        private SerializedProperty _playerSpawnPoint;
        private SerializedProperty _deactivateChildrenOnStartup;
        private SerializedProperty _additionalObjects;
        private SerializedProperty _onPlayerEnter;
        private SerializedProperty _onPlayerExit;

        private void OnEnable()
        {
            _radiusExtension = serializedObject.FindProperty("_radiusExtension");
            /*
            _playerSpawnPoint = serializedObject.FindProperty("_playerSpawnPoint");
            _deactivateChildrenOnStartup = serializedObject.FindProperty("_deactivateChildrenOnStartup");
            _onPlayerEnter = serializedObject.FindProperty("_onPlayerEnter");
            _onPlayerExit = serializedObject.FindProperty("_onPlayerExit");
            */
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Area area = target as Area;

            DrawDefaultInspector();

            /*
            EditorGUILayout.PropertyField(_radiusExtension);
            EditorGUILayout.PropertyField(_playerSpawnPoint);
            EditorGUILayout.PropertyField(_deactivateChildrenOnStartup);
            EditorGUILayout.PropertyField(_onPlayerEnter);
            EditorGUILayout.PropertyField(_onPlayerExit);
            */

            if ( GUILayout.Button("Auto Configure") )
            {
                area.gameObject.layer = GameLayers.area;

                SphereCollider sc = area.gameObject.GetComponent<SphereCollider>();
                sc.isTrigger = true;

                bool hasBounds;
                Bounds bounds = MathUtil.GetChildBounds(area.transform, out hasBounds);
                if( hasBounds )
                {
                    Vector3 center = bounds.center - area.transform.position;
                    center.y = 0;
                    sc.center = center;
                    sc.radius = MathUtil.GetRadius(bounds) + _radiusExtension.floatValue;
                }
                else
                {
                    sc.center = Vector3.zero;
                    sc.radius = _radiusExtension.floatValue;
                }
                
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnSceneGUI()
        {
            Area area = target as Area;

            Handles.matrix = area.transform.localToWorldMatrix;
            
            EditorGUI.BeginChangeCheck();
            if(area.playerSpawnPoint == Vector3.zero)
            {
                area.playerSpawnPoint = new Vector3(0, 0, -area.radius);
            }
            float size = HandleUtility.GetHandleSize(area.worldPlayerSpawnPoint);
            if( Camera.current.orthographic ) size *= 0.15f;
            else size *= 0.015f;

            Vector3 pos = area.playerSpawnPoint;
            Handles.color = Color.green;
            pos = Handles.Slider2D(pos, Vector3.up, Vector3.right, Vector3.forward, size, Handles.RectangleHandleCap, 0, false);
            if ( EditorGUI.EndChangeCheck() )
            {
                Vector3 pickPos;
                Vector3 pickNorm;
                if(HandleUtility.PlaceObject(UnityEngine.Event.current.mousePosition, out pickPos, out pickNorm))
                {
                    pos.y = pickPos.y;
                }
                else
                {
                    pos.y = 0;
                }

                area.playerSpawnPoint = pos;
            }

            Vector3 top = area.playerSpawnPoint + Vector3.up * 1.0f;
            Handles.color = Color.magenta;
            Handles.DrawDottedLine(area.playerSpawnPoint, top, 6.0f);
            Handles.DrawWireDisc(top, Vector3.down, 0.1f);
        }

    }
}

