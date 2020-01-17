using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.OwlAssetTools.Editor
{
    /// <summary>
    /// Import sprites made with ShoeBox.
    /// This tool assumes your Assets directory like:
    /// Assets
    ///   -.../...
    ///     - sprites_sheet.xml           <- resource ShoeBox sprite file
    ///     - sprites_sheet.png           <- resource ShoeBox sprite file
    ///     - asset_sprites_sheet.png     <- target sprite result
    ///   - Others
    /// </summary>
    public class ShoeBoxSpriteImporter
    {
        [MenuItem("Assets/OwlAssetTools/Import ShoeBox Sprite")]
        public static void Init()
        {
            var xmlPath = EditorUtility.OpenFilePanel("Import ShoeBox Sprite", Application.dataPath, "xml");
            // asset location directory
            var dir = Path.GetDirectoryName(xmlPath);

            var xml = XElement.Load(xmlPath);
            var imgPath = xml.Attribute("imagePath").Value;

            var texturePath = Path.Combine(dir, imgPath);
            // if texture file is not named as imagePath, attempt to use xml file name
            if (!File.Exists(texturePath))
            {
                texturePath = Path.Combine(dir,
                    Path.GetFileNameWithoutExtension(xmlPath) + Path.GetExtension(imgPath));
                if (!File.Exists(texturePath))
                {
                    Debug.LogError("Texture file not found");
                    return;
                }
            }

            var destName = "asset_" + Path.GetFileName(texturePath);
            var destPath = Path.Combine(dir, destName);

            File.Copy(texturePath, destPath);

            var assetPath = "Assets" + destPath.Substring(Application.dataPath.Length);
            AssetDatabase.Refresh();
            var texture = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
            var sheet = new List<SpriteMetaData>();
            var elements = xml.Elements("SubTexture");

            foreach (var e in elements)
            {
                var h = (float) e.Attribute("height");
                var sprite = new SpriteMetaData
                {
                    rect = new Rect
                    {
                        width = (float) e.Attribute("width"),
                        height = h,
                        x = (float) e.Attribute("x"),
                        y = texture.height - (float) e.Attribute("y") - h
                    },
                    name = e.Attribute("name").Value
                };
                sprite.name = Path.GetFileNameWithoutExtension(sprite.name);
                sheet.Add(sprite);
            }

            var isometricWidth = Mathf.Floor(CalPixelPerUnit(sheet[0].rect.width));
            var path = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.spritesheet = sheet.ToArray();
            importer.textureType = TextureImporterType.Sprite;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMode = (int) SpriteImportMode.Multiple;
            settings.textureFormat = TextureImporterFormat.AutomaticTruecolor;

            settings.spriteAlignment = (int) SpriteAlignment.Custom;
            settings.spritePivot = CalSpritePivot(isometricWidth, sheet[0].rect.height);

            settings.spritePixelsPerUnit = isometricWidth;
            settings.mipmapEnabled = false;

            importer.SetTextureSettings(settings);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        private static Vector2 CalSpritePivot(float isometricWidth, float height)
        {
            return new Vector2(0.5f, (float) (1f / 2f / (height / isometricWidth)));
        }


        private static float CalPixelPerUnit(float width)
        {
            //((width / 2) ^ 2 * 4 / 3) ^ (1 / 2);
            return Mathf.Pow((width * width) / 3, 0.5f);
            ;
        }
    }
}