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
    public string idCorreo;
    public string remitente;
    public string asunto;

    [TextArea(3, 10)]
    public string texto;

    public bool esBullying;
    public int dia;
    public int dificultad;
    public string idCasoRelacionado;
    public TipoDecisionCorreo decisionCorrecta;
    public string evidenciaQueDesbloquea;
    public bool esAmbiguo;
    public int severidad = 1;
    public string[] pistas;

    [TextArea(2, 4)]
    public string explicacionEducativa;
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
    public Button botonRevisarCaso;

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
    private GestorCasos gestorCasos;

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

    public DificultadEntryFilter dificultadEntryFilter = DificultadEntryFilter.Normal;
    public int bienestarEstudiantil = 70;
    public int confianzaEscolar = 70;
    public int precision = 0;
    private int evidenciasEncontradasDia;
    private int casosAbiertosDia;

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
        AsegurarGestorCasos();

        CargarProgreso();
        PrepararUiSiHaceFalta();
        PrepararPanelFinDiaSiHaceFalta();
        GuardarTamanosNormales();
        CargarCorreos(true);
        ActualizarErrores();
        AplicarModoLectura();
    }

    private void AsegurarGestorCasos()
    {
        if (gestorCasos != null)
        {
            gestorCasos.InicializarSiHaceFalta();
            return;
        }

        gestorCasos = FindAnyObjectByType<GestorCasos>(FindObjectsInactive.Include);

        if (gestorCasos == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>(true);
            GameObject contenedor = canvas != null ? canvas.gameObject : gameObject;
            gestorCasos = contenedor.GetComponent<GestorCasos>();

            if (gestorCasos == null)
            {
                gestorCasos = contenedor.AddComponent<GestorCasos>();
            }
        }

        gestorCasos.InicializarSiHaceFalta();
    }

    private void CargarCorreos(bool reiniciarContadoresDia)
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
        EnriquecerCorreosSiHaceFalta();
        colaCorreos.Clear();

        foreach (Correo c in correosActuales)
        {
            colaCorreos.Enqueue(c);
        }

        if (reiniciarContadoresDia)
        {
            correosClasificados = 0;
            evidenciasEncontradasDia = 0;
            casosAbiertosDia = 0;
        }

        activo = true;

        if (textoDia != null)
        {
            textoDia.text = "Dia " + dia;
        }

        if (colaCorreos.Count > 0)
        {
            MostrarCorreo();
        }
        else
        {
            activo = false;
            Invoke(nameof(MostrarPantallaFinDia), 0.2f);
        }
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

    private void EnriquecerCorreosSiHaceFalta()
    {
        for (int i = 0; i < correosActuales.Count; i++)
        {
            Correo correo = correosActuales[i];

            if (correo == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(correo.idCorreo))
            {
                correo.idCorreo = "correo_" + dia + "_" + i;
            }

            if (correo.dia <= 0)
            {
                correo.dia = dia;
            }

            if (correo.decisionCorrecta == TipoDecisionCorreo.SinDefinir)
            {
                correo.decisionCorrecta = correo.esBullying
                    ? TipoDecisionCorreo.Reportar
                    : TipoDecisionCorreo.Aceptar;
            }

            string texto = ((correo.asunto ?? string.Empty) + " " + (correo.texto ?? string.Empty)).ToLowerInvariant();

            bool requiereContexto =
                texto.Contains("reunion")
                || texto.Contains("captura")
                || texto.Contains("rumor")
                || texto.Contains("no digas")
                || texto.Contains("grupo")
                || texto.Contains("meme");

            if (requiereContexto && dificultadEntryFilter != DificultadEntryFilter.Facil)
            {
                correo.esAmbiguo = true;

                if (correo.decisionCorrecta == TipoDecisionCorreo.Reportar && dificultadEntryFilter == DificultadEntryFilter.Dificil)
                {
                    correo.decisionCorrecta = TipoDecisionCorreo.RevisarCaso;
                }
            }

            if (correo.severidad <= 0)
            {
                correo.severidad = correo.esBullying ? 2 : 1;
            }

            if (string.IsNullOrWhiteSpace(correo.idCasoRelacionado) && (correo.esAmbiguo || correo.esBullying))
            {
                correo.idCasoRelacionado = InferirCasoCorreo(correo);
            }

            if (correo.pistas == null || correo.pistas.Length == 0)
            {
                correo.pistas = CrearPistasCorreo(correo);
            }

            if (string.IsNullOrWhiteSpace(correo.explicacionEducativa))
            {
                correo.explicacionEducativa = CrearExplicacionCorreo(correo);
            }
        }
    }

    private string InferirCasoCorreo(Correo correo)
    {
        string texto = ((correo.asunto ?? string.Empty) + " " + (correo.texto ?? string.Empty)).ToLowerInvariant();

        if (texto.Contains("captura") || texto.Contains("chat"))
        {
            return "capturas_chat";
        }

        if (texto.Contains("rumor") || texto.Contains("dicen"))
        {
            return "rumor_curso";
        }

        if (texto.Contains("reunion") || texto.Contains("grupo"))
        {
            return "grupo_ciencias";
        }

        if (texto.Contains("meme") || texto.Contains("apodo") || texto.Contains("serie"))
        {
            return "el_apodo";
        }

        if (texto.Contains("no digas") || texto.Contains("callar"))
        {
            return "presion_para_callar";
        }

        return correo.esBullying ? "presion_para_callar" : string.Empty;
    }

    private string[] CrearPistasCorreo(Correo correo)
    {
        List<string> pistas = new List<string>();
        string texto = ((correo.asunto ?? string.Empty) + " " + (correo.texto ?? string.Empty)).ToLowerInvariant();

        if (texto.Contains("no digas") || texto.Contains("callar"))
        {
            pistas.Add("posible presion para guardar silencio");
        }

        if (texto.Contains("captura") || texto.Contains("chat"))
        {
            pistas.Add("posible difusion de informacion privada");
        }

        if (texto.Contains("reunion") || texto.Contains("grupo"))
        {
            pistas.Add("revisar si hay exclusion repetida");
        }

        if (texto.Contains("meme") || texto.Contains("apodo"))
        {
            pistas.Add("distinguir broma aislada de burla repetida");
        }

        if (pistas.Count == 0)
        {
            pistas.Add(correo.esAmbiguo ? "falta contexto" : "no hay senales claras");
        }

        return pistas.ToArray();
    }

    private string CrearExplicacionCorreo(Correo correo)
    {
        if (correo.decisionCorrecta == TipoDecisionCorreo.RevisarCaso)
        {
            return "La decision recomendada es revisar contexto antes de acusar o ignorar.";
        }

        if (correo.decisionCorrecta == TipoDecisionCorreo.Reportar)
        {
            return "Hay senales suficientes de dano, presion o acoso que deben reportarse.";
        }

        return "El correo parece comunicacion normal o conflicto sin evidencia suficiente de acoso.";
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

        string cuerpo = correoActual.texto;

        if (tieneFiltroSpam)
        {
            cuerpo += "\n\nPISTA DEL FILTRO: " + ObtenerPistaFiltro(correoActual);
        }

        if (textoRemitente != null) textoRemitente.text = remitente;
        if (textoAsunto != null) textoAsunto.text = correoActual.asunto;
        if (textoCorreo != null) textoCorreo.text = cuerpo;
        if (textoResultado != null) textoResultado.text = string.Empty;
        ActualizarCajaFeedback(false);
    }

    private string ObtenerPistaFiltro(Correo correo)
    {
        if (correo == null || correo.pistas == null || correo.pistas.Length == 0)
        {
            return "No se detectaron senales claras. Puede requerir contexto.";
        }

        return correo.pistas[0];
    }

    public void Evaluar(bool decisionJugador)
    {
        EvaluarDecision(decisionJugador ? TipoDecisionCorreo.Reportar : TipoDecisionCorreo.Aceptar);
    }

    public void EvaluarDecision(TipoDecisionCorreo decisionJugador)
    {
        if (!activo || colaCorreos.Count == 0)
        {
            return;
        }

        Correo correoActual = colaCorreos.Peek();
        TipoDecisionCorreo decisionEsperada = ObtenerDecisionEsperada(correoActual);

        bool decisionCorrecta = decisionJugador == decisionEsperada;
        string mensajeRevision = string.Empty;

        if (decisionCorrecta && decisionJugador == TipoDecisionCorreo.RevisarCaso)
        {
            mensajeRevision = RegistrarRevisionCaso(correoActual);
        }

        if (decisionCorrecta)
        {
            string mensajeCorrecto = "Correcto - " + ObtenerTextoDecision(decisionJugador);

            if (!string.IsNullOrWhiteSpace(mensajeRevision))
            {
                mensajeCorrecto += ". " + mensajeRevision;
            }

            MostrarResultado(mensajeCorrecto, EstiloUIJuego.Acento);
            correctos++;
            bienestarEstudiantil = Mathf.Min(100, bienestarEstudiantil + (decisionJugador == TipoDecisionCorreo.Reportar ? 2 : 1));
            confianzaEscolar = Mathf.Min(100, confianzaEscolar + 1);
        }
        else
        {
            if (tieneSeguro && !seguroUsado)
            {
                seguroUsado = true;
                MostrarResultado("Seguro usado: evitaste la penalizacion", EstiloUIJuego.Acento);
                correctos++;
                correosClasificados++;
                ActualizarPrecision();
                InvocarSiguienteCorreo(false);
                GuardarProgresoActual();
                return;
            }

            MostrarResultado("Incorrecto - " + correoActual.explicacionEducativa, EstiloUIJuego.Peligro);
            errores++;
            AplicarConsecuenciaError(decisionJugador, correoActual);
            ActualizarErrores();
            OnStatsChanged?.Invoke();
        }

        correosClasificados++;
        ActualizarPrecision();
        InvocarSiguienteCorreo(decisionCorrecta);
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

    public void EvaluarRevisarCaso()
    {
        EvaluarDecision(TipoDecisionCorreo.RevisarCaso);
    }

    public void AplicarImpactoDecisionCaso(DecisionCaso decision)
    {
        if (decision == null)
        {
            return;
        }

        bienestarEstudiantil = Mathf.Clamp(
            bienestarEstudiantil + decision.impactoBienestar,
            0,
            100
        );
        confianzaEscolar = Mathf.Clamp(
            confianzaEscolar + decision.impactoConfianza,
            0,
            100
        );
        precision = Mathf.Clamp(precision + decision.impactoPrecision, 0, 100);
        GuardarProgresoActual();
    }

    private TipoDecisionCorreo ObtenerDecisionEsperada(Correo correo)
    {
        if (correo != null && correo.decisionCorrecta != TipoDecisionCorreo.SinDefinir)
        {
            return correo.decisionCorrecta;
        }

        return correo != null && correo.esBullying
            ? TipoDecisionCorreo.Reportar
            : TipoDecisionCorreo.Aceptar;
    }

    private string ObtenerTextoDecision(TipoDecisionCorreo decision)
    {
        switch (decision)
        {
            case TipoDecisionCorreo.Reportar:
                return "Reportar";
            case TipoDecisionCorreo.RevisarCaso:
                return "Revisar contexto";
            default:
                return "Aceptar";
        }
    }

    private string RegistrarRevisionCaso(Correo correoActual)
    {
        AsegurarGestorCasos();

        if (gestorCasos == null)
        {
            MostrarResultado("No se encontro la app Casos.", EstiloUIJuego.Peligro);
            return "No se encontro la app Casos.";
        }

        bool agregado = gestorCasos.RegistrarRevisionCorreo(correoActual, dia, out string mensaje);

        if (agregado)
        {
            evidenciasEncontradasDia++;
            casosAbiertosDia = Mathf.Max(casosAbiertosDia, ContarCasosActivos());
        }

        return mensaje;
    }

    private int ContarCasosActivos()
    {
        if (gestorCasos == null)
        {
            return 0;
        }

        int total = 0;
        IReadOnlyList<CasoBullying> casos = gestorCasos.Casos;

        for (int i = 0; i < casos.Count; i++)
        {
            if (casos[i] != null && casos[i].desbloqueado)
            {
                total++;
            }
        }

        return total;
    }

    private void AplicarConsecuenciaError(TipoDecisionCorreo decisionJugador, Correo correo)
    {
        if (correo != null && correo.esBullying && decisionJugador == TipoDecisionCorreo.Aceptar)
        {
            bienestarEstudiantil = Mathf.Max(0, bienestarEstudiantil - 8);
            confianzaEscolar = Mathf.Max(0, confianzaEscolar - 2);
            return;
        }

        if (correo != null && !correo.esBullying && decisionJugador == TipoDecisionCorreo.Reportar)
        {
            confianzaEscolar = Mathf.Max(0, confianzaEscolar - 7);
            bienestarEstudiantil = Mathf.Max(0, bienestarEstudiantil - 1);
            return;
        }

        precision = Mathf.Max(0, precision - 3);
    }

    private void ActualizarPrecision()
    {
        precision = correosClasificados <= 0
            ? 0
            : Mathf.RoundToInt((float)correctos / Mathf.Max(1, correosClasificados) * 100f);
    }

    private void InvocarSiguienteCorreo(bool decisionCorrecta)
    {
        float tiempoEspera = decisionCorrecta
            ? (tieneTeclado ? 1.1f : 1.4f)
            : 3.8f;
        Invoke(nameof(SiguienteCorreo), tiempoEspera);
    }

    private void SiguienteCorreo()
    {
        if (colaCorreos.Count > 0)
        {
            colaCorreos.Dequeue();
        }

        if (correosClasificados >= ObtenerObjetivoCorreosDia())
        {
            activo = false;
            if (textoRemitente != null) textoRemitente.text = string.Empty;
            if (textoAsunto != null) textoAsunto.text = string.Empty;
            if (textoCorreo != null) textoCorreo.text = string.Empty;
            if (textoResultado != null) textoResultado.text = string.Empty;
            ActualizarCajaFeedback(false);

            Invoke(nameof(MostrarPantallaFinDia), 1f);
            return;
        }

        if (colaCorreos.Count > 0)
        {
            MostrarCorreo();
        }
        else
        {
            CargarCorreos(false);
        }
    }

    private int ObtenerObjetivoCorreosDia()
    {
        int objetivo;

        switch (dificultadEntryFilter)
        {
            case DificultadEntryFilter.Facil:
                objetivo = 8;
                break;
            case DificultadEntryFilter.Dificil:
                objetivo = 15;
                break;
            default:
                objetivo = 12;
                break;
        }

        if (tieneCafe)
        {
            objetivo += 1;
        }

        if (tieneTeclado)
        {
            objetivo += 1;
        }

        return objetivo;
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
                "\nCasos abiertos: " + casosAbiertosDia +
                "\nEvidencias encontradas: " + evidenciasEncontradasDia +
                "\nBienestar estudiantil: " + bienestarEstudiantil +
                "\nConfianza escolar: " + confianzaEscolar +
                "\nPrecision: " + precision + "%" +
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
        CargarCorreos(true);
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
        CargarCorreos(true);
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
            ActualizarCajaFeedback(!string.IsNullOrWhiteSpace(mensaje));
        }
    }

    private void ActualizarCajaFeedback(bool visible)
    {
        Transform caja = transform.Find("CajaFeedbackCorreo");

        if (caja != null)
        {
            caja.gameObject.SetActive(visible);
        }

        if (textoResultado != null)
        {
            textoResultado.transform.SetAsLastSibling();
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
                AcomodarBotonAccion(boton, -210f);
            }
            else if (etiqueta.Contains("Revisar"))
            {
                EstiloUIJuego.AplicarBoton(
                    boton,
                    new Color(0.16f, 0.3f, 0.58f, 1f),
                    EstiloUIJuego.AcentoCalido
                );
                AcomodarBotonAccion(boton, 0f);
            }
            else if (etiqueta.Contains("Aceptar"))
            {
                EstiloUIJuego.AplicarBoton(
                    boton,
                    new Color(0.14f, 0.58f, 0.32f, 1f),
                    new Color(0.24f, 0.78f, 0.42f, 1f)
                );
                AcomodarBotonAccion(boton, 210f);
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
        PrepararBotonRevisarCaso();
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

        Transform feedbackExistente = transform.Find("CajaFeedbackCorreo");

        if (feedbackExistente == null)
        {
            Image caja = EstiloUIJuego.CrearImagen(
                transform,
                "CajaFeedbackCorreo",
                new Vector2(0f, -98f),
                new Vector2(620f, 50f),
                new Color(0.02f, 0.02f, 0.06f, 0.88f)
            );
            caja.raycastTarget = false;
            caja.gameObject.SetActive(false);
        }
    }

    private void AcomodarTextosCorreo()
    {
        AcomodarTexto(textoDia, new Vector2(-262f, 168f), new Vector2(110f, 28f));
        AcomodarTexto(textoErrores, new Vector2(-142f, 168f), new Vector2(130f, 28f));
        AcomodarTexto(textoRemitente, new Vector2(-116f, 112f), new Vector2(410f, 28f));
        AcomodarTexto(textoAsunto, new Vector2(-116f, 78f), new Vector2(410f, 30f));
        AcomodarTexto(textoCorreo, new Vector2(0f, -8f), new Vector2(572f, 156f));
        AcomodarTexto(textoResultado, new Vector2(0f, -98f), new Vector2(590f, 44f));

        if (textoResultado != null)
        {
            textoResultado.alignment = TextAnchor.MiddleCenter;
            textoResultado.transform.SetAsLastSibling();
        }
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

    private void PrepararBotonRevisarCaso()
    {
        if (botonRevisarCaso == null)
        {
            Transform existente = transform.Find("BotonRevisarCaso");

            if (existente != null)
            {
                botonRevisarCaso = existente.GetComponent<Button>();
            }
        }

        if (botonRevisarCaso == null)
        {
            GameObject objeto = new GameObject(
                "BotonRevisarCaso",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button)
            );
            objeto.transform.SetParent(transform, false);

            EstiloUIJuego.CrearTextoTMP(
                objeto.transform,
                "Texto",
                "Revisar contexto",
                17f,
                Vector2.zero,
                new Vector2(170f, 42f),
                TextAlignmentOptions.Center
            );

            botonRevisarCaso = objeto.GetComponent<Button>();
        }

        botonRevisarCaso.onClick.RemoveAllListeners();
        botonRevisarCaso.onClick.AddListener(EvaluarRevisarCaso);

        TextMeshProUGUI textoBoton = botonRevisarCaso.GetComponentInChildren<TextMeshProUGUI>(true);

        if (textoBoton != null)
        {
            textoBoton.text = "Revisar contexto";
            textoBoton.fontSize = 16f;
        }

        EstiloUIJuego.AplicarBoton(
            botonRevisarCaso,
            new Color(0.16f, 0.3f, 0.58f, 1f),
            EstiloUIJuego.AcentoCalido
        );
        AcomodarBotonAccion(botonRevisarCaso, 0f);
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
        bienestarEstudiantil = datos.bienestarEstudiantil > 0 ? datos.bienestarEstudiantil : 70;
        confianzaEscolar = datos.confianzaEscolar > 0 ? datos.confianzaEscolar : 70;
        precision = datos.precision;

        if (!string.IsNullOrWhiteSpace(datos.dificultadEntryFilter) &&
            System.Enum.TryParse(datos.dificultadEntryFilter, out DificultadEntryFilter dificultadGuardada))
        {
            dificultadEntryFilter = dificultadGuardada;
        }

        if (gestorCasos == null)
        {
            gestorCasos = FindAnyObjectByType<GestorCasos>();
        }

        if (gestorCasos != null)
        {
            gestorCasos.CargarRegistrosGuardado(datos.casos);
        }

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
            accesibilidadReducirEfectos = false,
            dificultadEntryFilter = dificultadEntryFilter.ToString(),
            bienestarEstudiantil = bienestarEstudiantil,
            confianzaEscolar = confianzaEscolar,
            precision = precision,
            casos = gestorCasos != null
                ? gestorCasos.CrearRegistrosGuardado()
                : new List<RegistroCasoGuardado>()
        };
    }
}

