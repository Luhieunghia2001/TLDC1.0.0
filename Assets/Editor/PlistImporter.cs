using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Globalization;

public class PlistImporter : EditorWindow
{
    [MenuItem("Assets/Slice Sprites from Plist", true)]
    private static bool SlicerValidation()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return !string.IsNullOrEmpty(path) && path.EndsWith(".plist", System.StringComparison.OrdinalIgnoreCase);
    }

    [MenuItem("Assets/Slice Sprites from Plist")]
    private static void SliceSprites()
    {
        string plistPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        // Try to find the PNG. It could be named the same as the plist or specified in the metadata.
        string pngPath = plistPath.Replace(".plist", ".png");

        if (!File.Exists(Path.GetFullPath(pngPath)))
        {
            // Try looking for the filename inside the plist metadata
            string metadataPng = GetTextureNameFromPlist(plistPath);
            if (!string.IsNullOrEmpty(metadataPng))
            {
                string dir = Path.GetDirectoryName(plistPath);
                pngPath = Path.Combine(dir, metadataPng).Replace("\\", "/");
            }
        }

        if (!File.Exists(Path.GetFullPath(pngPath)))
        {
            Debug.LogError("Corresponding PNG not found for " + plistPath);
            return;
        }

        TextureImporter ti = AssetImporter.GetAtPath(pngPath) as TextureImporter;
        if (ti == null)
        {
            Debug.LogError("Could not get TextureImporter for " + pngPath);
            return;
        }

        // Force texture to be readable and Sprite mode to Multiple
        ti.isReadable = true;
        ti.spriteImportMode = SpriteImportMode.Multiple;

        // Load Plist
        XmlDocument doc = new XmlDocument();
        doc.Load(plistPath);

        XmlNode rootDict = doc.SelectSingleNode("plist/dict");
        if (rootDict == null)
        {
            Debug.LogError("Invalid Plist format: " + plistPath);
            return;
        }

        Dictionary<string, object> rootData = ParseDict(rootDict);

        if (!rootData.ContainsKey("frames"))
        {
            Debug.LogError("Plist does not contain 'frames' key: " + plistPath);
            return;
        }

        Dictionary<string, object> frames = rootData["frames"] as Dictionary<string, object>;
        
        // Get metadata for texture size
        Dictionary<string, object> metadata = rootData.ContainsKey("metadata") ? rootData["metadata"] as Dictionary<string, object> : null;
        Vector2 textureSize = Vector2.zero;
        if (metadata != null && metadata.ContainsKey("size"))
        {
            textureSize = ParseVector2(metadata["size"].ToString());
        }

        // If metadata doesn't have size, or it's zero, use the actual texture size
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
        if (tex != null)
        {
            if (textureSize == Vector2.zero)
                textureSize = new Vector2(tex.width, tex.height);
        }

        if (textureSize == Vector2.zero)
        {
            Debug.LogError("Could not determine texture size for " + pngPath);
            return;
        }

        List<SpriteMetaData> spriteSheet = new List<SpriteMetaData>();

        foreach (var frameKvp in frames)
        {
            string spriteName = frameKvp.Key;
            Dictionary<string, object> frameData = frameKvp.Value as Dictionary<string, object>;

            if (!frameData.ContainsKey("frame")) continue;

            string frameStr = frameData["frame"].ToString();
            // bool rotated = frameData.ContainsKey("rotated") && (bool)frameData["rotated"];
            
            // Format: {{x,y},{w,h}}
            Rect rect = ParseRect(frameStr);

            SpriteMetaData smd = new SpriteMetaData();
            smd.name = Path.GetFileNameWithoutExtension(spriteName);
            smd.alignment = (int)SpriteAlignment.Center;
            smd.pivot = new Vector2(0.5f, 0.5f);

            // Unity y starts from bottom
            // In plist, frame is {{x, y}, {w, h}} where y is from top
            float x = rect.x;
            float y = textureSize.y - rect.y - rect.height;
            float w = rect.width;
            float h = rect.height;

            smd.rect = new Rect(x, y, w, h);
            spriteSheet.Add(smd);
        }

        ti.spritesheet = spriteSheet.ToArray();
        
        AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);
        
        Debug.Log($"Successfully sliced {spriteSheet.Count} sprites from {plistPath} onto {pngPath}");
    }

    private static string GetTextureNameFromPlist(string path)
    {
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode metadataNode = doc.SelectSingleNode("plist/dict/key[text()='metadata']/following-sibling::dict");
            if (metadataNode != null)
            {
                XmlNode texNode = metadataNode.SelectSingleNode("key[text()='realTextureFileName']/following-sibling::string");
                if (texNode == null)
                    texNode = metadataNode.SelectSingleNode("key[text()='textureFileName']/following-sibling::string");
                
                if (texNode != null)
                    return texNode.InnerText;
            }
        }
        catch {}
        return null;
    }

    private static Dictionary<string, object> ParseDict(XmlNode dictNode)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        XmlNodeList children = dictNode.SelectNodes("*");
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].Name == "key")
            {
                string key = children[i].InnerText;
                XmlNode valueNode = children[++i];
                dict[key] = ParseValue(valueNode);
            }
        }
        return dict;
    }

    private static object ParseValue(XmlNode node)
    {
        switch (node.Name)
        {
            case "dict": return ParseDict(node);
            case "string": return node.InnerText;
            case "integer": return int.Parse(node.InnerText);
            case "real": return float.Parse(node.InnerText, CultureInfo.InvariantCulture);
            case "true": return true;
            case "false": return false;
            default: return node.InnerText;
        }
    }

    private static Rect ParseRect(string s)
    {
        s = s.Replace("{", "").Replace("}", "");
        string[] parts = s.Split(',');
        return new Rect(
            float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture),
            float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture),
            float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture),
            float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture)
        );
    }

    private static Vector2 ParseVector2(string s)
    {
        s = s.Replace("{", "").Replace("}", "");
        string[] parts = s.Split(',');
        return new Vector2(
            float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture),
            float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture)
        );
    }
}
