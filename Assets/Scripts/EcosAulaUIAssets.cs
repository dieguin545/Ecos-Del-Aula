using System.Collections.Generic;
using UnityEngine;

public static class EcosAulaUIAssets
{
    private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite ObtenerBoton(string color)
    {
        string nombre = string.IsNullOrWhiteSpace(color) ? "button_blue" : "button_" + color.ToLowerInvariant();
        return ObtenerSprite("UI/Kenney/" + nombre);
    }

    private static Sprite ObtenerSprite(string ruta)
    {
        if (cache.TryGetValue(ruta, out Sprite spriteCacheado))
        {
            return spriteCacheado;
        }

        Sprite sprite = Resources.Load<Sprite>(ruta);
        if (sprite == null)
        {
            Texture2D textura = Resources.Load<Texture2D>(ruta);
            if (textura != null)
            {
                sprite = Sprite.Create(
                    textura,
                    new Rect(0f, 0f, textura.width, textura.height),
                    new Vector2(0.5f, 0.5f),
                    Mathf.Max(textura.width, textura.height)
                );
            }
        }

        if (sprite != null)
        {
            cache[ruta] = sprite;
        }

        return sprite;
    }
}
