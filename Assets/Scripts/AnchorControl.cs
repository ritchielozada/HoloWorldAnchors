using UnityEngine;
using System.Collections;
using System.Text;
using HoloToolkit.Unity;

public class AnchorControl : MonoBehaviour
{
    public GameObject PlacementObject;
    public string SavedAnchorFriendlyName;
    private WorldAnchorManager anchorManager;
    private SpatialMappingManager spatialMappingManager;
    private TextToSpeechManager ttsMgr;

    private enum ControlState
    {
        WaitingForAnchorStore,
        CheckAnchorStatus,
        Ready,
        PlaceAnchor
    }

    private ControlState curentState;

    // Use this for initialization
    void Start()
    {
        curentState = ControlState.WaitingForAnchorStore;

        ttsMgr = GetComponent<TextToSpeechManager>();
        if (ttsMgr == null)
        {
            Debug.LogError("TextToSpeechManager Required");
        }

        anchorManager = WorldAnchorManager.Instance;
        if (anchorManager == null)
        {
            Debug.LogError("This script expects that you have a WorldAnchorManager component in your scene.");
        }

        spatialMappingManager = SpatialMappingManager.Instance;
        if (spatialMappingManager == null)
        {
            Debug.LogError("This script expects that you have a SpatialMappingManager component in your scene.");
        }

        //if (anchorManager != null && spatialMappingManager != null)
        //{
        //    anchorManager.AttachAnchor(this.gameObject, SavedAnchorFriendlyName);
        //    ttsMgr.SpeakText("Anchor Locked");
        //}
        //else
        //{
        //    ttsMgr.SpeakText("Cannot Lock Anchor");
        //}

        
    }

    // Update is called once per frame
    void Update()
    {
        switch (curentState)
        {
            case ControlState.WaitingForAnchorStore:
                if (anchorManager.AnchorStore != null)
                {
                    Debug.Log("Anchor Store Ready");
                    curentState = ControlState.CheckAnchorStatus;
                }
                break;
            case ControlState.CheckAnchorStatus:
                if (anchorManager.AnchorStore.anchorCount > 0)
                {
                    var sb = new StringBuilder("Found Anchors:");                                        
                    foreach (var ids in anchorManager.AnchorStore.GetAllIds())
                    {
                        sb.Append(ids);
                    }
                    Debug.Log(sb.ToString());
                    ttsMgr.SpeakText(sb.ToString());
                }
                else
                {
                    ttsMgr.SpeakText("No Anchors Found, Creating Anchor");
                    Debug.Log("No Anchors Found, Creating Anchor");
                }                
                anchorManager.AttachAnchor(PlacementObject, SavedAnchorFriendlyName);
                curentState = ControlState.Ready;
                break;
            case ControlState.Ready:
                break;
            case ControlState.PlaceAnchor:
                // TODO: Use GazeManager + Cursor Tracking instead of another Raycast
                var headPosition = Camera.main.transform.position;
                var gazeDirection = Camera.main.transform.forward;
                RaycastHit hitInfo;
                if (Physics.Raycast(headPosition, gazeDirection, out hitInfo,
                    30.0f, spatialMappingManager.LayerMask))
                {                    
                    PlacementObject.transform.position = hitInfo.point;

                    // Rotate this object to face the user.
                    //Quaternion toQuat = Camera.main.transform.localRotation;
                    //toQuat.x = 0;
                    //toQuat.z = 0;
                    //this.transform.rotation = toQuat;
                }
                break;
        }
    }

    public void PlaceAnchor()
    {
        if (curentState != ControlState.Ready)
        {
            ttsMgr.SpeakText("AnchorStore Not Ready");
            return;
        }
        
        anchorManager.RemoveAnchor(PlacementObject);
        curentState = ControlState.PlaceAnchor;
    }

    public void LockAnchor()
    {
        if (curentState != ControlState.PlaceAnchor)
        {
            ttsMgr.SpeakText("Not in Anchor Placement State");
            return;
        }

        
        // Add world anchor when object placement is done.
        anchorManager.AttachAnchor(PlacementObject, SavedAnchorFriendlyName);
        curentState = ControlState.Ready;
        ttsMgr.SpeakText("Anchor Placed");
    }
}
