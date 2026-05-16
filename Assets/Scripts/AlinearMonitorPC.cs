using UnityEngine;

[ExecuteAlways]
public class AlinearMonitorPC : MonoBehaviour
{
    [Header("Referencias")]
    public Transform pantallaMonitor;
    public RectTransform canvasPC;
    public Camera camaraEventos;

    [Header("Tamaño de la pantalla")]
    public float anchoPantalla = 1.65f;
    public float altoPantalla = 0.72f;
    public float separacionCanvas = 0.01f;

    [Header("Posición local dentro del Monitor_3D")]
    public Vector3 posicionLocalPantalla = new Vector3(0f, 0.35f, -0.04f);
    public Vector3 rotacionLocalPantalla = new Vector3(0f, 0f, 0f);

    [Header("Ajustes")]
    public bool invertirCanvas = false;
    public bool actualizarEnEditor = true;

    void Reset()
    {
        camaraEventos = Camera.main;
    }

    void OnValidate()
    {
        if (!Application.isPlaying && actualizarEnEditor)
        {
            Alinear();
        }
    }

    void Update()
    {
        if (!Application.isPlaying && actualizarEnEditor)
        {
            Alinear();
        }
    }

    [ContextMenu("Alinear ahora")]
    public void Alinear()
    {
        if (pantallaMonitor != null)
        {
            pantallaMonitor.SetParent(transform, false);
            pantallaMonitor.localPosition = posicionLocalPantalla;
            pantallaMonitor.localEulerAngles = rotacionLocalPantalla;
            pantallaMonitor.localScale = new Vector3(anchoPantalla, altoPantalla, 1f);
        }

        if (canvasPC != null)
        {
            Canvas canvas = canvasPC.GetComponent<Canvas>();

            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = camaraEventos != null ? camaraEventos : Camera.main;
            }

            canvasPC.SetParent(transform, false);

            Quaternion rotacion = Quaternion.Euler(rotacionLocalPantalla);
            Vector3 frente = rotacion * Vector3.back;

            canvasPC.localPosition = posicionLocalPantalla + frente * separacionCanvas;

            Vector3 rotacionCanvas = rotacionLocalPantalla;

            if (invertirCanvas)
            {
                rotacionCanvas += new Vector3(0f, 180f, 0f);
            }

            canvasPC.localEulerAngles = rotacionCanvas;

            canvasPC.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1280f);
            canvasPC.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 720f);

            float escalaX = anchoPantalla / 1280f;
            float escalaY = altoPantalla / 720f;
            float escalaFinal = Mathf.Min(escalaX, escalaY);

            canvasPC.localScale = Vector3.one * escalaFinal;
        }
    }
}