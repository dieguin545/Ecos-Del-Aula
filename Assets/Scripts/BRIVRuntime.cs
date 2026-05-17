using UnityEngine;
using UnityEngine.SceneManagement;

// BRIVRuntime: hace que toda la plataforma BRIV funcione sin que el usuario
// tenga que arrastrar nada en el Editor de Unity.
//
// - Se inicializa automaticamente antes de que se cargue la primera escena
//   gracias a [RuntimeInitializeOnLoadMethod] (no necesita estar en ninguna escena).
// - Crea un GameObject persistente con el componente BRIVHub.
// - Aplica el tema visual BRIV (color de fondo dark navy) a la camara principal
//   cada vez que se carga una escena.
// - Asi cumple el rol de "context" del patron Strategy con BRIVTheme/IBRIVTheme.
public static class BRIVRuntime
{
    static BRIVHub hubGlobal;
    static IBRIVTheme temaActivo = new BRIVTemaOscuro();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Inicializar()
    {
        var go = new GameObject("[BRIVRuntime]");
        Object.DontDestroyOnLoad(go);

        hubGlobal = go.AddComponent<BRIVHub>();
        hubGlobal.inicializarConCatalogoPorDefecto = true;

        SceneManager.sceneLoaded += OnEscenaCargada;
    }

    static void OnEscenaCargada(Scene escena, LoadSceneMode modo)
    {
        AplicarTemaACamara();
    }

    static void AplicarTemaACamara()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        cam.backgroundColor = temaActivo.ColorFondo();
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    public static BRIVHub HubGlobal()
    {
        return hubGlobal;
    }

    public static IBRIVTheme TemaActivo()
    {
        return temaActivo;
    }

    public static void CambiarTema(IBRIVTheme nuevoTema)
    {
        if (nuevoTema == null) return;
        temaActivo = nuevoTema;
        AplicarTemaACamara();
    }
}
