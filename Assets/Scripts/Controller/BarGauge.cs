using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarGauge : MonoBehaviour
{
    [SerializeField] private Image barImage;
    private float percentage;

    public void UpdateBar(float percentage)
    {
        barImage.fillAmount = percentage;
    }
}
