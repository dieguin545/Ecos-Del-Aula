using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CasoBullying
{
    public string idCaso;
    public string titulo;

    [TextArea(3, 6)]
    public string descripcion;

    public DificultadEntryFilter dificultad;
    public EstadoCasoBullying estado;
    public NivelRiesgoCaso nivelRiesgo;
    public bool desbloqueado;
    public TipoResolucionCaso resolucionCorrecta;

    public List<string> personajesInvolucrados = new List<string>();
    public List<EvidenciaCaso> evidencias = new List<EvidenciaCaso>();
    public List<DecisionCaso> decisionesTomadas = new List<DecisionCaso>();

    public int EvidenciasDescubiertas
    {
        get
        {
            int total = 0;

            for (int i = 0; i < evidencias.Count; i++)
            {
                if (evidencias[i] != null && evidencias[i].descubierta)
                {
                    total++;
                }
            }

            return total;
        }
    }

    public int EvidenciasNecesarias
    {
        get
        {
            switch (dificultad)
            {
                case DificultadEntryFilter.Facil:
                    return 1;
                case DificultadEntryFilter.Dificil:
                    return 3;
                default:
                    return 2;
            }
        }
    }

    public bool TieneEvidenciaSuficiente => EvidenciasDescubiertas >= EvidenciasNecesarias;
}
