using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    public int vidas = 3;
    public int vidasMaximas = 3;
    public bool juegoActivo = true;

    public Image[] iconosVida;
    public GameObject panelGameOver;

    void Awake() { instancia = this; }

    void Start()
    {
        ActualizarUI();
        panelGameOver.SetActive(false);
    }

    public void PerderVida()
    {
        if (!juegoActivo) return;
        vidas--;
        ActualizarUI();
        if (vidas <= 0) GameOver();
    }

    public void GanarVida()
    {
        if (vidas < vidasMaximas)
        {
            vidas++;
            ActualizarUI();
        }
    }

    void ActualizarUI()
    {
        for (int i = 0; i < iconosVida.Length; i++)
            iconosVida[i].enabled = (i < vidas);
    }

    void GameOver()
    {
        juegoActivo = false;
        panelGameOver.SetActive(true);
    }

    public void Reiniciar()
    {
        vidas = vidasMaximas;
        juegoActivo = true;
        panelGameOver.SetActive(false);
        ActualizarUI();

        foreach (var m in FindObjectsByType<Meteorito>(FindObjectsSortMode.None))
            Destroy(m.gameObject);
        foreach (var c in FindObjectsByType<Corazon>(FindObjectsSortMode.None))
            Destroy(c.gameObject);
        foreach (var b in FindObjectsByType<Bala>(FindObjectsSortMode.None))
            Destroy(b.gameObject);
    }
}