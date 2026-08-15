using UnityEngine;

namespace StageFilter.Common;

internal class TextureUtil
{
    public static Texture2D ChangeBrightness(Texture2D source, float brightness)
    {
        Texture2D texture = Object.Instantiate(source);
        Color[] pixels = texture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            pixels[i] = new Color(
                c.r * brightness,
                c.g * brightness,
                c.b * brightness,
                c.a);
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }

    // Adapted from AssetExtractor by Icebro
    // https://github.com/kinaclipse101/code-mods/blob/main/assetextractor/AssetExtractor/WikiFormat.cs#L187
    public static Texture2D MakeReadableTexture(
        Texture texture,
        int x,
        int y,
        int cropWidth,
        int cropHeight,
        int outputWidth = 128,
        int outputHeight = 128)
    {
        // Convert the cut dimensions to UV coordinates
        Vector2 scale = new(
            (float)cropWidth / texture.width,
            (float)cropHeight / texture.height);

        // Flip the Y-axis so that it starts at the top-left corner of the texture
        int flippedY = texture.height - y - cropHeight;

        // Convert the initial cut position to UV coordinates
        Vector2 offset = new(
            (float)x / texture.width,
            (float)flippedY / texture.height);

        // Create a temporary RenderTexture with the output dimensions
        RenderTexture tmp = RenderTexture.GetTemporary(
            outputWidth,
            outputHeight,
            0,
            RenderTextureFormat.ARGB32);

        // Copy the texture pixels to the temporary RenderTexture
        Graphics.Blit(texture, tmp, scale, offset);

        // Backup the current RenderTexture and set it to the temporary one
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;

        // Create a new readable Texture2D to copy the pixels to it
        var result = new Texture2D(outputWidth, outputHeight, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, outputWidth, outputHeight), 0, 0);
        result.Apply();

        // Reset the active RenderTexture and release the temporary one
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);

        return result;
    }
}
