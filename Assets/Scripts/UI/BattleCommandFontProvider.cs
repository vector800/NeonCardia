using TMPro;
using UnityEngine;

public static class BattleCommandFontProvider
{
    private const string PrewarmCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 -:/"
        + "\u30b9\u30ad\u30eb\u3092\u4f7f\u7528\u3057\u307e\u3059"
        + "\u63a7\u3048\u30e1\u30f3\u30d0\u30fc\u3068\u4ea4\u4ee3\u3057\u307e\u3059"
        + "\u30d0\u30c8\u30eb\u304b\u3089\u9003\u8d70\u3057\u307e\u3059";

    private static TMP_FontAsset cachedFontAsset;

    public static TMP_FontAsset GetFontAsset()
    {
        if (cachedFontAsset != null)
        {
            return cachedFontAsset;
        }

        cachedFontAsset = CreateFontAssetFromSystemFonts();
        if (cachedFontAsset == null)
        {
            return TMP_Settings.defaultFontAsset;
        }

        cachedFontAsset.name = "BattleCommandDynamicJapaneseTMPFont";
        cachedFontAsset.hideFlags = HideFlags.HideAndDontSave;
        cachedFontAsset.TryAddCharacters(PrewarmCharacters, out _);

        return cachedFontAsset;
    }

    private static TMP_FontAsset CreateFontAssetFromSystemFonts()
    {
        string[] fontNames =
        {
            "Meiryo UI",
            "Yu Gothic UI",
            "Meiryo",
            "Yu Gothic",
            "Noto Sans CJK JP",
            "Arial"
        };

        string[] styleNames = { "Regular", "Normal", string.Empty };
        for (int i = 0; i < fontNames.Length; i++)
        {
            for (int j = 0; j < styleNames.Length; j++)
            {
                TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(fontNames[i], styleNames[j], 64);
                if (fontAsset != null)
                {
                    return fontAsset;
                }
            }
        }

        return null;
    }
}
