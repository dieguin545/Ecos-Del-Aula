using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SistemaCombate : MonoBehaviour
{
    public static SistemaCombate Instance;

    [Header("Jefe Final")]
    public GameObject prefabJefe;
    public Transform spawnJefe;
    public Transform[] spawnsBlockers;
    public GameObject prefabBlocker;

    [Header("Stats Jefe")]
    public float vidaJefe = 100f;
    public float vidaJefeActual;
    public float intervaloAtaque = 4f;
    public float intervaloAtaqueEspecial = 8f;

    [Header("UI Combate")]
    public GameObject panelCombate;
    public Slider barraVidaJefe;
    public TextMeshProUGUI textoVidaJefe;
    public TextMeshProUGUI textoObjetoEquipado;
    public TextMeshProUGUI textoDurabilidad;
    public TextMeshProUGUI textoMensajeJefe;
    public GameObject panelMensajeJefe;

    private bool combateActivo = false;
    private List<Objeto> objetosAtaque = new List<Objeto>();
    private List<Objeto> objetosCuracion = new List<Objeto>();
    private int indiceObjetoActual = 0;
    private float timerAtaque = 0f;
    private float timerAtaqueEspecial = 0f;

    private string[] mensajesJefe = {
        "No eres nadie aqui.",
        "Nadie va a ayudarte.",
        "Debiste quedarte en casa.",
        "Aqui mando yo.",
        "Eres lo peor de este colegio."
    };

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (panelCombate != null)
            panelCombate.SetActive(false);
    }

    public void IniciarCombate()
    {
        if (combateActivo) return;
        combateActivo = true;

        // Spawna bloqueadores en las 3 puertas
        foreach (Transform spawn in spawnsBlockers)
            Instantiate(prefabBlocker, spawn.position, Quaternion.identity);

        // Spawna el jefe en el centro
        if (prefabJefe != null)
            Instantiate(prefabJefe, spawnJefe.position, Quaternion.identity);

        // Desactiva movimiento
        FindObjectOfType<MovimientoJugador>().enabled = false;
        FindObjectOfType<Animacion>().enabled = false;

        // Carga objetos del inventario
        CargarObjetosInventario();

        // Activa UI
        vidaJefeActual = vidaJefe;
        panelCombate.SetActive(true);
        ActualizarUI();

        // Mensaje inicial del jefe
        StartCoroutine(MensajeInicial());
    }

    private IEnumerator MensajeInicial()
    {
        yield return new WaitForSeconds(0.5f);
        MostrarMensajeJefe("De aqui no te vas si no es con un moreton.");
        yield return new WaitForSeconds(3f);
        StartCoroutine(AtaqueJefe());
    }

    private void CargarObjetosInventario()
    {
        objetosAtaque.Clear();
        objetosCuracion.Clear();

        foreach (Objeto obj in Inventario.Instance.GetObjetos())
        {
            if (obj.esAtaque) objetosAtaque.Add(obj);
            if (obj.esCuracion) objetosCuracion.Add(obj);
        }

        indiceObjetoActual = 0;
    }

    void Update()
    {
        if (!combateActivo) return;

        // Atacar con Espacio
        if (Input.GetKeyDown(KeyCode.Space))
            Atacar();

        // Ciclar objetos con Q y E
        if (Input.GetKeyDown(KeyCode.Q))
            CiclarObjeto(-1);
        if (Input.GetKeyDown(KeyCode.E))
            CiclarObjeto(1);

        // Curar con F
        if (Input.GetKeyDown(KeyCode.F))
            Curar();
    }

    private void Atacar()
    {
        if (objetosAtaque.Count == 0)
        {
            MostrarMensajeJefe("No tienes objetos para atacar.");
            return;
        }

        Objeto obj = objetosAtaque[indiceObjetoActual];
        vidaJefeActual -= obj.dano;
        obj.durabilidad--;

        MostrarMensajeJefe("Golpeaste con " + obj.nombre + "!");

        if (obj.durabilidad <= 0)
        {
            MostrarMensajeJefe(obj.nombre + " se rompio.");
            Inventario.Instance.EliminarObjeto(obj.nombre);
            objetosAtaque.RemoveAt(indiceObjetoActual);
            if (indiceObjetoActual >= objetosAtaque.Count)
                indiceObjetoActual = 0;
        }

        if (vidaJefeActual <= 0)
        {
            TerminarCombate(true);
            return;
        }

        ActualizarUI();
    }

    private void Curar()
    {
        if (objetosCuracion.Count == 0)
        {
            MostrarMensajeJefe("No tienes objetos de curacion.");
            return;
        }

        Objeto obj = objetosCuracion[0];
        AnxietySystem.Instance.DecreaseAnxiety(obj.valorCuracion);
        obj.durabilidad--;

        MostrarMensajeJefe("Usaste " + obj.nombre + ".");

        if (obj.durabilidad <= 0)
        {
            Inventario.Instance.EliminarObjeto(obj.nombre);
            objetosCuracion.RemoveAt(0);
        }
    }

    private void CiclarObjeto(int direccion)
    {
        if (objetosAtaque.Count == 0) return;
        indiceObjetoActual = (indiceObjetoActual + direccion + 
            objetosAtaque.Count) % objetosAtaque.Count;
        ActualizarUI();
    }

    private IEnumerator AtaqueJefe()
    {
        while (combateActivo)
        {
            yield return new WaitForSeconds(intervaloAtaque);
            if (!combateActivo) break;

            // Ataque normal
            string mensaje = mensajesJefe[
                Random.Range(0, mensajesJefe.Length)];
            MostrarMensajeJefe(mensaje);
            AnxietySystem.Instance.IncreaseAnxiety(15f);

            yield return new WaitForSeconds(
                intervaloAtaqueEspecial - intervaloAtaque);
            if (!combateActivo) break;

            // Ataque especial
            MostrarMensajeJefe("Te empuja con fuerza!");
            AnxietySystem.Instance.IncreaseAnxiety(25f);

            // Verifica si el jugador perdio
            if (AnxietySystem.Instance.GetCurrentAnxiety() >= 100f)
            {
                TerminarCombate(false);
                break;
            }
        }
    }

    private void MostrarMensajeJefe(string mensaje)
    {
        if (panelMensajeJefe == null) return;
        panelMensajeJefe.SetActive(true);
        textoMensajeJefe.text = mensaje;
        StartCoroutine(OcultarMensaje());
    }

    private IEnumerator OcultarMensaje()
    {
        yield return new WaitForSeconds(2.5f);
        if (panelMensajeJefe != null)
            panelMensajeJefe.SetActive(false);
    }

    private void ActualizarUI()
    {
        barraVidaJefe.value = vidaJefeActual / vidaJefe;
        textoVidaJefe.text = $"Vida: {Mathf.RoundToInt(vidaJefeActual)}/100";

        if (objetosAtaque.Count > 0)
        {
            Objeto obj = objetosAtaque[indiceObjetoActual];
            textoObjetoEquipado.text = obj.nombre;
            textoDurabilidad.text = $"Durabilidad: {obj.durabilidad}/{obj.durabilidadMaxima}";
        }
        else
        {
            textoObjetoEquipado.text = "Sin objetos";
            textoDurabilidad.text = "";
        }
    }

    private void TerminarCombate(bool gano)
    {
        combateActivo = false;
        StopAllCoroutines();
        panelCombate.SetActive(false);

        if (gano)
            ConfrontacionManager.Instance.MostrarResultado(false);
        else
            ConfrontacionManager.Instance.MostrarResultado(true);
    }
}
