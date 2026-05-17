using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AtaqueAnsiedad : MonoBehaviour, IAnxietyObserver
{
    public static AtaqueAnsiedad Instance;

    [Header("UI")]
    public GameObject panelAtaque;
    public TextMeshProUGUI textoAtaque;
    public Image imagenOverlay;

    [Header("Configuracion")]
    public float duracionAtaque = 5f;
    private bool enAtaque = false;
    private MovimientoJugador movimientoJugador;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        panelAtaque.SetActive(false);
    }

    void Start()
    {
        AnxietySystem.Instance.AddObserver(this);
        movimientoJugador = FindObjectOfType<MovimientoJugador>();
    }

    void OnDestroy()
    {
        AnxietySystem.Instance.RemoveObserver(this);
    }

    public void OnAnxietyChanged(float currentAnxiety, float maxAnxiety)
    {
        if (currentAnxiety >= maxAnxiety && !enAtaque)
        {
            StartCoroutine(EjecutarAtaque());
        }
    }

    private IEnumerator EjecutarAtaque()
    {
        enAtaque = true;

        // Desactiva el movimiento del jugador
        if (movimientoJugador != null)
            movimientoJugador.enabled = false;

        // Muestra el panel de ataque
        panelAtaque.SetActive(true);
        textoAtaque.text = "Estás sufriendo un ataque de ansiedad...\nRespira profundo.";

        // Efecto de overlay rojo pulsante
        float timer = 0f;
        while (timer < duracionAtaque)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Abs(Mathf.Sin(timer * 3f)) * 0.5f;
            imagenOverlay.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }

        // Termina el ataque
        panelAtaque.SetActive(false);
        imagenOverlay.color = new Color(0f, 0f, 0f, 0f);

        if (movimientoJugador != null)
            movimientoJugador.enabled = true;

        // Reduce la ansiedad a la mitad después del ataque
        AnxietySystem.Instance.DecreaseAnxiety(50f);
        enAtaque = false;
    }
}