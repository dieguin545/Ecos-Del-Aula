using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum BloqueTiempo
{
    Entrada,
    Descanso,
    Almuerzo,
    Salida
}

public class SistemaTiempo : MonoBehaviour
{
    public static SistemaTiempo Instance;

    [Header("Configuracion")]
    public float duracionBloque = 75f; // 2.5 minutos por bloque = 10 minutos total
    private float timerActual = 0f;
    private int bloqueActual = 0;

    [Header("UI")]
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoBloque;

    private BloqueTiempo[] bloques = {
        BloqueTiempo.Entrada,
        BloqueTiempo.Descanso,
        BloqueTiempo.Almuerzo,
        BloqueTiempo.Salida
    };

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        AplicarEstiloUI();
        ActualizarUI();
        OcultarTextoLegacy();
    }

    void Update()
    {
        timerActual += Time.deltaTime;

        // Actualiza el texto del tiempo restante
        float tiempoRestante = duracionBloque - timerActual;
        tiempoRestante = Mathf.Max(0f, tiempoRestante);
        int minutos = Mathf.FloorToInt(tiempoRestante / 60f);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60f);
        if (textoTiempo != null)
        {
            textoTiempo.text = $"{minutos:00}:{segundos:00}";
        }

        VidaEscolarHUD.Ensure().ActualizarTiempo(GetBloqueActual(), tiempoRestante);

        if (timerActual >= duracionBloque)
        {
            timerActual = 0f;
            AvanzarBloque();
        }
    }

    private void AvanzarBloque()
{
    bloqueActual++;
    if (SistemaGuardado.Instance != null)
    {
        SistemaGuardado.Instance.GuardarPartida(); // Guarda al cambiar bloque
    }
    
    if (bloqueActual >= bloques.Length)
    {
        TerminarDia();
        return;
    }

    ActualizarUI();
    NotificarBloque(bloques[bloqueActual]);
}

    private void ActualizarUI()
    {
        if (textoBloque == null || bloqueActual < 0 || bloqueActual >= bloques.Length)
        {
            return;
        }

        switch (bloques[bloqueActual])
        {
            case BloqueTiempo.Entrada:
                textoBloque.text = "Entrada";
                break;
            case BloqueTiempo.Descanso:
                textoBloque.text = "Descanso";
                break;
            case BloqueTiempo.Almuerzo:
                textoBloque.text = "Almuerzo";
                break;
            case BloqueTiempo.Salida:
                textoBloque.text = "Salida";
                break;
        }

        VidaEscolarHUD.Ensure().ActualizarTiempo(bloques[bloqueActual], duracionBloque - timerActual);
    }

    private void NotificarBloque(BloqueTiempo bloque)
    {
        Debug.Log("Nuevo bloque: " + bloque);
        // Aqui despues conectamos con el bully y las zonas
    }

    private void TerminarDia()
    {
        Debug.Log("El dia termino");
        // Aqui despues activamos la confrontacion final
    }

    public BloqueTiempo GetBloqueActual()
    {
        int indice = Mathf.Clamp(bloqueActual, 0, bloques.Length - 1);
        return bloques[indice];
    }

    private void AplicarEstiloUI()
    {
        EstilizarTexto(textoBloque, 28f, FontStyles.Bold, new Color(0.90f, 0.98f, 1f, 1f));
        EstilizarTexto(textoTiempo, 22f, FontStyles.Bold, Color.white);
        EstilizarContenedor(textoBloque);
        EstilizarContenedor(textoTiempo);
    }

    private void EstilizarTexto(TextMeshProUGUI texto, float fontSize, FontStyles fontStyle, Color color)
    {
        if (texto == null)
        {
            return;
        }

        texto.fontSize = Mathf.Max(texto.fontSize, fontSize);
        texto.fontStyle = fontStyle;
        texto.color = color;
        texto.alignment = TextAlignmentOptions.Center;

        Outline outline = texto.GetComponent<Outline>();
        if (outline == null)
        {
            outline = texto.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = new Color(0f, 0f, 0f, 0.72f);
        outline.effectDistance = new Vector2(2f, -2f);
    }

    private void EstilizarContenedor(TextMeshProUGUI texto)
    {
        if (texto == null || texto.transform.parent == null)
        {
            return;
        }

        Transform parent = texto.transform.parent;
        if (parent.GetComponent<Canvas>() != null)
        {
            return;
        }

        Image fondo = parent.GetComponent<Image>();
        if (fondo == null)
        {
            fondo = parent.gameObject.AddComponent<Image>();
        }

        fondo.color = new Color(0.025f, 0.018f, 0.055f, 0.62f);
        fondo.raycastTarget = false;
    }

    private void OcultarTextoLegacy()
    {
        if (textoBloque != null)
        {
            textoBloque.enabled = false;
        }

        if (textoTiempo != null)
        {
            textoTiempo.enabled = false;
        }
    }
}
