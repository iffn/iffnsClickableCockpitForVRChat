using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace iffnsStuff.iffnsVRCStuff.iffnsClickableCockpit
{
    public class ButtonClickForwarder : UdonSharpBehaviour
    {
        public UdonBehaviour TargetScript;
        [SerializeField] string UdonFunctionName = "KeyboardInput";
        [HideInInspector] public ButtonClickForwarder LinkedForwarder;

        private void Start()
        {
            enabled = false;
        }

        public void RunDfuncFunction()
        {
            TargetScript.SendCustomEvent(UdonFunctionName);
        }
    }
}