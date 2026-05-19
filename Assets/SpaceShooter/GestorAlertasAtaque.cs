using System.Collections.Generic;
using UnityEngine;

public class GestorAlertasAtaque : MonoBehaviour
{
    [Header("Alertas")]
    [SerializeField] private bool alertasActivas = true;
    [SerializeField] private float ventanaAgrupacion = 0.45f;
    [SerializeField] private float distanciaMaximaAlerta = 32f;

    private readonly List<Meteorito> amenazasPendientes = new List<Meteorito>();
    private float mostrarEn;

    private void Update()
    {
        if (!alertasActivas || amenazasPendientes.Count == 0 || Time.time < mostrarEn)
        {
            return;
        }

        LimpiarReferenciasNulas();

        if (amenazasPendientes.Count > 0 && GameManager.instancia != null)
        {
            GameManager.instancia.MostrarAlertaAmenaza(amenazasPendientes.Count);
        }

        amenazasPendientes.Clear();
    }

    public void RegistrarAmenaza(Meteorito amenaza, float distancia)
    {
        if (
            !alertasActivas
            || amenaza == null
            || distancia > distanciaMaximaAlerta
            || AccesibilidadSpaceShooter.ReducirEfectosActivo
        )
        {
            return;
        }

        if (!amenazasPendientes.Contains(amenaza))
        {
            amenazasPendientes.Add(amenaza);
        }

        if (mostrarEn <= Time.time)
        {
            mostrarEn = Time.time + ventanaAgrupacion;
        }
    }

    private void LimpiarReferenciasNulas()
    {
        amenazasPendientes.RemoveAll(amenaza => amenaza == null);
    }
}
