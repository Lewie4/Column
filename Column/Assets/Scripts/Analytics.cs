using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Analytics : MonoBehaviour
{
    void Awake()
    {
        Amplitude amplitude = Amplitude.Instance;
        amplitude.logging = true;
        amplitude.init("3f9c7b0d19b01caf443c0a376e6e10b0");
    }
}
