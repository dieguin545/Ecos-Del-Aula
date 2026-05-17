using System;
using UnityEngine;

public class AccesibilidadSpaceShooter : MonoBehaviour
{
    private const string ClaveModoDaltonico = "ModoDaltonico";

    public static bool ModoDaltonicoActivo { get; private set; }

    public event Action<bool> AlCambiarModoDaltonico;

    private void Awake()
    {
        ModoDaltonicoActivo = PlayerPrefs.GetInt(ClaveModoDaltonico, 0) == 1;
    }

    public void EstablecerModoDaltonico(bool activo)
    {
        ModoDaltonicoActivo = activo;
        PlayerPrefs.SetInt(ClaveModoDaltonico, activo ? 1 : 0);
        PlayerPrefs.Save();
        AplicarATodos();
        AlCambiarModoDaltonico?.Invoke(activo);
    }

    public void AplicarATodos()
    {
        CrosshairApuntado[] crosshairs = FindObjectsByType<CrosshairApuntado>();

        for (int i = 0; i < crosshairs.Length; i++)
        {
            crosshairs[i].AplicarModoDaltonico(ModoDaltonicoActivo);
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
    }
}
