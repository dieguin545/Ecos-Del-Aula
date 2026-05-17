using System.Collections.Generic;
using UnityEngine;

// Catalogo de juegos de BRIV. Centraliza la creacion del set de juegos
// disponibles en la plataforma. Sigue el patron Factory Method:
// cada CrearXxx() es una fabrica concreta que produce un BRIVGame configurado.
//
// Acceso real de cada juego desde el hub:
//   - Entry Filter    : tarjeta principal en SeleccionJuego
//   - Safe Halls      : segunda tarjeta en SeleccionJuego
//   - Cosmic Defender : accesible desde dentro de Entry Filter (boton interno)
public static class BRIVGameCatalog
{
    public const string ID_ENTRY_FILTER = "entry-filter";
    public const string ID_SAFE_HALLS = "safe-halls";
    public const string ID_CYBER_DEFENSE = "cyber-defense";

    // Catalogo completo: incluye los 3 juegos disponibles en la plataforma.
    public static List<BRIVGame> CatalogoPorDefecto()
    {
        var juegos = new List<BRIVGame>();
        juegos.Add(CrearEntryFilter());
        juegos.Add(CrearSafeHalls());
        juegos.Add(CrearCyberDefense());
        return juegos;
    }

    // Catalogo visible en el hub principal (solo los juegos que aparecen como
    // tarjeta en SeleccionJuego). Cyber Defense queda fuera porque su acceso
    // esta dentro de Entry Filter.
    public static List<BRIVGame> CatalogoHub()
    {
        var juegos = new List<BRIVGame>();
        juegos.Add(CrearEntryFilter());
        juegos.Add(CrearSafeHalls());
        return juegos;
    }

    public static BRIVGame CrearEntryFilter()
    {
        var g = new BRIVGame(ID_ENTRY_FILTER, "Entry Filter", "Juego");
        g.descripcion =
            "Detecta senales de bullying y protege a la comunidad escolar revisando " +
            "casos uno por uno: permite, reporta o pide apoyo segun la situacion.";
        g.esJuegoPrincipal = true;
        g.etiqueta = "PRINCIPAL";
        g.accentColor = new Color(0f, 0.851f, 1f, 1f);
        return g;
    }

    public static BRIVGame CrearSafeHalls()
    {
        var g = new BRIVGame(ID_SAFE_HALLS, "Safe Halls", "Juego2");
        g.descripcion =
            "Camina por la escuela controlando tu nivel de ansiedad. " +
            "Aprende a identificar zonas seguras y a pedir ayuda.";
        g.esJuegoPrincipal = false;
        g.etiqueta = "MINIJUEGO";
        g.accentColor = new Color(1f, 0.239f, 0.443f, 1f);
        return g;
    }

    public static BRIVGame CrearCyberDefense()
    {
        var g = new BRIVGame(ID_CYBER_DEFENSE, "Cyber Defense", "spaceshooter");
        g.descripcion =
            "Defiende tu espacio personal de comentarios toxicos. " +
            "Cada acierto reduce el ruido emocional del jugador.";
        g.esJuegoPrincipal = false;
        g.etiqueta = "MINIJUEGO";
        g.accentColor = new Color(0.6f, 0.3f, 1f, 1f);
        return g;
    }
}
