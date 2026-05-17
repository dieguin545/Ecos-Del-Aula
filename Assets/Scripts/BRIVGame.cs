using UnityEngine;

// TAD que representa un juego dentro de la plataforma BRIV.
// Es Serializable para poder asignarlo desde el inspector de Unity.
[System.Serializable]
public class BRIVGame
{
    public string id;
    public string displayName;
    [TextArea(2, 4)] public string descripcion;
    public string sceneName;
    public Sprite miniatura;
    public Color accentColor = new Color(0f, 0.851f, 1f, 1f);
    public bool esJuegoPrincipal;
    public string etiqueta;

    public BRIVGame() { }

    public BRIVGame(string id, string displayName, string sceneName)
    {
        this.id = id;
        this.displayName = displayName;
        this.sceneName = sceneName;
    }

    public string TextoEtiqueta()
    {
        if (esJuegoPrincipal) return "PRINCIPAL";
        if (!string.IsNullOrEmpty(etiqueta)) return etiqueta;
        return "MINIJUEGO";
    }
}
