using System;
using UnityEngine;

public class AccesibilidadSpaceShooter : MonoBehaviour
{
    private const string ClaveTextoGrande = "accesibilidad_texto_grande";
    private const string ClaveAltoContraste = "accesibilidad_alto_contraste";
    private const string ClaveTipoDaltonismo = "accesibilidad_tipo_daltonismo";

    public static bool TextoGrandeActivo { get; private set; }
    public static bool AltoContrasteActivo { get; private set; }
    public static bool ReducirEfectosActivo => false;
    public static TipoDaltonismo TipoDaltonismoActual { get; private set; }
    public static bool ModoDaltonicoActivo => TipoDaltonismoActual != TipoDaltonismo.Ninguno;

    public event Action AlCambiarConfiguracion;

    private void Awake()
    {
        TextoGrandeActivo = PlayerPrefs.GetInt(ClaveTextoGrande, 0) == 1;
        AltoContrasteActivo = PlayerPrefs.GetInt(ClaveAltoContraste, 0) == 1;
        TipoDaltonismoActual = (TipoDaltonismo)PlayerPrefs.GetInt(
            ClaveTipoDaltonismo,
            (int)TipoDaltonismo.Ninguno
        );
    }

    public void EstablecerModoDaltonico(bool activo)
    {
        EstablecerTipoDaltonismo(activo ? TipoDaltonismo.Deuteranopia : TipoDaltonismo.Ninguno);
    }

    public void EstablecerTipoDaltonismo(TipoDaltonismo tipo)
    {
        TipoDaltonismoActual = tipo;
        GuardarYAplicar();
    }

    public void EstablecerTextoGrande(bool activo)
    {
        TextoGrandeActivo = activo;
        GuardarYAplicar();
    }

    public void EstablecerAltoContraste(bool activo)
    {
        AltoContrasteActivo = activo;
        GuardarYAplicar();
    }

    public void EstablecerReducirEfectos(bool activo)
    {
        GuardarYAplicar();
    }

    public void AplicarATodos()
    {
        CrosshairApuntado[] crosshairs = FindObjectsByType<CrosshairApuntado>();

        for (int i = 0; i < crosshairs.Length; i++)
        {
            crosshairs[i].AplicarTipoDaltonismo(TipoDaltonismoActual);
        }

        Meteorito[] meteoritos = FindObjectsByType<Meteorito>();

        for (int i = 0; i < meteoritos.Length; i++)
        {
            meteoritos[i].AplicarAccesibilidadVisual();
        }

        ProyectilEnemigo[] proyectiles = FindObjectsByType<ProyectilEnemigo>();

        for (int i = 0; i < proyectiles.Length; i++)
        {
            proyectiles[i].AplicarAccesibilidadVisual();
        }

        GameManager gameManager = FindAnyObjectByType<GameManager>();

        if (gameManager != null)
        {
            gameManager.AplicarAccesibilidadVisual();
        }

        FondoEstelarSpaceShooter fondo = FindAnyObjectByType<FondoEstelarSpaceShooter>();

        if (fondo != null)
        {
            fondo.AplicarAccesibilidad();
        }
    }

    private void GuardarYAplicar()
    {
        PlayerPrefs.SetInt(ClaveTextoGrande, TextoGrandeActivo ? 1 : 0);
        PlayerPrefs.SetInt(ClaveAltoContraste, AltoContrasteActivo ? 1 : 0);
        PlayerPrefs.SetInt(ClaveTipoDaltonismo, (int)TipoDaltonismoActual);
        PlayerPrefs.Save();
        AplicarATodos();
        AplicadorAccesibilidadGlobal.AplicarEscenaActual();
        AlCambiarConfiguracion?.Invoke();
    }
}
