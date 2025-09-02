
using UnityEngine;

namespace Siroshton.Masaya.Util
{

    static public class ColorUtil
    {

        static public Color AddSaturation(Color color, float amount)
        {
            Color.RGBToHSV(color, out color.r, out color.g, out color.b);
            color.g += amount;
            if(color.g > 1 ) color.g = 1;
            else if(color.g < 0 ) color.g = 0;
            color = Color.HSVToRGB(color.r, color.g, color.b);
            return color;
        }

        static public Color AddValue(Color color, float amount)
        {
            Color.RGBToHSV(color, out color.r, out color.g, out color.b);
            color.b += amount;
            if (color.b > 1) color.b = 1;
            else if (color.b < 0) color.b = 0;
            color = Color.HSVToRGB(color.r, color.g, color.b);
            return color;
        }
    }
}

