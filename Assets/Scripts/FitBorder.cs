using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitBorder : MonoBehaviour
{
    [SerializeField] private GameObject LeftBorder;
    [SerializeField] private GameObject RightBorder;

    private float Ratio;

    private void Start()
    {
        Ratio = (float)Screen.width / (float)Screen.height;

        LeftBorder.transform.position = new Vector3(-5 * Ratio - 0.5f, 0, 0);
        RightBorder.transform.position = new Vector3(5 * Ratio + 0.5f, 0, 0);
    }
}
