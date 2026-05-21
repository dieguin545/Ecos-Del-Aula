using UnityEngine;

public class ObjetoRecogible : MonoBehaviour
{
    [Header("Configuracion")]
    public string nombreObjeto;
    public Sprite iconoObjeto;
    public bool recogerAlContacto = true; // true = al tocar, false = presionar E

    [Header("UI")]
    public GameObject indicador; // el signo de exclamacion

    private bool jugadorCerca = false;

    void Start()
    {
        if (indicador != null)
            indicador.SetActive(true);
    }

    void Update()
    {
        if (!recogerAlContacto && jugadorCerca && GestorEntradaGlobal.InteractuarPresionado(KeyCode.E))
        {
            Recoger();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (recogerAlContacto)
            {
                Recoger();
            }
            else
            {
                jugadorCerca = true;
                if (indicador != null)
                    indicador.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
        }
    }

    private void Recoger()
    {
        Inventario.Instance.AgregarObjeto(new Objeto(nombreObjeto, iconoObjeto));
        Debug.Log("Recogiste: " + nombreObjeto);
        Destroy(gameObject);
    }
}