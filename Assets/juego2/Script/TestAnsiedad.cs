using UnityEngine;

public class TestAnsiedad : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Llamando IncreaseAnxiety...");
            AnxietySystem.Instance.IncreaseAnxiety(25f);
        }
    }
}
