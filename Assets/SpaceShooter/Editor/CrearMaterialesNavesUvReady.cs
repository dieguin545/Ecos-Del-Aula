#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CrearMaterialesNavesUvReady
{
    private const string CarpetaTexturas = "Assets/SpaceShooter/Textures/Ships/";
    private const string CarpetaMateriales = "Assets/SpaceShooter/Materials/Ships/";

    [MenuItem("Tools/SpaceShooter/Crear materiales de naves UVReady")]
    public static void CrearMateriales()
    {
        CrearMaterial("MAT_Spaceship", "Spaceship_Texture.png");
        CrearMaterial("MAT_Spaceship2", "Spaceship2_Texture.png");
        CrearMaterial("MAT_Spaceship3", "Spaceship3_Texture.png");
        CrearMaterial("MAT_Spaceship4", "Spaceship4_Texture.png");
        CrearMaterial("MAT_Spaceship5", "Spaceship5_Texture.png");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CrearMaterial(string nombreMaterial, string nombreTextura)
    {
        string rutaMaterial = CarpetaMateriales + nombreMaterial + ".mat";
        string rutaTextura = CarpetaTexturas + nombreTextura;
        Texture2D textura = AssetDatabase.LoadAssetAtPath<Texture2D>(rutaTextura);

        if (textura == null)
        {
            Debug.LogWarning("No se encontro la textura de nave: " + rutaTextura);
            return;
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(rutaMaterial);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, rutaMaterial);
        }
        else if (shader != null)
        {
            material.shader = shader;
        }

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", textura);
        }

        if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", textura);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", Color.white);
        }

        if (material.HasProperty("_Color"))
        {
            material.color = Color.white;
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0.55f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.5f);
        }

        EditorUtility.SetDirty(material);
    }
}
#endif
