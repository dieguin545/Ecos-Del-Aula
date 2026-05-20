using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EstilizadorMenusIniciales
{
    private static readonly Color FondoAzul = new Color(0.08f, 0.18f, 0.32f, 1f);
    private static readonly Color FondoBoton = new Color(0.05f, 0.04f, 0.12f, 0.96f);
    private static readonly Color BotonSeleccionado = new Color(0.18f, 0.42f, 0.62f, 1f);
    private static readonly Color TextoClaro = new Color(0.98f, 0.98f, 1f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        EstilizarEscenaActual(SceneManager.GetActiveScene());
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        EstilizarEscenaActual(escena);
    }

    private static void EstilizarEscenaActual(Scene escena)
    {
        string nombre = escena.name.ToLowerInvariant();

        if (nombre == "inicio")
        {
            ReemplazarTextoVisible("Ecos del Aula: Entry Filter", "Ecos del Aula");
            EstilizarCanvasMenu();
            EstilizarBotonesMenu();
            return;
        }

        if (nombre == "seleccion" || nombre == "seleccionjuego")
        {
            ReemplazarTextoVisible("Filtro de entrada", "EntryFilter");
            ReemplazarTextoVisible("filtro entrada", "EntryFilter");
            EstilizarCanvasMenu();
            EstilizarBotonesMenu();
        }
    }

    private static void ReemplazarTextoVisible(string viejo, string nuevo)
    {
        TextMeshProUGUI[] textosTMP = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);

        for (int i = 0; i < textosTMP.Length; i++)
        {
            if (textosTMP[i] != null && textosTMP[i].text.Contains(viejo))
            {
                textosTMP[i].text = textosTMP[i].text.Replace(viejo, nuevo);
            }
        }

        Text[] textos = Object.FindObjectsByType<Text>(FindObjectsInactive.Include);

        for (int i = 0; i < textos.Length; i++)
        {
            if (textos[i] != null && textos[i].text.Contains(viejo))
            {
                textos[i].text = textos[i].text.Replace(viejo, nuevo);
            }
        }
    }

    private static void EstilizarCanvasMenu()
    {
        Camera camara = Camera.main;

        if (camara != null)
        {
            camara.backgroundColor = FondoAzul;
        }

        Image[] imagenes = Object.FindObjectsByType<Image>(FindObjectsInactive.Include);

        for (int i = 0; i < imagenes.Length; i++)
        {
            Image imagen = imagenes[i];

            if (imagen == null || imagen.sprite != null)
            {
                continue;
            }

            string nombre = imagen.gameObject.name.ToLowerInvariant();
            if (nombre.Contains("background") || nombre.Contains("fondo"))
            {
                imagen.color = FondoAzul;
            }
        }
    }

    private static void EstilizarBotonesMenu()
    {
        Button[] botones = Object.FindObjectsByType<Button>(FindObjectsInactive.Include);

        for (int i = 0; i < botones.Length; i++)
        {
            Button boton = botones[i];

            if (boton == null)
            {
                continue;
            }

            Image imagen = boton.GetComponent<Image>();
            if (imagen != null)
            {
                imagen.color = FondoBoton;
            }

            ColorBlock colores = boton.colors;
            colores.normalColor = FondoBoton;
            colores.highlightedColor = BotonSeleccionado;
            colores.selectedColor = BotonSeleccionado;
            colores.pressedColor = Color.Lerp(FondoBoton, Color.black, 0.25f);
            colores.disabledColor = new Color(0.2f, 0.2f, 0.24f, 0.65f);
            boton.colors = colores;

            TextMeshProUGUI textoTMP = boton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (textoTMP != null)
            {
                textoTMP.color = TextoClaro;
                textoTMP.fontSize = Mathf.Max(textoTMP.fontSize, 26f);
                textoTMP.alignment = TextAlignmentOptions.Center;
            }

            Text texto = boton.GetComponentInChildren<Text>(true);
            if (texto != null)
            {
                texto.color = TextoClaro;
                texto.fontSize = Mathf.Max(texto.fontSize, 26);
                texto.alignment = TextAnchor.MiddleCenter;
            }
        }
    }
}
