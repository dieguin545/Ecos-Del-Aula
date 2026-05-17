using UnityEngine;

// TAD - Clase que representa un mensaje de bullying
[System.Serializable]
public class BullyingMessage
{
    public string mensaje;
    public float ansiedadQueGenera;

    public BullyingMessage(string mensaje, float ansiedadQueGenera)
    {
        this.mensaje = mensaje;
        this.ansiedadQueGenera = ansiedadQueGenera;
    }
}