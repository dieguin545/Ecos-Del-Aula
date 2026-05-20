using System;
using UnityEngine;

[Serializable]
public class EvidenciaCaso
{
    public string idEvidencia;
    public string idCaso;

    [TextArea(2, 5)]
    public string descripcion;

    public string tipo;
    public int peso;
    public string origen;
    public bool descubierta;

    public EvidenciaCaso() { }

    public EvidenciaCaso(
        string idEvidencia,
        string idCaso,
        string descripcion,
        string tipo,
        int peso,
        string origen,
        bool descubierta
    )
    {
        this.idEvidencia = idEvidencia;
        this.idCaso = idCaso;
        this.descripcion = descripcion;
        this.tipo = tipo;
        this.peso = peso;
        this.origen = origen;
        this.descubierta = descubierta;
    }
}
