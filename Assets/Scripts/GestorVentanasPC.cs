using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GestorVentanasPC : MonoBehaviour
{
    [Header("Ventanas de aplicaciones")]
    [SerializeField] private List<GameObject> ventanas = new List<GameObject>();

    [Header("Fondo modal")]
    [SerializeField] private Image overlayEscritorio;

    public bool HayVentanaAbierta => ObtenerVentanaActiva() != null;

    public void ConfigurarVentanas(IEnumerable<GameObject> ventanasPc)
    {
        ventanas.Clear();

        foreach (GameObject ventana in ventanasPc)
        {
            RegistrarVentana(ventana);
        }

        AsegurarOverlay();
        CerrarTodas();
    }

    public void RegistrarVentana(GameObject ventana)
    {
        if (ventana == null || ventanas.Contains(ventana))
        {
            return;
        }

        ventanas.Add(ventana);
    }

    public void AbrirVentana(GameObject ventana)
    {
        if (ventana == null)
        {
            return;
        }

        RegistrarVentana(ventana);
        CerrarTodasExcepto(ventana);
        AsegurarOverlay();

        if (overlayEscritorio != null)
        {
            overlayEscritorio.gameObject.SetActive(true);
            overlayEscritorio.transform.SetAsLastSibling();
        }

        ventana.SetActive(true);
        ventana.transform.SetAsLastSibling();
    }

    public void AlternarVentana(GameObject ventana)
    {
        if (ventana == null)
        {
            return;
        }

        if (ventana.activeSelf)
        {
            CerrarVentana(ventana);
        }
        else
        {
            AbrirVentana(ventana);
        }
    }

    public void CerrarVentana(GameObject ventana)
    {
        if (ventana != null)
        {
            ventana.SetActive(false);
        }

        ActualizarOverlay();
    }

    public bool CerrarVentanaActiva()
    {
        GameObject ventanaActiva = ObtenerVentanaActiva();

        if (ventanaActiva == null)
        {
            return false;
        }

        ventanaActiva.SetActive(false);
        ActualizarOverlay();
        return true;
    }

    public void CerrarTodas()
    {
        for (int i = 0; i < ventanas.Count; i++)
        {
            if (ventanas[i] != null)
            {
                ventanas[i].SetActive(false);
            }
        }

        ActualizarOverlay();
    }

    private void CerrarTodasExcepto(GameObject ventanaActiva)
    {
        for (int i = 0; i < ventanas.Count; i++)
        {
            GameObject ventana = ventanas[i];

            if (ventana != null && ventana != ventanaActiva)
            {
                ventana.SetActive(false);
            }
        }
    }

    private void ActualizarOverlay()
    {
        if (overlayEscritorio == null)
        {
            return;
        }

        bool hayVentanaAbierta = false;

        for (int i = 0; i < ventanas.Count; i++)
        {
            if (ventanas[i] != null && ventanas[i].activeSelf)
            {
                hayVentanaAbierta = true;
                break;
            }
        }

        overlayEscritorio.gameObject.SetActive(hayVentanaAbierta);
    }

    private GameObject ObtenerVentanaActiva()
    {
        for (int i = ventanas.Count - 1; i >= 0; i--)
        {
            GameObject ventana = ventanas[i];

            if (ventana != null && ventana.activeSelf)
            {
                return ventana;
            }
        }

        return null;
    }

    private void AsegurarOverlay()
    {
        if (overlayEscritorio != null)
        {
            overlayEscritorio.color = new Color(0.03f, 0.01f, 0.08f, 0.98f);
            return;
        }

        Transform overlayExistente = transform.Find("OverlayVentanasPC");

        if (overlayExistente != null)
        {
            overlayEscritorio = overlayExistente.GetComponent<Image>();
            if (overlayEscritorio != null)
            {
                overlayEscritorio.color = new Color(0.03f, 0.01f, 0.08f, 0.98f);
            }
            return;
        }

        GameObject overlay = new GameObject(
            "OverlayVentanasPC",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        overlay.transform.SetParent(transform, false);

        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        overlayEscritorio = overlay.GetComponent<Image>();
        overlayEscritorio.color = new Color(0.03f, 0.01f, 0.08f, 0.98f);
        overlayEscritorio.raycastTarget = true;
        overlayEscritorio.gameObject.SetActive(false);
    }
}
