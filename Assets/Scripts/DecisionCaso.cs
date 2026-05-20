using System;
using UnityEngine;

[Serializable]
public class DecisionCaso
{
    public string idCaso;
    public TipoResolucionCaso tipoDecision;

    [TextArea(2, 4)]
    public string resultado;

    public int impactoBienestar;
    public int impactoConfianza;
    public int impactoPrecision;

    public DecisionCaso() { }

    public DecisionCaso(
        string idCaso,
        TipoResolucionCaso tipoDecision,
        string resultado,
        int impactoBienestar,
        int impactoConfianza,
        int impactoPrecision
    )
    {
        this.idCaso = idCaso;
        this.tipoDecision = tipoDecision;
        this.resultado = resultado;
        this.impactoBienestar = impactoBienestar;
        this.impactoConfianza = impactoConfianza;
        this.impactoPrecision = impactoPrecision;
    }
}
