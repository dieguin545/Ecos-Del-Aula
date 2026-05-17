using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;

// Modelo de un caso/correo dentro del sistema Entry Filter de BRIV.
// Mantiene retrocompatibilidad: los campos viejos (remitente, asunto, texto,
// esBullying) siguen igual y se llenan desde el inspector como antes.
// Los nuevos campos son opcionales: si quedan vacios el sistema simplemente
// no los muestra y el correo se ve como antes.
[System.Serializable]
public class Correo
{
    [Header("Datos del correo (original)")]
    public string remitente;
    public string asunto;
    [TextArea(3, 10)] public string texto;
    public bool esBullying;

    [Header("Datos del expediente (opcionales)")]
    public string nombreEstudiante;
    public string idCaso;
    public CategoriaCaso categoria;
    [Tooltip("Senales/red flags visibles para el jugador (max 3 lineas).")]
    public string[] senales;
}

public enum CategoriaCaso
{
    NoEspecificada,
    AcosoDirecto,
    ExclusionDigital,
    Suplantacion,
    Amenazas,
    Spam,
    CorreoLegitimo
}

[System.Serializable]
public class DatosPartida
{
    public int dineroTotal;
    public int diasTrabajados;
    public int ultimoSueldo;
    public int ultimoDia;
    public int errores;
    public int correctos;
    public int correosClasificados;
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

    public List<Correo> correosFaciles;
    public List<Correo> correosMedios;
    public List<Correo> correosDificiles;

    List<Correo> correosActuales;
    Queue<Correo> colaCorreos = new Queue<Correo>();

    public UnityEvent OnStatsChanged;

    int dia = 1;
    int errores = 0;
    int correctos = 0;
    int correosClasificados = 0;
    int sueldoBase = 100;
    bool activo = true;
    string rutaArchivo = "";

    bool modoLecturaActivado = false;

    int tamRemitenteNormal;
    int tamAsuntoNormal;
    int tamCorreoNormal;
    int tamResultadoNormal;
    int tamDiaNormal;
    int tamErroresNormal;

    public int dineroTotal = 0;
    public int diasTrabajados = 0;
    public int ultimoSueldo = 0;

    public bool tieneCafe = false;
    public bool tieneSeguro = false;
    public bool seguroUsado = false;
    public bool tieneFiltroSpam = false;
    public bool tieneTeclado = false;

    void Start()
    {
        panelAviso.SetActive(false);
        panelFinDia.SetActive(false);

        rutaArchivo = Path.Combine(Application.persistentDataPath, "partida_bullying.json");

        CargarArchivo();
        CargarCorreos();
        ActualizarErrores();
        GuardarTamanosNormales();
    }

    void CargarCorreos()
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

        textoDia.text = "Día " + dia;

        MostrarCorreo();
    }

    void MezclarCorreos()
    {
        for (int i = 0; i < correosActuales.Count; i++)
        {
            Correo temporal = correosActuales[i];
            int random = Random.Range(i, correosActuales.Count);

            correosActuales[i] = correosActuales[random];
            correosActuales[random] = temporal;
        }
    }

    void MostrarCorreo()
    {
        if (colaCorreos.Count > 0)
        {
            Correo correoActual = colaCorreos.Peek();
            string remitente = correoActual.remitente;

            if (tieneFiltroSpam == true)
            {
                if (!remitente.Contains("@uninorte.edu.co"))
                {
                    remitente = remitente + " [SOSPECHOSO]";
                }
            }

            textoRemitente.text = remitente;
            textoAsunto.text = correoActual.asunto;
            textoCorreo.text = FormatearExpediente(correoActual);
            textoResultado.text = "";
        }
    }

    // Toma un Correo y lo formatea como expediente para mostrar en el panel.
    // Usa rich text (UnityEngine.UI.Text soporta <b>, <i>, <color>, <size>).
    // Si los campos opcionales estan vacios, se omiten y se ve como texto normal.
    string FormatearExpediente(Correo c)
    {
        var sb = new System.Text.StringBuilder();
        bool tieneEncabezado = false;

        if (!string.IsNullOrEmpty(c.idCaso))
        {
            sb.Append("<color=#00D9FF><b>CASO #");
            sb.Append(c.idCaso);
            sb.Append("</b></color>   ");
            tieneEncabezado = true;
        }
        if (c.categoria != CategoriaCaso.NoEspecificada)
        {
            sb.Append("<color=#B4B7D0>");
            sb.Append(EtiquetaCategoria(c.categoria));
            sb.Append("</color>");
            tieneEncabezado = true;
        }
        if (tieneEncabezado) sb.Append("\n");

        if (!string.IsNullOrEmpty(c.nombreEstudiante))
        {
            sb.Append("<b>Estudiante:</b> ");
            sb.Append(c.nombreEstudiante);
            sb.Append("\n");
        }

        if (tieneEncabezado || !string.IsNullOrEmpty(c.nombreEstudiante))
        {
            sb.Append("\n");
        }

        sb.Append(c.texto);

        if (c.senales != null && c.senales.Length > 0)
        {
            sb.Append("\n\n<color=#FF3D71><b>Senales detectadas:</b></color>");
            for (int i = 0; i < c.senales.Length; i++)
            {
                if (string.IsNullOrEmpty(c.senales[i])) continue;
                sb.Append("\n  - ");
                sb.Append(c.senales[i]);
            }
        }

        return sb.ToString();
    }

    string EtiquetaCategoria(CategoriaCaso cat)
    {
        switch (cat)
        {
            case CategoriaCaso.AcosoDirecto: return "Acoso directo";
            case CategoriaCaso.ExclusionDigital: return "Exclusion digital";
            case CategoriaCaso.Suplantacion: return "Suplantacion";
            case CategoriaCaso.Amenazas: return "Amenazas";
            case CategoriaCaso.Spam: return "Spam";
            case CategoriaCaso.CorreoLegitimo: return "Correo legitimo";
            default: return "";
        }
    }

    public void Evaluar(bool decisionJugador)
    {
        // Si el juego no esta activo o la fila esta vacia, nos salimos
        if (activo == false || colaCorreos.Count == 0)
        {
            return;
        }

        // --- PASO CLAVE EDD: Miramos el correo al frente de la fila ---
        Correo correoActual = colaCorreos.Peek();

        if (decisionJugador == correoActual.esBullying)
        {
            textoResultado.text = "<color=#00D9FF><b>DECISION CORRECTA</b></color>\nCaso clasificado adecuadamente.";
            correctos++;
        }
        else
        {
            if (tieneSeguro == true && seguroUsado == false)
            {
                seguroUsado = true;
                textoResultado.text = "<color=#FFC857><b>SEGURO ACTIVADO</b></color>\nUn error fue evitado.";
                correctos++;
                correosClasificados++;

                float esperaSeguro = 0.5f;
                if (tieneTeclado == true) { esperaSeguro = 0.2f; }

                Invoke("SiguienteCorreo", esperaSeguro);
                return;
            }

            string motivo = correoActual.esBullying
                ? "Era un caso de bullying y lo dejaste pasar."
                : "El correo era legitimo y lo reportaste sin razon.";
            textoResultado.text = "<color=#FF3D71><b>ERROR</b></color>\n" + motivo;
            errores++;
            ActualizarErrores();

            OnStatsChanged.Invoke();
        }

        correosClasificados++;

        float tiempoEspera = 0.5f;
        if (tieneTeclado == true)
        {
            tiempoEspera = 0.2f;
        }

        Invoke("SiguienteCorreo", tiempoEspera);
    }

    public void EvaluarReportar()
    {
        Evaluar(true);
    }

    public void EvaluarAceptar()
    {
        Evaluar(false);
    }

    void SiguienteCorreo()
    {
        if (colaCorreos.Count > 0)
        {
            colaCorreos.Dequeue();
        }

        if (correosClasificados >= 15)
        {
            activo = false;
            textoRemitente.text = "";
            textoAsunto.text = "";
            textoCorreo.text = "";
            textoResultado.text = "";

            Invoke("MostrarPantallaFinDia", 1f);
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

    void MostrarPantallaFinDia()
    {
        panelFinDia.SetActive(true);

        int sueldoFinal;
        string mensajeSueldo;

        int limiteErrores = 10;

        if (tieneCafe)
        {
            limiteErrores = 11;
        }

        if (errores >= limiteErrores)
        {
            sueldoFinal = 0;
            mensajeSueldo = "Perdiste todo el sueldo por llegar al límite de errores.";
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

        GuardarArchivo();

        textoTituloFinDia.text = "Día " + dia + " terminado";

        textoResumenFinDia.text =
            "Correos clasificados: " + correosClasificados +
            "\nCorreos correctos: " + correctos +
            "\nCorreos incorrectos: " + errores +
            "\nSueldo base: $" + sueldoBase +
            "\nSueldo final: $" + sueldoFinal +
            "\n" + mensajeSueldo;

        StartCoroutine(FadeFinDia());
    }

    IEnumerator FadeFinDia()
    {
        canvasFinDia.alpha = 0;

        while (canvasFinDia.alpha < 1)
        {
            canvasFinDia.alpha += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(7f);

        while (canvasFinDia.alpha > 0)
        {
            canvasFinDia.alpha -= Time.deltaTime;
            yield return null;
        }

        panelFinDia.SetActive(false);

        tieneCafe = false;
        tieneSeguro = false;
        seguroUsado = false;

        dia++;
        errores = 0;
        correctos = 0;
        correosClasificados = 0;

        ActualizarErrores();
        CargarCorreos();
    }

    void ActualizarErrores()
    {
        textoErrores.text = "Errores: " + errores;
    }

    void MostrarAviso(string mensaje)
    {
        textoAviso.text = mensaje;
        panelAviso.SetActive(true);

        CancelInvoke("OcultarAviso");
        Invoke("OcultarAviso", 2f);
    }

    void OcultarAviso()
    {
        panelAviso.SetActive(false);
    }

    void GuardarArchivo()
    {
        DatosPartida datos = new DatosPartida();

        datos.dineroTotal = dineroTotal;
        datos.diasTrabajados = diasTrabajados;
        datos.ultimoSueldo = ultimoSueldo;
        datos.ultimoDia = dia;
        datos.errores = errores;
        datos.correctos = correctos;
        datos.correosClasificados = correosClasificados;

        string json = JsonUtility.ToJson(datos, true);
        File.WriteAllText(rutaArchivo, json);
    }

    void CargarArchivo()
    {
        if (File.Exists(rutaArchivo))
        {
            string json = File.ReadAllText(rutaArchivo);
            DatosPartida datos = JsonUtility.FromJson<DatosPartida>(json);

            dineroTotal = datos.dineroTotal;
            diasTrabajados = datos.diasTrabajados;
            ultimoSueldo = datos.ultimoSueldo;
        }
    }

    void GuardarTamanosNormales()
    {
        tamRemitenteNormal = textoRemitente.fontSize;
        tamAsuntoNormal = textoAsunto.fontSize;
        tamCorreoNormal = textoCorreo.fontSize;
        tamResultadoNormal = textoResultado.fontSize;
        tamDiaNormal = textoDia.fontSize;
        tamErroresNormal = textoErrores.fontSize;
    }

    public void AlternarModoLectura()
    {
        modoLecturaActivado = !modoLecturaActivado;

        if (modoLecturaActivado)
        {
            textoRemitente.fontSize = 24;
            textoAsunto.fontSize = 24;
            textoCorreo.fontSize = 30;
            textoResultado.fontSize = 22;
            textoDia.fontSize = 22;
            textoErrores.fontSize = 22;
        }
        else
        {
            textoRemitente.fontSize = tamRemitenteNormal;
            textoAsunto.fontSize = tamAsuntoNormal;
            textoCorreo.fontSize = tamCorreoNormal;
            textoResultado.fontSize = tamResultadoNormal;
            textoDia.fontSize = tamDiaNormal;
            textoErrores.fontSize = tamErroresNormal;
        }
    }
}