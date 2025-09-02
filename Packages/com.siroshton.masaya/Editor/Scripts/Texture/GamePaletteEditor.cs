using UnityEngine;
using UnityEditor;
using UnityEditor.Search;

namespace Siroshton.Masaya.Editor.Texture
{
    [CustomEditor(typeof(GamePalette))]
    public class GamePaletteEditor : UnityEditor.Editor
    {
        private bool _needsUpdate;
        private GUIStyle _materialLabel;
        private GUIStyle _colorLabel;

        private class ColorDrag
        {
            public int varient;
            public int colorIndex;
        }

        public override void OnInspectorGUI()
        {
            if(_materialLabel == null )
            {
                _materialLabel = new GUIStyle(GUI.skin.label);
                _materialLabel.fontStyle = FontStyle.Bold;
                _materialLabel.normal.textColor = Color.white;
                _materialLabel.alignment = TextAnchor.MiddleLeft;
            }

            if(_colorLabel == null )
            {
                _colorLabel = new GUIStyle(GUI.skin.label);
                _colorLabel.fontStyle = FontStyle.Normal;
                _colorLabel.alignment = TextAnchor.MiddleRight;
            }

            GamePalette gp = target as GamePalette;
            GamePalette.Palette palette = gp.palette;

            bool hasChanges = DrawDefaultInspector();

            EditorGUILayout.LabelField($"{gp.colorCount} colors ({gp.steps} x {gp.steps})");
            EditorGUILayout.LabelField($"Texture Size: {gp.textureSize} x {gp.textureSize}");
            EditorGUILayout.LabelField($"Swatch Size: {gp.swatchWidth} x {gp.swatchHeight}");

            bool saveTextures = GUILayout.Button("Save Textures");

            if (hasChanges || saveTextures || gp.needToBuildTextures)
            {
                gp.UpdateTexture(saveTextures);
            }

            bool isHorz = false;
            float paletteWidth = EditorGUIUtility.currentViewWidth - 50;
            if (paletteWidth > 900 )
            {
                isHorz = true;
                paletteWidth /= gp.variantCount;
                EditorGUILayout.BeginHorizontal();
            }

            for (int i=0;i<gp.variantCount;i++)
            {
                RenderTexture tex = gp.GetVariantTexture(i);
                if (tex != null)
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField(gp.GetTextureName(i), new GUILayoutOption[] { GUILayout.Width(paletteWidth) });
                    Rect r = EditorGUILayout.GetControlRect(false, paletteWidth, new GUILayoutOption[] { GUILayout.Width(paletteWidth) });
                    EditorGUI.DrawPreviewTexture(r, tex);
                    HandlePaletteDragDrop(gp, i, r);
                    EditorGUILayout.EndVertical();
                }
            }

            if( isHorz ) EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void HandlePaletteDragDrop(GamePalette gp, int variant, Rect r)
        {
            Vector2 mousePos = UnityEngine.Event.current.mousePosition;
            if (r.Contains(mousePos))
            {
                if (UnityEngine.Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                }
                else if (UnityEngine.Event.current.type == EventType.DragPerform)
                {
                    bool accept = false;
                    for (int oi = 0; oi < DragAndDrop.objectReferences.Length; oi++)
                    {
                        Object o = DragAndDrop.objectReferences[oi];
                        if (o is Texture2D)
                        {
                            // remap texture colors
                            string path = DragAndDrop.paths[oi];

                            int p1 = path.LastIndexOf("/");
                            int p2 = path.LastIndexOf(".");
                            string pathRoot = path.Substring(0, p1 + 1);
                            string file = path.Substring(p1 + 1);
                            string name = path.Substring(p1 + 1, p2 - p1 - 1);
                            string ext = path.Substring(p2 + 1);
                            string newFile = name + "-remapped." + ext;
                            string newFilePath = pathRoot + newFile;
                            Debug.Log($"Remapping colors from {file} to {newFile}");

                            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                            psi.FileName = "magick";
                            psi.UseShellExecute = false;
                            psi.RedirectStandardOutput = true;
                            psi.Arguments = $"convert {path} +dither -remap {gp.GetTexturePath(variant)} {newFilePath}";
                            System.Diagnostics.Process proc = System.Diagnostics.Process.Start(psi);
                            proc.WaitForExit();

                            if (ext.ToLower() == "png")
                            {
                                // the above command does not preserve transparency, copy transparency from the source to the new remapped file
                                // magick composite -compose dst_in amplify-content-background.png amplify-content-background-remapped.png -alpha set amplify-content-background-remapped.png
                                Debug.Log($"Copying Transparency to {newFile}...");

                                psi = new System.Diagnostics.ProcessStartInfo();
                                psi.FileName = "magick";
                                psi.UseShellExecute = false;
                                psi.RedirectStandardOutput = true;
                                psi.Arguments = $"composite -compose dst_in {path} {newFilePath} -alpha set {newFilePath}";
                                proc = System.Diagnostics.Process.Start(psi);
                                proc.WaitForExit();
                            }
                        }
                    }

                    if (accept)
                    {
                        DragAndDrop.AcceptDrag();
                    }
                }
            }
        }

    }

}