using System;
using UnityEngine;

public enum TipoProductoTienda
{
    Cafe,
    Seguro,
    Filtro,
    Teclado
}

public enum CategoriaProductoTienda
{
    Temporal,
    Permanente
}

[Serializable]
public class ProductoTienda
{
    public TipoProductoTienda tipo;
    public string nombre;
    public int precio;
    [TextArea(2, 4)] public string descripcion;
    public CategoriaProductoTienda categoria;
    public Sprite sprite;
}
