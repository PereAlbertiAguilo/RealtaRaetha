using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class Sensitivity : MonoBehaviour
{
    [SerializeField] Slider snesX;
    [SerializeField] Slider snesY;

    [SerializeField] float defaultSensX;
    [SerializeField] float defaultSensY;

    float sensX = 0;
    float sensY = 0;

    public CinemachineFreeLook cinemachineFreeLook;

    private void Awake()
    {

    }

    private void Start()
    {
        UpdateSliders();
    }

    void UpdateSliders()
    {
        SetSensX(sensX);
        snesX.value = sensX;
        SetSensY(sensY);
        snesY.value = sensY;
    }

    public void SetSensX(float sensX)
    {
        cinemachineFreeLook.m_XAxis.m_MaxSpeed = sensX;

        PlayerPrefs.SetFloat("sensX", sensX);
    }

    public void SetSensY(float SensY)
    {
        cinemachineFreeLook.m_YAxis.m_MaxSpeed = SensY;

        PlayerPrefs.SetFloat("sensY", SensY);
    }
}
