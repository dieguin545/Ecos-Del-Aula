using UnityEngine;

public static class EstiloVisualSpaceShooter
{
    public static readonly Color ColorNave = new Color(0.25f, 0.9f, 1f);
    public static readonly Color ColorEnemigo = new Color(1f, 0.2f, 0.45f);
    public static readonly Color ColorEnemigoDaltonico = new Color(1f, 0.78f, 0.15f);
    public static readonly Color ColorBala = new Color(0.35f, 1f, 0.95f);
    public static readonly Color ColorAtaqueEnemigo = new Color(1f, 0.24f, 0.18f);
    public static readonly Color ColorAtaqueEnemigoDaltonico = new Color(1f, 0.82f, 0.15f);

    private static Material materialNave;
    private static Material materialEnemigo;
    private static Material materialEnemigoDaltonico;
    private static Material materialBala;
    private static Material materialAtaqueEnemigo;
    private static Material materialAtaqueEnemigoDaltonico;

    public static void AplicarANave(GameObject objeto)
    {
        AplicarMaterial(objeto, ObtenerMaterialNave());
    }

    public static void AplicarAEnemigo(GameObject objeto, bool modoDaltonico)
    {
        AplicarMaterial(
            objeto,
            modoDaltonico ? ObtenerMaterialEnemigoDaltonico() : ObtenerMaterialEnemigo()
        );
    }

    public static void AplicarABala(GameObject objeto)
    {
        AplicarMaterial(objeto, ObtenerMaterialBala());
    }

    public static void AplicarAProyectilEnemigo(GameObject objeto, bool modoDaltonico)
    {
        AplicarMaterial(
            objeto,
            modoDaltonico
                ? ObtenerMaterialAtaqueEnemigoDaltonico()
                : ObtenerMaterialAtaqueEnemigo()
        );
    }

    public static Material CrearMaterialEstrella(Color color)
    {
        return CrearMaterialSolido(color, color * 1.2f);
    }

    private static void AplicarMaterial(GameObject objeto, Material material)
    {
        if (objeto == null || material == null)
        {
            return;
        }

        Renderer[] renderers = objeto.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].sharedMaterial = material;
            }
        }
    }

    private static Material ObtenerMaterialNave()
    {
        if (materialNave == null)
        {
            materialNave = CrearMaterialSolido(ColorNave, ColorNave * 0.8f);
        }

        return materialNave;
    }

    private static Material ObtenerMaterialEnemigo()
    {
        if (materialEnemigo == null)
        {
            materialEnemigo = CrearMaterialSolido(ColorEnemigo, ColorEnemigo * 0.7f);
        }

        return materialEnemigo;
    }

    private static Material ObtenerMaterialEnemigoDaltonico()
    {
        if (materialEnemigoDaltonico == null)
        {
            materialEnemigoDaltonico = CrearMaterialSolido(
                ColorEnemigoDaltonico,
                ColorEnemigoDaltonico * 0.7f
            );
        }

        return materialEnemigoDaltonico;
    }

    private static Material ObtenerMaterialBala()
    {
        if (materialBala == null)
        {
            materialBala = CrearMaterialSolido(ColorBala, ColorBala);
        }

        return materialBala;
    }

    private static Material ObtenerMaterialAtaqueEnemigo()
    {
        if (materialAtaqueEnemigo == null)
        {
            materialAtaqueEnemigo = CrearMaterialSolido(
                ColorAtaqueEnemigo,
                ColorAtaqueEnemigo
            );
        }

        return materialAtaqueEnemigo;
    }

    private static Material ObtenerMaterialAtaqueEnemigoDaltonico()
    {
        if (materialAtaqueEnemigoDaltonico == null)
        {
            materialAtaqueEnemigoDaltonico = CrearMaterialSolido(
                ColorAtaqueEnemigoDaltonico,
                ColorAtaqueEnemigoDaltonico
            );
        }

        return materialAtaqueEnemigoDaltonico;
    }

    private static Material CrearMaterialSolido(Color color, Color emision)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader == null)
        {
            return null;
        }

        Material material = new Material(shader);

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.color = color;
        }

        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emision);
        }

        return material;
    }
}
