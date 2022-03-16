using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARAnchorManager))]
public class PlacementIndicator : MonoBehaviour
{
    public GameObject indicator;
    public GameObject building;
    private Pose placement;
    private ARAnchor anchor;
    private ARAnchorManager anchorManager;
    private bool validPlacement = false;
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    public Button placeBtn;
    public Text scaleTxt;
    public Button confirmBtn;
    public Button cancelBtn;
    public Slider scaleSlider;
    private Vector3 buildingScale;
    private float buildingScaler;
    public Camera mainCamera;

    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        anchorManager = FindObjectOfType<ARAnchorManager>();
        planeManager = FindObjectOfType<ARPlaneManager>();
        building.SetActive(false);
        buildingScale = building.transform.localScale;
        confirmBtn.gameObject.SetActive(false);
        cancelBtn.gameObject.SetActive(false);
        scaleSlider.value = 1;
        scaleSlider.gameObject.SetActive(false);
        scaleTxt.gameObject.SetActive(false);
        placeBtn.gameObject.SetActive(true);
        if (building.GetComponent<ARAnchor>() == null) building.AddComponent<ARAnchor>();
    }

    void Update()
    {
        UpdatePlacement();
    }

    private void UpdatePlacement()
    {
        var screenCentre = mainCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);
        var hitPoints = new List<ARRaycastHit>();
        raycastManager.Raycast(screenCentre, hitPoints, TrackableType.Planes);

        validPlacement = hitPoints.Count > 0;
        if (validPlacement)
        {
            placement = hitPoints[0].pose;

            var cameraPointing = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraPointing.x, 0, cameraPointing.z + 180).normalized;

            indicator.transform.position = placement.position;
            indicator.transform.rotation = Quaternion.LookRotation(cameraBearing);

            if (!indicator.activeInHierarchy && !building.activeInHierarchy) indicator.SetActive(true);
            placeBtn.interactable = true;
        }
        else
        {
            indicator.SetActive(false);
            if (placeBtn.GetComponentInChildren<Text>().text == "Place Building") placeBtn.interactable = false;
        }
    }

    public void confirmBtnClicked()
    {
        confirmBtn.gameObject.SetActive(false);
        cancelBtn.gameObject.SetActive(false);
        scaleSlider.gameObject.SetActive(false);
        placeBtn.gameObject.SetActive(true);
        scaleTxt.gameObject.SetActive(false);


    }

    public void cancelBtnClicked()
    {
        placeBtn.GetComponentInChildren<Text>().text = "Place Building";
        building.gameObject.SetActive(false);
        confirmBtn.gameObject.SetActive(false);
        cancelBtn.gameObject.SetActive(false);
        scaleSlider.gameObject.SetActive(false);
        placeBtn.gameObject.SetActive(true);
        scaleTxt.gameObject.SetActive(false);
    }

    public void SliderChange()
    {
        buildingScaler = scaleSlider.value;
        var scale = buildingScale * buildingScaler;
        building.transform.localScale = scale;
    }

    public void PlaceBuildingClicked()
    {
        if (building.activeInHierarchy)
        {
            building.SetActive(false);
            indicator.SetActive(true);
            placeBtn.GetComponentInChildren<Text>().text = "Place Building";
        }
        else
        {
            if (validPlacement)
            {
                scaleSlider.value = 1;
                indicator.SetActive(false);
                confirmBtn.gameObject.SetActive(true);
                cancelBtn.gameObject.SetActive(true);
                scaleSlider.gameObject.SetActive(true);
                placeBtn.gameObject.SetActive(false);
                scaleTxt.gameObject.SetActive(true);
                anchor = anchorManager.AddAnchor(placement);
                // model.transform.parent = anchor?.transform ?? transform;
                building.transform.position = anchor.transform.position;
                building.transform.rotation = anchor.transform.rotation;
                building.transform.localScale = buildingScale * buildingScaler;
                building.SetActive(true);
                placeBtn.GetComponentInChildren<Text>().text = "Remove Building";
            }
        }
    }
}
