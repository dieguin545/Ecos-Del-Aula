using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Define el contrato que debe cumplir cualquier tema visual de BRIV.
// Esto es el rol "Strategy" del patron de diseno Strategy:
// el componente que aplique el tema no conoce los colores concretos,
// solo los recibe a traves de esta interfaz.
public interface IBRIVTheme
{
    string Nombre();
    Color ColorFondo();
    Color ColorSuperficie();
    Color ColorAcentoPrimario();
    Color ColorAcentoSecundario();
    Color ColorTextoPrincipal();
    Color ColorTextoSecundario();
    Color ColorBoton();
    Color ColorBotonHover();
}

// Tema oscuro por defecto inspirado en plataformas de minijuegos modernas
// (fondo profundo navy + acentos neon cyan / coral).
public class BRIVTemaOscuro : IBRIVTheme
{
    public string Nombre() { return "BRIV Dark"; }
    public Color ColorFondo() { return new Color(0.039f, 0.055f, 0.153f, 1f); }
    public Color ColorSuperficie() { return new Color(0.102f, 0.122f, 0.306f, 1f); }
    public Color ColorAcentoPrimario() { return new Color(0f, 0.851f, 1f, 1f); }
    public Color ColorAcentoSecundario() { return new Color(1f, 0.239f, 0.443f, 1f); }
    public Color ColorTextoPrincipal() { return new Color(1f, 1f, 1f, 1f); }
    public Color ColorTextoSecundario() { return new Color(0.706f, 0.718f, 0.816f, 1f); }
    public Color ColorBoton() { return new Color(0f, 0.851f, 1f, 1f); }
    public Color ColorBotonHover() { return new Color(0.4f, 0.9f, 1f, 1f); }
}

// Tema alternativo con paleta calida, util como modo accesibilidad/alto contraste.
public class BRIVTemaCalido : IBRIVTheme
{
    public string Nombre() { return "BRIV Warm"; }
    public Color ColorFondo() { return new Color(0.07f, 0.04f, 0.10f, 1f); }
    public Color ColorSuperficie() { return new Color(0.16f, 0.08f, 0.15f, 1f); }
    public Color ColorAcentoPrimario() { return new Color(1f, 0.45f, 0.2f, 1f); }
    public Color ColorAcentoSecundario() { return new Color(1f, 0.85f, 0.3f, 1f); }
    public Color ColorTextoPrincipal() { return new Color(1f, 0.97f, 0.92f, 1f); }
    public Color ColorTextoSecundario() { return new Color(0.85f, 0.78f, 0.70f, 1f); }
    public Color ColorBoton() { return new Color(1f, 0.45f, 0.2f, 1f); }
    public Color ColorBotonHover() { return new Color(1f, 0.6f, 0.35f, 1f); }
}

// MonoBehaviour que aplica un IBRIVTheme a un conjunto de elementos UI.
// Es el rol "Context" del patron Strategy.
public class BRIVTheme : MonoBehaviour
{
    public enum TemaDisponible { Oscuro, Calido }

    [Header("Tema activo")]
    public TemaDisponible temaSeleccionado = TemaDisponible.Oscuro;

    [Header("Elementos UI a teñir automaticamente")]
    public Camera camaraFondo;
    public Image[] superficies;
    public Image[] botones;
    public TMP_Text[] textosPrincipales;
    public TMP_Text[] textosSecundarios;
    public TMP_Text[] textosAcento;

    IBRIVTheme tema;

    void Start()
    {
        Aplicar();
    }

    public void Aplicar()
    {
        tema = ConstruirTema(temaSeleccionado);

        if (camaraFondo != null)
        {
            camaraFondo.backgroundColor = tema.ColorFondo();
        }

        AplicarColor(superficies, tema.ColorSuperficie());
        AplicarColor(botones, tema.ColorBoton());
        AplicarColorTexto(textosPrincipales, tema.ColorTextoPrincipal());
        AplicarColorTexto(textosSecundarios, tema.ColorTextoSecundario());
        AplicarColorTexto(textosAcento, tema.ColorAcentoPrimario());
    }

    public void CambiarTema(TemaDisponible nuevoTema)
    {
        temaSeleccionado = nuevoTema;
        Aplicar();
    }

    static IBRIVTheme ConstruirTema(TemaDisponible t)
    {
        if (t == TemaDisponible.Calido) return new BRIVTemaCalido();
        return new BRIVTemaOscuro();
    }

    static void AplicarColor(Image[] imagenes, Color c)
    {
        if (imagenes == null) return;
        for (int i = 0; i < imagenes.Length; i++)
        {
            if (imagenes[i] != null) imagenes[i].color = c;
        }
    }

    static void AplicarColorTexto(TMP_Text[] textos, Color c)
    {
        if (textos == null) return;
        for (int i = 0; i < textos.Length; i++)
        {
            if (textos[i] != null) textos[i].color = c;
        }
    }
}
