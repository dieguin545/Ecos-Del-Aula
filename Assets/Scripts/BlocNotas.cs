using UnityEngine;
using UnityEngine.UI;

public class BlocNotas : MonoBehaviour
{
    public GameObject panel;
    public InputField campoNotas;

    private const string SAVE_KEY = "briv_notas";

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Abrir()
    {
        if (panel == null) return;
        bool estaAbierto = panel.activeSelf;
        panel.SetActive(!estaAbierto);
        if (panel.activeSelf && campoNotas != null)
            campoNotas.text = PlayerPrefs.GetString(SAVE_KEY, "");
    }

    public void Cerrar()
    {
        Guardar();
        if (panel != null) panel.SetActive(false);
    }

    public void Guardar()
    {
        if (campoNotas != null)
        {
            PlayerPrefs.SetString(SAVE_KEY, campoNotas.text);
            PlayerPrefs.Save();
        }
    }
}
