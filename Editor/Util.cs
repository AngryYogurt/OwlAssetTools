using UnityEngine;

namespace Assets.OwlAssetTools.Editor
{
    class Util
    {
        public static Vector2 CalSpritePivot(float isometricWidth, float height)
        {
            return new Vector2(0.5f, (float) (isometricWidth / 2f / height));
        }


        public static float CalPixelPerUnit(float width)
        {
            // ((width / 2) ^ 2 * 4 / 3) ^ (1 / 2);
            return Mathf.Pow((width * width) / 3, 0.5f);
            ;
        }

        public static string GetAssetsPath(string fullPath)
        {
            return "Assets" + fullPath.Substring(Application.dataPath.Length);
        }
    }
}