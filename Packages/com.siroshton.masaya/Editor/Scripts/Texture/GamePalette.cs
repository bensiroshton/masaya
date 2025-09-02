using UnityEngine;
using UnityEditor;
using Unity.Collections;
using System;
using Siroshton.Masaya.Util;
using Unity.Jobs;
using System.Collections.Generic;
using System.Linq;

namespace Siroshton.Masaya.Editor.Texture
{
    [CreateAssetMenu(fileName="GamePalette.asset")]
    public class GamePalette : ScriptableObject
    {
        public enum TextureSize
        {
            size_256 = 256,
            size_512 = 512,
            size_1024 = 1024,
            size_2048 = 2048,
            size_4096 = 4096
        }

#pragma warning disable CS0660
#pragma warning disable CS0661
        [Serializable]
        public struct Palette
        {
            public Color red;
            public Color yellow;
            public Color blue;
            public TextureSize textureSize;
            [Range(1, 1024)] public int steps;
            [Range(0, 1)] public float topShade;
            [Range(0, 1)] public float bottomShade;

            public Palette(Color red, Color yellow, Color blue, int steps)
            {
                this.red = red;
                this.yellow = yellow;
                this.blue = blue;
                this.steps = steps;
                this.topShade = 1;
                this.bottomShade = 0.125f;
                this.textureSize = TextureSize.size_512;
            }

            public static bool operator ==(Palette p1, Palette p2)
            {
                return p1.Equals(p2);
            }

            public static bool operator !=(Palette p1, Palette p2)
            {
                return !p1.Equals(p2);
            }
        }
#pragma warning restore CS0660
#pragma warning restore CS0661

        [Serializable]
        public class PaletteVariant
        {
            [Range(-1, 1)] public float saturationAdjustment;
            [NonSerialized] public RenderTexture texture;

            public static PaletteVariant Clone(PaletteVariant other)
            {
                PaletteVariant pv = new PaletteVariant();
                pv.Copy(other);
                return pv;
            }

            public void Copy(PaletteVariant other)
            {
                saturationAdjustment = other.saturationAdjustment;
                texture = other.texture;
            }

            public bool Equals(PaletteVariant other)
            {
                return saturationAdjustment == other.saturationAdjustment && texture == other.texture;
            }
        }

        [SerializeField] private Palette _palette = new Palette(Color.red, Color.yellow, Color.blue, 16);
        [SerializeField] private PaletteVariant[] _variants;
        [SerializeField] private Gradient _gradient;
        [SerializeField] private Material _paletteMaterial;

        private Palette _lastPalette;
        private PaletteVariant[] _lastVariants;
        private Texture2D _tempTex;

        public Palette palette
        {
            get => _palette;
            set => _palette = value;
        }

        public int variantCount
        {
            get
            {
                if( _variants == null ) return 0;
                else return _variants.Length;
            }
        }

        public int steps => _palette.steps;

        public int colorCount => steps * steps;
        public int swatchWidth => textureSize / steps;
        public int swatchHeight => textureSize / steps;
        public int textureSize => ((int)_palette.textureSize);

        private void OnEnable()
        {
        }

        public string GetTexturePath(int variant)
        {
            string path = AssetDatabase.GetAssetPath(this);
            return path.Replace(".asset", $"_V{variant}.png");
        }

        public string GetTextureName(int variant)
        {
            string path = GetTexturePath(variant);
            return path.Substring(path.LastIndexOf("/") + 1);
        }

        public Texture2D GetTextureAsset(int variant)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(GetTexturePath(variant));
        }

        public RenderTexture GetVariantTexture(int variant)
        {
            if (_variants == null) return null;
            return _variants[variant].texture;
        }

        public bool needToBuildTextures => _variants != null && _variants.Length > 0 && _variants[0].texture == null;

        public int GetVariantIndex(PaletteVariant variant)
        {
            if( _variants == null ) return -1;
            
            for (int i = 0; i < _variants.Length; i++)
            {
                if (_variants[i] == variant) return i;
            }

            return -1;
        }

        private void BuildGradient()
        {
            _gradient = new Gradient();
            GradientColorKey[] colorKeys;
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1, 0);
            alphaKeys[1] = new GradientAlphaKey(1, 1);
            colorKeys = new GradientColorKey[4];
            colorKeys[0] = new GradientColorKey(_palette.red, 0);
            colorKeys[1] = new GradientColorKey(_palette.yellow, 0.33333333f);
            colorKeys[2] = new GradientColorKey(_palette.blue, 0.66666666f);
            colorKeys[3] = new GradientColorKey(_palette.red, 1);

            _gradient.SetKeys(colorKeys, alphaKeys);
        }


        /// <summary>
        /// Updates the textures.
        /// </summary>
        /// <returns>true if any were updated, otherwise false.</returns>
        public bool UpdateTexture(bool saveTextures)
        {
            bool isDirty = _lastPalette != _palette;
            _lastPalette = _palette;

            if( _variants != null )
            {
                if( _lastVariants == null || _lastVariants.Length != _variants.Length ) _lastVariants = new PaletteVariant[_variants.Length];

                for(int i=0;i<_variants.Length;i++)
                {
                    if (_lastVariants[i] == null)
                    {
                        _lastVariants[i] = new PaletteVariant();
                        isDirty = true;
                    }

                    if ( _variants[i].texture == null )
                    {
                        isDirty = true;
                    }
                    else if (!_lastVariants[i].Equals(_variants[i]) )
                    {
                        if(_lastVariants[i] == null ) _lastVariants[i] = PaletteVariant.Clone(_variants[i]); 
                        else _lastVariants[i].Copy(_variants[i]);

                        isDirty = true;
                    }
                }
            }

            if(isDirty || saveTextures)
            {
                BuildGradient();

                if (_paletteMaterial == null)
                {
                    _paletteMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Levels/PaletteMaterial.mat");
                }

                _paletteMaterial.SetFloat("_Steps", _palette.steps);
                _paletteMaterial.SetFloat("_TopShade", _palette.topShade);
                _paletteMaterial.SetFloat("_BottomShade", _palette.bottomShade);

                for(int i=0;i<_variants.Length;i++)
                {
                    if( _variants[i].texture == null || _variants[i].texture.width != textureSize )
                    {
                        RenderTextureDescriptor desc = new RenderTextureDescriptor(textureSize, textureSize);
                        desc.depthBufferBits = 0;
                        desc.sRGB = true;
                        _variants[i].texture = new RenderTexture(desc);
                    }

                    _paletteMaterial.SetFloat("_Saturation", _variants[i].saturationAdjustment);

                    Graphics.Blit(null, _variants[i].texture, _paletteMaterial);

                    if( saveTextures )
                    {
                        if( _tempTex == null || _tempTex.width != textureSize )
                        {
                            _tempTex = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, 0, true);
                        }

                        _tempTex.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
                        byte[] data = ImageConversion.EncodeToPNG(_tempTex);
                        System.IO.File.WriteAllBytes(GetTexturePath(i), data);
                    }
                }

                RenderTexture.active = null;
            }

            return isDirty;
        }

    }

}