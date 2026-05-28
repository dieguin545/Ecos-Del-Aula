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
    private List<Objeto> objetosEquipados = new List<Objeto>();
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

        // Dynamic binding fix!
        // Find the GameObject named "TextoMensajeJefe" inside panelCombate or globally,
        // and assign its TextMeshProUGUI to textoMensajeJefe dynamically to avoid unassigned/wrong bindings in the scene.
        if (textoMensajeJefe == null || textoMensajeJefe.gameObject.name != "TextoMensajeJefe")
        {
            GameObject searchRoot = panelCombate != null ? panelCombate : (panelMensajeJefe != null ? panelMensajeJefe : null);
            if (searchRoot != null)
            {
                TextMeshProUGUI[] tmps = searchRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI tmp in tmps)
                {
                    if (tmp.gameObject.name == "TextoMensajeJefe")
                    {
                        textoMensajeJefe = tmp;
                        Debug.Log("Dynamic Combat Fix: Bound textoMensajeJefe to " + tmp.gameObject.name);
                        break;
                    }
                }
            }
            if (textoMensajeJefe == null || textoMensajeJefe.gameObject.name != "TextoMensajeJefe")
            {
                TextMeshProUGUI[] tmps = FindObjectsOfType<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI tmp in tmps)
                {
                    if (tmp.gameObject.name == "TextoMensajeJefe")
                    {
                        textoMensajeJefe = tmp;
                        Debug.Log("Dynamic Combat Fix (Global): Bound textoMensajeJefe to " + tmp.gameObject.name);
                        break;
                    }
                }
            }
        }

        // Initialize message text to empty to prevent showing default inspector text (like "New Text")
        if (textoMensajeJefe != null)
        {
            textoMensajeJefe.text = "";
        }

        // Deactivate the old static prompt text to avoid it overlapping with the boss messages
        if (panelCombate != null)
        {
            Transform oldText = panelCombate.transform.Find("Text (TMP)");
            if (oldText != null)
            {
                oldText.gameObject.SetActive(false);
                Debug.Log("Dynamic Combat Fix: Deactivated old static instructions Text (TMP) to avoid overlapping.");
            }
        }

        EstilizarPanel(panelCombate);
        AjustarDiseñoUI();

        if (panelCombate != null)
            panelCombate.SetActive(false);
    }

    private void EstilizarPanel(GameObject panel)
    {
        if (panel == null) return;
        Image fondo = panel.GetComponent<Image>();
        if (fondo == null) fondo = panel.AddComponent<Image>();
        
        if (panel == panelCombate)
        {
            // Transparent background for the main combat screen to keep world sprites fully visible!
            fondo.color = new Color(0f, 0f, 0f, 0f);
            Outline outline = panel.GetComponent<Outline>();
            if (outline != null) Destroy(outline);
        }
        else
        {
            fondo.color = new Color(0.025f, 0.018f, 0.055f, 0.88f); // Midnight-blue premium semi-transparente
            Outline outline = panel.GetComponent<Outline>();
            if (outline == null) outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.25f, 0.88f, 1f, 0.55f);
            outline.effectDistance = new Vector2(2f, -2f);
        }
    }

    private void AjustarRectTransform(RectTransform rect, Vector2 posicion, Vector2 tamaño)
    {
        if (rect == null) return;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamaño;
    }

    private void AjustarDiseñoUI()
    {
        if (panelCombate != null)
        {
            AjustarRectTransform(panelCombate.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(800f, 480f));
        }

        if (barraVidaJefe != null)
        {
            AjustarRectTransform(barraVidaJefe.GetComponent<RectTransform>(), new Vector2(0f, 160f), new Vector2(500f, 30f));
        }

        if (textoVidaJefe != null)
        {
            AjustarRectTransform(textoVidaJefe.GetComponent<RectTransform>(), new Vector2(0f, 110f), new Vector2(400f, 40f));
            textoVidaJefe.fontSize = 22f;
            textoVidaJefe.alignment = TextAlignmentOptions.Center;
            textoVidaJefe.fontStyle = FontStyles.Bold;
        }

        if (textoObjetoEquipado != null)
        {
            // Positioned near the bottom-left above the prompt bar (y = -160f) to clear the center for sprites
            AjustarRectTransform(textoObjetoEquipado.GetComponent<RectTransform>(), new Vector2(-180f, -160f), new Vector2(350f, 45f));
            textoObjetoEquipado.fontSize = 20f;
            textoObjetoEquipado.alignment = TextAlignmentOptions.Center;
            textoObjetoEquipado.color = new Color(0.25f, 0.88f, 1f, 1f); // Cyan brillante
        }

        if (textoDurabilidad != null)
        {
            // Positioned near the bottom-right above the prompt bar (y = -160f) to clear the center for sprites
            AjustarRectTransform(textoDurabilidad.GetComponent<RectTransform>(), new Vector2(180f, -160f), new Vector2(350f, 45f));
            textoDurabilidad.fontSize = 20f;
            textoDurabilidad.alignment = TextAlignmentOptions.Center;
            textoDurabilidad.color = new Color(1f, 0.78f, 0.18f, 1f); // Oro premium
        }

        if (panelMensajeJefe != null)
        {
            // Positioned slightly lower (y = -100f) to clear more screen space for the world sprites
            AjustarRectTransform(panelMensajeJefe.GetComponent<RectTransform>(), new Vector2(0f, -100f), new Vector2(680f, 80f));
            
            Image msgFondo = panelMensajeJefe.GetComponent<Image>();
            if (msgFondo == null) msgFondo = panelMensajeJefe.AddComponent<Image>();
            msgFondo.color = new Color(0.12f, 0.05f, 0.18f, 0.85f); // Terciopelo púrpura oscuro

            Outline msgBorde = panelMensajeJefe.GetComponent<Outline>();
            if (msgBorde == null) msgBorde = panelMensajeJefe.AddComponent<Outline>();
            msgBorde.effectColor = new Color(1f, 0.78f, 0.18f, 0.35f); // Borde dorado tenue
            msgBorde.effectDistance = new Vector2(1f, -1f);
        }

        if (textoMensajeJefe != null)
        {
            AjustarRectTransform(textoMensajeJefe.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(650f, 60f));
            textoMensajeJefe.fontSize = 18f;
            textoMensajeJefe.alignment = TextAlignmentOptions.Center;
            textoMensajeJefe.fontStyle = FontStyles.Italic;
            textoMensajeJefe.color = new Color(1f, 0.95f, 0.98f, 1f);
        }
    }

    public void IniciarCombate()
    {
        if (combateActivo) return;
        combateActivo = true;

        // Failsafe: Asegurar Time.timeScale = 1f al iniciar el combate final
        Time.timeScale = 1f;

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
        
        if (panelCombate != null)
        {
            panelCombate.transform.SetAsLastSibling();
            panelCombate.SetActive(true);
            AjustarDiseñoUI();
            
            // Inyectar prompts interactivos premium
            EcosAulaPromptUI.CrearBarraPrompts(panelCombate.transform,
                (AccionLogica.Confirmar, "Usar Objeto"),
                (AccionLogica.AnteriorPestana, "Anterior"),
                (AccionLogica.SiguientePestana, "Siguiente"));
        }

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
        objetosEquipados.Clear();

        // Load all items in the inventory that can be used (both attack and healing/defense)
        foreach (Objeto obj in Inventario.Instance.GetObjetos())
        {
            if (obj.esAtaque || obj.esCuracion)
            {
                objetosEquipados.Add(obj);
            }
        }

        // Failsafe fallback: Fists (Puños) with low damage to make the player use multiple inventory weapons
        if (objetosEquipados.Count == 0)
        {
            objetosEquipados.Add(new Objeto("Puños", null, dano: 2, durabilidad: 999, esAtaque: true));
        }

        indiceObjetoActual = 0;
    }

    void Update()
    {
        if (!combateActivo) return;

        // Use selected item with Space, Enter, F or Xbox A/X/Y
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.F) ||
            Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton2) || Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            UsarObjetoEquipado();
        }

        // Cycle objects with Q/E or LB/RB of Xbox
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.JoystickButton4))
            CiclarObjeto(-1);
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton5))
            CiclarObjeto(1);
    }

    private void UsarObjetoEquipado()
    {
        if (objetosEquipados.Count == 0)
        {
            MostrarMensajeJefe("No tienes objetos en tu inventario.");
            return;
        }

        Objeto obj = objetosEquipados[indiceObjetoActual];
        
        if (obj.esAtaque)
        {
            // Attack the boss with reduced damage
            vidaJefeActual -= obj.dano;
            obj.durabilidad--;
            
            MostrarMensajeJefe("¡Golpeaste con " + obj.nombre + "! (Daño: " + obj.dano + ")");

            if (obj.durabilidad <= 0)
            {
                MostrarMensajeJefe(obj.nombre + " se rompió.");
                Inventario.Instance.EliminarObjeto(obj.nombre);
                objetosEquipados.RemoveAt(indiceObjetoActual);
                if (indiceObjetoActual >= objetosEquipados.Count)
                    indiceObjetoActual = 0;
            }

            if (vidaJefeActual <= 0)
            {
                TerminarCombate(true);
                return;
            }
        }
        else if (obj.esCuracion)
        {
            // Heal/defend the player (decrease anxiety)
            AnxietySystem.Instance.DecreaseAnxiety(obj.valorCuracion);
            obj.durabilidad--;
            
            MostrarMensajeJefe("¡Usaste " + obj.nombre + " para calmarte! (Ansiedad: -" + obj.valorCuracion + ")");

            if (obj.durabilidad <= 0)
            {
                MostrarMensajeJefe(obj.nombre + " se consumió.");
                Inventario.Instance.EliminarObjeto(obj.nombre);
                objetosEquipados.RemoveAt(indiceObjetoActual);
                if (indiceObjetoActual >= objetosEquipados.Count)
                    indiceObjetoActual = 0;
            }
        }

        // Failsafe fallback: If all objects are used and broken/consumed, immediately equip Fists!
        if (objetosEquipados.Count == 0)
        {
            objetosEquipados.Add(new Objeto("Puños", null, dano: 2, durabilidad: 999, esAtaque: true));
            indiceObjetoActual = 0;
        }

        ActualizarUI();
    }

    private void CiclarObjeto(int direccion)
    {
        if (objetosEquipados.Count == 0) return;
        indiceObjetoActual = (indiceObjetoActual + direccion + 
            objetosEquipados.Count) % objetosEquipados.Count;
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

        if (objetosEquipados.Count > 0)
        {
            Objeto obj = objetosEquipados[indiceObjetoActual];
            string tipoLabel = obj.esAtaque ? " [Ataque]" : " [Defensa]";
            textoObjetoEquipado.text = obj.nombre + tipoLabel;
            textoDurabilidad.text = $"Durabilidad: {obj.durabilidad}/{obj.durabilidadMaxima}";
        }
        else
        {
            textoObjetoEquipado.text = "Sin objetos";
            textoDurabilidad.text = "";
        }
    }

    public bool CombateActivo => combateActivo;

    public void TerminarCombatePublico(bool gano)
    {
        TerminarCombate(gano);
    }

    private void TerminarCombate(bool gano)
    {
        combateActivo = false;
        StopAllCoroutines();
        panelCombate.SetActive(false);

        if (gano)
        {
            // El jugador ganó físicamente la pelea (sancionado por responder con violencia)
            ConfrontacionManager.Instance.MostrarResultado(false);
        }
        else
        {
            // El jugador perdió físicamente la pelea (nivel de ansiedad superado)
            ConfrontacionManager.Instance.MostrarDerrotaCombate();
        }
    }
}
