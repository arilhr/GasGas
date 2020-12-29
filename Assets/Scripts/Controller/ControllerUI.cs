using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerUI : MonoBehaviour
{
    public static GameObject controller;

    public GameObject controllerUI = null;

    private void Awake()
    {
        controller = controllerUI;
    }

    public static void EnabledController()
    {
        controller.SetActive(true);
    }
}
