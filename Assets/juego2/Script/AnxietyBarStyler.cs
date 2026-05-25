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
        ResolverReferencias();
        AplicarEstiloBase();

        if (AnxietySystem.Instance != null)
        {
            AnxietySystem.Instance.AddObserver(this);
            OnAnxietyChanged(AnxietySystem.Instance.GetCurrentAnxiety(), AnxietySystem.Instance.maxAnxiety);
        }
    }

    void OnDestroy()
    {
        if (AnxietySystem.Instance != null)
        {
            AnxietySystem.Instance.RemoveObserver(this);
        }
    }

    void Update()
    {
        if (isPulsing)
        {
            pulseTimer += Time.deltaTime * 5f;
            float alpha = Mathf.Abs(Mathf.Sin(pulseTimer));
            if (fillImage != null)
            {
                fillImage.color = new Color(colorCritico.r, colorCritico.g, colorCritico.b, Mathf.Lerp(0.58f, 1f, alpha));
            }
        }
    }

    public void OnAnxietyChanged(float currentAnxiety, float maxAnxiety)
    {
        ResolverReferencias();

        if (maxAnxiety <= 0f)
        {
            maxAnxiety = 100f;
        }

        if (slider != null)
        {
            slider.maxValue = maxAnxiety;
            slider.value = Mathf.Clamp(currentAnxiety, 0f, maxAnxiety);
        }

        float porcentaje = currentAnxiety / maxAnxiety;

        // Cambia color segun nivel
        if (porcentaje < 0.33f)
        {
            AplicarColorRelleno(colorBajo);
            isPulsing = false;
        }
        else if (porcentaje < 0.66f)
        {
            AplicarColorRelleno(colorMedio);
            isPulsing = false;
        }
        else if (porcentaje < 0.9f)
        {
            AplicarColorRelleno(colorAlto);
            isPulsing = false;
        }
        else
        {
            isPulsing = true;
            pulseTimer = 0f;
        }

        // Actualiza texto
        if (textoAnsiedad != null)
        {
            textoAnsiedad.text = $"Ansiedad: {Mathf.RoundToInt(currentAnxiety)}%";
        }
    }

    private void ResolverReferencias()
    {
        if (slider == null)
        {
            slider = GetComponentInChildren<Slider>(true);
        }

        if (fillImage == null && slider != null && slider.fillRect != null)
        {
            fillImage = slider.fillRect.GetComponent<Image>();
        }

        if (textoAnsiedad == null)
        {
            textoAnsiedad = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }

    private void AplicarEstiloBase()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0.025f, 0.018f, 0.055f, 0.86f);
            backgroundImage.raycastTarget = false;

            Outline outline = backgroundImage.GetComponent<Outline>();
            if (outline == null)
            {
                outline = backgroundImage.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0.20f, 0.86f, 1f, 0.58f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        if (textoAnsiedad != null)
        {
            textoAnsiedad.color = new Color(0.90f, 0.98f, 1f, 1f);
            textoAnsiedad.fontSize = Mathf.Max(textoAnsiedad.fontSize, 18f);
            textoAnsiedad.fontStyle = FontStyles.Bold;
        }
    }

    private void AplicarColorRelleno(Color color)
    {
        if (fillImage != null)
        {
            fillImage.color = color;
        }
    }
}
