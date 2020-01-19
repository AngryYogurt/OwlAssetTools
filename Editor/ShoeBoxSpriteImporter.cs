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
    /// This tool assumes your asset is isometric tiles and Assets directory like:
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
            var smds = new List<SpriteMetaData>();
            var elements = xml.Elements("SubTexture");

            foreach (var e in elements)
            {
                var h = (float) e.Attribute("height");
                var r = new Rect
                {
                    width = (float) e.Attribute("width"),
                    height = h,
                    x = (float) e.Attribute("x"),
                    y = texture.height - (float) e.Attribute("y") - h
                };
                var sprite = new SpriteMetaData
                {
                    rect = r,
                    name = Path.GetFileNameWithoutExtension(e.Attribute("name").Value)
                };
                var isometricWidth = Mathf.Floor(Util.CalPixelPerUnit(r.width));
                sprite.pivot = Util.CalSpritePivot(isometricWidth, r.height);
                sprite.alignment = (int) SpriteAlignment.Custom;
                smds.Add(sprite);
            }

            var path = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.spritesheet = smds.ToArray();
            importer.textureType = TextureImporterType.Sprite;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMode = (int) SpriteImportMode.Multiple;
            settings.textureFormat = TextureImporterFormat.AutomaticTruecolor;
            settings.spritePixelsPerUnit = Mathf.Floor(Util.CalPixelPerUnit(smds[0].rect.width));
            settings.mipmapEnabled = false;

            importer.SetTextureSettings(settings);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }
}