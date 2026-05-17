using UnityEngine;
using TMPro;

public class ControlAccesoHabitacion : MonoBehaviour
{
    [Header("Configuracion")]
    public ZonaTipo zonaQueControla;
    public BloqueTiempo[] bloquesPermitidos;

    [Header("Bloqueo visual")]
    public GameObject panelBloqueado;
    public TextMeshProUGUI textoBloqueado;
    public GameObject puertaBloqueada;

    private bool bloqueado = false;
    private Collider2D colliderBloqueo;

    void Start()
    {
        colliderBloqueo = GetComponent<Collider2D>();
        VerificarAcceso();
    }

    void Update()
    {
        VerificarAcceso();
    }

    private void VerificarAcceso()
    {
        BloqueTiempo bloqueActual = SistemaTiempo.Instance.GetBloqueActual();
        bloqueado = true;

        foreach (BloqueTiempo bloque in bloquesPermitidos)
        {
            if (bloque == bloqueActual)
            {
                bloqueado = false;
                break;
            }
        }

        if (colliderBloqueo != null)
            colliderBloqueo.isTrigger = !bloqueado;

        if (puertaBloqueada != null)
            puertaBloqueada.SetActive(bloqueado);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && bloqueado)
        {
            MostrarMensajeBloqueado();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (panelBloqueado != null)
                panelBloqueado.SetActive(false);
        }
    }

    private void MostrarMensajeBloqueado()
    {
        if (panelBloqueado == null) return;

        panelBloqueado.SetActive(true);

        switch (zonaQueControla)
        {
            case ZonaTipo.Salon:
                textoBloqueado.text = "Debes ir al salón ahora.";
                break;
            case ZonaTipo.Cafeteria:
                textoBloqueado.text = "La cafetería abre en el almuerzo.";
                break;
            case ZonaTipo.Gimnasio:
                textoBloqueado.text = "El gimnasio abre en el descanso.";
                break;
            case ZonaTipo.Biblioteca:
                textoBloqueado.text = "La biblioteca abre en el descanso.";
                break;
            case ZonaTipo.Patio:
                textoBloqueado.text = "El patio abre en el descanso.";
                break;
            default:
                textoBloqueado.text = "Esta área no está disponible ahora.";
                break;
        }

        Invoke("OcultarMensaje", 2f);
    }

    private void OcultarMensaje()
    {
        if (panelBloqueado != null)
            panelBloqueado.SetActive(false);
    }
}