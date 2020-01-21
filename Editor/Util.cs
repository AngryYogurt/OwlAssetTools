using System.IO;
using UnityEngine;

namespace Assets.OwlAssetTools.Editor
{
    class Util
    {
        public static string[] ValidFileExt = {".png"};
        public static string SpriteFilePrefix = "spr_";
        public static string SpriteDirPrefix = "spr_";

        public static string ToolsResourceDir = Path.Combine(Application.dataPath, "OwlResource");

        public static string ResourceStockDir =
            Path.Combine(Path.GetDirectoryName(Application.dataPath), "ResourceStock");

        public static Vector2 CalSpritePivot(float width, float height)
        {

//            return new Vector2(0.5f, 0.0863f);
            return new Vector2(0.5f, (float) (CalPixelPerUnit(width) / height));
        }


        public static float CalPixelPerUnit(float width)
        {
            // ((width / 2) ^ 2 * 4 / 3) ^ (1 / 2);
            return Mathf.Pow((width * width) / 3, 0.5f);
        }

        public static string GetAssetsPath(string fullPath)
        {
            return "Assets" + fullPath.Substring(Application.dataPath.Length);
        }


        public static void CopyToStock(string src)
        {
            var dest = Path.Combine(Util.ResourceStockDir, Path.GetFileName(src));
            if (File.Exists(src))
            {
                File.Copy(src, src.Replace(src, dest), true);
            }
            else if (Directory.Exists(src))
            {
                Directory.CreateDirectory(dest);
                foreach (var dirPath in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(src, dest));

                foreach (var srcPath in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories))
                    File.Copy(srcPath, srcPath.Replace(src, dest), true);
            }
        }
    }
}