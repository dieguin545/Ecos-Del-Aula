using System.Collections;
using System.Collections.Generic;

public class ListaCircular<T> : IEnumerable<T>
{
    private class Nodo
    {
        public T valor;
        public Nodo siguiente;
        public Nodo anterior;

        public Nodo(T valor)
        {
            this.valor = valor;
        }
    }

    private Nodo actual;

    public int Cantidad { get; private set; }
    public bool EstaVacia => Cantidad == 0;
    public T Actual => actual != null ? actual.valor : default;

    public void Agregar(T valor)
    {
        Nodo nuevo = new Nodo(valor);

        if (actual == null)
        {
            actual = nuevo;
            nuevo.siguiente = nuevo;
            nuevo.anterior = nuevo;
        }
        else
        {
            Nodo ultimo = actual.anterior;
            ultimo.siguiente = nuevo;
            nuevo.anterior = ultimo;
            nuevo.siguiente = actual;
            actual.anterior = nuevo;
        }

        Cantidad++;
    }

    public T Siguiente()
    {
        if (actual == null)
        {
            return default;
        }

        actual = actual.siguiente;
        return actual.valor;
    }

    public T Anterior()
    {
        if (actual == null)
        {
            return default;
        }

        actual = actual.anterior;
        return actual.valor;
    }

    public void Limpiar()
    {
        actual = null;
        Cantidad = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (actual == null)
        {
            yield break;
        }

        Nodo nodo = actual;

        for (int i = 0; i < Cantidad; i++)
        {
            yield return nodo.valor;
            nodo = nodo.siguiente;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
