using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class Correo
{
    public string remitente;
    public string asunto;

    [TextArea(3, 10)]
    public string texto;

    public bool esBullying;
}

public class ControlCorreo : MonoBehaviour
{
    public Text textoRemitente;
    public Text textoAsunto;
    public Text textoCorreo;
    public Text textoResultado;
    public Text textoDia;
    public Text textoErrores;

    public GameObject panelAviso;
    public Text textoAviso;

    public GameObject panelFinDia;
    public CanvasGroup canvasFinDia;
    public Text textoTituloFinDia;
    public Text textoResumenFinDia;
    public Button botonContinuarFinDia;

    public List<Correo> correosFaciles;
    public List<Correo> correosMedios;
    public List<Correo> correosDificiles;

    private List<Correo> correosActuales;
    private readonly Queue<Correo> colaCorreos = new Queue<Correo>();

    public UnityEvent OnStatsChanged;

    private int dia = 1;
    private int errores;
    private int correctos;
    private int correosClasificados;
    private readonly int sueldoBase = 100;
    private bool activo = true;
    private bool modoLecturaActivado;
    private bool resumenFinDiaAbierto;
    private GestorGuardadoJuego gestorGuardado;

    private int tamRemitenteNormal;
    private int tamAsuntoNormal;
    private int tamCorreoNormal;
    private int tamResultadoNormal;
    private int tamDiaNormal;
    private int tamErroresNormal;
    private bool uiPreparada;

    public int dineroTotal;
    public int diasTrabajados;
    public int ultimoSueldo;

    public bool tieneCafe;
    public bool tieneSeguro;
    public bool seguroUsado;
    public bool tieneFiltroSpam;
    public bool tieneTeclado;

    public int DiaActual => dia;
    public int Errores => errores;
    public int Correctos => correctos;
    public int CorreosClasificados => correosClasificados;
    public bool LecturaFacilActiva => modoLecturaActivado;
    public string RutaGuardado => gestorGuardado != null ? gestorGuardado.RutaArchivo : string.Empty;

    private void Start()
    {
        if (panelAviso != null) panelAviso.SetActive(false);
        if (panelFinDia != null) panelFinDia.SetActive(false);

        gestorGuardado = new GestorGuardadoJuego(
            Path.Combine(Application.persistentDataPath, "partida_bullying.json")
        );

        CargarProgreso();
        PrepararUiSiHaceFalta();
        PrepararPanelFinDiaSiHaceFalta();
        GuardarTamanosNormales();
        CargarCorreos();
        ActualizarErrores();
        AplicarModoLectura();
    }

    private void CargarCorreos()
    {
        if (dia == 1)
        {
            correosActuales = new List<Correo>(correosFaciles);
        }
        else if (dia == 2)
        {
            correosActuales = new List<Correo>(correosMedios);
        }
        else
        {
            correosActuales = new List<Correo>(correosDificiles);
        }

        MezclarCorreos();
        colaCorreos.Clear();

        foreach (Correo c in correosActuales)
        {
            colaCorreos.Enqueue(c);
        }

        correosClasificados = 0;
        activo = true;

        if (textoDia != null)
        {
            textoDia.text = "Dia " + dia;
        }

        MostrarCorreo();
    }

    private void MezclarCorreos()
    {
        for (int i = 0; i < correosActuales.Count; i++)
        {
            Correo temporal = correosActuales[i];
            int random = Random.Range(i, correosActuales.Count);

            correosActuales[i] = correosActuales[random];
            correosActuales[random] = temporal;
        }
    }

    private void MostrarCorreo()
    {
        if (colaCorreos.Count <= 0)
        {
            return;
        }

        Correo correoActual = colaCorreos.Peek();
        string remitente = correoActual.remitente;

        if (tieneFiltroSpam && !remitente.Contains("@uninorte.edu.co"))
        {
            remitente += " [SOSPECHOSO]";
        }

        if (textoRemitente != null) textoRemitente.text = remitente;
        if (textoAsunto != null) textoAsunto.text = correoActual.asunto;
        if (textoCorreo != null) textoCorreo.text = correoActual.texto;
        if (textoResultado != null) textoResultado.text = string.Empty;
    }

    public void Evaluar(bool decisionJugador)
    {
        if (!activo || colaCorreos.Count == 0)
        {
            return;
        }

        Correo correoActual = colaCorreos.Peek();

        if (decisionJugador == correoActual.esBullying)
        {
            MostrarResultado("Correcto", EstiloUIJuego.Acento);
            correctos++;
        }
        else
        {
            if (tieneSeguro && !seguroUsado)
            {
                seguroUsado = true;
                MostrarResultado("Seguro utilizado", EstiloUIJuego.Acento);
                correctos++;
                correosClasificados++;
                InvocarSiguienteCorreo();
                GuardarProgresoActual();
                return;
            }

            MostrarResultado("Incorrecto", EstiloUIJuego.Peligro);
            errores++;
            ActualizarErrores();
            OnStatsChanged?.Invoke();
        }

        correosClasificados++;
        InvocarSiguienteCorreo();
        GuardarProgresoActual();
    }

    public void EvaluarReportar()
    {
        Evaluar(true);
    }

    public void EvaluarAceptar()
    {
        Evaluar(false);
    }

    private void InvocarSiguienteCorreo()
    {
        float tiempoEspera = tieneTeclado ? 0.2f : 0.5f;
        Invoke(nameof(SiguienteCorreo), tiempoEspera);
    }

    private void SiguienteCorreo()
    {
        if (colaCorreos.Count > 0)
        {
            colaCorreos.Dequeue();
        }

        if (correosClasificados >= 15)
        {
            activo = false;
            if (textoRemitente != null) textoRemitente.text = string.Empty;
            if (textoAsunto != null) textoAsunto.text = string.Empty;
            if (textoCorreo != null) textoCorreo.text = string.Empty;
            if (textoResultado != null) textoResultado.text = string.Empty;

            Invoke(nameof(MostrarPantallaFinDia), 1f);
            return;
        }

        if (colaCorreos.Count > 0)
        {
            MostrarCorreo();
        }
        else
        {
            CargarCorreos();
        }
    }

    private void MostrarPantallaFinDia()
    {
        if (panelFinDia != null)
        {
            panelFinDia.SetActive(true);
        }

        int sueldoFinal;
        string mensajeSueldo;
        int limiteErrores = tieneCafe ? 11 : 10;

        if (errores >= limiteErrores)
        {
            sueldoFinal = 0;
            mensajeSueldo = "Perdiste todo el sueldo por llegar al limite de errores.";
        }
        else
        {
            int descuentos = errores / 2;
            sueldoFinal = sueldoBase - (descuentos * 10);
            mensajeSueldo = "Descuentos aplicados: " + descuentos;
        }

        ultimoSueldo = sueldoFinal;
        dineroTotal += sueldoFinal;
        diasTrabajados++;

        if (textoTituloFinDia != null)
        {
            textoTituloFinDia.text = "Resumen del dia " + dia;
        }

        if (textoResumenFinDia != null)
        {
            textoResumenFinDia.text =
                "Correos clasificados: " + correosClasificados +
                "\nCorreos correctos: " + correctos +
                "\nCorreos incorrectos: " + errores +
                "\nSueldo base: $" + sueldoBase +
                "\nSueldo final: $" + sueldoFinal +
                "\n" + mensajeSueldo +
                "\n\nGuardado automatico en JSON.\nPulsa Continuar para seguir jugando.";
        }

        PrepararSiguienteDiaYGuardar();
        resumenFinDiaAbierto = true;

        if (canvasFinDia != null)
        {
            canvasFinDia.alpha = 1f;
            canvasFinDia.interactable = true;
            canvasFinDia.blocksRaycasts = true;
        }
    }

    private IEnumerator FadeFinDia()
    {
        if (canvasFinDia == null)
        {
            yield break;
        }

        canvasFinDia.alpha = 0f;

        while (canvasFinDia.alpha < 1f)
        {
            canvasFinDia.alpha += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(8f);

        while (canvasFinDia.alpha > 0f)
        {
            canvasFinDia.alpha -= Time.deltaTime;
            yield return null;
        }

        if (panelFinDia != null)
        {
            panelFinDia.SetActive(false);
        }

        ActualizarErrores();
        CargarCorreos();
    }

    public void ContinuarDespuesDelResumen()
    {
        if (!resumenFinDiaAbierto)
        {
            return;
        }

        resumenFinDiaAbierto = false;

        if (panelFinDia != null)
        {
            panelFinDia.SetActive(false);
        }

        if (canvasFinDia != null)
        {
            canvasFinDia.alpha = 1f;
            canvasFinDia.interactable = false;
            canvasFinDia.blocksRaycasts = false;
        }

        ActualizarErrores();
        CargarCorreos();
        GuardarProgresoActual();
    }

    private void PrepararSiguienteDiaYGuardar()
    {
        tieneCafe = false;
        tieneSeguro = false;
        seguroUsado = false;
        dia++;
        errores = 0;
        correctos = 0;
        correosClasificados = 0;
        GuardarAlFinalDelDia();
    }

    private void ActualizarErrores()
    {
        if (textoErrores != null)
        {
            textoErrores.text = "Errores: " + errores;
        }
    }

    private void MostrarResultado(string mensaje, Color color)
    {
        if (textoResultado != null)
        {
            textoResultado.text = mensaje;
            textoResultado.color = color;
        }
    }

    private void GuardarTamanosNormales()
    {
        tamRemitenteNormal = textoRemitente != null ? textoRemitente.fontSize : 20;
        tamAsuntoNormal = textoAsunto != null ? textoAsunto.fontSize : 22;
        tamCorreoNormal = textoCorreo != null ? textoCorreo.fontSize : 22;
        tamResultadoNormal = textoResultado != null ? textoResultado.fontSize : 20;
        tamDiaNormal = textoDia != null ? textoDia.fontSize : 18;
        tamErroresNormal = textoErrores != null ? textoErrores.fontSize : 18;
    }

    public void AlternarModoLectura()
    {
        modoLecturaActivado = !modoLecturaActivado;
        AplicarModoLectura();
        GuardarProgresoActual();
    }

    private void AplicarModoLectura()
    {
        if (modoLecturaActivado)
        {
            if (textoRemitente != null) textoRemitente.fontSize = 24;
            if (textoAsunto != null) textoAsunto.fontSize = 24;
            if (textoCorreo != null) textoCorreo.fontSize = 30;
            if (textoResultado != null) textoResultado.fontSize = 22;
            if (textoDia != null) textoDia.fontSize = 22;
            if (textoErrores != null) textoErrores.fontSize = 22;
        }
        else
        {
            if (textoRemitente != null) textoRemitente.fontSize = tamRemitenteNormal;
            if (textoAsunto != null) textoAsunto.fontSize = tamAsuntoNormal;
            if (textoCorreo != null) textoCorreo.fontSize = tamCorreoNormal;
            if (textoResultado != null) textoResultado.fontSize = tamResultadoNormal;
            if (textoDia != null) textoDia.fontSize = tamDiaNormal;
            if (textoErrores != null) textoErrores.fontSize = tamErroresNormal;
        }
    }

    private void AplicarEstiloCorreo()
    {
        Image fondo = GetComponent<Image>();
        EstiloUIJuego.AplicarPanel(fondo, EstiloUIJuego.FondoPrincipal);
        EstiloUIJuego.AplicarTexto(textoRemitente, 20, EstiloUIJuego.TextoPrincipal);
        EstiloUIJuego.AplicarTexto(textoAsunto, 22, EstiloUIJuego.TextoPrincipal);
        EstiloUIJuego.AplicarTexto(textoCorreo, 22, EstiloUIJuego.TextoSecundario);
        EstiloUIJuego.AplicarTexto(textoResultado, 20, EstiloUIJuego.TextoPrincipal);
        EstiloUIJuego.AplicarTexto(textoDia, 18, EstiloUIJuego.TextoPrincipal);
        EstiloUIJuego.AplicarTexto(textoErrores, 18, EstiloUIJuego.TextoPrincipal);

        Button[] botones = GetComponentsInChildren<Button>(true);

        for (int i = 0; i < botones.Length; i++)
        {
            Button boton = botones[i];

            if (boton == null)
            {
                continue;
            }

            TextMeshProUGUI texto = boton.GetComponentInChildren<TextMeshProUGUI>(true);
            string etiqueta = texto != null ? texto.text.Trim() : string.Empty;

            if (etiqueta.Contains("Reportar"))
            {
                EstiloUIJuego.AplicarBoton(
                    boton,
                    new Color(0.78f, 0.22f, 0.24f, 1f),
                    new Color(0.96f, 0.34f, 0.34f, 1f)
                );
                AcomodarBotonAccion(boton, -100f);
            }
            else if (etiqueta.Contains("Aceptar"))
            {
                EstiloUIJuego.AplicarBoton(
                    boton,
                    new Color(0.14f, 0.58f, 0.32f, 1f),
                    new Color(0.24f, 0.78f, 0.42f, 1f)
                );
                AcomodarBotonAccion(boton, 100f);
            }
        }
    }

    private void PrepararUiSiHaceFalta()
    {
        if (uiPreparada)
        {
            return;
        }

        AjustarVentanaCorreo();
        AsegurarDecoracionCorreo();
        AplicarEstiloCorreo();
        AcomodarTextosCorreo();
        PrepararBotonCerrarCorreo();
        uiPreparada = true;
    }

    private void AjustarVentanaCorreo()
    {
        RectTransform rect = GetComponent<RectTransform>();

        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 12f);
        rect.sizeDelta = new Vector2(700f, 400f);
    }

    private void AsegurarDecoracionCorreo()
    {
        Transform headerExistente = transform.Find("HeaderCorreo");

        if (headerExistente == null)
        {
            EstiloUIJuego.CrearImagen(
                transform,
                "HeaderCorreo",
                new Vector2(0f, 168f),
                new Vector2(700f, 56f),
                EstiloUIJuego.FondoSecundario
            ).transform.SetAsFirstSibling();
        }

        if (transform.Find("TituloCorreo") == null)
        {
            EstiloUIJuego.CrearTextoTMP(
                transform,
                "TituloCorreo",
                "Correo",
                26f,
                new Vector2(0f, 168f),
                new Vector2(200f, 34f),
                TextAlignmentOptions.Center
            );
        }

        Transform cuerpoExistente = transform.Find("CuerpoCorreo");

        if (cuerpoExistente == null)
        {
            EstiloUIJuego.CrearImagen(
                transform,
                "CuerpoCorreo",
                new Vector2(0f, -4f),
                new Vector2(624f, 214f),
                new Color(0.06f, 0.09f, 0.17f, 1f)
            ).transform.SetSiblingIndex(1);
        }
    }

    private void AcomodarTextosCorreo()
    {
        AcomodarTexto(textoDia, new Vector2(-262f, 168f), new Vector2(110f, 28f));
        AcomodarTexto(textoErrores, new Vector2(-142f, 168f), new Vector2(130f, 28f));
        AcomodarTexto(textoRemitente, new Vector2(-116f, 112f), new Vector2(410f, 28f));
        AcomodarTexto(textoAsunto, new Vector2(-116f, 78f), new Vector2(410f, 30f));
        AcomodarTexto(textoCorreo, new Vector2(0f, -8f), new Vector2(572f, 156f));
        AcomodarTexto(textoResultado, new Vector2(0f, -142f), new Vector2(220f, 28f));
    }

    private void AcomodarTexto(Text texto, Vector2 posicion, Vector2 tamano)
    {
        if (texto == null)
        {
            return;
        }

        RectTransform rect = texto.GetComponent<RectTransform>();

        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;
    }

    private void PrepararBotonCerrarCorreo()
    {
        Transform existente = transform.Find("CerrarCorreoVentana");

        if (existente == null)
        {
            GameObject objeto = new GameObject(
                "CerrarCorreoVentana",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button)
            );
            objeto.transform.SetParent(transform, false);
            existente = objeto.transform;

            EstiloUIJuego.CrearTextoTMP(
                objeto.transform,
                "Texto",
                "X",
                18f,
                Vector2.zero,
                new Vector2(36f, 36f),
                TextAlignmentOptions.Center
            );
        }

        RectTransform rect = existente.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-24f, -24f);
            rect.sizeDelta = new Vector2(36f, 36f);
        }

        Button boton = existente.GetComponent<Button>();

        if (boton == null)
        {
            boton = existente.gameObject.AddComponent<Button>();
        }

        if (existente.GetComponent<Image>() == null)
        {
            existente.gameObject.AddComponent<Image>();
        }

        EstiloUIJuego.AplicarBoton(
            boton,
            new Color(0.62f, 0.16f, 0.22f, 1f),
            new Color(0.82f, 0.22f, 0.28f, 1f)
        );
        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(CerrarVentanaCorreo);
    }

    private void PrepararPanelFinDiaSiHaceFalta()
    {
        if (panelFinDia == null)
        {
            return;
        }

        if (canvasFinDia == null)
        {
            canvasFinDia = panelFinDia.GetComponent<CanvasGroup>();

            if (canvasFinDia == null)
            {
                canvasFinDia = panelFinDia.AddComponent<CanvasGroup>();
            }
        }

        canvasFinDia.alpha = 1f;
        canvasFinDia.interactable = false;
        canvasFinDia.blocksRaycasts = false;

        Image fondo = panelFinDia.GetComponent<Image>();

        if (fondo != null)
        {
            fondo.color = new Color(0f, 0f, 0f, 0.9f);
        }

        if (botonContinuarFinDia == null)
        {
            Transform existente = panelFinDia.transform.Find("BotonContinuarResumen");

            if (existente != null)
            {
                botonContinuarFinDia = existente.GetComponent<Button>();
            }
        }

        if (botonContinuarFinDia == null)
        {
            GameObject objeto = new GameObject(
                "BotonContinuarResumen",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button)
            );
            objeto.transform.SetParent(panelFinDia.transform, false);

            RectTransform rect = objeto.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -210f);
            rect.sizeDelta = new Vector2(220f, 52f);

            EstiloUIJuego.CrearTextoTMP(
                objeto.transform,
                "Texto",
                "Continuar",
                24f,
                Vector2.zero,
                new Vector2(210f, 48f),
                TextAlignmentOptions.Center
            );

            botonContinuarFinDia = objeto.GetComponent<Button>();
        }

        EstiloUIJuego.AplicarBoton(
            botonContinuarFinDia,
            new Color(0.08f, 0.28f, 0.42f, 0.96f),
            EstiloUIJuego.Acento
        );
        botonContinuarFinDia.onClick.RemoveAllListeners();
        botonContinuarFinDia.onClick.AddListener(ContinuarDespuesDelResumen);
    }

    private void CerrarVentanaCorreo()
    {
        GestorVentanasPC gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();

        if (gestorVentanas != null)
        {
            gestorVentanas.CerrarVentana(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void AcomodarBotonAccion(Button boton, float posicionX)
    {
        RectTransform rect = boton != null ? boton.GetComponent<RectTransform>() : null;

        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(posicionX, 16f);
        rect.sizeDelta = new Vector2(180f, 46f);
    }

    public void GuardarProgresoActual()
    {
        if (gestorGuardado == null)
        {
            return;
        }

        gestorGuardado.GuardarProgreso(CrearDatosGuardado());
    }

    private void GuardarAlFinalDelDia()
    {
        if (gestorGuardado == null)
        {
            return;
        }

        gestorGuardado.GuardarAlFinalDelDia(CrearDatosGuardado());
    }

    private void CargarProgreso()
    {
        DatosGuardadoJuego datos = gestorGuardado.CargarProgreso();

        if (datos == null)
        {
            return;
        }

        int diaGuardado = datos.diaActual > 0 ? datos.diaActual : datos.ultimoDia;
        dia = Mathf.Max(1, diaGuardado);
        dineroTotal = datos.dineroTotal;
        diasTrabajados = datos.diasTrabajados;
        ultimoSueldo = datos.ultimoSueldo;
        errores = datos.errores;
        correctos = datos.correctos;
        correosClasificados = datos.correosClasificados;
        tieneCafe = datos.tieneCafe;
        tieneSeguro = datos.tieneSeguro;
        seguroUsado = datos.seguroUsado;
        tieneFiltroSpam = datos.tieneFiltroSpam;
        tieneTeclado = datos.tieneTeclado;
        modoLecturaActivado = datos.lecturaFacilActiva;

        ConfiguracionAccesibilidadJuego.Guardar(
            datos.accesibilidadTextoGrande,
            datos.accesibilidadAltoContraste,
            (TipoDaltonismo)datos.accesibilidadTipoDaltonismo,
            datos.accesibilidadReducirEfectos
        );
    }

    private DatosGuardadoJuego CrearDatosGuardado()
    {
        return new DatosGuardadoJuego
        {
            diaActual = dia,
            ultimoDia = dia,
            dineroTotal = dineroTotal,
            diasTrabajados = diasTrabajados,
            ultimoSueldo = ultimoSueldo,
            errores = errores,
            correctos = correctos,
            correosClasificados = correosClasificados,
            tieneCafe = tieneCafe,
            tieneSeguro = tieneSeguro,
            seguroUsado = seguroUsado,
            tieneFiltroSpam = tieneFiltroSpam,
            tieneTeclado = tieneTeclado,
            lecturaFacilActiva = modoLecturaActivado,
            accesibilidadTextoGrande = ConfiguracionAccesibilidadJuego.TextoGrandeActivo,
            accesibilidadAltoContraste = ConfiguracionAccesibilidadJuego.AltoContrasteActivo,
            accesibilidadTipoDaltonismo = (int)ConfiguracionAccesibilidadJuego.TipoDaltonismoActual,
            accesibilidadReducirEfectos = false
        };
    }
}

