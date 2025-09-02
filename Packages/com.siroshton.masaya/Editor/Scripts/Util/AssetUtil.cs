using Siroshton.Masaya.Util;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace Siroshton.Masaya.Editor.Util
{

    public static class AssetUtil
    {
        public struct FoundDetails
        {
            public GameObject root;
            public UnityEngine.Component component;
            public bool isPrefab;
            public string assetPath;
            public System.Object data;
        }

        /// <returns>true if the object was updated, otherwise false.  When true this will save the prefab or mark an object as dirty where applicable.</returns>
        public delegate bool FoundComponent(FoundDetails details);

        static public int FindInstances<T>(FoundComponent onFound, System.Object data = null) where T : UnityEngine.Component
        {
            Debug.Log($"FindInstances<{typeof(T).Name}>");

            int count = 0;
            int updated = 0;

            // Search Scene Objects
            T[] objects = GameObject.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            count += objects.Length;

            if (onFound != null)
            {
                foreach (T o in objects)
                {
                    FoundDetails details = new FoundDetails();
                    details.root = o.gameObject;
                    details.component = o;
                    details.isPrefab = false;
                    details.data = data;
                    if( onFound(details) )
                    {
                        EditorUtility.SetDirty(details.root);
                        EditorUtility.SetDirty(o);
                        updated++;
                    }
                }
            }

            // Search Prefabs
            string[] prefabs = AssetDatabase.FindAssets($"t:prefab");
            foreach (string guid in prefabs)
            {
                FoundDetails details = new FoundDetails();
                details.assetPath = AssetDatabase.GUIDToAssetPath(guid);
                details.root = PrefabUtility.LoadPrefabContents(details.assetPath);
                
                bool isDirty = false;

                T[] components = details.root.GetComponentsInChildren<T>(true);
                if( components.Length > 0 )
                {
                    count += components.Length;
                    if( onFound != null )
                    {
                        details.data = data;
                        details.isPrefab = true;
                        for(int i=0;i<components.Length;i++)
                        {
                            details.component = components[i];
                            if( onFound(details) )
                            {
                                updated++;
                                isDirty = true;
                            }
                        }
                    }
                }

                if( isDirty ) PrefabUtility.SaveAsPrefabAsset(details.root, details.assetPath);

                PrefabUtility.UnloadPrefabContents(details.root);
            }

            Debug.Log($"found {count} and udpated {updated} items.");
            return count;
        }

    }
}