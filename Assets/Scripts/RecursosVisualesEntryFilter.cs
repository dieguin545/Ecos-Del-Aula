using UnityEngine;

public static class RecursosVisualesEntryFilter
{
    public static Sprite CargarSpriteEditor(string nombreArchivo)
    {
#if UNITY_EDITOR
        if (string.IsNullOrWhiteSpace(nombreArchivo))
        {
            return null;
        }

        string rutaTexturas = "Assets/Texturas/EntryFilter/" + nombreArchivo;
        Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(rutaTexturas);

        if (sprite != null)
        {
            return sprite;
        }

        string rutaTextures = "Assets/Textures/EntryFilter/" + nombreArchivo;
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(rutaTextures);
#else
        return null;
#endif
    }
}
