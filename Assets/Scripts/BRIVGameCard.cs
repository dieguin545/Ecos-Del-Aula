using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Tarjeta de juego dentro del hub BRIV.
// Se asocia a un BRIVGame y se encarga de mostrar el nombre, la descripcion,
// la etiqueta (PRINCIPAL / MINIJUEGO) y el color de acento.
// Al pulsar el boton de la tarjeta, delega la apertura del juego al BRIVHub.
[RequireComponent(typeof(RectTransform))]
public class BRIVGameCard : MonoBehaviour
{
    [Header("Datos del juego")]
    public string idJuego;
    public BRIVGame juego;

    [Header("UI")]
    public TMP_Text textoNombre;
    public TMP_Text textoDescripcion;
    public TMP_Text textoEtiqueta;
    public Image fondoTarjeta;
    public Image bordeAcento;
    public Image miniatura;
    public Button botonJugar;

    [Header("Referencias")]
    public BRIVHub hub;

    void Start()
    {
        if (hub == null)
        {
            hub = FindObjectOfType<BRIVHub>();
        }

        ResolverJuego();
        AplicarADisplay();

        if (botonJugar != null)
        {
            botonJugar.onClick.AddListener(OnPulsarJugar);
        }
    }

    void ResolverJuego()
    {
        if (juego != null) return;

        if (hub != null && !string.IsNullOrEmpty(idJuego))
        {
            foreach (var j in hub.juegos)
            {
                if (j.id == idJuego)
                {
                    juego = j;
                    return;
                }
            }
        }
    }

    void AplicarADisplay()
    {
        if (juego == null) return;

        if (textoNombre != null) textoNombre.text = juego.displayName;
        if (textoDescripcion != null) textoDescripcion.text = juego.descripcion;
        if (textoEtiqueta != null) textoEtiqueta.text = juego.TextoEtiqueta();

        if (bordeAcento != null) bordeAcento.color = juego.accentColor;
        if (miniatura != null && juego.miniatura != null) miniatura.sprite = juego.miniatura;
    }

    public void OnPulsarJugar()
    {
        if (hub != null && juego != null)
        {
            hub.AbrirJuegoPorId(juego.id);
        }
    }
}
