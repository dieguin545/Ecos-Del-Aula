using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MisionManager : MonoBehaviour
{
    public static MisionManager Instance;

    [Header("UI")]
    public GameObject panelMisiones;
    public TextMeshProUGUI textoMisiones;

    // Lista enlazada de misiones activas
    private LinkedList<Mision> misionesActivas = new LinkedList<Mision>();
    private LinkedList<Mision> misionesCompletadas = new LinkedList<Mision>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        if (panelMisiones != null)
            panelMisiones.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (panelMisiones.activeSelf)
                panelMisiones.SetActive(false);
            else
            {
                panelMisiones.SetActive(true);
                ActualizarUI();
            }
        }
    }

    public void RegistrarMision(Mision mision)
    {
        misionesActivas.AddLast(mision);
        ActualizarUI();
        panelMisiones.SetActive(true);
        Invoke("OcultarPanel", 3f);
    }

    public void CompletarMision(string id)
    {
        LinkedListNode<Mision> nodo = misionesActivas.First;
        while (nodo != null)
        {
            if (nodo.Value.id == id)
            {
                misionesCompletadas.AddLast(nodo.Value);
                misionesActivas.Remove(nodo);
                break;
            }
            nodo = nodo.Next;
        }
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (textoMisiones == null) return;

        string texto = "=== MISIONES ACTIVAS ===\n";

        if (misionesActivas.Count == 0)
            texto += "No tienes misiones activas\n";
        else
            foreach (Mision m in misionesActivas)
                texto += "• " + m.dialogoInicio + "\n";

        texto += "\n=== COMPLETADAS ===\n";

        if (misionesCompletadas.Count == 0)
            texto += "Ninguna aún\n";
        else
            foreach (Mision m in misionesCompletadas)
                texto += "✓ " + m.id + "\n";

        textoMisiones.text = texto;
    }

    private void OcultarPanel()
    {
        if (panelMisiones != null)
            panelMisiones.SetActive(false);
    }
}