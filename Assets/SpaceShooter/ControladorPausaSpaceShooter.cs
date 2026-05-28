using System.Collections;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControladorPausaSpaceShooter : MonoBehaviour
{
    private GameManager gameManager;
    private MenuSpaceShooter menuSpaceShooter;
    private GameObject panelPausa;
    private CanvasGroup grupoPausa;
    private bool pausado;

    public bool EstaPausado => pausado;

    private void Update()
    {
        if (gameManager == null || !gameManager.PuedePausar)
        {
            return;
        }

        if (pausado && GestorEntradaGlobal.CancelarPresionado())
        {
            Reanudar();
            return;
        }

        if (GestorEntradaGlobal.PausaPresionada())
        {
            if (pausado)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }
    }

    public void Inicializar(
        GameManager gameManager,
        MenuSpaceShooter menuSpaceShooter,
        Canvas canvas
    )
    {
        this.gameManager = gameManager;
        this.menuSpaceShooter = menuSpaceShooter;

        if (panelPausa == null && canvas != null)
        {
            panelPausa = CrearPanelPausa(canvas.transform);
        }
    }

    public void Pausar()
    {
        if (panelPausa == null || gameManager == null || !gameManager.PuedePausar)
        {
            return;
        }

        pausado = true;
        Time.timeScale = 0f;
        panelPausa.SetActive(true);
        AnimarPanel(true);
        StartCoroutine(AsignarFocoPausa());
        gameManager.NotificarPausa(true);
        UIAudioManager.PlayOpen();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reanudar()
    {
        pausado = false;
        Time.timeScale = 1f;

        AnimarPanel(false);

        if (gameManager != null)
        {
            gameManager.NotificarPausa(false);
        }

        UIAudioManager.PlayClose();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Reiniciar()
    {
        Reanudar();
        gameManager.Reiniciar();
    }

    public void VolverAlMenu()
    {
        Reanudar();
        gameManager.VolverAlMenuInicial();
    }

    public void SalirAlJuegoPrincipal()
    {
        pausado = false;
        Time.timeScale = 1f;

        if (gameManager != null)
        {
            gameManager.NotificarPausa(false);
        }

        InteraccionPC.ResetearEstadoGlobalPC();
        MenuPausaAccesibilidad.ResetearEstadoGlobalPausa();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("Juego");
    }

    private GameObject CrearPanelPausa(Transform padre)
    {
        GameObject panel = MenuSpaceShooter.CrearPanelBase(
            padre,
            "PanelPausaSpaceShooter",
            new Color(0f, 0f, 0f, 0.85f)
        );

        CrearTarjetaCentral(panel.transform);

        MenuSpaceShooter.CrearTexto(
            panel.transform,
            "TituloPausa",
            "PAUSA",
            48f,
            new Vector2(0f, 220f),
            new Vector2(500f, 70f),
            TextAlignmentOptions.Center
        );

        MenuSpaceShooter.CrearBoton(
            panel.transform,
            "BotonReanudar",
            "Reanudar",
            new Vector2(0f, 100f),
            Reanudar
        );
        MenuSpaceShooter.CrearBoton(
            panel.transform,
            "BotonReiniciar",
            "Reiniciar",
            new Vector2(0f, 20f),
            Reiniciar
        );
        MenuSpaceShooter.CrearBoton(
            panel.transform,
            "BotonMenu",
            "Volver al menu",
            new Vector2(0f, -60f),
            VolverAlMenu
        );
        MenuSpaceShooter.CrearBoton(
            panel.transform,
            "BotonSalirJuego",
            "Salir a la PC",
            new Vector2(0f, -140f),
            SalirAlJuegoPrincipal
        );

        panel.SetActive(false);
        grupoPausa = panel.GetComponent<CanvasGroup>();
        if (grupoPausa == null)
        {
            grupoPausa = panel.AddComponent<CanvasGroup>();
        }

        // Prompts en el panel de pausa
        EcosAulaPromptUI.CrearBarraPrompts(panel.transform,
            (AccionLogica.Navegar, "Navegar"),
            (AccionLogica.Confirmar, "Confirmar"),
            (AccionLogica.Cancelar, "Reanudar"));

        return panel;
    }

    private void CrearTarjetaCentral(Transform padre)
    {
        GameObject tarjeta = new GameObject(
            "TarjetaPausa",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        tarjeta.transform.SetParent(padre, false);
        tarjeta.transform.SetAsFirstSibling();

        RectTransform rect = tarjeta.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(420f, 390f);

        Image imagen = tarjeta.GetComponent<Image>();
        imagen.color = new Color(0.02f, 0.05f, 0.1f, 0.94f);
        imagen.raycastTarget = false;

        Outline borde = tarjeta.AddComponent<Outline>();
        borde.effectColor = new Color(0.25f, 0.9f, 1f, 0.5f);
        borde.effectDistance = new Vector2(2f, -2f);

        Shadow sombra = tarjeta.AddComponent<Shadow>();
        sombra.effectColor = new Color(0f, 0f, 0f, 0.55f);
        sombra.effectDistance = new Vector2(0f, -8f);
    }

    private void AnimarPanel(bool mostrar)
    {
        if (panelPausa == null)
        {
            return;
        }

        if (grupoPausa == null)
        {
            grupoPausa = panelPausa.GetComponent<CanvasGroup>();
            if (grupoPausa == null)
            {
                grupoPausa = panelPausa.AddComponent<CanvasGroup>();
            }
        }

        grupoPausa.DOKill();
        panelPausa.transform.DOKill();

        if (mostrar)
        {
            grupoPausa.alpha = 0f;
            panelPausa.transform.localScale = Vector3.one * 0.94f;
            grupoPausa.DOFade(1f, 0.16f).SetUpdate(true).SetLink(panelPausa);
            panelPausa.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(panelPausa);
            return;
        }

        grupoPausa.DOFade(0f, 0.12f)
            .SetUpdate(true)
            .SetLink(panelPausa)
            .OnComplete(() =>
            {
                if (panelPausa != null)
                {
                    panelPausa.SetActive(false);
                }
            });
    }

    private IEnumerator AsignarFocoPausa()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        if (panelPausa == null || !panelPausa.activeSelf || UnityEngine.EventSystems.EventSystem.current == null) yield break;
        Transform botonReanudar = panelPausa.transform.Find("BotonReanudar");
        if (botonReanudar != null)
        {
            Selectable sel = botonReanudar.GetComponent<Selectable>();
            if (sel != null && sel.gameObject.activeInHierarchy && sel.interactable)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(sel.gameObject);
            }
        }
    }
}
