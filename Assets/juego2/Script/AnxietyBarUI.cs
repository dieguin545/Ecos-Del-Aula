using UnityEngine;
using UnityEngine.UI;

// OBSERVER PATTERN - Observer concreto que actualiza la UI
public class AnxietyBarUI : MonoBehaviour, IAnxietyObserver
{
    public Slider slider;

    void Start()
    {
        if (slider == null)
        {
            slider = GetComponentInChildren<Slider>(true);
        }

        if (AnxietySystem.Instance != null)
        {
            AnxietySystem.Instance.AddObserver(this);
            OnAnxietyChanged(AnxietySystem.Instance.GetCurrentAnxiety(), AnxietySystem.Instance.maxAnxiety);
        }
    }

    void OnDestroy()
    {
        if (AnxietySystem.Instance != null)
        {
            AnxietySystem.Instance.RemoveObserver(this);
        }
    }

    public void OnAnxietyChanged(float currentAnxiety, float maxAnxiety)
    {
        if (slider == null)
        {
            return;
        }

        slider.maxValue = maxAnxiety;
        slider.value = Mathf.Clamp(currentAnxiety, 0f, maxAnxiety);
    }
}
