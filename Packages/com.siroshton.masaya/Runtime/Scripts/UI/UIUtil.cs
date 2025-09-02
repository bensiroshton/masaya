using UnityEngine;

namespace Siroshton.Masaya.UI
{
    public static class UIUtil
    {

        public static Vector3 GetWorldCenter(RectTransform transform)
        {
            Vector3[] corners = new Vector3[4];
            transform.GetWorldCorners(corners);

            return (corners[0] + corners[2]) * 0.5f;
        }

    }
}