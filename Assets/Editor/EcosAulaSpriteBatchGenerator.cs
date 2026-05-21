using UnityEditor;
using UnityEngine;

public static class EcosAulaSpriteBatchGenerator
{
    [MenuItem("Ecos del Aula/Generar Contenedor de Sprites")]
    public static void GenerateAsset()
    {
        Debug.Log("[EcosAulaSpriteBatchGenerator] Iniciando generación de asset de sprites...");
        EcosAulaSpriteLoader.InicializarSiHaceFalta();
        Debug.Log("[EcosAulaSpriteBatchGenerator] Finalizado.");
    }
}
