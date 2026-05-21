using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public static class EcosAulaSpriteLoader
{
    private static Dictionary<int, Sprite> cacheXbox = new Dictionary<int, Sprite>();
    private static Dictionary<int, Sprite> cacheTeclado = new Dictionary<int, Sprite>();
    private static bool inicializado = false;

    private const string RutaXbox = "Assets/Texturas/SpritesDeControles/gdb-xbox-2.png";
    private const string RutaTeclado = "Assets/Texturas/SpritesDeControles/gdb-keyboard-2.png";
    private const string NombreResource = "EcosAulaSpriteContainer";
    private const string RutaResourceAsset = "Assets/Resources/" + NombreResource + ".asset";

    public static void InicializarSiHaceFalta()
    {
        if (inicializado) return;

        // 1. Intentar cargar desde Resources
        EcosAulaSpriteContainer container = Resources.Load<EcosAulaSpriteContainer>(NombreResource);
        if (container != null)
        {
            CargarDesdeContainer(container);
        }

#if UNITY_EDITOR
        // 2. Si estamos en el editor y el container no existe o está vacío, lo regeneramos
        if (container == null || container.spritesXbox.Count == 0 || container.spritesTeclado.Count == 0)
        {
            Debug.Log("[EcosAulaSpriteLoader] Inicializando y regenerando EcosAulaSpriteContainer...");
            
            cacheXbox.Clear();
            cacheTeclado.Clear();

            CargarSpritesheetEditor(RutaXbox, cacheXbox);
            CargarSpritesheetEditor(RutaTeclado, cacheTeclado);

            RegenerarYGuardarAssetEditor();
        }
#endif

        inicializado = true;
    }

    private static void CargarDesdeContainer(EcosAulaSpriteContainer container)
    {
        cacheXbox.Clear();
        foreach (var sprite in container.spritesXbox)
        {
            if (sprite != null)
            {
                int idx = ExtraerIndice(sprite.name);
                if (idx != -1) cacheXbox[idx] = sprite;
            }
        }

        cacheTeclado.Clear();
        foreach (var sprite in container.spritesTeclado)
        {
            if (sprite != null)
            {
                int idx = ExtraerIndice(sprite.name);
                if (idx != -1) cacheTeclado[idx] = sprite;
            }
        }
    }

    private static int ExtraerIndice(string nombre)
    {
        int ultimoGuion = nombre.LastIndexOf('_');
        if (ultimoGuion != -1 && int.TryParse(nombre.Substring(ultimoGuion + 1), out int indice))
        {
            return indice;
        }
        return -1;
    }

#if UNITY_EDITOR
    private static void CargarSpritesheetEditor(string ruta, Dictionary<int, Sprite> cache)
    {
        object[] assets = AssetDatabase.LoadAllAssetsAtPath(ruta);
        if (assets == null || assets.Length == 0)
        {
            Debug.LogWarning($"[EcosAulaSpriteLoader] No se pudo cargar el spritesheet en: {ruta}");
            return;
        }

        foreach (var asset in assets)
        {
            if (asset is Sprite sprite)
            {
                int idx = ExtraerIndice(sprite.name);
                if (idx != -1)
                {
                    cache[idx] = sprite;
                }
            }
        }
    }

    private static void RegenerarYGuardarAssetEditor()
    {
        string dirPath = Path.Combine(Application.dataPath, "Resources");
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
            AssetDatabase.Refresh();
        }

        EcosAulaSpriteContainer container = AssetDatabase.LoadAssetAtPath<EcosAulaSpriteContainer>(RutaResourceAsset);
        bool esNuevo = false;
        if (container == null)
        {
            container = ScriptableObject.CreateInstance<EcosAulaSpriteContainer>();
            esNuevo = true;
        }

        container.spritesXbox.Clear();
        // Ordenamos por clave para tener un listado limpio
        var keysXbox = new List<int>(cacheXbox.Keys);
        keysXbox.Sort();
        foreach (int key in keysXbox)
        {
            container.spritesXbox.Add(cacheXbox[key]);
        }

        container.spritesTeclado.Clear();
        var keysTeclado = new List<int>(cacheTeclado.Keys);
        keysTeclado.Sort();
        foreach (int key in keysTeclado)
        {
            container.spritesTeclado.Add(cacheTeclado[key]);
        }

        if (esNuevo)
        {
            AssetDatabase.CreateAsset(container, RutaResourceAsset);
        }
        else
        {
            EditorUtility.SetDirty(container);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[EcosAulaSpriteLoader] Guardado contenedor de sprites en: {RutaResourceAsset} con {container.spritesXbox.Count} sprites de Xbox y {container.spritesTeclado.Count} de Teclado.");
    }
#endif

    public static Sprite ObtenerSpriteXbox(int indice)
    {
        InicializarSiHaceFalta();
        if (cacheXbox.TryGetValue(indice, out Sprite sprite))
        {
            return sprite;
        }
        return null;
    }

    public static Sprite ObtenerSpriteTeclado(int indice)
    {
        InicializarSiHaceFalta();
        if (cacheTeclado.TryGetValue(indice, out Sprite sprite))
        {
            return sprite;
        }
        return null;
    }
}
