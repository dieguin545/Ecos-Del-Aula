using System.Collections.Generic;
using UnityEngine;

public class GestorCasos : MonoBehaviour
{
    [SerializeField] private List<CasoBullying> casos = new List<CasoBullying>();
    [SerializeField] private List<PersonajeCaso> personajes = new List<PersonajeCaso>();

    private readonly Dictionary<string, CasoBullying> casosPorId =
        new Dictionary<string, CasoBullying>();
    private readonly Dictionary<string, PersonajeCaso> personajesPorId =
        new Dictionary<string, PersonajeCaso>();
    private readonly Stack<DecisionCaso> historialDecisiones = new Stack<DecisionCaso>();

    public IReadOnlyList<CasoBullying> Casos
    {
        get
        {
            InicializarSiHaceFalta();
            return casos;
        }
    }

    public IReadOnlyDictionary<string, PersonajeCaso> Personajes
    {
        get
        {
            InicializarSiHaceFalta();
            return personajesPorId;
        }
    }

    private void Awake()
    {
        InicializarSiHaceFalta();
    }

    public void InicializarSiHaceFalta()
    {
        if (personajes == null)
        {
            personajes = new List<PersonajeCaso>();
        }

        if (casos == null)
        {
            casos = new List<CasoBullying>();
        }

        if (personajes.Count == 0)
        {
            CrearPersonajesBase();
        }

        if (casos.Count == 0)
        {
            CrearCasosBase();
        }

        ReconstruirIndices();
    }

    public bool RegistrarRevisionCorreo(Correo correo, int dia, out string mensaje)
    {
        InicializarSiHaceFalta();
        mensaje = "Este correo no requiere expediente.";

        if (correo == null)
        {
            return false;
        }

        string idCaso = string.IsNullOrWhiteSpace(correo.idCasoRelacionado)
            ? InferirCasoDesdeCorreo(correo)
            : correo.idCasoRelacionado;

        if (string.IsNullOrWhiteSpace(idCaso) || !casosPorId.TryGetValue(idCaso, out CasoBullying caso))
        {
            return false;
        }

        caso.desbloqueado = true;

        if (caso.estado == EstadoCasoBullying.Nuevo)
        {
            caso.estado = EstadoCasoBullying.EnInvestigacion;
        }

        string idEvidencia = !string.IsNullOrWhiteSpace(correo.evidenciaQueDesbloquea)
            ? correo.evidenciaQueDesbloquea
            : "correo_" + dia + "_" + Mathf.Abs((correo.asunto + correo.remitente).GetHashCode());

        EvidenciaCaso evidencia = BuscarEvidencia(caso, idEvidencia);

        if (evidencia == null)
        {
            evidencia = new EvidenciaCaso(
                idEvidencia,
                caso.idCaso,
                CrearDescripcionEvidencia(correo, dia),
                correo.esAmbiguo ? "Contexto" : "Correo",
                Mathf.Max(1, correo.severidad),
                "Correo dia " + dia,
                true
            );
            caso.evidencias.Add(evidencia);
        }
        else
        {
            evidencia.descubierta = true;
        }

        if (caso.TieneEvidenciaSuficiente)
        {
            caso.estado = EstadoCasoBullying.EvidenciaSuficiente;
        }

        historialDecisiones.Push(
            new DecisionCaso(
                caso.idCaso,
                TipoResolucionCaso.ObservarMas,
                "Se agrego evidencia desde correo.",
                1,
                1,
                2
            )
        );

        mensaje = "Evidencia agregada al caso: " + caso.titulo;
        return true;
    }

    public CasoBullying ObtenerCaso(string idCaso)
    {
        InicializarSiHaceFalta();
        casosPorId.TryGetValue(idCaso, out CasoBullying caso);
        return caso;
    }

    public PersonajeCaso ObtenerPersonaje(string idPersonaje)
    {
        InicializarSiHaceFalta();
        personajesPorId.TryGetValue(idPersonaje, out PersonajeCaso personaje);
        return personaje;
    }

    public DecisionCaso ResolverCaso(string idCaso, TipoResolucionCaso decision)
    {
        InicializarSiHaceFalta();

        if (string.IsNullOrWhiteSpace(idCaso) || !casosPorId.TryGetValue(idCaso, out CasoBullying caso))
        {
            return null;
        }

        bool correcta = decision == caso.resolucionCorrecta && caso.TieneEvidenciaSuficiente;
        string resultado;
        int bienestar;
        int confianza;
        int precision;

        if (decision == TipoResolucionCaso.ObservarMas)
        {
            resultado = "Se mantiene el caso en investigación para buscar más contexto.";
            bienestar = 0;
            confianza = 1;
            precision = 1;
            caso.estado = EstadoCasoBullying.EnInvestigacion;
        }
        else if (correcta)
        {
            resultado = "Decisión proporcional: la evidencia respalda la acción tomada.";
            bienestar = 8;
            confianza = 6;
            precision = 8;
            caso.estado = EstadoCasoBullying.ResueltoCorrectamente;
        }
        else if (!caso.TieneEvidenciaSuficiente)
        {
            resultado = "Decisión riesgosa: faltaba evidencia para resolver con seguridad.";
            bienestar = -2;
            confianza = -7;
            precision = -5;
            caso.estado = EstadoCasoBullying.ResueltoIncorrectamente;
        }
        else
        {
            resultado = "La acción no corresponde al nivel de riesgo del caso.";
            bienestar = -5;
            confianza = -4;
            precision = -6;
            caso.estado = EstadoCasoBullying.ResueltoIncorrectamente;
        }

        DecisionCaso registro = new DecisionCaso(
            caso.idCaso,
            decision,
            resultado,
            bienestar,
            confianza,
            precision
        );
        caso.decisionesTomadas.Add(registro);
        historialDecisiones.Push(registro);
        return registro;
    }

    public List<RegistroCasoGuardado> CrearRegistrosGuardado()
    {
        InicializarSiHaceFalta();
        List<RegistroCasoGuardado> registros = new List<RegistroCasoGuardado>();

        for (int i = 0; i < casos.Count; i++)
        {
            CasoBullying caso = casos[i];

            if (caso == null || !caso.desbloqueado)
            {
                continue;
            }

            registros.Add(
                new RegistroCasoGuardado
                {
                    idCaso = caso.idCaso,
                    estado = caso.estado,
                    evidenciasDescubiertas = caso.EvidenciasDescubiertas,
                    evidencias = CrearEvidenciasGuardadas(caso)
                }
            );
        }

        return registros;
    }

    private List<EvidenciaCaso> CrearEvidenciasGuardadas(CasoBullying caso)
    {
        List<EvidenciaCaso> evidenciasGuardadas = new List<EvidenciaCaso>();

        if (caso == null || caso.evidencias == null)
        {
            return evidenciasGuardadas;
        }

        for (int i = 0; i < caso.evidencias.Count; i++)
        {
            EvidenciaCaso evidencia = caso.evidencias[i];

            if (evidencia == null || !evidencia.descubierta)
            {
                continue;
            }

            evidenciasGuardadas.Add(
                new EvidenciaCaso(
                    evidencia.idEvidencia,
                    evidencia.idCaso,
                    evidencia.descripcion,
                    evidencia.tipo,
                    evidencia.peso,
                    evidencia.origen,
                    true
                )
            );
        }

        return evidenciasGuardadas;
    }

    public void CargarRegistrosGuardado(List<RegistroCasoGuardado> registros)
    {
        InicializarSiHaceFalta();

        if (registros == null)
        {
            return;
        }

        for (int i = 0; i < registros.Count; i++)
        {
            RegistroCasoGuardado registro = registros[i];

            if (registro == null || string.IsNullOrWhiteSpace(registro.idCaso))
            {
                continue;
            }

            if (!casosPorId.TryGetValue(registro.idCaso, out CasoBullying caso))
            {
                continue;
            }

            caso.desbloqueado = true;
            caso.estado = registro.estado;

            if (registro.evidencias != null && registro.evidencias.Count > 0)
            {
                for (int j = 0; j < registro.evidencias.Count; j++)
                {
                    EvidenciaCaso evidenciaGuardada = registro.evidencias[j];

                    if (evidenciaGuardada == null || string.IsNullOrWhiteSpace(evidenciaGuardada.idEvidencia))
                    {
                        continue;
                    }

                    EvidenciaCaso evidenciaActual = BuscarEvidencia(caso, evidenciaGuardada.idEvidencia);

                    if (evidenciaActual == null)
                    {
                        caso.evidencias.Add(
                            new EvidenciaCaso(
                                evidenciaGuardada.idEvidencia,
                                string.IsNullOrWhiteSpace(evidenciaGuardada.idCaso) ? caso.idCaso : evidenciaGuardada.idCaso,
                                evidenciaGuardada.descripcion,
                                evidenciaGuardada.tipo,
                                evidenciaGuardada.peso,
                                evidenciaGuardada.origen,
                                true
                            )
                        );
                    }
                    else
                    {
                        evidenciaActual.descripcion = evidenciaGuardada.descripcion;
                        evidenciaActual.tipo = evidenciaGuardada.tipo;
                        evidenciaActual.peso = evidenciaGuardada.peso;
                        evidenciaActual.origen = evidenciaGuardada.origen;
                        evidenciaActual.descubierta = true;
                    }
                }

                continue;
            }

            for (int j = 0; j < caso.evidencias.Count && j < registro.evidenciasDescubiertas; j++)
            {
                if (caso.evidencias[j] != null)
                {
                    caso.evidencias[j].descubierta = true;
                }
            }
        }
    }

    private void ReconstruirIndices()
    {
        personajesPorId.Clear();

        for (int i = 0; i < personajes.Count; i++)
        {
            if (personajes[i] != null && !string.IsNullOrWhiteSpace(personajes[i].idPersonaje))
            {
                personajesPorId[personajes[i].idPersonaje] = personajes[i];
            }
        }

        casosPorId.Clear();

        for (int i = 0; i < casos.Count; i++)
        {
            if (casos[i] != null && !string.IsNullOrWhiteSpace(casos[i].idCaso))
            {
                casosPorId[casos[i].idCaso] = casos[i];
            }
        }
    }

    private EvidenciaCaso BuscarEvidencia(CasoBullying caso, string idEvidencia)
    {
        if (caso == null || string.IsNullOrWhiteSpace(idEvidencia))
        {
            return null;
        }

        for (int i = 0; i < caso.evidencias.Count; i++)
        {
            if (caso.evidencias[i] != null && caso.evidencias[i].idEvidencia == idEvidencia)
            {
                return caso.evidencias[i];
            }
        }

        return null;
    }

    private string InferirCasoDesdeCorreo(Correo correo)
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

        if (texto.Contains("reunion") || texto.Contains("reunión") || texto.Contains("grupo"))
        {
            return "grupo_ciencias";
        }

        if (texto.Contains("apodo") || texto.Contains("meme") || texto.Contains("serie"))
        {
            return "el_apodo";
        }

        if (correo.esBullying || correo.esAmbiguo)
        {
            return "presion_para_callar";
        }

        return string.Empty;
    }

    private string CrearDescripcionEvidencia(Correo correo, int dia)
    {
        string detalle = string.Empty;

        if (correo.pistas != null && correo.pistas.Length > 0)
        {
            detalle = correo.pistas[0];
        }

        if (string.IsNullOrWhiteSpace(detalle))
        {
            detalle = string.IsNullOrWhiteSpace(correo.explicacionEducativa)
                ? "Correo guardado para revisar contexto."
                : correo.explicacionEducativa;
        }

        return "Día "
            + dia
            + " - "
            + correo.asunto
            + ": "
            + detalle;
    }

    private void CrearPersonajesBase()
    {
        personajes.Add(new PersonajeCaso("laura", "Laura", "afectada", "Estudiante aplicada del grupo de ciencias."));
        personajes.Add(new PersonajeCaso("mateo", "Mateo", "testigo", "Compañero que aporta contexto del chat."));
        personajes.Add(new PersonajeCaso("valeria", "Valeria", "involucrada", "Participa en el grupo y puede mediar."));
        personajes.Add(new PersonajeCaso("samuel", "Samuel", "involucrado", "Estudiante nuevo que intenta integrarse."));
        personajes.Add(new PersonajeCaso("camila", "Camila", "reportada", "Su rol depende de la evidencia, no de su apariencia."));
        personajes.Add(new PersonajeCaso("nicolas", "Nicolás", "testigo", "Observa patrones repetidos en el curso."));
    }

    private void CrearCasosBase()
    {
        casos.Add(
            CrearCaso(
                "grupo_ciencias",
                "Grupo de ciencias",
                "Exclusión en un trabajo grupal. El objetivo es distinguir una queja académica de un patrón de aislamiento.",
                DificultadEntryFilter.Normal,
                NivelRiesgoCaso.Medio,
                TipoResolucionCaso.MediarConversacion,
                "laura",
                "mateo",
                "valeria"
            )
        );
        casos.Add(
            CrearCaso(
                "capturas_chat",
                "Capturas del chat",
                "Difusión de conversaciones privadas para humillar a una persona.",
                DificultadEntryFilter.Normal,
                NivelRiesgoCaso.Alto,
                TipoResolucionCaso.ReportarOrientacion,
                "samuel",
                "nicolas"
            )
        );
        casos.Add(
            CrearCaso(
                "el_apodo",
                "El apodo",
                "Burlas repetidas por gustos personales. La repetición y el impacto importan.",
                DificultadEntryFilter.Normal,
                NivelRiesgoCaso.Medio,
                TipoResolucionCaso.MediarConversacion,
                "camila",
                "valeria"
            )
        );
        casos.Add(
            CrearCaso(
                "rumor_curso",
                "Rumor en el curso",
                "Correos ambiguos sobre rumores y aislamiento social. Requiere evidencia antes de acusar.",
                DificultadEntryFilter.Dificil,
                NivelRiesgoCaso.Alto,
                TipoResolucionCaso.ReportarOrientacion,
                "laura",
                "samuel",
                "nicolas"
            )
        );
        casos.Add(
            CrearCaso(
                "comentario_discriminatorio",
                "Comentario discriminatorio",
                "Situación sensible que debe tratarse sin normalizar insultos ni estereotipos.",
                DificultadEntryFilter.Dificil,
                NivelRiesgoCaso.Critico,
                TipoResolucionCaso.ActivarProtocoloGrave,
                "samuel",
                "camila"
            )
        );
        casos.Add(
            CrearCaso(
                "estudiante_aplicado",
                "El estudiante aplicado",
                "Burlas por rendimiento académico. El juego no castiga estudiar: analiza el daño social.",
                DificultadEntryFilter.Normal,
                NivelRiesgoCaso.Medio,
                TipoResolucionCaso.MediarConversacion,
                "laura",
                "mateo"
            )
        );
        casos.Add(
            CrearCaso(
                "malentendido",
                "Malentendido",
                "Parece acoso, pero el contexto puede demostrar que era un conflicto normal.",
                DificultadEntryFilter.Dificil,
                NivelRiesgoCaso.Bajo,
                TipoResolucionCaso.CerrarComoConflictoNormal,
                "valeria",
                "nicolas"
            )
        );
        casos.Add(
            CrearCaso(
                "presion_para_callar",
                "Presión para callar",
                "Mensajes indirectos que buscan impedir que alguien pida ayuda.",
                DificultadEntryFilter.Dificil,
                NivelRiesgoCaso.Alto,
                TipoResolucionCaso.ReportarOrientacion,
                "samuel",
                "mateo"
            )
        );
    }

    private CasoBullying CrearCaso(
        string id,
        string titulo,
        string descripcion,
        DificultadEntryFilter dificultad,
        NivelRiesgoCaso riesgo,
        TipoResolucionCaso resolucion,
        params string[] personajesCaso
    )
    {
        CasoBullying caso = new CasoBullying
        {
            idCaso = id,
            titulo = titulo,
            descripcion = descripcion,
            dificultad = dificultad,
            nivelRiesgo = riesgo,
            estado = EstadoCasoBullying.Nuevo,
            desbloqueado = false,
            resolucionCorrecta = resolucion,
            personajesInvolucrados = new List<string>(personajesCaso)
        };

        caso.evidencias.Add(
            new EvidenciaCaso(
                id + "_base",
                id,
                "Evidencia pendiente. Usa Revisar contexto desde Correo para agregar contexto.",
                "Pendiente",
                1,
                "Sistema",
                false
            )
        );

        return caso;
    }
}
