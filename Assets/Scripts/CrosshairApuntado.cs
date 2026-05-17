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

    private Vector3 escalaOriginal = Vector3.one;
    private bool modoDaltonico;
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
        AplicarModoDaltonico(AccesibilidadSpaceShooter.ModoDaltonicoActivo);
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

        if (Physics.Raycast(rayo, out RaycastHit hit, distanciaDeteccion, ~0, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.CompareTag(tagEnemigo))
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

        imagenCrosshair.color = Color.white;
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
            imagenCrosshair.color = Color.red;
        }

        transform.localScale = modoDaltonico ? escalaOriginal * 1.25f : escalaOriginal;
    }

    public void AplicarModoDaltonico(bool activo)
    {
        modoDaltonico = activo;
    }

    public void EstablecerVisible(bool visible)
    {
        visiblePorEstado = visible;

        if (imagenCrosshair != null)
        {
            imagenCrosshair.enabled = visible;
        }
    }
}
