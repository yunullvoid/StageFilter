using System.IO;
using System.Reflection;
using UnityEngine;
using static StageFilter.Common.TextureUtil;

namespace StageFilter.Common;

public static class AssetLoader
{
    public static Sprite LoadSpriteFromTexture(Texture2D texture, bool isEnabledTexture)
    {
        if (isEnabledTexture)
        {
            return Sprite.Create(
               texture,
               new Rect(0, 0, texture.width, texture.height),
               new Vector2(0.5f, 0.5f),
               100f);
        }
        else
        {
            Texture2D disabledTexture = ChangeBrightness(texture, 0.3f);

            return Sprite.Create(
                disabledTexture,
                new Rect(0, 0, disabledTexture.width, disabledTexture.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }
    }

    public static Sprite LoadSpriteFromResource(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
            return null;

        byte[] data = new byte[stream.Length];
        stream.Read(data, 0, data.Length);

        Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(data);

        return Sprite.Create(
           texture,
           new Rect(0, 0, texture.width, texture.height),
           new Vector2(0.5f, 0.5f),
           100f);

    }
}
