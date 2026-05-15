using System.Collections.Generic;
using UnityEngine;

public class ParallaxVentanaSuave : MonoBehaviour
{
    [System.Serializable]
    public class CapaParallax
    {
        public Transform capa;
        public float fuerzaX = 0.01f;
        public float fuerzaY = 0.005f;

        [HideInInspector] public Vector3 posicionInicial;
    }

    public Transform camara;
    public float suavidad = 6f;

    public List<CapaParallax> capas = new List<CapaParallax>();

    Vector3 posicionInicialCamara;

    void Start()
    {
        if (camara == null && Camera.main != null)
        {
            camara = Camera.main.transform;
        }

        if (camara == null)
        {
            Debug.LogWarning("No se asignó la cámara al parallax de ventana.");
            return;
        }

        posicionInicialCamara = camara.position;

        foreach (CapaParallax capaParallax in capas)
        {
            if (capaParallax.capa != null)
            {
                capaParallax.posicionInicial = capaParallax.capa.localPosition;
            }
        }
    }

    void LateUpdate()
    {
        if (camara == null) return;

        Vector3 diferencia = camara.position - posicionInicialCamara;

        foreach (CapaParallax capaParallax in capas)
        {
            if (capaParallax.capa == null) continue;

            Vector3 posicionObjetivo = capaParallax.posicionInicial + new Vector3(
                -diferencia.x * capaParallax.fuerzaX,
                -diferencia.y * capaParallax.fuerzaY,
                0
            );

            capaParallax.capa.localPosition = Vector3.Lerp(
                capaParallax.capa.localPosition,
                posicionObjetivo,
                Time.deltaTime * suavidad
            );
        }
    }
}