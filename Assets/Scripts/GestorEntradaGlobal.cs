using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TipoDispositivoEntrada
{
    TecladoMouse,
    ControlXbox
}

public class GestorEntradaGlobal : MonoBehaviour
{
    private static TipoDispositivoEntrada dispositivoActual = TipoDispositivoEntrada.TecladoMouse;
    private static float mouseXAnterior;
    private static float mouseYAnterior;

    public static event Action<TipoDispositivoEntrada> AlCambiarDispositivo;

    public static TipoDispositivoEntrada DispositivoActual => dispositivoActual;
    public static bool UsandoControl => dispositivoActual == TipoDispositivoEntrada.ControlXbox;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CrearSiHaceFalta()
    {
        if (FindAnyObjectByType<GestorEntradaGlobal>() != null)
        {
            return;
        }

        GameObject objeto = new GameObject("GestorEntradaGlobal");
        DontDestroyOnLoad(objeto);
        objeto.AddComponent<GestorEntradaGlobal>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void Update()
    {
        DetectarDispositivo();
    }

    public static bool InteractuarPresionado(KeyCode teclaTeclado)
    {
        return Input.GetKeyDown(teclaTeclado) || Input.GetKeyDown(KeyCode.JoystickButton2);
    }

    public static bool ConfirmarPresionado()
    {
        return Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.JoystickButton0);
    }

    public static bool CancelarPresionado()
    {
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1);
    }

    public static bool PausaPresionada()
    {
        return Input.GetKeyDown(KeyCode.Escape)
            || Input.GetKeyDown(KeyCode.JoystickButton7)
            || Input.GetKeyDown(KeyCode.JoystickButton9);
    }

    public static bool DisparoActivo()
    {
        return Input.GetMouseButton(0) || Input.GetKey(KeyCode.JoystickButton0);
    }

    public static bool TurboActivo()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.JoystickButton5);
    }

    public static bool DashIzquierdaPresionado()
    {
        return Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.JoystickButton4);
    }

    public static bool DashDerechaPresionado()
    {
        return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton1);
    }

    public static string ObtenerPromptInteractuarPC()
    {
        return UsandoControl && HayJoystickConectado()
            ? "Boton X para usar PC"
            : "Presiona E para usar PC";
    }

    public static string ObtenerPromptPausa()
    {
        return UsandoControl ? "[Menu] Pausa" : "Esc para pausar";
    }

    public static string ObtenerPromptCerrar()
    {
        return UsandoControl ? "[B] Cerrar" : "Esc / X cerrar";
    }

    private static void CambiarDispositivo(TipoDispositivoEntrada nuevo)
    {
        if (dispositivoActual == nuevo)
        {
            return;
        }

        dispositivoActual = nuevo;
        AlCambiarDispositivo?.Invoke(dispositivoActual);
    }

    private static void DetectarDispositivo()
    {
        if (
            HayJoystickConectado()
            && (
            Input.GetKeyDown(KeyCode.JoystickButton0)
            || Input.GetKeyDown(KeyCode.JoystickButton1)
            || Input.GetKeyDown(KeyCode.JoystickButton2)
            || Input.GetKeyDown(KeyCode.JoystickButton3)
            || Input.GetKeyDown(KeyCode.JoystickButton4)
            || Input.GetKeyDown(KeyCode.JoystickButton5)
            || Input.GetKeyDown(KeyCode.JoystickButton7)
            || Input.GetKeyDown(KeyCode.JoystickButton8)
            || Input.GetKeyDown(KeyCode.JoystickButton9)
            )
        )
        {
            CambiarDispositivo(TipoDispositivoEntrada.ControlXbox);
            return;
        }

        float mouseX = Input.mousePosition.x;
        float mouseY = Input.mousePosition.y;
        bool mouseMovido = Mathf.Abs(mouseX - mouseXAnterior) + Mathf.Abs(mouseY - mouseYAnterior) > 0.5f;
        mouseXAnterior = mouseX;
        mouseYAnterior = mouseY;

        if (Input.anyKeyDown && !HayBotonJoystickPresionado())
        {
            CambiarDispositivo(TipoDispositivoEntrada.TecladoMouse);
            return;
        }

        if (mouseMovido || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            CambiarDispositivo(TipoDispositivoEntrada.TecladoMouse);
        }
    }

    private static bool HayBotonJoystickPresionado()
    {
        if (!HayJoystickConectado())
        {
            return false;
        }

        for (int i = 0; i <= 19; i++)
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton0 + i))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HayJoystickConectado()
    {
        string[] nombres = Input.GetJoystickNames();

        for (int i = 0; i < nombres.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(nombres[i]))
            {
                return true;
            }
        }

        return false;
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        CambiarDispositivo(TipoDispositivoEntrada.TecladoMouse);
        AlCambiarDispositivo?.Invoke(dispositivoActual);
    }
}
