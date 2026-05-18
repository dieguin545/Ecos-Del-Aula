#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PrepararNavesLimpiasSpaceShooter
{
    private const string CarpetaMateriales = "Assets/SpaceShooter/Materials/Ships/CleanMaterials";
    private const string CarpetaPrefabs = "Assets/SpaceShooter/Prefabs/Ships";
    private const string RutaEscena = "Assets/Scenes/spaceshooter.unity";

    private sealed class PaletaNave
    {
        public string nombre;
        public string prefab;
        public Color metal;
        public Color cristal;
        public Color acento;
    }

    private static readonly PaletaNave[] Paletas =
    {
        new PaletaNave
        {
            nombre = "Alfa",
            prefab = "Spaceship_UVReady.prefab",
            metal = new Color(0.18f, 0.2f, 0.24f),
            cristal = new Color(0.22f, 0.9f, 1f),
            acento = new Color(0.18f, 0.45f, 1f)
        },
        new PaletaNave
        {
            nombre = "Beta",
            prefab = "Spaceship2_UVReady.prefab",
            metal = new Color(0.28f, 0.3f, 0.34f),
            cristal = new Color(1f, 0.55f, 0.18f),
            acento = new Color(0.95f, 0.95f, 1f)
        },
        new PaletaNave
        {
            nombre = "Gamma",
            prefab = "Spaceship3_UVReady.prefab",
            metal = new Color(0.12f, 0.12f, 0.15f),
            cristal = new Color(1f, 0.22f, 0.26f),
            acento = new Color(1f, 0.42f, 0.08f)
        },
        new PaletaNave
        {
            nombre = "Delta",
            prefab = "Spaceship4_UVReady.prefab",
            metal = new Color(0.58f, 0.61f, 0.66f),
            cristal = new Color(0.22f, 0.58f, 1f),
            acento = new Color(0.82f, 0.9f, 1f)
        },
        new PaletaNave
        {
            nombre = "Epsilon",
            prefab = "Spaceship5_UVReady.prefab",
            metal = new Color(0.19f, 0.12f, 0.28f),
            cristal = new Color(0.25f, 0.95f, 1f),
            acento = new Color(0.72f, 0.32f, 1f)
        }
    };

    [MenuItem("Tools/SpaceShooter/Preparar naves limpias")]
    public static void PrepararNaves()
    {
        CrearCarpetas();
        List<GameObject> prefabs = new List<GameObject>();

        for (int i = 0; i < Paletas.Length; i++)
        {
            PaletaNave paleta = Paletas[i];
            Material metal = CrearMaterial("MAT_Ship_" + paleta.nombre + "_Metal", paleta.metal, 0.62f, 0.52f, Color.black);
            Material cristal = CrearMaterial("MAT_Ship_" + paleta.nombre + "_Glass", paleta.cristal, 0.18f, 0.82f, paleta.cristal * 0.45f);
            Material acento = CrearMaterial("MAT_Ship_" + paleta.nombre + "_Accent", paleta.acento, 0.22f, 0.68f, paleta.acento * 0.35f);

            string rutaPrefab = CarpetaPrefabs + "/" + paleta.prefab;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(rutaPrefab);

            if (prefab == null)
            {
                Debug.LogWarning("No se encontro el prefab de nave: " + rutaPrefab);
                continue;
            }

            PrepararPrefab(rutaPrefab, metal, cristal, acento);
            prefabs.Add(prefab);
        }

        ConectarSelectorNave(prefabs);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CrearCarpetas()
    {
        CrearCarpetaSiHaceFalta("Assets/SpaceShooter/Materials", "Ships");
        CrearCarpetaSiHaceFalta("Assets/SpaceShooter/Materials/Ships", "CleanMaterials");
    }

    private static void CrearCarpetaSiHaceFalta(string padre, string nombre)
    {
        string ruta = padre + "/" + nombre;

        if (!AssetDatabase.IsValidFolder(ruta))
        {
            AssetDatabase.CreateFolder(padre, nombre);
        }
    }

    private static Material CrearMaterial(
        string nombre,
        Color color,
        float metallic,
        float smoothness,
        Color emission
    )
    {
        string ruta = CarpetaMateriales + "/" + nombre + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, ruta);
        }
        else if (shader != null)
        {
            material.shader = shader;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.color = color;
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", metallic);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        if (material.HasProperty("_EmissionColor"))
        {
            if (emission.maxColorComponent > 0f)
            {
                material.EnableKeyword("_EMISSION");
            }

            material.SetColor("_EmissionColor", emission);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void PrepararPrefab(
        string rutaPrefab,
        Material metal,
        Material cristal,
        Material acento
    )
    {
        GameObject raiz = PrefabUtility.LoadPrefabContents(rutaPrefab);

        EliminarComponentesNoJugables<Camera>(raiz);
        EliminarComponentesNoJugables<Light>(raiz);
        EliminarComponentesNoJugables<AudioListener>(raiz);

        Renderer[] renderers = raiz.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            Material[] actuales = renderers[i].sharedMaterials;

            if (actuales == null || actuales.Length == 0)
            {
                continue;
            }

            Material[] nuevos = new Material[actuales.Length];

            for (int slot = 0; slot < nuevos.Length; slot++)
            {
                nuevos[slot] = slot == 0 ? metal : slot == 1 ? cristal : acento;
            }

            renderers[i].sharedMaterials = nuevos;
        }

        PrefabUtility.SaveAsPrefabAsset(raiz, rutaPrefab);
        PrefabUtility.UnloadPrefabContents(raiz);
    }

    private static void EliminarComponentesNoJugables<T>(GameObject raiz)
        where T : Component
    {
        T[] componentes = raiz.GetComponentsInChildren<T>(true);

        for (int i = 0; i < componentes.Length; i++)
        {
            if (componentes[i] != null)
            {
                Object.DestroyImmediate(componentes[i], true);
            }
        }
    }

    private static void ConectarSelectorNave(List<GameObject> prefabs)
    {
        if (prefabs.Count < Paletas.Length)
        {
            Debug.LogWarning("No se conectaron todas las naves porque faltan prefabs.");
            return;
        }

        Scene escena = EditorSceneManager.OpenScene(RutaEscena, OpenSceneMode.Single);
        SelectorNave selector = Object.FindFirstObjectByType<SelectorNave>();

        if (selector == null)
        {
            Debug.LogWarning("No se encontro SelectorNave en la escena spaceshooter.");
            return;
        }

        SerializedObject serializado = new SerializedObject(selector);
        SerializedProperty modelos = serializado.FindProperty("modelosDisponibles");
        modelos.arraySize = Paletas.Length;

        for (int i = 0; i < Paletas.Length; i++)
        {
            SerializedProperty elemento = modelos.GetArrayElementAtIndex(i);
            elemento.FindPropertyRelative("nombre").stringValue = "Nave " + Paletas[i].nombre;
            elemento.FindPropertyRelative("modelo").objectReferenceValue = prefabs[i];
            elemento.FindPropertyRelative("posicionLocal").vector3Value = Vector3.zero;
            elemento.FindPropertyRelative("rotacionLocal").vector3Value = new Vector3(-90f, 0f, 0f);
            elemento.FindPropertyRelative("escalaLocal").vector3Value = Vector3.one;
        }

        serializado.ApplyModifiedProperties();
        EditorUtility.SetDirty(selector);
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
    }
}
#endif
