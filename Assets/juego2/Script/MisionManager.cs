using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

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
        {
            Instance = this;
        }

        VidaEscolarHUD.Ensure().ActualizarMisiones(misionesActivas, misionesCompletadas);

        if (panelMisiones != null)
        {
            AplicarEstiloPanel();
            panelMisiones.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.JoystickButton6))
        {
            if (panelMisiones == null)
            {
                return;
            }

            if (panelMisiones.activeSelf)
            {
                panelMisiones.SetActive(false);
            }
            else
            {
                panelMisiones.SetActive(true);
                ActualizarUI();
            }
        }
    }

    public void RegistrarMision(Mision mision)
    {
        if (mision == null)
        {
            return;
        }

        misionesActivas.AddLast(mision);
        ActualizarUI();
        UIAudioManager.PlayNotification();
        VidaEscolarHUD.Ensure().MostrarToast("Nueva tarea agregada");

        if (panelMisiones != null)
        {
            panelMisiones.SetActive(true);
            CancelInvoke(nameof(OcultarPanel));
            Invoke(nameof(OcultarPanel), 3f);
        }
    }

    public void CompletarMision(string id)
    {
        LinkedListNode<Mision> nodo = misionesActivas.First;
        while (nodo != null)
        {
            LinkedListNode<Mision> siguiente = nodo.Next;
            if (nodo.Value != null && nodo.Value.id == id)
            {
                misionesCompletadas.AddLast(nodo.Value);
                misionesActivas.Remove(nodo);
                break;
            }

            nodo = siguiente;
        }

        ActualizarUI();
        UIAudioManager.PlayMissionComplete();
        VidaEscolarHUD.Ensure().MostrarToast("Tarea completada");
    }

    private void ActualizarUI()
    {
        if (textoMisiones == null)
        {
            return;
        }

        AplicarEstiloPanel();

        string texto = "MISIONES ACTIVAS\n";

        if (misionesActivas.Count == 0)
        {
            texto += "No tienes misiones activas\n";
        }
        else
        {
            foreach (Mision m in misionesActivas)
            {
                if (m != null)
                {
                    texto += "• " + m.dialogoInicio + "\n";
                }
            }
        }

        texto += "\nCOMPLETADAS\n";

        if (misionesCompletadas.Count == 0)
        {
            texto += "Ninguna aún\n";
        }
        else
        {
            foreach (Mision m in misionesCompletadas)
            {
                if (m != null)
                {
                    texto += "✓ " + m.id + "\n";
                }
            }
        }

        textoMisiones.text = texto;
        VidaEscolarHUD.Ensure().ActualizarMisiones(misionesActivas, misionesCompletadas);
    }

    private void OcultarPanel()
    {
        if (panelMisiones != null)
        {
            panelMisiones.SetActive(false);
        }
    }

    private void AplicarEstiloPanel()
    {
        if (panelMisiones != null)
        {
            Image fondo = panelMisiones.GetComponent<Image>();
            if (fondo == null)
            {
                fondo = panelMisiones.AddComponent<Image>();
            }

            fondo.color = new Color(0.025f, 0.018f, 0.055f, 0.78f);
            fondo.raycastTarget = false;

            Outline outline = panelMisiones.GetComponent<Outline>();
            if (outline == null)
            {
                outline = panelMisiones.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0.25f, 0.88f, 1f, 0.55f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        if (textoMisiones != null)
        {
            textoMisiones.color = new Color(0.92f, 0.98f, 1f, 1f);
            textoMisiones.fontSize = Mathf.Max(textoMisiones.fontSize, 18f);
        }
    }
}
