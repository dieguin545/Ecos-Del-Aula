using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppTienda : MonoBehaviour
{
    public ControlCorreo controlCorreo;

    [Header("UI existente")]
    public Text textoDinero;
    public Text textoMensaje;
    public Button botonCafe;
    public Button botonSeguro;
    public Button botonFiltro;
    public Button botonTeclado;

    [Header("Sprites futuros")]
    [SerializeField] private Sprite spriteCafe;
    [SerializeField] private Sprite spriteSeguro;
    [SerializeField] private Sprite spriteFiltro;
    [SerializeField] private Sprite spriteTeclado;

    [Header("Catalogo")]
    [SerializeField] private List<ProductoTienda> productos = new List<ProductoTienda>();

    private readonly Dictionary<TipoProductoTienda, ProductoTienda> productosPorTipo =
        new Dictionary<TipoProductoTienda, ProductoTienda>();
    private readonly Dictionary<TipoProductoTienda, Button> botonesCompra =
        new Dictionary<TipoProductoTienda, Button>();

    private bool uiPreparada;

    private void Awake()
    {
        CargarSpritesSiHaceFalta();
        PrepararCatalogoSiHaceFalta();
        NormalizarCatalogoBase();
        ReconstruirIndiceProductos();
    }

    private void OnEnable()
    {
        PrepararUiSiHaceFalta();
        MostrarMensaje(string.Empty);
        ActualizarUI();
    }

    public void ComprarCafe()
    {
        Comprar(TipoProductoTienda.Cafe);
    }

    public void ComprarSeguro()
    {
        Comprar(TipoProductoTienda.Seguro);
    }

    public void ComprarFiltro()
    {
        Comprar(TipoProductoTienda.Filtro);
    }

    public void ComprarTeclado()
    {
        Comprar(TipoProductoTienda.Teclado);
    }

    private void Comprar(TipoProductoTienda tipo)
    {
        if (controlCorreo == null || !productosPorTipo.TryGetValue(tipo, out ProductoTienda producto))
        {
            return;
        }

        if (EsProductoPermanenteYaComprado(tipo))
        {
            MostrarMensaje("Ya compraste " + producto.nombre + ".");
            ActualizarUI();
            return;
        }

        if (controlCorreo.dineroTotal < producto.precio)
        {
            MostrarMensaje("No tienes suficiente dinero para " + producto.nombre + ".");
            ActualizarUI();
            return;
        }

        controlCorreo.dineroTotal -= producto.precio;
        AplicarEfecto(tipo);
        controlCorreo.GuardarProgresoActual();
        MostrarMensaje(ObtenerFeedbackCompra(tipo, producto.nombre));
        ActualizarUI();
    }

    private void AplicarEfecto(TipoProductoTienda tipo)
    {
        switch (tipo)
        {
            case TipoProductoTienda.Cafe:
                controlCorreo.tieneCafe = true;
                break;
            case TipoProductoTienda.Seguro:
                controlCorreo.tieneSeguro = true;
                controlCorreo.seguroUsado = false;
                break;
            case TipoProductoTienda.Filtro:
                controlCorreo.tieneFiltroSpam = true;
                break;
            case TipoProductoTienda.Teclado:
                controlCorreo.tieneTeclado = true;
                break;
        }
    }

    private string ObtenerFeedbackCompra(TipoProductoTienda tipo, string nombre)
    {
        switch (tipo)
        {
            case TipoProductoTienda.Cafe:
                return "Café usado: tendrás energía para un correo extra.";
            case TipoProductoTienda.Seguro:
                return "Seguro activo: evita la penalización del primer error.";
            case TipoProductoTienda.Filtro:
                return "Filtro activo: resalta señales sospechosas.";
            case TipoProductoTienda.Teclado:
                return "Teclado activo: mejoras tu eficiencia diaria.";
            default:
                return "Compraste " + nombre + ".";
        }
    }

    private bool EsProductoPermanenteYaComprado(TipoProductoTienda tipo)
    {
        switch (tipo)
        {
            case TipoProductoTienda.Filtro:
                return controlCorreo != null && controlCorreo.tieneFiltroSpam;
            case TipoProductoTienda.Teclado:
                return controlCorreo != null && controlCorreo.tieneTeclado;
            default:
                return false;
        }
    }

    private void ActualizarUI()
    {
        if (textoDinero != null && controlCorreo != null)
        {
            textoDinero.text = "Dinero: $" + controlCorreo.dineroTotal;
        }

        foreach (KeyValuePair<TipoProductoTienda, Button> par in botonesCompra)
        {
            if (par.Value == null)
            {
                continue;
            }

            bool permanenteComprado = EsProductoPermanenteYaComprado(par.Key);
            par.Value.interactable = !permanenteComprado;
            TMP_Text textoBoton = par.Value.GetComponentInChildren<TMP_Text>(true);

            if (textoBoton != null)
            {
                textoBoton.text = permanenteComprado ? "Comprado" : "Comprar";
            }
        }

        ActualizarBotonesLegacy();
    }

    private void ActualizarBotonesLegacy()
    {
        if (botonFiltro != null)
        {
            botonFiltro.interactable = !EsProductoPermanenteYaComprado(TipoProductoTienda.Filtro);
        }

        if (botonTeclado != null)
        {
            botonTeclado.interactable = !EsProductoPermanenteYaComprado(TipoProductoTienda.Teclado);
        }
    }

    private void PrepararCatalogoSiHaceFalta()
    {
        if (productos != null && productos.Count > 0)
        {
            return;
        }

        productos = new List<ProductoTienda>
        {
            CrearProducto(
                TipoProductoTienda.Cafe,
                "Café",
                250,
                "Aumenta tu energía temporalmente.",
                CategoriaProductoTienda.Temporal,
                spriteCafe
            ),
            CrearProducto(
                TipoProductoTienda.Seguro,
                "Seguro",
                300,
                "Te protege de un error.",
                CategoriaProductoTienda.Temporal,
                spriteSeguro
            ),
            CrearProducto(
                TipoProductoTienda.Filtro,
                "Filtro",
                400,
                "Mejora la detección de correos sospechosos.",
                CategoriaProductoTienda.Permanente,
                spriteFiltro
            ),
            CrearProducto(
                TipoProductoTienda.Teclado,
                "Teclado",
                350,
                "Aumenta tu eficiencia al responder.",
                CategoriaProductoTienda.Permanente,
                spriteTeclado
            )
        };
    }

    private void CargarSpritesSiHaceFalta()
    {
        if (spriteCafe == null)
        {
            spriteCafe = RecursosVisualesEntryFilter.CargarSpriteEditor("item_cafe.png");
        }

        if (spriteSeguro == null)
        {
            spriteSeguro = RecursosVisualesEntryFilter.CargarSpriteEditor("item_seguro.png");
        }

        if (spriteFiltro == null)
        {
            spriteFiltro = RecursosVisualesEntryFilter.CargarSpriteEditor("item_filtro.png");
        }

        if (spriteTeclado == null)
        {
            spriteTeclado = RecursosVisualesEntryFilter.CargarSpriteEditor("item_teclado.png");
        }
    }

    private void NormalizarCatalogoBase()
    {
        ReemplazarOAgregarProducto(
            TipoProductoTienda.Cafe,
            "Café",
            250,
            "Recupera energía para revisar un correo extra.",
            CategoriaProductoTienda.Temporal,
            spriteCafe
        );
        ReemplazarOAgregarProducto(
            TipoProductoTienda.Seguro,
            "Seguro",
            300,
            "Evita la penalización del primer error del día.",
            CategoriaProductoTienda.Temporal,
            spriteSeguro
        );
        ReemplazarOAgregarProducto(
            TipoProductoTienda.Filtro,
            "Filtro",
            400,
            "Resalta señales sospechosas sin darte la respuesta.",
            CategoriaProductoTienda.Permanente,
            spriteFiltro
        );
        ReemplazarOAgregarProducto(
            TipoProductoTienda.Teclado,
            "Teclado",
            350,
            "Mejora tu eficiencia: procesas un correo extra.",
            CategoriaProductoTienda.Permanente,
            spriteTeclado
        );
    }

    private void ReemplazarOAgregarProducto(
        TipoProductoTienda tipo,
        string nombre,
        int precio,
        string descripcion,
        CategoriaProductoTienda categoria,
        Sprite sprite
    )
    {
        if (productos == null)
        {
            productos = new List<ProductoTienda>();
        }

        ProductoTienda producto = productos.Find(item => item != null && item.tipo == tipo);

        if (producto == null)
        {
            producto = new ProductoTienda();
            productos.Add(producto);
        }

        producto.tipo = tipo;
        producto.nombre = nombre;
        producto.precio = precio;
        producto.descripcion = descripcion;
        producto.categoria = categoria;
        producto.sprite = sprite;
    }

    private ProductoTienda CrearProducto(
        TipoProductoTienda tipo,
        string nombre,
        int precio,
        string descripcion,
        CategoriaProductoTienda categoria,
        Sprite sprite
    )
    {
        return new ProductoTienda
        {
            tipo = tipo,
            nombre = nombre,
            precio = precio,
            descripcion = descripcion,
            categoria = categoria,
            sprite = sprite
        };
    }

    private void ReconstruirIndiceProductos()
    {
        productosPorTipo.Clear();

        for (int i = 0; i < productos.Count; i++)
        {
            if (productos[i] != null)
            {
                productosPorTipo[productos[i].tipo] = productos[i];
            }
        }
    }

    private void PrepararUiSiHaceFalta()
    {
        if (uiPreparada)
        {
            return;
        }

        AsegurarReferenciasTexto();
        Image fondo = GetComponent<Image>();
        EstiloUIJuego.AplicarPanel(fondo, EstiloUIJuego.FondoPrincipal);
        AjustarVentana();
        DesactivarVisualesLegacyTienda();
        PrepararBotonCerrar();

        DesactivarBotonesLegacy();
        EstiloUIJuego.AplicarTexto(textoDinero, 24, EstiloUIJuego.TextoPrincipal);
        EstiloUIJuego.AplicarTexto(textoMensaje, 20, EstiloUIJuego.TextoSecundario);
        AcomodarTextoSuperior(textoDinero, new Vector2(-220f, 155f), new Vector2(220f, 34f));
        AcomodarTextoSuperior(textoMensaje, new Vector2(0f, -180f), new Vector2(440f, 28f));

        Transform contenedorExistente = transform.Find("ContenidoTienda");
        GameObject contenedor = contenedorExistente != null
            ? contenedorExistente.gameObject
            : new GameObject("ContenidoTienda", typeof(RectTransform));

        if (contenedor.transform.parent != transform)
        {
            contenedor.transform.SetParent(transform, false);
        }

        LimpiarHijos(contenedor.transform);

        RectTransform rectContenedor = contenedor.GetComponent<RectTransform>();
        rectContenedor.anchorMin = new Vector2(0.5f, 0.5f);
        rectContenedor.anchorMax = new Vector2(0.5f, 0.5f);
        rectContenedor.pivot = new Vector2(0.5f, 0.5f);
        rectContenedor.sizeDelta = new Vector2(760f, 390f);
        rectContenedor.anchoredPosition = new Vector2(0f, -8f);

        EstiloUIJuego.CrearTextoTMP(
            contenedor.transform,
            "TituloTienda",
            "Tienda Virtual",
            30f,
            new Vector2(0f, 170f),
            new Vector2(540f, 42f),
            TextAlignmentOptions.Center
        );
        EstiloUIJuego.CrearTextoTMP(
            contenedor.transform,
            "SeccionTemporales",
            "Temporales",
            22f,
            new Vector2(-185f, 118f),
            new Vector2(220f, 30f),
            TextAlignmentOptions.Center
        );
        EstiloUIJuego.CrearTextoTMP(
            contenedor.transform,
            "SeccionPermanentes",
            "Permanentes",
            22f,
            new Vector2(185f, 118f),
            new Vector2(220f, 30f),
            TextAlignmentOptions.Center
        );

        CrearTarjetaProducto(contenedor.transform, productosPorTipo[TipoProductoTienda.Cafe], new Vector2(-185f, 42f));
        CrearTarjetaProducto(contenedor.transform, productosPorTipo[TipoProductoTienda.Seguro], new Vector2(-185f, -98f));
        CrearTarjetaProducto(contenedor.transform, productosPorTipo[TipoProductoTienda.Filtro], new Vector2(185f, 42f));
        CrearTarjetaProducto(contenedor.transform, productosPorTipo[TipoProductoTienda.Teclado], new Vector2(185f, -98f));

        uiPreparada = true;
    }

    private void AjustarVentana()
    {
        RectTransform rect = GetComponent<RectTransform>();

        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 12f);
        rect.sizeDelta = new Vector2(800f, 430f);
    }

    private void PrepararBotonCerrar()
    {
        Transform existente = transform.Find("CerrarTienda");

        if (existente == null)
        {
            GameObject objeto = new GameObject(
                "CerrarTienda",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button)
            );
            objeto.transform.SetParent(transform, false);
            existente = objeto.transform;

            EstiloUIJuego.CrearTextoTMP(
                existente,
                "Texto",
                "X",
                18f,
                Vector2.zero,
                new Vector2(38f, 38f),
                TextAlignmentOptions.Center
            );
        }

        Button botonCerrar = existente.GetComponent<Button>();
        RectTransform rect = existente.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-24f, -24f);
            rect.sizeDelta = new Vector2(38f, 38f);
        }

        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveAllListeners();
            botonCerrar.onClick.AddListener(CerrarVentanaActual);
            EstiloUIJuego.AplicarBoton(
                botonCerrar,
                new Color(0.62f, 0.16f, 0.22f, 1f),
                new Color(0.82f, 0.22f, 0.28f, 1f)
            );
        }
    }

    private void CerrarVentanaActual()
    {
        GestorVentanasPC gestorVentanas = FindAnyObjectByType<GestorVentanasPC>();

        if (gestorVentanas != null)
        {
            gestorVentanas.CerrarVentana(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void AsegurarReferenciasTexto()
    {
        if (textoDinero == null)
        {
            textoDinero = BuscarTextoLegacy("dinero");
        }

        if (textoMensaje == null)
        {
            textoMensaje = BuscarTextoLegacy("mensaje");
        }
    }

    private Text BuscarTextoLegacy(string fragmentoNombre)
    {
        Text[] textos = GetComponentsInChildren<Text>(true);

        for (int i = 0; i < textos.Length; i++)
        {
            if (textos[i] != null && textos[i].name.ToLowerInvariant().Contains(fragmentoNombre))
            {
                return textos[i];
            }
        }

        return null;
    }

    private void AcomodarTextoSuperior(Text texto, Vector2 posicion, Vector2 tamano)
    {
        if (texto == null)
        {
            return;
        }

        RectTransform rect = texto.GetComponent<RectTransform>();

        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;
    }

    private void CrearTarjetaProducto(Transform padre, ProductoTienda producto, Vector2 posicion)
    {
        Image tarjeta = EstiloUIJuego.CrearImagen(
            padre,
            "Tarjeta_" + producto.tipo,
            posicion,
            new Vector2(330f, 118f),
            EstiloUIJuego.FondoTarjeta
        );

        Image icono = EstiloUIJuego.CrearImagen(
            tarjeta.transform,
            ObtenerNombreIcono(producto.tipo),
            new Vector2(-126f, 0f),
            new Vector2(58f, 58f),
            new Color(0.13f, 0.24f, 0.42f, 1f)
        );
        icono.sprite = producto.sprite;
        icono.preserveAspect = producto.sprite != null;
        icono.raycastTarget = false;
        icono.color = producto.sprite != null
            ? Color.white
            : new Color(0.13f, 0.24f, 0.42f, 1f);

        if (producto.sprite == null)
        {
            EstiloUIJuego.CrearTextoTMP(
                icono.transform,
                "Inicial",
                producto.nombre.Substring(0, 1),
                28f,
                Vector2.zero,
                new Vector2(58f, 58f),
                TextAlignmentOptions.Center
            );
        }

        EstiloUIJuego.CrearTextoTMP(
            tarjeta.transform,
            "Nombre",
            producto.nombre,
            22f,
            new Vector2(8f, 32f),
            new Vector2(180f, 28f),
            TextAlignmentOptions.Left
        );
        EstiloUIJuego.CrearTextoTMP(
            tarjeta.transform,
            "Precio",
            "$" + producto.precio,
            20f,
            new Vector2(112f, 32f),
            new Vector2(70f, 28f),
            TextAlignmentOptions.Right
        );
        EstiloUIJuego.CrearTextoTMP(
            tarjeta.transform,
            "Descripcion",
            producto.descripcion,
            16f,
            new Vector2(25f, -2f),
            new Vector2(210f, 42f),
            TextAlignmentOptions.Left
        );

        Button boton = CrearBotonCompra(tarjeta.transform, producto.tipo);
        botonesCompra[producto.tipo] = boton;
    }

    private string ObtenerNombreIcono(TipoProductoTienda tipo)
    {
        switch (tipo)
        {
            case TipoProductoTienda.Cafe:
                return "ImagenCafe";
            case TipoProductoTienda.Seguro:
                return "ImagenSeguro";
            case TipoProductoTienda.Filtro:
                return "ImagenFiltro";
            case TipoProductoTienda.Teclado:
                return "ImagenTeclado";
            default:
                return "ImagenProducto";
        }
    }

    private Button CrearBotonCompra(Transform padre, TipoProductoTienda tipo)
    {
        GameObject objeto = new GameObject(
            "Comprar_" + tipo,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(102f, -34f);
        rect.sizeDelta = new Vector2(108f, 30f);

        Button boton = objeto.GetComponent<Button>();
        boton.onClick.AddListener(() => Comprar(tipo));
        EstiloUIJuego.AplicarBoton(
            boton,
            new Color(0.12f, 0.42f, 0.58f, 1f),
            new Color(0.18f, 0.62f, 0.78f, 1f)
        );

        EstiloUIJuego.CrearTextoTMP(
            objeto.transform,
            "Texto",
            "Comprar",
            16f,
            Vector2.zero,
            new Vector2(104f, 28f),
            TextAlignmentOptions.Center
        );

        return boton;
    }

    private void DesactivarBotonesLegacy()
    {
        if (botonCafe != null) botonCafe.gameObject.SetActive(false);
        if (botonSeguro != null) botonSeguro.gameObject.SetActive(false);
        if (botonFiltro != null) botonFiltro.gameObject.SetActive(false);
        if (botonTeclado != null) botonTeclado.gameObject.SetActive(false);
    }

    private void DesactivarVisualesLegacyTienda()
    {
        Transform contenido = transform.Find("ContenidoTienda");
        Transform cerrar = transform.Find("CerrarTienda");

        Text[] textosLegacy = GetComponentsInChildren<Text>(true);

        for (int i = 0; i < textosLegacy.Length; i++)
        {
            Text texto = textosLegacy[i];

            if (texto == null || texto == textoDinero || texto == textoMensaje)
            {
                continue;
            }

            if (contenido != null && texto.transform.IsChildOf(contenido))
            {
                continue;
            }

            if (cerrar != null && texto.transform.IsChildOf(cerrar))
            {
                continue;
            }

            texto.gameObject.SetActive(false);
        }

        TextMeshProUGUI[] textosTmp = GetComponentsInChildren<TextMeshProUGUI>(true);

        for (int i = 0; i < textosTmp.Length; i++)
        {
            TextMeshProUGUI texto = textosTmp[i];

            if (texto == null)
            {
                continue;
            }

            if (contenido != null && texto.transform.IsChildOf(contenido))
            {
                continue;
            }

            if (cerrar != null && texto.transform.IsChildOf(cerrar))
            {
                continue;
            }

            texto.gameObject.SetActive(false);
        }
    }

    private void LimpiarHijos(Transform padre)
    {
        for (int i = padre.childCount - 1; i >= 0; i--)
        {
            GameObject hijo = padre.GetChild(i).gameObject;

            if (Application.isPlaying)
            {
                Destroy(hijo);
            }
            else
            {
                DestroyImmediate(hijo);
            }
        }
    }

    private void MostrarMensaje(string mensaje)
    {
        if (textoMensaje != null)
        {
            textoMensaje.text = mensaje;
            textoMensaje.gameObject.SetActive(!string.IsNullOrWhiteSpace(mensaje));
        }
    }
}
