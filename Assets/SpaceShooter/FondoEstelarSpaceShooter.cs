using System.Collections.Generic;
using UnityEngine;

public class FondoEstelarSpaceShooter : MonoBehaviour
{
    private class Estrella
    {
        public Transform transform;
    }

    [Header("Fondo")]
    [SerializeField] private Color colorFondo = new Color(0.005f, 0.01f, 0.03f);
    [SerializeField] private int estrellasLejanas = 42;
    [SerializeField] private int estrellasMedias = 28;
    [SerializeField] private int estrellasCercanas = 16;
    [SerializeField] private float ancho = 120f;
    [SerializeField] private float alto = 80f;
    [SerializeField] private float profundidad = 120f;
    [SerializeField] private float distanciaMinima = 12f;

    private readonly List<Estrella> estrellas = new List<Estrella>();
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
    }

    private void CrearRaiz()
    {
        GameObject objetoRaiz = new GameObject("FondoEstelar");
        raiz = objetoRaiz.transform;
    }

    private void CrearCapa(int cantidad, float escala, Color color)
    {
        Material material = EstiloVisualSpaceShooter.CrearMaterialEstrella(color);

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
}
