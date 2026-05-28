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

        // Auto-configure stats based on name in Start to show correct info in Editor or if instantiated dynamically
        string nombreNormalizado = string.IsNullOrEmpty(nombreObjeto) ? "" : nombreObjeto.Trim().ToLower();
        if (nombreNormalizado.Contains("regla"))
        {
            esAtaque = true;
            esCuracion = false;
            dano = 5;
            durabilidad = 6;
        }
        else if (nombreNormalizado.Contains("audifonos") || nombreNormalizado.Contains("audífonos"))
        {
            esAtaque = false;
            esCuracion = true;
            valorCuracion = 25f;
            durabilidad = 6;
        }
        else if (nombreNormalizado.Contains("chocolate"))
        {
            esAtaque = false;
            esCuracion = true;
            valorCuracion = 35f;
            durabilidad = 6;
        }
        else if (nombreNormalizado.Contains("termo"))
        {
            esAtaque = true;
            esCuracion = false;
            dano = 4;
            durabilidad = 6;
        }
        else if (nombreNormalizado.Contains("escudo"))
        {
            esAtaque = false;
            esCuracion = true;
            valorCuracion = 40f;
            durabilidad = 6;
        }
        else if (nombreNormalizado.Contains("rosa"))
        {
            esAtaque = false;
            esCuracion = true;
            valorCuracion = 20f;
            durabilidad = 6;
        }
        else
        {
            if (durabilidad <= 1)
                durabilidad = 6;
        }
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