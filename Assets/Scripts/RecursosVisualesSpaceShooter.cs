using UnityEngine;

public static class RecursosVisualesSpaceShooter
{
    public static Sprite CargarSpriteEditor(string nombreArchivo)
    {
#if UNITY_EDITOR
        if (string.IsNullOrWhiteSpace(nombreArchivo))
        {
            return null;
        }

        string rutaTexturas = "Assets/Texturas/SpaceShooter/" + nombreArchivo;
        Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(rutaTexturas);

        if (sprite != null)
        {
            return sprite;
        }

        string rutaTextures = "Assets/Textures/SpaceShooter/" + nombreArchivo;
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(rutaTextures);
#else
        return null;
#endif
    }
}
