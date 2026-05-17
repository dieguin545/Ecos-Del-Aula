using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// OBSERVER PATTERN - Interface del Observer
public interface IAnxietyObserver
{
    void OnAnxietyChanged(float currentAnxiety, float maxAnxiety);
}

// OBSERVER PATTERN - Subject que notifica a los observers
public class AnxietySystem : MonoBehaviour
{
    public static AnxietySystem Instance;

    [Header("Configuracion")]
    public float maxAnxiety = 100f;
    private float currentAnxiety = 0f;

    // Lista enlazada de observers - ESTRUCTURA DE DATOS
    private LinkedList<IAnxietyObserver> observers = new LinkedList<IAnxietyObserver>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Registrar observer
    public void AddObserver(IAnxietyObserver observer)
    {
        observers.AddLast(observer);
    }

    // Remover observer
    public void RemoveObserver(IAnxietyObserver observer)
    {
        observers.Remove(observer);
    }

    // Notificar a todos los observers
    private void NotifyObservers()
    {
        foreach (IAnxietyObserver observer in observers)
        {
            observer.OnAnxietyChanged(currentAnxiety, maxAnxiety);
        }
    }

    // Subir ansiedad
    public void IncreaseAnxiety(float amount)
    {
        currentAnxiety = Mathf.Clamp(currentAnxiety + amount, 0, maxAnxiety);
        NotifyObservers();

        if (currentAnxiety >= maxAnxiety)
        {
            TriggerAnxietyAttack();
        }
    }

    // Bajar ansiedad
    public void DecreaseAnxiety(float amount)
    {
        currentAnxiety = Mathf.Clamp(currentAnxiety - amount, 0, maxAnxiety);
        NotifyObservers();
    }

    public float GetCurrentAnxiety() => currentAnxiety;

    private void TriggerAnxietyAttack()
    {
        Debug.Log("ATAQUE DE ANSIEDAD");
        // Aqui despues ponemos la logica del ataque
    }
}
