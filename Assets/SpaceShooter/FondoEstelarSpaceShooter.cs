using System.Collections.Generic;
using UnityEngine;

public class FondoEstelarSpaceShooter : MonoBehaviour
{
    private class Estrella
    {
        public Transform transform;
    }

    private class NodoDigital
    {
        public Transform transform;
    }

    [Header("Fondo")]
    [SerializeField] private Color colorFondo = new Color(0.01f, 0.015f, 0.055f);
    [SerializeField, Range(0.1f, 2f)] private float intensidadFondo = 1f;
    [SerializeField] private int estrellasLejanas = 56;
    [SerializeField] private int estrellasMedias = 36;
    [SerializeField] private int estrellasCercanas = 22;
    [SerializeField] private int nodosDigitales = 24;
    [SerializeField] private float ancho = 160f;
    [SerializeField] private float alto = 110f;
    [SerializeField] private float profundidad = 170f;
    [SerializeField] private float distanciaMinima = 12f;
    [SerializeField] private bool fondoDigitalActivo = true;
    [SerializeField] private bool reducirEfectosLocal;

    [Header("Capas opcionales")]
    [SerializeField] private Texture2D fondoPrincipal;
    [SerializeField] private Texture2D fondoSecundario;
    [SerializeField] private Vector2 velocidadParallax = new Vector2(0.004f, 0.0025f);

    private readonly List<Estrella> estrellas = new List<Estrella>();
    private readonly List<NodoDigital> nodos = new List<NodoDigital>();
    private readonly List<Renderer> capasFondo = new List<Renderer>();
    private Transform raiz;
    private Transform referencia;

    private void Start()
    {
        Camera camara = Camera.main;

        if (camara == null)
        {
            enabled = false;
            return;
        }

        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = colorFondo;
        NaveController nave = FindAnyObjectByType<NaveController>();
        referencia = nave != null ? nave.transform : camara.transform;

        CrearRaiz();
        CrearCapa(estrellasLejanas, 0.04f, new Color(0.55f, 0.7f, 1f, 0.65f));
        CrearCapa(estrellasMedias, 0.07f, new Color(0.72f, 0.85f, 1f, 0.82f));
        CrearCapa(estrellasCercanas, 0.11f, Color.white);
        CrearNodosDigitales();
        CrearCapasOpcionales();
        AplicarAccesibilidad();
    }

    private void LateUpdate()
    {
        if (referencia == null)
        {
            return;
        }

        for (int i = 0; i < estrellas.Count; i++)
        {
            Estrella estrella = estrellas[i];
            estrella.transform.position = ReubicarSiSaleDelVolumen(estrella.transform.position);
        }

        for (int i = 0; i < nodos.Count; i++)
        {
            NodoDigital nodo = nodos[i];
            nodo.transform.position = ReubicarSiSaleDelVolumen(nodo.transform.position);
        }

        DesplazarCapasOpcionales();
    }

    private void CrearRaiz()
    {
        GameObject objetoRaiz = new GameObject("FondoEstelar");
        raiz = objetoRaiz.transform;
    }

    private void CrearCapa(int cantidad, float escala, Color color)
    {
        Material material = EstiloVisualSpaceShooter.CrearMaterialEstrella(color * intensidadFondo);

        for (int i = 0; i < cantidad; i++)
        {
            GameObject estrella = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            estrella.name = "Estrella";
            estrella.transform.SetParent(raiz, false);
            estrella.transform.position = CrearPosicion();
            estrella.transform.localScale = Vector3.one * escala;

            Collider collider = estrella.GetComponent<Collider>();

            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = estrella.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            estrellas.Add(
                new Estrella
                {
                    transform = estrella.transform
                }
            );
        }
    }

    private Vector3 CrearPosicion()
    {
        Vector3 centro = referencia != null ? referencia.position : Vector3.zero;
        Vector3 posicion;

        do
        {
            posicion =
                centro
                + new Vector3(
                    Random.Range(-ancho * 0.5f, ancho * 0.5f),
                    Random.Range(-alto * 0.5f, alto * 0.5f),
                    Random.Range(-profundidad * 0.5f, profundidad * 0.5f)
                );
        } while ((posicion - centro).sqrMagnitude < distanciaMinima * distanciaMinima);

        return posicion;
    }

    private Vector3 ReubicarSiSaleDelVolumen(Vector3 posicion)
    {
        Vector3 centro = referencia.position;
        Vector3 relativa = posicion - centro;
        float mitadAncho = ancho * 0.5f;
        float mitadAlto = alto * 0.5f;
        float mitadProfundidad = profundidad * 0.5f;

        if (relativa.x > mitadAncho)
        {
            relativa.x = -mitadAncho;
        }
        else if (relativa.x < -mitadAncho)
        {
            relativa.x = mitadAncho;
        }

        if (relativa.y > mitadAlto)
        {
            relativa.y = -mitadAlto;
        }
        else if (relativa.y < -mitadAlto)
        {
            relativa.y = mitadAlto;
        }

        if (relativa.z > mitadProfundidad)
        {
            relativa.z = -mitadProfundidad;
        }
        else if (relativa.z < -mitadProfundidad)
        {
            relativa.z = mitadProfundidad;
        }

        return centro + relativa;
    }

    private void CrearNodosDigitales()
    {
        if (!fondoDigitalActivo)
        {
            return;
        }

        Material material = EstiloVisualSpaceShooter.CrearMaterialEstrella(
            new Color(0.35f, 0.8f, 1f, 0.35f) * intensidadFondo
        );

        for (int i = 0; i < nodosDigitales; i++)
        {
            GameObject nodo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nodo.name = "NodoDigital";
            nodo.transform.SetParent(raiz, false);
            nodo.transform.position = CrearPosicion();
            nodo.transform.localScale = Vector3.one * Random.Range(0.14f, 0.24f);

            Collider collider = nodo.GetComponent<Collider>();

            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = nodo.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            nodos.Add(new NodoDigital { transform = nodo.transform });
        }
    }

    public void AplicarAccesibilidad()
    {
        bool reducirEfectos = reducirEfectosLocal || AccesibilidadSpaceShooter.ReducirEfectosActivo;

        for (int i = 0; i < estrellas.Count; i++)
        {
            if (estrellas[i].transform != null)
            {
                estrellas[i].transform.gameObject.SetActive(!reducirEfectos || i % 2 == 0);
            }
        }

        for (int i = 0; i < nodos.Count; i++)
        {
            if (nodos[i].transform != null)
            {
                nodos[i].transform.gameObject.SetActive(
                    fondoDigitalActivo && !reducirEfectos
                );
            }
        }

        for (int i = 0; i < capasFondo.Count; i++)
        {
            if (capasFondo[i] != null)
            {
                capasFondo[i].gameObject.SetActive(!reducirEfectos);
            }
        }
    }

    private void CrearCapasOpcionales()
    {
        CrearCapaOpcional("FondoPrincipal", fondoPrincipal, 140f, 0.12f);
        CrearCapaOpcional("FondoSecundario", fondoSecundario, 132f, 0.08f);
    }

    private void CrearCapaOpcional(string nombre, Texture2D textura, float distancia, float alpha)
    {
        if (textura == null || referencia == null)
        {
            return;
        }

        Shader shader = Shader.Find("Unlit/Transparent");

        if (shader == null)
        {
            shader = Shader.Find("Unlit/Texture");
        }

        if (shader == null)
        {
            return;
        }

        GameObject capa = GameObject.CreatePrimitive(PrimitiveType.Quad);
        capa.name = nombre;
        capa.transform.SetParent(raiz, false);
        capa.transform.position = referencia.position + Vector3.forward * distancia;
        capa.transform.localScale = new Vector3(ancho * 1.45f, alto * 1.45f, 1f);

        Collider collider = capa.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = capa.GetComponent<Renderer>();

        if (renderer == null)
        {
            return;
        }

        Material material = new Material(shader);
        material.mainTexture = textura;

        if (material.HasProperty("_Color"))
        {
            material.color = new Color(1f, 1f, 1f, alpha * intensidadFondo);
        }

        renderer.sharedMaterial = material;
        capasFondo.Add(renderer);
    }

    private void DesplazarCapasOpcionales()
    {
        for (int i = 0; i < capasFondo.Count; i++)
        {
            Renderer renderer = capasFondo[i];

            if (renderer == null || renderer.sharedMaterial == null)
            {
                continue;
            }

            float factor = 1f + i * 0.55f;
            Vector2 desplazamiento = velocidadParallax * factor * Time.time;
            renderer.sharedMaterial.mainTextureOffset = desplazamiento;
        }
    }
}
