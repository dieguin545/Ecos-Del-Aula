using System;
using System.Collections.Generic;

public enum TipoDecisionCorreo
{
    SinDefinir,
    Aceptar,
    Reportar,
    RevisarCaso
}

public enum DificultadEntryFilter
{
    Facil,
    Normal,
    Dificil
}

public enum EstadoCasoBullying
{
    Nuevo,
    EnInvestigacion,
    EvidenciaSuficiente,
    ResueltoCorrectamente,
    ResueltoIncorrectamente,
    CerradoSinAccion
}

public enum NivelRiesgoCaso
{
    Bajo,
    Medio,
    Alto,
    Critico
}

public enum TipoResolucionCaso
{
    ObservarMas,
    MediarConversacion,
    ReportarOrientacion,
    ActivarProtocoloGrave,
    CerrarComoConflictoNormal
}

[Serializable]
public class RegistroCasoGuardado
{
    public string idCaso;
    public EstadoCasoBullying estado;
    public int evidenciasDescubiertas;
    public List<EvidenciaCaso> evidencias = new List<EvidenciaCaso>();
}
