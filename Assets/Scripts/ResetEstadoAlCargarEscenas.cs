using UnityEngine;
using UnityEngine.SceneManagement;

public static class ResetEstadoAlCargarEscenas
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;

        ResetearSiHaceFalta(SceneManager.GetActiveScene());
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        ResetearSiHaceFalta(escena);
    }

    private static void ResetearSiHaceFalta(Scene escena)
    {
        string nombre = escena.name.ToLowerInvariant();
        bool escenaDelJuego = nombre == "juego"
            || nombre == "juego2"
            || nombre == "inicio"
            || nombre == "seleccion"
            || nombre == "seleccionjuego"
            || nombre == "spaceshooter";

        if (!escenaDelJuego)
        {
            return;
        }

        InteraccionPC.ResetearEstadoGlobalPC();
        MenuPausaAccesibilidad.ResetearEstadoGlobalPausa();

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AplicadorAccesibilidadGlobal.AplicarEscenaActual();

        if (nombre == "juego")
        {
            ConfiguradorEscenaJuego configurador = Object.FindAnyObjectByType<ConfiguradorEscenaJuego>();
            if (configurador == null)
            {
                GameObject objetoConfigurador = new GameObject("ConfiguradorEscenaJuego");
                configurador = objetoConfigurador.AddComponent<ConfiguradorEscenaJuego>();
            }

            if (configurador != null)
            {
                configurador.ConfigurarEscenaActual();
            }

            MenuPausaAccesibilidad[] menus = Object.FindObjectsByType<MenuPausaAccesibilidad>(FindObjectsInactive.Include);
            for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i] != null)
                {
                    menus[i].ReinicializarTrasCargaEscena();
                }
            }
        }

        if (nombre == "juego2")
        {
            MenuPausa[] menusJuego2 = Object.FindObjectsByType<MenuPausa>(FindObjectsInactive.Include);
            for (int i = 0; i < menusJuego2.Length; i++)
            {
                if (menusJuego2[i] != null)
                {
                    menusJuego2[i].ReinicializarTrasCargaEscena();
                }
            }
        }
    }
}
