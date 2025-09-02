using UnityEditor;
using Siroshton.Masaya.Animation;
using UnityEngine;
using UnityEngine.Events;

namespace Siroshton.Masaya.Editor.Animation
{
    [CustomEditor(typeof(AnimatorEvents))]
    public class AnimatorEventsEditor : UnityEditor.Editor
    {
        private SerializedProperty _animator;
        private SerializedProperty _clipEvents;

        private void OnEnable()
        {
            _animator = serializedObject.FindProperty("_animator");
            _clipEvents = serializedObject.FindProperty("_clipEvents");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_animator);

            AnimatorEvents animatorEvents = serializedObject.targetObject as AnimatorEvents;

            Animator animator = _animator.objectReferenceValue as Animator;
            if( animator != null )
            {
                for (int i=0;i<animator.runtimeAnimatorController.animationClips.Length;i++)
                {
                    AnimationClip clip = animator.runtimeAnimatorController.animationClips[i];

                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.LabelField(clip.name);

                    for (int ci=0; ci < _clipEvents.arraySize; ci++)
                    {
                        SerializedProperty clipEvent = _clipEvents.GetArrayElementAtIndex(ci);
                        if (clipEvent.objectReferenceValue)
                        {
                            SerializedObject clipRef = new SerializedObject(clipEvent.objectReferenceValue);
                            SerializedProperty clipIndex = clipRef.FindProperty("_clipIndex");
                            SerializedProperty time = clipRef.FindProperty("_time");
                            SerializedProperty eventHandler = clipRef.FindProperty("_event");

                            if (clipIndex.intValue == i)
                            {
                                clipRef.Update();
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.PropertyField(time);
                                if (GUILayout.Button("-"))
                                {
                                    // Remove item.
                                    _clipEvents.DeleteArrayElementAtIndex(ci);
                                    break;
                                }
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.PropertyField(eventHandler);
                                clipRef.ApplyModifiedProperties();
                            }
                        }
                    }


                    if( GUILayout.Button("+") )
                    {
                        AnimationClipEvent clipEvent = ScriptableObject.CreateInstance<AnimationClipEvent>();
                        clipEvent.clipIndex = i;
                        //animatorEvents.clipEvents.Add(clipEvent);
                        _clipEvents.arraySize += 1;
                        _clipEvents.GetArrayElementAtIndex(_clipEvents.arraySize - 1).objectReferenceValue = clipEvent;
                    }

                    EditorGUILayout.EndVertical();
                }

            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}