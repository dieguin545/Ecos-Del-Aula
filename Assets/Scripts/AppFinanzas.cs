using UnityEngine;
using UnityEngine.UI;

public class AppFinanzas : MonoBehaviour
{
    public ControlCorreo controlCorreo;

    public Text textoDineroTotal;
    public Text textoPromedio;
    public Text textoUltimoPago;
    public Text textoDiasTrabajados;

    void OnEnable()
    {
        ActualizarFinanzas();
    }

    public void ActualizarFinanzas()
    {
        textoDineroTotal.text = "Saldo total: $" + controlCorreo.dineroTotal;
        textoUltimoPago.text = "Último pago: $" + controlCorreo.ultimoSueldo;
        textoDiasTrabajados.text = "Días trabajados: " + controlCorreo.diasTrabajados;

        float promedio = 0;

        if (controlCorreo.diasTrabajados > 0)
        {
            promedio = (float)controlCorreo.dineroTotal / controlCorreo.diasTrabajados;
        }

        textoPromedio.text = "Promedio diario: $" + promedio.ToString("0.0");
    }
}