using UnityEngine;
using UnityEngine.UI;
using TMPro;
[System.Serializable]
public class Mision
{
    public string id;
    public string dialogoInicio;
    public string dialogoCompletado;
    public string objetoRequerido;
    public string objetoRecompensa;
    public bool completada = false;
}

public class MisionSecundaria : MonoBehaviour
{
    [Header("Configuracion")]
    public Mision mision;
    public bool misionIniciada = false;
    public bool esSoloDador = false;

    [Header("Sprites")]
    public Sprite iconoRecompensa;

    [Header("UI")]
    public GameObject burbujaDialogo;
    public TextMeshProUGUI textoBurbuja;

    private bool jugadorCerca = false;

    void Start()
    {
        if (burbujaDialogo != null)
            burbujaDialogo.SetActive(false);

        // Failsafe auto-detección del rol dador
        if (!string.IsNullOrEmpty(mision.objetoRecompensa) && string.IsNullOrEmpty(mision.objetoRequerido))
        {
            esSoloDador = true;
        }
    }

    void Update()
    {
        if (jugadorCerca && GestorEntradaGlobal.InteractuarPresionado(KeyCode.E))
        {
            Interactuar();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            MostrarIndicador();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            if (burbujaDialogo != null)
                burbujaDialogo.SetActive(false);
        }
    }

    private void MostrarIndicador()
    {
        if (burbujaDialogo == null) return;
        burbujaDialogo.SetActive(true);

        bool activaEnManager = MisionManager.Instance != null && MisionManager.Instance.EstaMisionActiva(mision.id);
        bool completadaEnManager = MisionManager.Instance != null && MisionManager.Instance.EstaMisionCompletada(mision.id);

        if (completadaEnManager || mision.completada)
        {
            DesactivarPromptYMostrarTexto("¡Gracias por tu ayuda!");
        }
        else if (esSoloDador)
        {
            if (!misionIniciada)
                EcosAulaPromptUI.InyectarEn(textoBurbuja.gameObject, AccionLogica.Interactuar, "Hablar");
            else
                DesactivarPromptYMostrarTexto("Lleva el objeto a quien corresponde.");
        }
        else // Es Receptor
        {
            if (activaEnManager)
                EcosAulaPromptUI.InyectarEn(textoBurbuja.gameObject, AccionLogica.Interactuar, "Entregar");
            else
                DesactivarPromptYMostrarTexto("Hola...");
        }
    }

    private void DesactivarPromptYMostrarTexto(string texto)
    {
        EcosAulaPromptUI prompt = textoBurbuja.GetComponent<EcosAulaPromptUI>();
        if (prompt != null)
        {
            prompt.enabled = false;
            Transform img = textoBurbuja.transform.Find("_IconoPrincipal");
            if (img != null) img.gameObject.SetActive(false);
            Transform txt = textoBurbuja.transform.Find("_TextoVerbo");
            if (txt != null) txt.gameObject.SetActive(false);
        }
        textoBurbuja.enabled = true;
        textoBurbuja.text = texto;
    }

    private void Interactuar()
    {
        bool completadaEnManager = MisionManager.Instance != null && MisionManager.Instance.EstaMisionCompletada(mision.id);
        if (mision.completada || completadaEnManager)
        {
            MostrarDialogo("¡Gracias por tu ayuda!");
            return;
        }

        if (esSoloDador)
        {
            if (!misionIniciada)
            {
                IniciarMision();
            }
            else
            {
                MostrarDialogo("Por favor, lleva el objeto a quien corresponde.");
            }
        }
        else // Es Receptor
        {
            bool activaEnManager = MisionManager.Instance != null && MisionManager.Instance.EstaMisionActiva(mision.id);
            if (activaEnManager)
            {
                VerificarMision();
            }
            else
            {
                MostrarDialogo("Hola. Qué día tan tranquilo...");
            }
        }
    }

    private void IniciarMision()
{
    misionIniciada = true;
    MostrarDialogo(mision.dialogoInicio);
    MisionManager.Instance.RegistrarMision(mision);

    // Solo da el objeto al inventario, NO completa la mision
    // La mision se completa cuando el jugador va donde el receptor
    if (mision.objetoRecompensa != "" && mision.objetoRecompensa != null)
        Inventario.Instance.AgregarObjeto(new Objeto(mision.objetoRecompensa, iconoRecompensa));
}

    private void VerificarMision()
    {
        // Si no requiere objeto simplemente completa
        if (mision.objetoRequerido == "" || mision.objetoRequerido == null)
        {
            mision.completada = true;
            if (mision.objetoRecompensa != "")
                Inventario.Instance.AgregarObjeto(new Objeto(mision.objetoRecompensa));
            MostrarDialogo(mision.dialogoCompletado);
            MisionManager.Instance.CompletarMision(mision.id);
            return;
        }

        if (Inventario.Instance.TieneObjeto(mision.objetoRequerido))
        {
            mision.completada = true;
            Inventario.Instance.EliminarObjeto(mision.objetoRequerido);
            if (mision.objetoRecompensa != "")
                Inventario.Instance.AgregarObjeto(new Objeto(mision.objetoRecompensa));
            MostrarDialogo(mision.dialogoCompletado);
            MisionManager.Instance.CompletarMision(mision.id);
        }
        else
        {
            MostrarDialogo("Aún no tienes lo que necesito...");
        }
    }

    private void MostrarDialogo(string mensaje)
    {
        if (burbujaDialogo == null) return;
        DesactivarPromptYMostrarTexto(mensaje);
        burbujaDialogo.SetActive(true);
        Invoke("OcultarDialogo", 3f);
    }

    private void OcultarDialogo()
    {
        if (burbujaDialogo != null)
            burbujaDialogo.SetActive(false);
    }
}
