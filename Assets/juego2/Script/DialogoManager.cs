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

        panelDialogo.SetActive(false);
    }

    public void MostrarMensaje(string mensaje, Vector3 posicionNPC)
    {
        if (coroutineActual != null)
            StopCoroutine(coroutineActual);

        coroutineActual = StartCoroutine(MostrarYOcultar(mensaje));
    }

    private IEnumerator MostrarYOcultar(string mensaje)
    {
        textoDialogo.text = mensaje;
        panelDialogo.SetActive(true);

        yield return new WaitForSeconds(tiempoVisible);

        panelDialogo.SetActive(false);
    }
}