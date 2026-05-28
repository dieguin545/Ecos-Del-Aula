using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TutorialPresentation : MonoBehaviour
{
    [Header("Panel raíz")]
    public GameObject panelTutorial;
    public CanvasGroup panelCanvasGroup;

    [Header("Contenido")]
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoDescripcion;
    public Image iconoImagen;
    public Sprite[] iconosPantallas;

    [Header("Navegación")]
    public Button btnSiguiente;
    public Button btnAnterior;
    public TextMeshProUGUI textoBtnSiguiente;

    [Header("Dots")]
    public Image[] dots;
    public Color colorDotActivo   = Color.white;
    public Color colorDotInactivo = new Color(1f, 1f, 1f, 0.3f);

    [Header("Ajustes")]
    public float duracionFade = 0.25f;

    private int paginaActual = 0;
    private bool animando = false;

    private readonly string[] titulos = {
        "Ponte en sus zapatos",
        "Controla tu ansiedad",
        "Misiones y objetos",
        "El enfrentamiento final"
    };

    private readonly string[] descripciones = {
        "Hoy eres quien sufre el bullying.\nCada insulto sube tu barra de ansiedad.\nSi llega al máximo sufrirás un ataque.",
        "Busca los signos ! repartidos por el mapa.\nRealizar esas actividades baja tu ansiedad\ny te ayuda a aguantar el día completo.",
        "Completa misiones y recoge objetos clave.\nRecolectar evidencia cambia radicalmente\ncómo termina el juego.",
        "Puedes pelear, gritar por ayuda,\no si tienes suficiente evidencia\nsaltarte la pelea y ganar de inmediato."
    };

    void Start()
    {
        panelTutorial.SetActive(true);
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.DOFade(1f, duracionFade).SetUpdate(true);
        ActualizarContenido();

        // Pausar el juego al inicio si el tutorial está activo
        Time.timeScale = 0f;

        MovimientoJugador mov = FindAnyObjectByType<MovimientoJugador>();
        if (mov != null) mov.enabled = false;
    }

    public void Siguiente()
    {
        if (animando) return;

        if (paginaActual < titulos.Length - 1)
            CambiarPagina(paginaActual + 1);
        else
            CerrarTutorial();
    }

    public void Anterior()
    {
        if (animando || paginaActual == 0) return;
        CambiarPagina(paginaActual - 1);
    }

    private void CambiarPagina(int index)
    {
        animando = true;

        // Fade out del contenido, swap, fade in
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); // Permite que la secuencia corra en tiempo real cuando la escala de tiempo es 0

        seq.Append(panelCanvasGroup.DOFade(0.3f, duracionFade * 0.5f));
        seq.AppendCallback(() =>
        {
            paginaActual = index;
            ActualizarContenido();
        });
        seq.Append(panelCanvasGroup.DOFade(1f, duracionFade * 0.5f));
        seq.OnComplete(() => animando = false);
    }

    private void ActualizarContenido()
    {
        textoTitulo.text = titulos[paginaActual];
        textoDescripcion.text = descripciones[paginaActual];

        if (iconosPantallas != null &&
            paginaActual < iconosPantallas.Length &&
            iconosPantallas[paginaActual] != null)
        {
            iconoImagen.sprite = iconosPantallas[paginaActual];
            iconoImagen.color = Color.white; // Evita que la imagen se vea negra si el color de tinte en el Inspector estaba en negro
            iconoImagen.enabled = true;
        }
        else
        {
            iconoImagen.enabled = false;
        }

        // Dots
        for (int i = 0; i < dots.Length; i++)
            dots[i].color = (i == paginaActual) ? colorDotActivo : colorDotInactivo;

        // Botón anterior
        btnAnterior.gameObject.SetActive(paginaActual > 0);

        // Texto del botón siguiente
        bool esUltima = paginaActual == titulos.Length - 1;
        textoBtnSiguiente.text = esUltima ? "[ JUGAR ]" : "SIGUIENTE >";
    }

    private void CerrarTutorial()
    {
        animando = true;
        panelCanvasGroup.DOFade(0f, duracionFade).SetUpdate(true).OnComplete(() =>
        {
            panelTutorial.SetActive(false);
            animando = false;

            // Reanudar el juego
            Time.timeScale = 1f;

            MovimientoJugador mov = FindAnyObjectByType<MovimientoJugador>();
            if (mov != null) mov.enabled = true;
        });
    }

    public void AbrirTutorial()
    {
        paginaActual = 0;
        panelTutorial.SetActive(true);
        panelCanvasGroup.alpha = 0f;
        ActualizarContenido();
        panelCanvasGroup.DOFade(1f, duracionFade).SetUpdate(true);

        // Pausar el juego
        Time.timeScale = 0f;

        MovimientoJugador mov = FindAnyObjectByType<MovimientoJugador>();
        if (mov != null) mov.enabled = false;
    }
}
