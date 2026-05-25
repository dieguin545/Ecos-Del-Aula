using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

public class SistemaEvidencia : MonoBehaviour
{
    public static SistemaEvidencia Instance;

    [Header("Configuracion")]
    public int evidenciasNecesarias = 3;
    private int evidenciasRecolectadas = 0;

    // Lista enlazada de evidencias recolectadas
    private LinkedList<string> listaEvidencias = new LinkedList<string>();

    [Header("UI")]
    public TextMeshProUGUI textoEvidencia;
    public GameObject panelEvidencia;

    private string ultimoMensajeBullying = "";
    private bool puedeGuardar = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        VidaEscolarHUD.Ensure().ActualizarEvidencias(evidenciasRecolectadas, evidenciasNecesarias);

        if (panelEvidencia != null)
        {
            panelEvidencia.SetActive(false);
            AplicarEstiloPanel(panelEvidencia);
        }
    }

    void Update()
    {
        if (puedeGuardar && GestorEntradaGlobal.InteractuarPresionado(KeyCode.E))
        {
            GuardarEvidencia();
        }
    }

    public void SetUltimoMensaje(string mensaje, bool esBullying)
    {
        if (esBullying)
        {
            ultimoMensajeBullying = mensaje;
            puedeGuardar = true;
            MostrarIndicador();
        }
    }

    private void MostrarIndicador()
    {
        if (panelEvidencia != null)
        {
            panelEvidencia.SetActive(true);
            AplicarEstiloPanel(panelEvidencia);
            AnimarPanelEvidencia(true);
        }

        if (textoEvidencia != null)
        {
            PrepararRectPrompt(textoEvidencia.rectTransform);
            MostrarPromptInyectado(textoEvidencia.transform);
            string verbo = $"Guardar evidencia\nEvidencias: {evidenciasRecolectadas}/{evidenciasNecesarias}";
            EcosAulaPromptUI.InyectarEn(textoEvidencia.gameObject, AccionLogica.Interactuar, verbo);
            AjustarPromptInyectado(textoEvidencia.transform);
        }

        CancelInvoke("OcultarIndicador");
        Invoke("OcultarIndicador", 3f);
    }

    private void OcultarIndicador()
    {
        if (panelEvidencia != null)
        {
            AnimarPanelEvidencia(false);
        }
        puedeGuardar = false;
    }

    private void GuardarEvidencia()
    {
        if (ultimoMensajeBullying != "")
        {
            listaEvidencias.AddLast(ultimoMensajeBullying);
            evidenciasRecolectadas++;
            puedeGuardar = false;
            if (panelEvidencia != null)
            {
                panelEvidencia.SetActive(true);
            }
            if (textoEvidencia != null)
            {
                OcultarPromptInyectado(textoEvidencia.transform);
                textoEvidencia.enabled = true;
                textoEvidencia.text = $"Evidencia guardada\n{evidenciasRecolectadas}/{evidenciasNecesarias}";
            }
            VidaEscolarHUD.Ensure().ActualizarEvidencias(evidenciasRecolectadas, evidenciasNecesarias);
            UIAudioManager.PlayEvidenceSaved();
            VidaEscolarHUD.Ensure().MostrarToast($"Evidencia guardada {evidenciasRecolectadas}/{evidenciasNecesarias}");
            CancelInvoke("OcultarIndicador");
            Invoke("OcultarIndicador", 1.4f);
            Debug.Log("Evidencia guardada: " + ultimoMensajeBullying);
        }
    }

    public bool TieneSuficienteEvidencia()
    {
        return evidenciasRecolectadas >= evidenciasNecesarias;
    }

    public int GetEvidencias()
    {
        return evidenciasRecolectadas;
    }

    public int GetEvidenciasNecesarias()
    {
        return evidenciasNecesarias;
    }

    public LinkedList<string> GetListaEvidencias()
    {
        return listaEvidencias;
    }

    private void AplicarEstiloPanel(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        Image fondo = panel.GetComponent<Image>();
        if (fondo == null)
        {
            fondo = panel.AddComponent<Image>();
        }

        fondo.color = new Color(0.025f, 0.018f, 0.055f, 0.74f);
        fondo.raycastTarget = false;

        RectTransform rect = panel.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 92f);
            rect.sizeDelta = new Vector2(420f, 94f);
        }

        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = panel.AddComponent<CanvasGroup>();
        }
        group.blocksRaycasts = false;
        group.interactable = false;

        Outline outline = panel.GetComponent<Outline>();
        if (outline == null)
        {
            outline = panel.AddComponent<Outline>();
        }

        outline.effectColor = new Color(0.25f, 0.88f, 1f, 0.55f);
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow sombra = panel.GetComponent<Shadow>();
        if (sombra == null)
        {
            sombra = panel.AddComponent<Shadow>();
        }

        sombra.effectColor = new Color(0f, 0f, 0f, 0.42f);
        sombra.effectDistance = new Vector2(4f, -4f);
    }

    private void PrepararRectPrompt(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(372f, 66f);
    }

    private void AjustarPromptInyectado(Transform raiz)
    {
        if (raiz == null)
        {
            return;
        }

        Transform icono = raiz.Find("_IconoPrincipal");
        if (icono != null)
        {
            RectTransform rectIcono = icono.GetComponent<RectTransform>();
            if (rectIcono != null)
            {
                rectIcono.sizeDelta = new Vector2(38f, 38f);
                rectIcono.anchoredPosition = new Vector2(0f, 0f);
            }
        }

        Transform texto = raiz.Find("_TextoVerbo");
        if (texto != null)
        {
            RectTransform rectTexto = texto.GetComponent<RectTransform>();
            if (rectTexto != null)
            {
                rectTexto.anchoredPosition = new Vector2(50f, 0f);
                rectTexto.sizeDelta = new Vector2(288f, 0f);
            }

            TextMeshProUGUI tmp = texto.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.fontSize = 19f;
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.overflowMode = TextOverflowModes.Ellipsis;
            }
        }
    }

    private void AnimarPanelEvidencia(bool mostrar)
    {
        if (panelEvidencia == null)
        {
            return;
        }

        CanvasGroup group = panelEvidencia.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = panelEvidencia.AddComponent<CanvasGroup>();
        }

        panelEvidencia.transform.DOKill();
        group.DOKill();

        if (mostrar)
        {
            group.alpha = 0f;
            panelEvidencia.transform.localScale = Vector3.one * 0.96f;
            group.DOFade(1f, 0.14f).SetEase(Ease.OutQuad).SetLink(panelEvidencia);
            panelEvidencia.transform.DOScale(1f, 0.16f).SetEase(Ease.OutBack).SetLink(panelEvidencia);
        }
        else
        {
            group.DOFade(0f, 0.14f)
                .SetEase(Ease.InQuad)
                .SetLink(panelEvidencia)
                .OnComplete(() =>
                {
                    if (panelEvidencia != null)
                    {
                        panelEvidencia.SetActive(false);
                    }
                });
        }
    }

    private void OcultarPromptInyectado(Transform raiz)
    {
        if (raiz == null)
        {
            return;
        }

        Transform icono = raiz.Find("_IconoPrincipal");
        if (icono != null)
        {
            icono.gameObject.SetActive(false);
        }

        Transform texto = raiz.Find("_TextoVerbo");
        if (texto != null)
        {
            texto.gameObject.SetActive(false);
        }
    }

    private void MostrarPromptInyectado(Transform raiz)
    {
        if (raiz == null)
        {
            return;
        }

        Transform icono = raiz.Find("_IconoPrincipal");
        if (icono != null)
        {
            icono.gameObject.SetActive(true);
        }

        Transform texto = raiz.Find("_TextoVerbo");
        if (texto != null)
        {
            texto.gameObject.SetActive(true);
        }
    }
}
