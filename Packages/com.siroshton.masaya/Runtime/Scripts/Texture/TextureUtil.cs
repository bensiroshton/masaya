using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

static public class TextureUtil
{

    public struct FillRectJob : IJob
    {
        [ReadOnly] public int textureWidth;
        [ReadOnly] public int textureHeight;
        [ReadOnly] public RectInt rect;
        [ReadOnly] public Color32 color;
        [WriteOnly, NativeDisableContainerSafetyRestriction] NativeArray<Color32> texture;

        public FillRectJob(NativeArray<Color32> tex, int texWidth, int texHeight, RectInt rect, Color32 color)
        {
            this.texture = tex;
            this.textureWidth = texWidth;
            this.textureHeight = texHeight;
            this.rect = rect;
            this.color = color;
        }

        public void Execute()
        {
            TextureUtil.FillRect(texture, textureWidth, textureHeight, rect, color);
        }
    }

    static public void FillRect(NativeArray<Color32> tex, int texWidth, int texHeight, Color32 color)
    {
        FillRect(tex, texWidth, texHeight, new RectInt(0, 0, texWidth, texHeight), color);
    }

    static public void FillRect(NativeArray<Color32> tex, int texWidth, int texHeight, RectInt rect, Color32 color)
    {
        int x1, y1, index;

        for(y1 = 0; y1 < rect.height; y1++)
        {
            index = (rect.y + y1) * texHeight + rect.x;
            for(x1 = 0; x1 < rect.width; x1++)
            {
                tex[index + x1] = color;
            }
        }
    }

    static public void ClearTexture(Texture2D tex, Color32 color)
    {
        FillRect(tex.GetRawTextureData<Color32>(), tex.width, tex.height, color);
        tex.Apply();
    }

    static public Texture2D CreateColoredTexture(int width, int height, Color32 color)
    {
        Texture2D tex = new Texture2D(width, height);
        ClearTexture(tex, color);
        return tex;
    }

}