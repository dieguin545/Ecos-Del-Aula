using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectorNave : MonoBehaviour
{
    [Header("Modelos disponibles")]
    [SerializeField] private List<ModeloNaveDisponible> modelosDisponibles =
        new List<ModeloNaveDisponible>();

    [Header("Destino visual")]
    [SerializeField] private Transform contenedorVisual;
    [SerializeField] private Renderer[] renderersBase = Array.Empty<Renderer>();

    private readonly ListaCircular<ModeloNaveDisponible> carruselNaves =
        new ListaCircular<ModeloNaveDisponible>();

    private GameObject instanciaVisualActual;
    private ModeloNaveDisponible seleccionActual;

    public event Action<ModeloNaveDisponible> AlCambiarSeleccion;

    public ModeloNaveDisponible SeleccionActual => seleccionActual;
    public int CantidadModelos => carruselNaves.Cantidad;

    private void Awake()
    {
        PrepararContenedor();
        ReconstruirCarrusel();
    }

    public void ConfigurarDestino(Transform destino, Renderer[] renderers)
    {
        contenedorVisual = destino;
        renderersBase = renderers;
        PrepararContenedor();
        AplicarSeleccionActual();
    }

    public void ReconstruirCarrusel()
    {
        carruselNaves.Limpiar();

        for (int i = 0; i < modelosDisponibles.Count; i++)
        {
            if (modelosDisponibles[i] != null)
            {
                carruselNaves.Agregar(modelosDisponibles[i]);
            }
        }

        seleccionActual = carruselNaves.Actual;
        AplicarSeleccionActual();
    }

    public ModeloNaveDisponible SeleccionarSiguiente()
    {
        seleccionActual = carruselNaves.Siguiente();
        AplicarSeleccionActual();
        return seleccionActual;
    }

    public ModeloNaveDisponible SeleccionarAnterior()
    {
        seleccionActual = carruselNaves.Anterior();
        AplicarSeleccionActual();
        return seleccionActual;
    }

    private void PrepararContenedor()
    {
        if (contenedorVisual != null)
        {
            return;
        }

        Transform nave = transform;

        if (!CompareTag("Nave"))
        {
            GameObject objNave = GameObject.FindWithTag("Nave");
            nave = objNave != null ? objNave.transform : transform;
        }

        Transform existente = nave.Find("ModeloSeleccionado");

        if (existente != null)
        {
            contenedorVisual = existente;
            return;
        }

        GameObject contenedor = new GameObject("ModeloSeleccionado");
        contenedor.transform.SetParent(nave, false);
        contenedorVisual = contenedor.transform;
    }

    private void AplicarSeleccionActual()
    {
        if (contenedorVisual == null)
        {
            return;
        }

        if (instanciaVisualActual != null)
        {
            Destroy(instanciaVisualActual);
        }

        bool hayModelo = seleccionActual != null && seleccionActual.modelo != null;
        ActivarRenderersBase(!hayModelo);

        if (!hayModelo)
        {
            EstiloVisualSpaceShooter.AplicarANave(gameObject);
            AlCambiarSeleccion?.Invoke(seleccionActual);
            return;
        }

        instanciaVisualActual = Instantiate(
            seleccionActual.modelo,
            contenedorVisual.position,
            contenedorVisual.rotation,
            contenedorVisual
        );

        instanciaVisualActual.transform.localPosition = seleccionActual.posicionLocal;
        instanciaVisualActual.transform.localRotation = Quaternion.Euler(seleccionActual.rotacionLocal);
        instanciaVisualActual.transform.localScale = seleccionActual.escalaLocal;

        FirewallDelAulaVisuales.AplicarANave(gameObject);
        AlCambiarSeleccion?.Invoke(seleccionActual);
    }

    private void ActivarRenderersBase(bool activos)
    {
        if (renderersBase == null)
        {
            return;
        }

        for (int i = 0; i < renderersBase.Length; i++)
        {
            if (renderersBase[i] != null)
            {
                renderersBase[i].enabled = activos;
            }
        }
    }
}
