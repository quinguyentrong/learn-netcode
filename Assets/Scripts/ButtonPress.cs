using UnityEngine;
using System;
using UnityEngine.UI;

public enum ButtonActions
{
    Ready = 0,
    NotReady = 1,
}

public class ButtonPress : MonoBehaviour
{
    public static Action<ButtonActions> OnButtonPress;

    [SerializeField] private ButtonActions ButtonAction;
    [SerializeField] private Button SelfButton;

    private void Start()
    {
        SelfButton.onClick.AddListener(OnPress);
    }

    public void OnPress()
    {
        OnButtonPress?.Invoke(ButtonAction);
    }
}
