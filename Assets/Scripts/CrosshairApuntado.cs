using UnityEngine;
using UnityEngine.UI;

public class CrosshairApuntado : MonoBehaviour
{
    [Header("Referencias")]
    public Camera camara;
    public Image imagenCrosshair;

    [Header("Sprites")]
    public Sprite crosshairBlanco;
    public Sprite crosshairRojo;

    [Header("Deteccion")]
    public float distanciaDeteccion = 500f;
    public string tagEnemigo = "Enemigo";
    [SerializeField] private LayerMask capasDeteccion = ~0;

    private Vector3 escalaOriginal = Vector3.one;
    private TipoDaltonismo tipoDaltonismo;
    private bool visiblePorEstado = true;

    private void Start()
    {
        if (camara == null)
        {
            camara = Camera.main;
        }

        if (imagenCrosshair == null)
        {
            imagenCrosshair = GetComponent<Image>();
        }

        escalaOriginal = transform.localScale;
        PrepararRectTransform();
        AplicarTipoDaltonismo(AccesibilidadSpaceShooter.TipoDaltonismoActual);
        PonerBlanco();
        EstablecerVisible(visiblePorEstado);
    }

    private void Update()
    {
        if (!visiblePorEstado || camara == null || imagenCrosshair == null)
        {
            return;
        }

        Ray rayo = camara.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit[] impactos = Physics.RaycastAll(
            rayo,
            distanciaDeteccion,
            capasDeteccion,
            QueryTriggerInteraction.Collide
        );

        System.Array.Sort(impactos, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < impactos.Length; i++)
        {
            Collider collider = impactos[i].collider;

            if (EsEnemigo(collider))
            {
                PonerRojo();
                return;
            }
        }

        PonerBlanco();
    }

    private void PonerBlanco()
    {
        if (crosshairBlanco != null)
        {
            imagenCrosshair.sprite = crosshairBlanco;
        }

        imagenCrosshair.color =
            tipoDaltonismo == TipoDaltonismo.Acromatopsia ? Color.white : Color.white;
        transform.localScale = escalaOriginal;
    }

    private void PonerRojo()
    {
        if (crosshairRojo != null)
        {
            imagenCrosshair.sprite = crosshairRojo;
            imagenCrosshair.color = Color.white;
        }
        else
        {
            imagenCrosshair.color = ObtenerColorApuntado();
        }

        transform.localScale =
            tipoDaltonismo == TipoDaltonismo.Ninguno
                ? escalaOriginal
                : escalaOriginal * 1.25f;
    }

    public void AplicarTipoDaltonismo(TipoDaltonismo tipo)
    {
        tipoDaltonismo = tipo;
    }

    public void EstablecerVisible(bool visible)
    {
        visiblePorEstado = visible;

        if (imagenCrosshair != null)
        {
            imagenCrosshair.enabled = visible;
        }
    }

    private void PrepararRectTransform()
    {
        RectTransform rect = transform as RectTransform;

        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(56f, 56f);

        if (imagenCrosshair != null)
        {
            imagenCrosshair.preserveAspect = true;
            imagenCrosshair.raycastTarget = false;
        }
    }

    private bool EsEnemigo(Collider collider)
    {
        if (collider == null)
        {
            return false;
        }

        Transform actual = collider.transform;

        while (actual != null)
        {
            if (actual.CompareTag(tagEnemigo))
            {
                return true;
            }

            actual = actual.parent;
        }

        return collider.transform.root != null
            && collider.transform.root.CompareTag(tagEnemigo);
    }

    private Color ObtenerColorApuntado()
    {
        switch (tipoDaltonismo)
        {
            case TipoDaltonismo.Protanopia:
            case TipoDaltonismo.Deuteranopia:
                return new Color(1f, 0.82f, 0.15f);
            case TipoDaltonismo.Tritanopia:
                return new Color(1f, 0.2f, 0.55f);
            case TipoDaltonismo.Acromatopsia:
                return Color.white;
            default:
                return Color.red;
        }
    }
}
