using UnityEngine;

public static class RecursosVisualesSpaceShooter
{
    public static Sprite CargarSpriteEditor(string nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(nombreArchivo))
        {
            return null;
        }

        string nombreSinExtension = nombreArchivo;
        if (nombreSinExtension.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
        {
            nombreSinExtension = nombreSinExtension.Substring(0, nombreSinExtension.Length - 4);
        }

        // Cargar desde Resources (compatible con Build)
        Sprite sprite = Resources.Load<Sprite>("SpaceShooter/" + nombreSinExtension);
        if (sprite != null)
        {
            return sprite;
        }

        Sprite[] sprites = Resources.LoadAll<Sprite>("SpaceShooter/" + nombreSinExtension);
        if (sprites != null && sprites.Length > 0)
        {
            return sprites[0];
        }

#if UNITY_EDITOR
        // Fallback para Editor
        string rutaResources = "Assets/Resources/SpaceShooter/" + nombreArchivo;
        sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(rutaResources);
        if (sprite != null)
        {
            return sprite;
        }

        string rutaTexturas = "Assets/Texturas/SpaceShooter/" + nombreArchivo;
        sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(rutaTexturas);
        if (sprite != null)
        {
            return sprite;
        }
#endif
        return null;
    }
}
