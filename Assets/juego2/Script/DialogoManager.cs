using UnityEngine;
using TMPro;
using System.Collections;

public class DialogoManager : MonoBehaviour
{
    public static DialogoManager Instance;

    [Header("UI")]
    public GameObject panelDialogo;
    public TextMeshProUGUI textoDialogo;

    [Header("Configuracion")]
    public float tiempoVisible = 3f;

    private Coroutine coroutineActual;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        if (panelDialogo != null)
        {
            panelDialogo.SetActive(false);
        }
    }

    public void MostrarMensaje(string mensaje, Vector3 posicionNPC)
    {
        if (coroutineActual != null)
            StopCoroutine(coroutineActual);

        coroutineActual = StartCoroutine(MostrarYOcultar(mensaje));
    }

    private IEnumerator MostrarYOcultar(string mensaje)
    {
        if (panelDialogo == null || textoDialogo == null)
        {
            Debug.LogWarning("DialogoManager no tiene referencias UI asignadas.");
            yield break;
        }

        textoDialogo.text = mensaje;
        panelDialogo.SetActive(true);

        yield return new WaitForSeconds(tiempoVisible);

        panelDialogo.SetActive(false);
    }
}
