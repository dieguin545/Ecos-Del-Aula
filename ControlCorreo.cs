using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class Correo
{
    public string texto;
    public bool esBullying;
}

public class ControlCorreo : MonoBehaviour
{
    public Text textoCorreo;
    public Text textoResultado;
    public Text textoDia;

    // Listas por nivel
    public List<Correo> correosFaciles;
    public List<Correo> correosMedios;
    public List<Correo> correosDificiles;

    // Lista actual
    List<Correo> correosActuales;

    int indice = 0;
    int dia = 1;
    bool activo = true;

    void Start()
    {
        CargarCorreos();
    }

    void CargarCorreos()
    {
        if (dia == 1)
        {
            correosActuales = correosFaciles;
        }
        else if (dia == 2)
        {
            correosActuales = correosMedios;
        }
        else
        {
            correosActuales = correosDificiles;
        }

        indice = 0;
        activo = true;

        textoDia.text = "Día " + dia;

        MostrarCorreo();
    }

    void MostrarCorreo()
    {
        if (indice < correosActuales.Count)
        {
            textoCorreo.text = correosActuales[indice].texto;
            textoResultado.text = "";
        }
    }

    public void Evaluar(bool decisionJugador)
{

    Debug.Log("Evaluando...");
    
    if (!activo)
    {
        return;
    }

    if (indice >= correosActuales.Count)
    {
        return;
    }

    if (decisionJugador == correosActuales[indice].esBullying)
    {
        textoResultado.text = "Correcto";
    }
    else
    {
        textoResultado.text = "Incorrecto";
    }

    Invoke("SiguienteCorreo",0.5f);
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
        indice++;

        if (indice < correosActuales.Count)
        {
            MostrarCorreo();
        }
        else
        {
            activo = false;
            textoCorreo.text = "";
            textoResultado.text = "Fin del día";

            Invoke("SiguienteDia", 2f);
        }
    }

    void SiguienteDia()
    {
        dia++;
        CargarCorreos();
    }
}
