using System;
using UnityEngine;

[Serializable]
public class PersonajeCaso
{
    public string idPersonaje;
    public string nombre;
    public Sprite sprite;
    public string rolActual;

    [TextArea(2, 4)]
    public string descripcionNeutral;

    public PersonajeCaso() { }

    public PersonajeCaso(string idPersonaje, string nombre, string rolActual, string descripcionNeutral)
    {
        this.idPersonaje = idPersonaje;
        this.nombre = nombre;
        this.rolActual = rolActual;
        this.descripcionNeutral = descripcionNeutral;
    }
}
