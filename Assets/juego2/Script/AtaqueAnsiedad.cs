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
        {
            Instance = this;
        }

        if (panelAtaque != null)
        {
            panelAtaque.SetActive(false);
        }
    }

    void Start()
    {
        if (AnxietySystem.Instance != null)
        {
            AnxietySystem.Instance.AddObserver(this);
        }

        movimientoJugador = FindAnyObjectByType<MovimientoJugador>();
    }

    void OnDestroy()
    {
        if (AnxietySystem.Instance != null)
        {
            AnxietySystem.Instance.RemoveObserver(this);
        }
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

        if (movimientoJugador != null)
        {
            movimientoJugador.enabled = false;
        }

        if (panelAtaque != null)
        {
            panelAtaque.SetActive(true);
        }
        if (textoAtaque != null)
        {
            textoAtaque.text = "Estás sufriendo un ataque de ansiedad...\nRespira profundo.";
        }

        float timer = 0f;
        while (timer < duracionAtaque)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Abs(Mathf.Sin(timer * 3f)) * 0.5f;
            if (imagenOverlay != null)
            {
                imagenOverlay.color = new Color(1f, 0f, 0f, alpha);
            }
            yield return null;
        }

        if (panelAtaque != null)
        {
            panelAtaque.SetActive(false);
        }
        if (imagenOverlay != null)
        {
            imagenOverlay.color = new Color(0f, 0f, 0f, 0f);
        }

        if (movimientoJugador != null)
        {
            movimientoJugador.enabled = true;
        }

        if (AnxietySystem.Instance != null)
        {
            AnxietySystem.Instance.DecreaseAnxiety(50f);
        }

        enAtaque = false;
    }
}
