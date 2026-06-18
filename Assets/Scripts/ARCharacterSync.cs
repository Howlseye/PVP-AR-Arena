using UnityEngine;
using Unity.Netcode;

public class ARCharacterSync : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // When this character spawns, we want to place it on the active Tracked Image
        if (ARCharacterSelectionManager.CurrentTrackedImageTransform != null)
        {
            transform.SetParent(ARCharacterSelectionManager.CurrentTrackedImageTransform, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            
            // Set scale to a proper value, usually handled by ARCharacterSelectionManager
            transform.localScale = Vector3.one * 0.1f; // fallback scale
        }
        else
        {
            Debug.LogWarning("ARCharacterSync: Spawned, but no Tracked Image found yet!");
        }

        // If we are the owner, we hook it up to our local UI!
        if (IsOwner)
        {
            ARCharacterSelectionManager.Instance.HookupLocalCharacter(this.gameObject);
        }
    }
}
