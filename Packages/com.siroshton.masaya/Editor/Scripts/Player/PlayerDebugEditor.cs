using Siroshton.Masaya.Environment;
using Siroshton.Masaya.Player;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Siroshton.Masaya.Editor.Player
{
    [CustomEditor(typeof(PlayerDebug))]
    public class PlayerDebugEditor : UnityEditor.Editor
    {
        private SerializedProperty _equipment;
        
        private List<Area> _areas = new List<Area>();
        private bool _areaFoldout = true;
        private bool _equipmentFoldout = true;

        private void OnEnable()
        {
            _equipment = serializedObject.FindProperty("_equipment");

            CollectAreas();
        }

        private void CollectAreas()
        {
            _areas.Clear();

            Scene scene = SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject o = roots[i];
                if (o.GetComponent<Area>() is Area a) _areas.Add(a);
            }

            _areas.Sort((a, b) => { return a.name.CompareTo(b.name); });
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PlayerDebug playerD = target as PlayerDebug;
            Masaya.Player.Player player = playerD.player;

            if( player.isInvincible ) EditorGUILayout.LabelField("--> Player is Invincible <--", EditorStyles.boldLabel);

            if(_areas != null)
            {
                _areaFoldout = EditorGUILayout.Foldout(_areaFoldout, "Areas");
                if(_areaFoldout )
                {
                    for (int i = 0; i < _areas.Count; i++)
                    {
                        Area area = _areas[i];
                        if( area == null )
                        {
                            CollectAreas();
                            break;
                        }
                        if (GUILayout.Button(area.name))
                        {
                            area.Editor_TransportPlayer(player);
                        }
                    }
                }

                if (Application.isPlaying)
                {
                    if (playerD.equipment != null)
                    {
                        _equipmentFoldout = EditorGUILayout.Foldout(_equipmentFoldout, "Equipment");
                        if (_equipmentFoldout)
                        {
                            for (int i = 0; i < playerD.equipment.Length; i++)
                            {
                                GameObject o = playerD.equipment[i];
                                if (o == null) continue;

                                IEquipment e = o.GetComponent<IEquipment>();
                                if (e == null) continue;

                                if (GUILayout.Button(o.name))
                                {
                                    o = GameObject.Instantiate<GameObject>(o);
                                    e = o.GetComponent<IEquipment>();
                                    playerD.player.Equip(e);
                                }
                            }

                        }
                    }

                    EditorGUILayout.Foldout(true, "Spawn Experience");
                    if (GUILayout.Button("distance 1")) player.SpawnExperience(player.transform.forward * 1.0f + Vector3.up, 1.0f, 100);
                    if (GUILayout.Button("distance 3")) player.SpawnExperience(player.transform.forward * 3.0f + Vector3.up, 1.0f, 100);
                    if (GUILayout.Button("distance 5")) player.SpawnExperience(player.transform.forward * 5.0f + Vector3.up, 1.0f, 100);
                    if (GUILayout.Button("distance 8")) player.SpawnExperience(player.transform.forward * 8.0f + Vector3.up, 1.0f, 100);
                    if (GUILayout.Button("distance 10")) player.SpawnExperience(player.transform.forward * 10.0f + Vector3.up, 1.0f, 100);
                }
            }

            EditorGUILayout.PropertyField(_equipment);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

