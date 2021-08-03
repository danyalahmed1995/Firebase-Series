using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeEnvironmentCheck : MonoBehaviour
{
    [SerializeField] GameObject apple_sign_in_button;
    [SerializeField] GameObject google_sign_in_button;


    private void Start()
    {
#if UNITY_IOS
        apple_sign_in_button.SetActive(true);
#elif UNITY_ANDROID
   google_sign_in_button.SetActive(true);
#endif


    }
}
