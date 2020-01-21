using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.OwlAssetTools.Editor
{
    class ImageTileImporter
    {
        [MenuItem("Assets/OwlAssetTools/Import Tile Images Folder")]
        public static void Init()
        {
            var imgDir = EditorUtility.OpenFolderPanel("Import Tile Images Folder", Application.dataPath, "");
            if (String.IsNullOrEmpty(imgDir) || !Directory.Exists(imgDir))
            {
                return;
            }
            Util.CopyToStock(imgDir);

            // target directory 
            var targetDir = Path.Combine(Util.ToolsResourceDir, Util.SpriteDirPrefix + Path.GetFileName(imgDir));
            if (Directory.Exists(targetDir))
            {
                Debug.LogError("Target folder already exist, target=" + targetDir);
                return;
            }

            // Copy empty directory tree
            Directory.CreateDirectory(targetDir);
            foreach (var srcPath in Directory.GetDirectories(imgDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(srcPath.Replace(imgDir, targetDir));
            }

            AssetDatabase.Refresh();


            var targetFiles = new List<string>();
            // Copy file with transparent bounding box
            foreach (var srcFile in Directory.GetFiles(imgDir, "*.*", SearchOption.AllDirectories))
            {
                if (!Util.ValidFileExt.Contains(Path.GetExtension(srcFile).ToLower()))
                {
                    continue;
                }

                var srcFileName = Path.GetFileName(srcFile);
                var targetFile = Path.Combine(targetDir, Util.SpriteFilePrefix + srcFileName);

                // Calculate bounding box
                var srcAssetsPath = Util.GetAssetsPath(srcFile);
                var srcTexture = AssetDatabase.LoadAssetAtPath(srcAssetsPath, typeof(Texture2D)) as Texture2D;
                TextureImporter impt = (TextureImporter) AssetImporter.GetAtPath(srcAssetsPath);
                impt.isReadable = true;
                AssetDatabase.ImportAsset(srcAssetsPath, ImportAssetOptions.ForceUpdate);
                int maxX = 0, minX = srcTexture.width, maxY = 0, minY = srcTexture.height;
                for (var x = 0; x < srcTexture.width; x++)
                {
                    for (var y = 0; y < srcTexture.height; y++)
                    {
                        if (!(Mathf.Abs(srcTexture.GetPixel(x, y).a) <= Mathf.Epsilon))
                        {
                            if (x > maxX) maxX = x;
                            if (x < minX) minX = x;
                            if (y > maxY) maxY = y;
                            if (y < minY) minY = y;
                        }
                    }
                }

                targetFiles.Add(targetFile);
                var newTexture = GetCopyOfTexture2DArea(srcTexture, minX, maxX, minY, maxY);
                File.WriteAllBytes(targetFile, newTexture.EncodeToPNG());
            }

            AssetDatabase.Refresh();

            foreach (var targetFile in targetFiles)
            {
                // Load processed texture
                var texture =
                    AssetDatabase.LoadAssetAtPath(Util.GetAssetsPath(targetFile), typeof(Texture2D)) as Texture2D;
                var r = new Rect
                {
                    width = texture.width,
                    height = texture.height,
                    x = 0,
                    y = 0,
                };
                var sprite = new SpriteMetaData
                {
                    rect = r,
                    name = Path.GetFileNameWithoutExtension(targetFile)
                };

                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
                importer.spritesheet = new SpriteMetaData[1] {sprite};
                importer.textureType = TextureImporterType.Sprite;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spritePivot = Util.CalSpritePivot(r.width, r.height);
                settings.spriteAlignment = (int) SpriteAlignment.Custom;
                settings.textureType = TextureImporterType.Sprite;
                settings.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                settings.spriteMode = (int) SpriteImportMode.Single;
                settings.spritePixelsPerUnit = Mathf.Floor(Util.CalPixelPerUnit(texture.width));
                settings.mipmapEnabled = false;
                importer.SetTextureSettings(settings);

                AssetDatabase.ImportAsset(targetDir, ImportAssetOptions.ForceUpdate);
            }

            FileUtil.DeleteFileOrDirectory(imgDir);
            AssetDatabase.Refresh();
        }

        private static Texture2D GetCopyOfTexture2DArea(Texture2D src, int srcMinX, int srcMaxX, int srcMinY,
            int srcMaxY)
        {
            var width = srcMaxX - srcMinX + 1;
            var height = srcMaxY - srcMinY + 1;
            var dest = new Texture2D(width, height);
            var colors = src.GetPixels(srcMinX, srcMinY, width, height);
            dest.SetPixels(0, 0, width, height, colors);
            return dest;
        }
    }
}