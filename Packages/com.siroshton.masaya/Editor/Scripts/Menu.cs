
using Siroshton.Masaya.Animation;
using Siroshton.Masaya.Audio;
using Siroshton.Masaya.Component;
using Siroshton.Masaya.Editor.Util;
using Siroshton.Masaya.Environment;
using Siroshton.Masaya.Event;
using Siroshton.Masaya.Motion;
using Siroshton.Masaya.Navigation;
using Siroshton.Masaya.Util;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.Search;
using UnityEngine;

namespace Siroshton.Masaya.Editor
{

    public static class Menu
    {

        [MenuItem("CONTEXT/AudioSource/Copy 3D Settings To All Others (except GameManager)")]
        static private void CopyAudioSource(MenuCommand command)
        {
            AudioSource copyFrom = command.context as AudioSource;

            AssetUtil.FindInstances<AudioSource>(OnCopyAudioSourceItem, copyFrom);
        }

        static private bool OnCopyAudioSourceItem(AssetUtil.FoundDetails details)
        {
            if( details.data as AudioSource == details.component as AudioSource ) return false;
            else if (details.component.gameObject.GetComponent<DontCopyAudioSettings>() != null) return false;

            CopyAudioSource(details.data as AudioSource, details.component as AudioSource);

            return true;
        }

        static private void CopyAudioSource(AudioSource from, AudioSource to)
        {
            to.minDistance = from.minDistance;
            to.maxDistance = from.maxDistance;
            to.dopplerLevel = from.dopplerLevel;
            to.spread = from.spread;
            to.spatialBlend = from.spatialBlend;
            to.rolloffMode = from.rolloffMode;
            to.SetCustomCurve(AudioSourceCurveType.CustomRolloff, from.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
            to.SetCustomCurve(AudioSourceCurveType.SpatialBlend, from.GetCustomCurve(AudioSourceCurveType.SpatialBlend));
            to.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, from.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix));
            to.SetCustomCurve(AudioSourceCurveType.Spread, from.GetCustomCurve(AudioSourceCurveType.Spread));
        }

        [MenuItem("Masaya/Print World Position [Selected Object(s)]")]
        static private void PrintWorldPosition(MenuCommand command)
        {
            foreach(GameObject o in Selection.gameObjects)
            {
                Vector3 p = o.transform.TransformPoint(o.transform.localPosition); // we don't use transform.position since this does not apply any scaling.
                Vector3 r = o.transform.rotation.eulerAngles;
                string f = "0.######";
                Debug.Log($"[{o.name}] position: {p.x.ToString(f)}, {p.y.ToString(f)}, {p.z.ToString(f)}, rotation: {r.x.ToString(f)}, {r.y.ToString(f)}, {r.z.ToString(f)}, blender y rotation: {(180 - r.y).ToString(f)}");
            }
        }

#if true
        [MenuItem("Masaya/FindInstances")]
        static private void FindInstances(MenuCommand command)
        {
            int count = AssetUtil.FindInstances<Emitter>(OnFindInstancesItem);
        }

        static private bool OnFindInstancesItem(AssetUtil.FoundDetails details)
        {
            if( !details.isPrefab )
            {
                string name = details.component.name;
                Transform t = details.component.transform;
                while (t.parent != null)
                {
                    t = t.parent;
                    name = t.name + " -> " + name;
                }
                Debug.Log(name);
            }
            else
            {
                Debug.Log(details.assetPath);
            }

            return false;
        }
#endif

#if false
        [MenuItem("Masaya/Update Emitter")]
        static private void UpdateGuns(MenuCommand command)
        {
            AssetUtil.FindInstances<Emitter>(OnFoundObject<Emitter>);
        }

        static private bool OnFoundObject<T>(AssetUtil.FoundDetails details) where T : UnityEngine.Component
        {
            // Make sure the object implements a "public void Editor_UpdateObject()" method

            T obj = details.component as T;
            MethodInfo m = typeof(T).GetMethod("Editor_UpdateObject");
            if( m == null )
            {
                Debug.LogError($"Editor_UpdateObject() method not void on type {typeof(T).FullName}.");
                return false;
            }

            m.Invoke(obj, null);
            return true;
        }
#endif

        [MenuItem("Masaya/Sea Feathers/Merge")]
        static private void MergeSeaFeathers(MenuCommand command)
        {

            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Levels/Decor/SeaCritters/SeaFeather-01.prefab");
            GameObject[] feathers = PrefabUtility.FindAllInstancesOfPrefab(prefabRoot);
            HashSet<GameObject> mergedFeathers = new HashSet<GameObject>();
            int layerMask = 1 << prefabRoot.layer;

            int mergedRootCount = 0;

            for (int i = 0; i < feathers.Length; i++)
            {
                GameObject feather = feathers[i];
                if (mergedFeathers.Contains(feather)) continue;

                SphereCollider sc = feather.GetComponent<SphereCollider>();
                if (sc == null) continue;
                float scRadiusSqr = sc.bounds.extents.x * sc.bounds.extents.x;

                MonoBehaviourEvents featherMbe = feather.GetComponent<MonoBehaviourEvents>();
                mergedFeathers.Add(feather);

                mergedRootCount++;

                Collider[] others = UnityEngine.Physics.OverlapSphere(sc.bounds.center, sc.radius, layerMask);
                for (int oi = 0; oi < others.Length; oi++)
                {
                    GameObject other = others[oi].gameObject;
                    if (mergedFeathers.Contains(other)) continue; // skip if its already merged
                    else if (other.transform.parent != feather.transform.parent) continue; // only merge feathers with the same parent
                    else if( (other.transform.position - feather.transform.position).sqrMagnitude > scRadiusSqr) continue; // only merge if the other feather is _within_ our sphere collider.

                    MonoBehaviourEvents otherMbe = other.GetComponent<MonoBehaviourEvents>();
                    UnityEventTools.AddPersistentListener(featherMbe.onTriggerEnter, otherMbe.OnTriggerEnter);
                    UnityEventTools.AddPersistentListener(featherMbe.onTriggerExit, otherMbe.OnTriggerExit);

                    GameObject.DestroyImmediate(other.GetComponent<SphereCollider>());
                    mergedFeathers.Add(other);
                    EditorUtility.SetDirty(other);
                }
                
                EditorUtility.SetDirty(feather);
                PrefabUtility.RecordPrefabInstancePropertyModifications(featherMbe);
            }

            Debug.Log($"Merged {feathers.Length} down to {mergedRootCount} roots.");

        }

        [MenuItem("Masaya/Sea Feathers/Revert")]
        static private void RevertSeaFeathers(MenuCommand command)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Levels/Decor/SeaCritters/SeaFeather-01.prefab");
            GameObject[] feathers = PrefabUtility.FindAllInstancesOfPrefab(prefabRoot);
            for (int i = 0; i < feathers.Length; i++)
            {
                Vector3 scale = feathers[i].transform.localScale;
                Quaternion rotation = feathers[i].transform.localRotation;

                PrefabUtility.RevertPrefabInstance(feathers[i], InteractionMode.AutomatedAction);

                feathers[i].transform.localScale = scale;
                feathers[i].transform.localRotation = rotation;
                PrefabUtility.RecordPrefabInstancePropertyModifications(feathers[i].transform);
            }

            Debug.Log($"Reverted {feathers.Length} Sea Feathers.");
        }
    }
}
