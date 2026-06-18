using UnityEngine;
using Unity.Netcode;

public class ARCharacterSync : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log($"[ARCharacterSync] OnNetworkSpawn called. IsOwner={IsOwner}, Position={transform.position}");
        
        // In 3D Virtual mode (multiplayer), do NOT parent to tracked image
        bool isMultiplayerVirtual = ARCharacterSelectionManager.Instance != null 
            && !ARCharacterSelectionManager.Instance.isMultiplayerLobbyPhase 
            && ARCharacterSelectionManager.CurrentTrackedImageTransform == null;

        if (isMultiplayerVirtual)
        {
            // 3D Virtual Mode: Keep the spawn position set by the server, use normal scale
            transform.localScale = Vector3.one;
            Debug.Log($"[ARCharacterSync] 3D Virtual Mode. Keeping server position: {transform.position}");
        }
        else if (ARCharacterSelectionManager.CurrentTrackedImageTransform != null)
        {
            // AR Mode (Training): Parent to tracked image
            transform.SetParent(ARCharacterSelectionManager.CurrentTrackedImageTransform, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one * 0.1f;
            Debug.Log("[ARCharacterSync] AR Mode. Parented to tracked image.");
        }
        else
        {
            Debug.LogWarning("[ARCharacterSync] Spawned, but no Tracked Image and not in Virtual mode.");
        }

        // If we are the owner, hook up to our local UI
        if (IsOwner)
        {
            Debug.Log("[ARCharacterSync] We are the owner! Hooking up local character...");
            if (ARCharacterSelectionManager.Instance != null)
            {
                ARCharacterSelectionManager.Instance.HookupLocalCharacter(this.gameObject);
            }
            else
            {
                Debug.LogError("[ARCharacterSync] ARCharacterSelectionManager.Instance is NULL!");
            }
        }
    }
}
