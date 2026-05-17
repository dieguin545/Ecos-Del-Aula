using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Controlador central de la plataforma BRIV.
// Mantiene la lista de juegos disponibles y gestiona la navegacion entre ellos.
// Se apoya en BRIVGameCatalog (Factory) para construir el set de juegos por defecto.
public class BRIVHub : MonoBehaviour
{
    [Header("Juegos disponibles en la plataforma")]
    public List<BRIVGame> juegos = new List<BRIVGame>();

    [Header("Comportamiento")]
    public bool inicializarConCatalogoPorDefecto = true;
    public string escenaInicio = "inicio";
    public string escenaSeleccionPersonaje = "seleccion";
    public string escenaHub = "SeleccionJuego";

    void Awake()
    {
        if (inicializarConCatalogoPorDefecto && (juegos == null || juegos.Count == 0))
        {
            juegos = BRIVGameCatalog.CatalogoPorDefecto();
        }
    }

    public void AbrirJuegoPorId(string id)
    {
        BRIVGame juego = BuscarPorId(id);
        if (juego != null)
        {
            CargarEscena(juego.sceneName);
        }
        else
        {
            Debug.LogWarning("[BRIV] Juego no encontrado con id: " + id);
        }
    }

    public void AbrirJuegoPorIndice(int indice)
    {
        if (indice >= 0 && indice < juegos.Count)
        {
            CargarEscena(juegos[indice].sceneName);
        }
    }

    public void IrAlHub() { CargarEscena(escenaHub); }
    public void IrAInicio() { CargarEscena(escenaInicio); }
    public void IrASeleccionPersonaje() { CargarEscena(escenaSeleccionPersonaje); }

    public BRIVGame JuegoPrincipal()
    {
        foreach (var juego in juegos)
        {
            if (juego.esJuegoPrincipal) return juego;
        }
        return null;
    }

    BRIVGame BuscarPorId(string id)
    {
        foreach (var juego in juegos)
        {
            if (juego.id == id) return juego;
        }
        return null;
    }

    void CargarEscena(string nombre)
    {
        if (string.IsNullOrEmpty(nombre))
        {
            Debug.LogWarning("[BRIV] Intento de cargar escena vacia");
            return;
        }
        SceneManager.LoadScene(nombre);
    }
}
