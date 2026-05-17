using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnxietyBarStyler : MonoBehaviour, IAnxietyObserver
{
    [Header("Componentes")]
    public Slider slider;
    public Image fillImage;
    public TextMeshProUGUI textoAnsiedad;
    public Image backgroundImage;

    [Header("Colores")]
    public Color colorBajo = new Color(0.1f, 0.6f, 0.1f);      // Verde
    public Color colorMedio = new Color(0.9f, 0.6f, 0.0f);     // Naranja
    public Color colorAlto = new Color(0.7f, 0.0f, 0.0f);      // Rojo oscuro
    public Color colorCritico = new Color(1f, 0.0f, 0.0f);     // Rojo brillante

    private float pulseTimer = 0f;
    private bool isPulsing = false;

    void Start()
    {
        AnxietySystem.Instance.AddObserver(this);
        backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    }

    void OnDestroy()
    {
        AnxietySystem.Instance.RemoveObserver(this);
    }

    void Update()
    {
        if (isPulsing)
        {
            pulseTimer += Time.deltaTime * 5f;
            float alpha = Mathf.Abs(Mathf.Sin(pulseTimer));
            fillImage.color = new Color(colorCritico.r, colorCritico.g, colorCritico.b, alpha);
        }
    }

    public void OnAnxietyChanged(float currentAnxiety, float maxAnxiety)
    {
        slider.value = currentAnxiety;
        slider.maxValue = maxAnxiety;

        float porcentaje = currentAnxiety / maxAnxiety;

        // Cambia color segun nivel
        if (porcentaje < 0.33f)
        {
            fillImage.color = colorBajo;
            isPulsing = false;
        }
        else if (porcentaje < 0.66f)
        {
            fillImage.color = colorMedio;
            isPulsing = false;
        }
        else if (porcentaje < 0.9f)
        {
            fillImage.color = colorAlto;
            isPulsing = false;
        }
        else
        {
            isPulsing = true;
            pulseTimer = 0f;
        }

        // Actualiza texto
        textoAnsiedad.text = $"Ansiedad: {Mathf.RoundToInt(currentAnxiety)}%";
    }
}