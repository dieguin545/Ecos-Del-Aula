using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SistemaEvidencia : MonoBehaviour
{
    public static SistemaEvidencia Instance;

    [Header("Configuracion")]
    public int evidenciasNecesarias = 3;
    private int evidenciasRecolectadas = 0;

    // Lista enlazada de evidencias recolectadas
    private LinkedList<string> listaEvidencias = new LinkedList<string>();

    [Header("UI")]
    public TextMeshProUGUI textoEvidencia;
    public GameObject panelEvidencia;

    private string ultimoMensajeBullying = "";
    private bool puedeGuardar = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        panelEvidencia.SetActive(false);
    }

    void Update()
    {
        if (puedeGuardar && Input.GetKeyDown(KeyCode.E))
        {
            GuardarEvidencia();
        }
    }

    public void SetUltimoMensaje(string mensaje, bool esBullying)
    {
        if (esBullying)
        {
            ultimoMensajeBullying = mensaje;
            puedeGuardar = true;
            MostrarIndicador();
        }
    }

    private void MostrarIndicador()
    {
        panelEvidencia.SetActive(true);
        textoEvidencia.text = $"Presiona E para guardar evidencia\nEvidencias: {evidenciasRecolectadas}/{evidenciasNecesarias}";
        Invoke("OcultarIndicador", 3f);
    }

    private void OcultarIndicador()
    {
        panelEvidencia.SetActive(false);
        puedeGuardar = false;
    }

    private void GuardarEvidencia()
    {
        if (ultimoMensajeBullying != "")
        {
            listaEvidencias.AddLast(ultimoMensajeBullying);
            evidenciasRecolectadas++;
            puedeGuardar = false;
            panelEvidencia.SetActive(false);
            Debug.Log("Evidencia guardada: " + ultimoMensajeBullying);
        }
    }

    public bool TieneSuficienteEvidencia()
    {
        return evidenciasRecolectadas >= evidenciasNecesarias;
    }

    public int GetEvidencias()
    {
        return evidenciasRecolectadas;
    }

    public LinkedList<string> GetListaEvidencias()
    {
        return listaEvidencias;
    }
}