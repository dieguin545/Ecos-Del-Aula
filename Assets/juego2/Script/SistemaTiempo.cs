using UnityEngine;
using TMPro;

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
    public float duracionBloque = 150f; // 2.5 minutos por bloque = 10 minutos total
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
        ActualizarUI();
    }

    void Update()
    {
        timerActual += Time.deltaTime;

        // Actualiza el texto del tiempo restante
        float tiempoRestante = duracionBloque - timerActual;
        int minutos = Mathf.FloorToInt(tiempoRestante / 60f);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60f);
        textoTiempo.text = $"{minutos:00}:{segundos:00}";

        if (timerActual >= duracionBloque)
        {
            timerActual = 0f;
            AvanzarBloque();
        }
    }

    private void AvanzarBloque()
    {
        bloqueActual++;

        if (bloqueActual >= bloques.Length)
        {
            // Se acabó el día
            TerminarDia();
            return;
        }

        ActualizarUI();
        NotificarBloque(bloques[bloqueActual]);
    }

    private void ActualizarUI()
    {
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
        return bloques[bloqueActual];
    }
}