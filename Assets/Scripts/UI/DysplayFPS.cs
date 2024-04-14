using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DysplayFPS : MonoBehaviour
{
    //FPS
    [SerializeField] TextMeshProUGUI fpsText;
    private float[] frameDeltaTimeArray;
    private int lastFrameIndex;

    private void Awake()
    {
        frameDeltaTimeArray = new float[50];
    }

    private float CalculateFPS()
    {
        float total = 0f;
        foreach (float deltaTime in frameDeltaTimeArray)
        {
            total += deltaTime;
        }
        return frameDeltaTimeArray.Length / total;
    }

    private void Update()
    {
        frameDeltaTimeArray[lastFrameIndex] = Time.deltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;

        fpsText.text = Mathf.RoundToInt(CalculateFPS()) + " FPS";
    }
}
