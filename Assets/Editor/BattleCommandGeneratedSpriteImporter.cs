using UnityEditor;
using UnityEngine;

public sealed class BattleCommandGeneratedSpriteImporter : AssetPostprocessor
{
    private const string GeneratedBattleCommandPath = "Assets/_Generated/UI/BattleCommand/";
    private const string GeneratedBattleCommandSimplePath = "Assets/_Generated/UI/BattleCommandSimple/";

    private void OnPreprocessTexture()
    {
        if ((!assetPath.StartsWith(GeneratedBattleCommandPath, System.StringComparison.Ordinal)
                && !assetPath.StartsWith(GeneratedBattleCommandSimplePath, System.StringComparison.Ordinal))
            || !System.IO.Path.GetFileName(assetPath).StartsWith("BC_", System.StringComparison.Ordinal))
        {
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
        importer.filterMode = UnityEngine.FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.spritePixelsPerUnit = 100f;
        Vector4 spriteBorder = GetSpriteBorder(System.IO.Path.GetFileName(assetPath));
        importer.spriteBorder = spriteBorder;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteBorder = spriteBorder;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.spriteBorder = spriteBorder;
    }

    private static Vector4 GetSpriteBorder(string fileName)
    {
        switch (fileName)
        {
            case "BC_CardRow_Normal.png":
            case "BC_CardRow_Selected.png":
            case "BC_Skills_Normal.png":
            case "BC_Skills_Selected.png":
            case "BC_Change_Normal.png":
            case "BC_Change_Selected.png":
            case "BC_Run_Normal.png":
            case "BC_Run_Selected.png":
                return new Vector4(28f, 18f, 28f, 18f);
            case "BC_SimpleButton_Normal.png":
            case "BC_SimpleButton_Selected.png":
            case "BC_SimpleButton_Shadow.png":
                return new Vector4(24f, 10f, 24f, 10f);
            case "BC_LeftIconFrame.png":
                return new Vector4(18f, 18f, 18f, 18f);
            case "BC_InfoPanel_OuterFrame.png":
            case "BC_InfoPanel_InnerBackground.png":
                return new Vector4(48f, 44f, 48f, 44f);
            default:
                return Vector4.zero;
        }
    }
}
