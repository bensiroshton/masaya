#if UNITY_EDITOR
using Siroshton.Masaya.Math;
using System;
using UnityEditor;
using UnityEngine;

namespace Siroshton.Masaya.Mesh
{
    public class PrefabPainter : MonoBehaviour
    {
        public const float MaxBrushRadius = 5;
        public const float MaxEraserSize = 200;

        public enum BrushMode
        {
            Draw,
            Erase
        }

        public enum ParentMode
        {
            ParentToManagedNode,
            ParentToObject
        }

        [Serializable]
        public class Brush
        {
            [Tooltip("Object to place.")]
            public GameObject prefab;
            [Tooltip("Scale variance for placed objects.")]
            public IntervalFloat scale = new IntervalFloat(1, 1);
            [Tooltip("This is the max angle off the normal allowed when placing the object.")]
            public IntervalFloat tiltAngle = new IntervalFloat(0, 0);
            [Tooltip("This is the max angle off the normal allowed when placing the object.")]
            public IntervalFloat yRotation = new IntervalFloat(-180, 180);
            [Tooltip("Adjust the normal angle to point upwards. Angle Range will be applied after this.")]
            [Range(0, 90)]
            public float maxUprightAdjustment = 0;
        }

        [Serializable]
        public class PaintTool
        {
            [Range(0.0001f, MaxBrushRadius)] public float radius = 1;
            [Range(0, 1)] public float flow = 0.2f;
            [Range(0, 10)] public float scaleModifier = 1;
            public bool autoSwitchMesh = false;
            [Tooltip("Set the method in which added objects are parented.")]
            public ParentMode parentMode = ParentMode.ParentToManagedNode;
        }

        [Serializable]
        public class EraserTool
        {
            [Range(1, MaxEraserSize)] public float size = MaxEraserSize / 2;
        }

        [SerializeField] private BrushMode _brushMode = BrushMode.Draw;
        [SerializeField] private PaintTool _paintTool = new PaintTool();
        [SerializeField] private EraserTool _eraserTool = new EraserTool();
        [SerializeField] private Brush[] _brushes;
        [SerializeField] private GameObject[] _ignoreList;

        private int _selectedIndex;
        [SerializeField, HideInInspector] private int _lastBrushArraySize;

        public BrushMode brushMode { get => _brushMode; set => _brushMode = value; }
        public PaintTool paintTool => _paintTool;
        public EraserTool eraserTool => _eraserTool;
        public Brush[] brushes => _brushes;
        public GameObject[] ignoreList => _ignoreList;

        public int selectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if( value < 0 ) _selectedIndex = 0;
                else if( _brushes != null && value >= _brushes.Length ) _selectedIndex = _brushes.Length - 1;
                else _selectedIndex = value;
            }
        }
            
        public Brush selectedBrush
        {
            get
            {
                if( _brushes == null ) return null;
                else if( _selectedIndex >= _brushes.Length ) _selectedIndex = _brushes.Length - 1;
                    
                return _brushes[_selectedIndex];
            }
        }

        private void OnValidate()
        {
            if( _brushes != null && _brushes.Length > _lastBrushArraySize )
            {
                for(int i=_lastBrushArraySize;i<_brushes.Length;i++)
                {
                    _brushes[i] = new Brush();
                }
                _lastBrushArraySize = _brushes.Length;
            }
        }

        [MenuItem("GameObject/Prefab Painter")]
        public static void CreatePrefabPainter()
        {
            GameObject o = new GameObject("Prefab Painter");
            o.AddComponent<PrefabPainter>();
            Selection.activeObject = o;
        }
    }
}
#endif
