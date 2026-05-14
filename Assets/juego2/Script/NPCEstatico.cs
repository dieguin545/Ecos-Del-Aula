using UnityEngine;

public class NPCEstatico : NPCBase
{
    public GameObject burbujaDialogo;
    public TMPro.TextMeshProUGUI textoBurbuja;

    void Start()
    {
        burbujaDialogo.SetActive(false);
    }

    protected override void Interactuar()
    {
        bool esNeutro = Random.Range(0f, 1f) < 0.3f;
        BullyingMessage mensaje = BullyingDatabase.Instance.GetMensaje(esNeutro);

        if (mensaje != null)
        {
            MostrarBurbuja(mensaje.mensaje);
            if (!esNeutro)
                SistemaEvidencia.Instance.SetUltimoMensaje(mensaje.mensaje, true);  
            if (!esNeutro && mensaje.ansiedadQueGenera > 0)
                AnxietySystem.Instance.IncreaseAnxiety(mensaje.ansiedadQueGenera);
        }
    }

    private void MostrarBurbuja(string mensaje)
    {
        textoBurbuja.text = mensaje;
        burbujaDialogo.SetActive(true);
        Invoke("OcultarBurbuja", 3f);
    }

    private void OcultarBurbuja()
    {
        burbujaDialogo.SetActive(false);
    }
}