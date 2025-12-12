using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{

    public TextMeshProUGUI FPSCounterText;
    public TextMeshProUGUI FTCounterText;

    private int counter = 10;
    void Update()
    {
        if (counter <= 0)
        {
            FPSCounterText.text = $"{Mathf.RoundToInt(1 / Time.deltaTime)}FPS";
            FTCounterText.text = $"{Time.deltaTime}ms";
            counter = 10;
        }
        else
        {
            counter--;
        }
    }
}
