using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MinMaxFloat
{
    public float min;
    public float max;

    // You can also add a constructor if you'd like
    public MinMaxFloat(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}

public class MinMaxExample : MonoBehaviour
{
    [MinMaxSlider(0f, 100f)] public Vector2 range = new Vector2(10f, 50f); // Default values

    [MinMaxSlider(0f, 100f)]
    public MinMaxFloat minMaxFloat = new MinMaxFloat(0f, 100f);

    //  [Range(0f,100f)]
    // public float minMaxSlider = 50f;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}