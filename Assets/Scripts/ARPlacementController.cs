using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARPlacementController : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private GameObject arenaHubPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private float characterScale = 1f;
    
    private GameObject spawnedArenaHub;

    private void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    private void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Loop through newly detected images
        foreach (var trackedImage in eventArgs.added)
        {
            if (spawnedArenaHub == null)
            {
                // Spawn the anchor game object directly at the physical card's position
                spawnedArenaHub = Instantiate(arenaHubPrefab, trackedImage.transform.position, trackedImage.transform.rotation);
                
                // Set the scale of the spawned character/hub
                spawnedArenaHub.transform.localScale = Vector3.one * characterScale;
            }
        }

        // Loop through images currently being updated by the AR framework
        foreach (var trackedImage in eventArgs.updated)
        {
            if (spawnedArenaHub != null && trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                // Dynamically snap the arena anchor to the card if it shifts slightly in real life
                spawnedArenaHub.transform.position = trackedImage.transform.position;
                
                // Ensure the rotation matches the marker, which is assumed to be lying flat on the surface
                spawnedArenaHub.transform.rotation = trackedImage.transform.rotation;
                
                // Ensure scale is maintained (in case it gets modified in the inspector during runtime)
                spawnedArenaHub.transform.localScale = Vector3.one * characterScale;
            }
        }
    }
}