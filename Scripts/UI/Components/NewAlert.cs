using System;
using UnityEngine;
using UnityEngine.Events;

public class NewAlert : MonoBehaviour
{
    public GameObject Image;
    [Serializable]
    public class AlertEvent : UnityEvent<GameObject> { }
    public AlertEvent OnAlertEvent;

    private void OnEnable()
    {
        Image.SetActive(false);
    }

    private void Update()
    {
        OnAlertEvent.Invoke(Image);
    }
}
