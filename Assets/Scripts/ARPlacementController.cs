using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARPlacementController : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private GameObject arenaHubPrefab;
    
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
                // Spawn the anchor game object directly at the physical card's position and rotation
                spawnedArenaHub = Instantiate(arenaHubPrefab, trackedImage.transform.position, trackedImage.transform.rotation);
            }
        }

        // Loop through images currently being updated by the AR framework
        foreach (var trackedImage in eventArgs.updated)
        {
            if (spawnedArenaHub != null && trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                // Dynamically snap the arena anchor to the card if it shifts slightly in real life
                spawnedArenaHub.transform.position = trackedImage.transform.position;
                spawnedArenaHub.transform.rotation = trackedImage.transform.rotation;
            }
        }
    }
}