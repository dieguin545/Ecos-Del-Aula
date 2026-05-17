using UnityEngine;
using UnityEngine.UI;

public class AppTienda : MonoBehaviour
{
    public ControlCorreo controlCorreo;

    public Text textoDinero;
    public Text textoMensaje;

    public Button botonCafe;
    public Button botonSeguro;
    public Button botonFiltro;
    public Button botonTeclado;

    int precioCafe = 250;
    int precioSeguro = 300;
    int precioFiltro = 400;
    int precioTeclado = 350;

    void OnEnable()
    {
        ActualizarUI();
    }
    public void ComprarCafe()
    {
        if (controlCorreo.dineroTotal >= precioCafe)
        {
            controlCorreo.dineroTotal -= precioCafe;
            controlCorreo.tieneCafe = true;
            textoMensaje.text = "Compraste café energético";
        }
        else
        {
            textoMensaje.text = "No tienes suficiente dinero";
        }

        ActualizarUI();
    }
    public void ComprarSeguro()
    {
        if (controlCorreo.dineroTotal >= precioSeguro)
        {
            controlCorreo.dineroTotal -= precioSeguro;
            controlCorreo.tieneSeguro = true;
            textoMensaje.text = "Compraste seguro de clasificación";
        }
        else
        {
            textoMensaje.text = "No tienes suficiente dinero";
        }

        ActualizarUI();
    }
    public void ComprarFiltro()
    {
        if (controlCorreo.tieneFiltroSpam)
        {
            textoMensaje.text = "Ya compraste el filtro de spam";
            return;
        }

        if (controlCorreo.dineroTotal >= precioFiltro)
        {
            controlCorreo.dineroTotal -= precioFiltro;
            controlCorreo.tieneFiltroSpam = true;
            textoMensaje.text = "Compraste filtro de spam";
        }
        else
        {
            textoMensaje.text = "No tienes suficiente dinero";
        }

        ActualizarUI();
    }
    public void ComprarTeclado()
    {
        if (controlCorreo.tieneTeclado)
        {
            textoMensaje.text = "Ya compraste el teclado mecánico";
            return;
        }

        if (controlCorreo.dineroTotal >= precioTeclado)
        {
            controlCorreo.dineroTotal -= precioTeclado;
            controlCorreo.tieneTeclado = true;
            textoMensaje.text = "Compraste teclado mecánico";
        }
        else
        {
            textoMensaje.text = "No tienes suficiente dinero";
        }

        ActualizarUI();
    }
    void ActualizarUI()
    {
        textoDinero.text = "Dinero: $" + controlCorreo.dineroTotal;

        if (controlCorreo.tieneFiltroSpam)
        {
            botonFiltro.interactable = false;
        }

        if (controlCorreo.tieneTeclado)
        {
            botonTeclado.interactable = false;
        }
    }
}
