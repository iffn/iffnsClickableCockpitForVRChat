
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ButtonClickForwarder : UdonSharpBehaviour
{
    public UdonBehaviour LinkedDFUNC;
    [SerializeField] string UdonFunctionName = "KeyboardInput";
    [HideInInspector] public ButtonClickForwarder LinkedForwarder;

    private void Start()
    {
        enabled = false;
    }

    public void RunDfuncFunction()
    {
        LinkedDFUNC.SendCustomEvent(UdonFunctionName);
    }
}
