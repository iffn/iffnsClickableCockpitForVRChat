
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

[RequireComponent(typeof(VRCStation))]
public class ClickableCockpitController : UdonSharpBehaviour
{
    [SerializeField] ButtonClickForwarder[] ButtonHolders;
    [SerializeField] Transform TestDirectionIndicator;
    [SerializeField] LayerMask ButtonColliderMask;
    [SerializeField] Transform LeftVRHandIndicator;
    [SerializeField] Transform RightVRHandIndicator;
    [SerializeField] GameObject EmptyGameObjectReferenceBecauseUdonCannotInstantiateEmptyGameobjectsOnItsOwn;
    
    Transform cockpitSimulator;

    Quaternion handOffset = Quaternion.Euler(0, 60, 0);
    const float rayLength = 0.5f;

    bool seated = false;

    ButtonClickForwarder MainForwarder;
    
    ButtonClickForwarder RightForwarder;
    ButtonClickForwarder LeftForwarder;

    Vector3 localOrigin = Vector3.zero;
    Vector3 localDirection = Vector3.zero;

    Vector3 localHandOriginLeft = Vector3.zero;
    Vector3 localHandDirectionLeft = Vector3.zero;
    Vector3 localHandOriginRight = Vector3.zero;
    Vector3 localHandDirectionRight = Vector3.zero;

    void Start()
    {
        GameObject cockpitSimulatorObject = GameObject.Instantiate(EmptyGameObjectReferenceBecauseUdonCannotInstantiateEmptyGameobjectsOnItsOwn);
        cockpitSimulator = cockpitSimulatorObject.transform;
        cockpitSimulator.position = 10 * Vector3.down;
        cockpitSimulator.rotation = Quaternion.identity;
        cockpitSimulator.localScale = Vector3.one;
        cockpitSimulatorObject.SetActive(false);

        foreach (ButtonClickForwarder forwarder in ButtonHolders)
        {
            //continue;

            Transform newForwarderTransform = Instantiate(forwarder.gameObject).transform;
            newForwarderTransform.parent = cockpitSimulator.transform;
            
            newForwarderTransform.localPosition = forwarder.transform.localPosition;
            newForwarderTransform.localRotation = forwarder.transform.localRotation;

            newForwarderTransform.transform.GetComponent<Collider>().enabled = true;
            newForwarderTransform.transform.GetComponent<MeshRenderer>().enabled = false;

            ButtonClickForwarder newForwarder = newForwarderTransform.GetComponent<ButtonClickForwarder>();
            newForwarder.LinkedDFUNC = forwarder.LinkedDFUNC;
            newForwarder.LinkedForwarder = forwarder;
            forwarder.LinkedForwarder = newForwarder;
            
            forwarder.gameObject.SetActive(false);
        }

        LeftVRHandIndicator.gameObject.SetActive(false);
        RightVRHandIndicator.gameObject.SetActive(false);
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (!player.isLocal) return;

        seated = true;

        cockpitSimulator.gameObject.SetActive(true);

        if (Networking.LocalPlayer.IsUserInVR())
        {
            LeftVRHandIndicator.gameObject.SetActive(true);
            RightVRHandIndicator.gameObject.SetActive(true);
        }
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        if (!player.isLocal) return;

        seated = false;

        cockpitSimulator.gameObject.SetActive(false);

        if (Networking.LocalPlayer.IsUserInVR())
        {
            LeftVRHandIndicator.gameObject.SetActive(false);
            RightVRHandIndicator.gameObject.SetActive(false);
        }
    }

    //Calculate the current tracking vectors
    private void LateUpdate()
    {
        if (!seated) return;

        if (Networking.LocalPlayer.IsUserInVR())
        {
            Vector3 leftOrigin = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
            localHandOriginLeft = transform.InverseTransformPoint(leftOrigin);

            Vector3 leftDirection = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation * handOffset * Vector3.forward;
            localHandDirectionLeft = transform.InverseTransformDirection(leftDirection);

            Vector3 rightOrigin = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
            localHandOriginRight = transform.InverseTransformPoint(rightOrigin);

            Vector3 rightDirection = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation * handOffset * Vector3.forward;
            localHandDirectionRight = transform.InverseTransformDirection(rightDirection);

            LeftVRHandIndicator.SetPositionAndRotation(leftOrigin, Quaternion.LookRotation(leftDirection));
            RightVRHandIndicator.SetPositionAndRotation(rightOrigin, Quaternion.LookRotation(rightDirection));
        }
        else
        {
            Vector3 origin = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            localOrigin = transform.InverseTransformPoint(origin);

            Vector3 direction = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Vector3.forward;
            localDirection = transform.InverseTransformDirection(direction);

            /*
            TestDirectionIndicator.transform.localPosition = localOrigin;
            TestDirectionIndicator.transform.localRotation = Quaternion.LookRotation(localDirection);
            */
        }
    }

    //Get the selected button
    private void FixedUpdate()
    {
        if (!seated) return;

        if (Networking.LocalPlayer.IsUserInVR())
        {
            //Left
            Ray leftRay;
            RaycastHit leftHit;

            if (LeftForwarder != null)
            {
                LeftForwarder.LinkedForwarder.gameObject.SetActive(false);
                LeftForwarder = null;
            }

            leftRay = new Ray(origin: cockpitSimulator.TransformPoint(localHandOriginLeft), direction: cockpitSimulator.TransformDirection(localHandDirectionLeft));

            if (Physics.Raycast(ray: leftRay, hitInfo: out leftHit, maxDistance: rayLength))
            {
                if (leftHit.collider != null)
                {
                    LeftForwarder = leftHit.collider.transform.GetComponent<ButtonClickForwarder>();

                    if (LeftForwarder != null)
                    {
                        LeftForwarder.LinkedForwarder.gameObject.SetActive(true);
                    }
                }
            }

            //Right
            Ray rightRay;
            RaycastHit rightHit;

            if (RightForwarder != null)
            {
                RightForwarder.LinkedForwarder.gameObject.SetActive(false);
                RightForwarder = null;
            }

            rightRay = new Ray(origin: cockpitSimulator.TransformPoint(localHandOriginRight), direction: cockpitSimulator.TransformDirection(localHandDirectionRight));

            if (Physics.Raycast(ray: rightRay, hitInfo: out rightHit, maxDistance: rayLength))
            {
                if (rightHit.collider != null)
                {
                    RightForwarder = rightHit.collider.transform.GetComponent<ButtonClickForwarder>();

                    if (RightForwarder != null)
                    {
                        RightForwarder.LinkedForwarder.gameObject.SetActive(true);
                    }
                }
            }
        }
        else
        {
            if (MainForwarder != null)
            {
                MainForwarder.LinkedForwarder.gameObject.SetActive(false);
                MainForwarder = null;
            }                

            Ray ray = new Ray(origin: cockpitSimulator.TransformPoint(localOrigin), direction: cockpitSimulator.TransformDirection(localDirection));

            RaycastHit hit;
            //if (Physics.Raycast(ray, out hit, ButtonColliderMask))
            if (Physics.Raycast(ray: ray, hitInfo: out hit, maxDistance: rayLength))
            {
                if (hit.collider == null) return;

                MainForwarder = hit.collider.transform.GetComponent<ButtonClickForwarder>();

                if (MainForwarder != null)
                {
                    MainForwarder.LinkedForwarder.gameObject.SetActive(true);
                }
            }
        }
    }

    /*
    //Use the selected button
    private void Update()
    {
        if (Networking.LocalPlayer.IsUserInVR())
        {
            
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (MainForwarder != null)
                {
                    MainForwarder.RunDfuncFunction();
                }
            }
        }
    }
    */

    public override void InputUse(bool value, UdonInputEventArgs args)
    {
        if (!value) return;

        if (Networking.LocalPlayer.IsUserInVR())
        {
            switch (args.handType)
            {
                case HandType.RIGHT:
                    if (RightForwarder != null)
                    {
                        RightForwarder.RunDfuncFunction();
                    }
                    break;
                case HandType.LEFT:
                    if (LeftForwarder != null)
                    {
                        LeftForwarder.RunDfuncFunction();
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (MainForwarder != null)
            {
                MainForwarder.RunDfuncFunction();
            }
        }
    }
}
