using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TipoDispositivoEntrada
{
    TecladoMouse,
    ControlXbox
}

public class GestorEntradaGlobal : MonoBehaviour
{
    private const string ClaveDispositivo = "EcosAula.UltimoDispositivo";
    private static TipoDispositivoEntrada dispositivoActual = TipoDispositivoEntrada.TecladoMouse;
    private static float mouseXAnterior;
    private static float mouseYAnterior;
    private static readonly HashSet<string> ejesNoDisponibles = new HashSet<string>();
    private static bool joystickConectadoAnterior = false;

    public static event Action<TipoDispositivoEntrada> AlCambiarDispositivo;

    public static TipoDispositivoEntrada DispositivoActual => dispositivoActual;
    public static bool UsandoControl => dispositivoActual == TipoDispositivoEntrada.ControlXbox;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CargarDispositivoGuardado()
    {
        // Si hay un control conectado físicamente, priorizarlo de entrada.
        if (VerificarNombresJoystick())
        {
            dispositivoActual = TipoDispositivoEntrada.ControlXbox;
            joystickConectadoAnterior = true;
            return;
        }

        dispositivoActual = (TipoDispositivoEntrada)PlayerPrefs.GetInt(
            ClaveDispositivo,
            (int)TipoDispositivoEntrada.TecladoMouse
        );
    }

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
        objeto.AddComponent<EcosAulaNavegacionUI>();
    }

    private void Awake()
    {
        mouseXAnterior = Input.mousePosition.x;
        mouseYAnterior = Input.mousePosition.y;
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
        return Input.GetKeyDown(teclaTeclado) || Input.GetKeyDown(KeyCode.JoystickButton0);
    }

    public static bool RevisarContextoPresionado()
    {
        return Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton2);
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

    public static float ObtenerCamaraHorizontal()
    {
        return LeerEjeSeguro("CameraHorizontal");
    }

    public static float ObtenerCamaraVertical()
    {
        return LeerEjeSeguro("CameraVertical");
    }

    public static string ObtenerPromptInteractuarPC()
    {
        return UsandoControl && HayJoystickConectado()
            ? "Botón A para usar PC"
            : "Presiona E para usar PC";
    }

    public static string ObtenerPromptPausa()
    {
        return UsandoControl ? "[Menu] Pausa" : "Esc para pausar";
    }

    public static string ObtenerPromptCerrar()
    {
        return UsandoControl ? "Botón B para cerrar" : "Esc / X cerrar";
    }

    private static void CambiarDispositivo(TipoDispositivoEntrada nuevo)
    {
        if (dispositivoActual == nuevo)
        {
            return;
        }

        dispositivoActual = nuevo;
        PlayerPrefs.SetInt(ClaveDispositivo, (int)dispositivoActual);
        PlayerPrefs.Save();
        AlCambiarDispositivo?.Invoke(dispositivoActual);
    }

    private static void DetectarDispositivo()
    {
        bool hayActividadJoystick = false;

        // 1. Detectar si cambió la conexión física (se conectó un joystick) en caliente
        bool joystickConectado = VerificarNombresJoystick();
        if (joystickConectado && !joystickConectadoAnterior)
        {
            hayActividadJoystick = true;
        }
        joystickConectadoAnterior = joystickConectado;

        // 2. Detectar si se presiona o mantiene cualquier botón del mando (JoystickButton0 a JoystickButton19)
        if (!hayActividadJoystick)
        {
            for (int i = 0; i <= 19; i++)
            {
                if (Input.GetKey(KeyCode.JoystickButton0 + i) || Input.GetKeyDown(KeyCode.JoystickButton0 + i))
                {
                    hayActividadJoystick = true;
                    break;
                }
            }
        }

        // 3. Detectar si se mueve cualquier eje analógico relevante con deadzone responsiva
        if (!hayActividadJoystick)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            float camH = ObtenerCamaraHorizontal();
            float camV = ObtenerCamaraVertical();
            float dpadH = 0f;
            float dpadV = 0f;
            try
            {
                dpadH = Input.GetAxisRaw("Debug Horizontal");
                dpadV = Input.GetAxisRaw("Debug Vertical");
            }
            catch {}

            // Umbral/deadzone razonable de 0.15f para máxima sensibilidad
            const float deadzoneDeteccion = 0.15f;

            if ((Mathf.Abs(h) > deadzoneDeteccion || Mathf.Abs(v) > deadzoneDeteccion || 
                 Mathf.Abs(camH) > deadzoneDeteccion || Mathf.Abs(camV) > deadzoneDeteccion ||
                 Mathf.Abs(dpadH) > deadzoneDeteccion || Mathf.Abs(dpadV) > deadzoneDeteccion) && 
                !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && 
                !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D) &&
                !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) &&
                !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            {
                hayActividadJoystick = true;
            }
        }

        if (hayActividadJoystick)
        {
            CambiarDispositivo(TipoDispositivoEntrada.ControlXbox);
            return;
        }

        // 4. Detectar actividad de teclado/mouse con filtro robusto contra ruido de hardware y deltas de centrado
        bool mouseMovido = (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.05f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.05f);

        if (Input.anyKeyDown && !HayBotonJoystickPresionado())
        {
            CambiarDispositivo(TipoDispositivoEntrada.TecladoMouse);
            return;
        }

        if (mouseMovido || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            CambiarDispositivo(TipoDispositivoEntrada.TecladoMouse);
        }
    }

    private static bool HayBotonJoystickPresionado()
    {
        for (int i = 0; i <= 19; i++)
        {
            if (Input.GetKey(KeyCode.JoystickButton0 + i) || Input.GetKeyDown(KeyCode.JoystickButton0 + i))
            {
                return true;
            }
        }
        return false;
    }

    private static bool VerificarNombresJoystick()
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

    private static bool HayJoystickConectado()
    {
        if (UsandoControl)
        {
            return true;
        }
        return VerificarNombresJoystick();
    }

    private static float LeerEjeSeguro(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre) || ejesNoDisponibles.Contains(nombre))
        {
            return 0f;
        }

        try
        {
            return Input.GetAxisRaw(nombre);
        }
        catch (ArgumentException)
        {
            ejesNoDisponibles.Add(nombre);
            return 0f;
        }
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        AlCambiarDispositivo?.Invoke(dispositivoActual);
    }
}
