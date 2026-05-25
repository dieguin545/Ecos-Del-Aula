using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class FirewallDelAulaVisuales
{
    private const string RutaBase = "SpaceShooter/FireWallDelAula/";

    private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    public static Sprite CargarSprite(string rutaRelativaSinExtension)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativaSinExtension))
        {
            return null;
        }

        string ruta = RutaBase + rutaRelativaSinExtension.Trim().Replace("\\", "/");

        if (sprites.TryGetValue(ruta, out Sprite spriteCacheado))
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
                    512f
                );
                sprite.name = textura.name;
            }
        }

        sprites[ruta] = sprite;
        return sprite;
    }

    public static void AplicarANave(GameObject objeto)
    {
        if (objeto == null)
        {
            return;
        }

        Sprite sprite = CargarSprite("Icons/ffffff/transparent/1x1/lorc/riot-shield");

        if (sprite == null)
        {
            sprite = CargarSprite("Icons/ffffff/transparent/1x1/delapouite/firewall");
        }

        SpriteRenderer renderer = CrearSprite(objeto.transform, "VisualFirewallDelAula", sprite, Vector3.zero, 2.9f);
        renderer.color = new Color(0.35f, 1f, 0.95f, 0.42f);
        renderer.sortingOrder = 8;
    }

    public static void AplicarAmenaza(GameObject objeto, TipoAmenaza tipo)
    {
        if (objeto == null)
        {
            return;
        }

        OcultarRenderers3D(objeto);
        string etiqueta = ObtenerEtiquetaAmenaza(tipo);
        Sprite sprite = CargarSprite("Bubbles/X4/Chat-" + ObtenerIndiceBurbuja(tipo));
        SpriteRenderer renderer = CrearSprite(objeto.transform, "BurbujaAmenaza", sprite, Vector3.zero, ObtenerEscalaBurbuja(tipo));
        renderer.color = ObtenerColorAmenaza(tipo);
        CrearEtiqueta(objeto.transform, etiqueta.ToUpperInvariant(), new Vector3(0f, 0.1f, 0f), 3.6f, ObtenerColorTextoAmenaza(tipo));
    }

    public static void AplicarProyectilReporte(GameObject objeto)
    {
        if (objeto == null)
        {
            return;
        }

        OcultarRenderers3D(objeto);
        Sprite sprite = CargarSprite("Icons/ffffff/transparent/1x1/delapouite/paper-plane");
        if (sprite == null)
        {
            sprite = CargarSprite("Icons/ffffff/transparent/1x1/lorc/paper-arrow");
        }

        SpriteRenderer renderer = CrearSprite(objeto.transform, "VisualReporte", sprite, Vector3.zero, 1.55f);
        renderer.color = new Color(0.35f, 1f, 0.95f, 1f);
        renderer.sortingOrder = 25;
    }

    public static void AplicarProyectilAmenaza(GameObject objeto)
    {
        if (objeto == null)
        {
            return;
        }

        OcultarRenderers3D(objeto);
        Sprite sprite = CargarSprite("Bubbles/Icons-6");
        SpriteRenderer renderer = CrearSprite(objeto.transform, "VisualAlertaDañina", sprite, Vector3.zero, 0.85f);
        renderer.color = new Color(1f, 0.22f, 0.18f, 0.95f);
    }

    public static void AplicarPowerUp(GameObject objeto, TipoPowerUp tipo)
    {
        if (objeto == null)
        {
            return;
        }

        OcultarRenderers3D(objeto);
        SpriteRenderer renderer = CrearSprite(
            objeto.transform,
            "VisualPowerUpFirewall",
            CargarSprite(ObtenerRutaPowerUp(tipo)),
            Vector3.zero,
            1.5f
        );
        renderer.color = ObtenerColorPowerUp(tipo);
        CrearEtiqueta(objeto.transform, ObtenerNombrePowerUp(tipo), new Vector3(0f, 1.05f, 0f), 0.62f, Color.white);
    }

    public static string ObtenerEtiquetaAmenaza(TipoAmenaza tipo)
    {
        switch (tipo)
        {
            case TipoAmenaza.RumorViral:
                return "Rumor";
            case TipoAmenaza.AtaqueCoordinado:
                return "Exclusión";
            case TipoAmenaza.NodoCorrupto:
                return "Spam";
            case TipoAmenaza.TiradorDigital:
            case TipoAmenaza.LaserCorrupto:
                return "Amenaza";
            default:
                return "Burla";
        }
    }

    private static string ObtenerIndiceBurbuja(TipoAmenaza tipo)
    {
        switch (tipo)
        {
            case TipoAmenaza.RumorViral:
                return "2";
            case TipoAmenaza.AtaqueCoordinado:
                return "4";
            case TipoAmenaza.NodoCorrupto:
                return "5";
            case TipoAmenaza.TiradorDigital:
                return "3";
            case TipoAmenaza.LaserCorrupto:
                return "6";
            default:
                return "1";
        }
    }

    private static float ObtenerEscalaBurbuja(TipoAmenaza tipo)
    {
        switch (tipo)
        {
            case TipoAmenaza.RumorViral:
                return 2.45f;
            case TipoAmenaza.AtaqueCoordinado:
                return 2.85f;
            case TipoAmenaza.LaserCorrupto:
                return 2.65f;
            default:
                return 2.55f;
        }
    }

    private static Color ObtenerColorAmenaza(TipoAmenaza tipo)
    {
        switch (tipo)
        {
            case TipoAmenaza.RumorViral:
                return new Color(1f, 0.88f, 0.24f, 0.98f);
            case TipoAmenaza.AtaqueCoordinado:
                return new Color(0.85f, 0.42f, 1f, 0.98f);
            case TipoAmenaza.NodoCorrupto:
                return new Color(0.55f, 0.7f, 1f, 0.95f);
            case TipoAmenaza.TiradorDigital:
            case TipoAmenaza.LaserCorrupto:
                return new Color(1f, 0.24f, 0.28f, 0.98f);
            default:
                return new Color(1f, 0.45f, 0.55f, 0.98f);
        }
    }

    private static Color ObtenerColorTextoAmenaza(TipoAmenaza tipo)
    {
        return tipo == TipoAmenaza.NodoCorrupto || tipo == TipoAmenaza.RumorViral
            ? new Color(0.04f, 0.06f, 0.12f)
            : Color.white;
    }

    private static string ObtenerRutaPowerUp(TipoPowerUp tipo)
    {
        switch (tipo)
        {
            case TipoPowerUp.Vida:
                return "Icons/ffffff/transparent/1x1/lorc/shining-heart";
            case TipoPowerUp.Escudo:
                return "Icons/ffffff/transparent/1x1/delapouite/rule-book";
            case TipoPowerUp.Dash:
                return "Icons/ffffff/transparent/1x1/lorc/treasure-map";
            case TipoPowerUp.DisparoMejorado:
                return "Icons/ffffff/transparent/1x1/lorc/papers";
            case TipoPowerUp.LimpiezaDigital:
                return "Icons/ffffff/transparent/1x1/delapouite/firewall";
            default:
                return "Icons/ffffff/transparent/1x1/delapouite/newspaper";
        }
    }

    private static string ObtenerNombrePowerUp(TipoPowerUp tipo)
    {
        switch (tipo)
        {
            case TipoPowerUp.Vida:
                return "Empatía";
            case TipoPowerUp.Escudo:
                return "Protocolo";
            case TipoPowerUp.Dash:
                return "Contexto";
            case TipoPowerUp.DisparoMejorado:
                return "Evidencia";
            case TipoPowerUp.LimpiezaDigital:
                return "Limpieza";
            default:
                return "Convivencia";
        }
    }

    private static Color ObtenerColorPowerUp(TipoPowerUp tipo)
    {
        switch (tipo)
        {
            case TipoPowerUp.Vida:
                return new Color(0.25f, 1f, 0.55f, 1f);
            case TipoPowerUp.Escudo:
                return new Color(0.35f, 0.85f, 1f, 1f);
            case TipoPowerUp.Dash:
                return new Color(1f, 0.9f, 0.25f, 1f);
            default:
                return Color.white;
        }
    }

    private static SpriteRenderer CrearSprite(
        Transform padre,
        string nombre,
        Sprite sprite,
        Vector3 posicionLocal,
        float escala
    )
    {
        Transform existente = padre.Find(nombre);
        GameObject objeto = existente != null ? existente.gameObject : new GameObject(nombre);
        objeto.transform.SetParent(padre, false);
        objeto.transform.localPosition = posicionLocal;
        objeto.transform.localScale = Vector3.one * escala;

        SpriteRenderer renderer = objeto.GetComponent<SpriteRenderer>();

        if (renderer == null)
        {
            renderer = objeto.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = sprite;
        renderer.sortingOrder = 20;

        if (objeto.GetComponent<FirewallBillboard>() == null)
        {
            objeto.AddComponent<FirewallBillboard>();
        }

        return renderer;
    }

    private static TextMeshPro CrearEtiqueta(
        Transform padre,
        string texto,
        Vector3 posicionLocal,
        float tamano,
        Color color
    )
    {
        const string nombre = "EtiquetaFirewall";
        Transform existente = padre.Find(nombre);
        GameObject objeto = existente != null ? existente.gameObject : new GameObject(nombre);
        objeto.transform.SetParent(padre, false);
        objeto.transform.localPosition = posicionLocal;
        objeto.transform.localScale = Vector3.one;

        TextMeshPro etiqueta = objeto.GetComponent<TextMeshPro>();

        if (etiqueta == null)
        {
            etiqueta = objeto.AddComponent<TextMeshPro>();
        }

        etiqueta.text = texto;
        etiqueta.fontSize = tamano;
        etiqueta.alignment = TextAlignmentOptions.Center;
        etiqueta.textWrappingMode = TextWrappingModes.NoWrap;
        etiqueta.fontStyle = FontStyles.Bold;
        etiqueta.enableAutoSizing = false;
        etiqueta.color = color;
        etiqueta.outlineColor = Color.black;
        etiqueta.outlineWidth = 0.32f;

        RectTransform rect = etiqueta.rectTransform;
        rect.sizeDelta = new Vector2(13f, 3.2f);

        Renderer renderer = etiqueta.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.sortingOrder = 30;
        }

        if (objeto.GetComponent<FirewallBillboard>() == null)
        {
            objeto.AddComponent<FirewallBillboard>();
        }

        return etiqueta;
    }

    private static void OcultarRenderers3D(GameObject objeto)
    {
        Renderer[] renderers = objeto.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            if (renderer == null)
            {
                continue;
            }

            if (renderer is SpriteRenderer)
            {
                continue;
            }

            if (renderer.GetComponentInParent<TextMeshPro>() != null)
            {
                continue;
            }

            renderer.enabled = false;
        }
    }
}

public class FirewallBillboard : MonoBehaviour
{
    private Camera camara;

    private void LateUpdate()
    {
        if (camara == null)
        {
            camara = Camera.main;
        }

        if (camara != null)
        {
            transform.rotation = camara.transform.rotation;
        }
    }
}
