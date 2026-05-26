using UnityEngine;

public class ObjetoRecogible : MonoBehaviour
{
    [Header("Configuracion")]
    public string nombreObjeto;
    public Sprite iconoObjeto;
    public bool recogerAlContacto = true;
    public int dano = 0;
    public int durabilidad = 1;
    public bool esAtaque = false;
    public bool esCuracion = false;
    public float valorCuracion = 0f;

    [Header("UI")]
    public GameObject indicador;

    private bool jugadorCerca = false;

    void Start()
    {
        if (indicador != null)
            indicador.SetActive(true);
    }

    void Update()
    {
        if (!recogerAlContacto && jugadorCerca 
            && Input.GetKeyDown(KeyCode.E))
        {
            Recoger();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (recogerAlContacto)
                Recoger();
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
            jugadorCerca = false;
    }

    private void Recoger()
    {
        Inventario.Instance.AgregarObjeto(new Objeto(
            nombreObjeto, iconoObjeto,
            dano, durabilidad,
            esAtaque, esCuracion, valorCuracion));
        Debug.Log("Recogiste: " + nombreObjeto);
        Destroy(gameObject);
    }
}