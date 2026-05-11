using UnityEngine;
using UnityEngine.UI;

// OBSERVER PATTERN - Observer concreto que actualiza la UI
public class AnxietyBarUI : MonoBehaviour, IAnxietyObserver
{
    public Slider slider;

    void Start()
    {
        AnxietySystem.Instance.AddObserver(this);
    }

    void OnDestroy()
    {
        AnxietySystem.Instance.RemoveObserver(this);
    }

    public void OnAnxietyChanged(float currentAnxiety, float maxAnxiety)
    {
        slider.value = currentAnxiety;
        slider.maxValue = maxAnxiety;
    }
}