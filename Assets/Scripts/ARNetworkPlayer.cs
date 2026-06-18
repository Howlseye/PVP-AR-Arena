using UnityEngine;
using Unity.Netcode;

public class ARNetworkPlayer : NetworkBehaviour
{
    // Make it accessible locally
    public static ARNetworkPlayer LocalPlayer;

    [Header("Character Prefabs (Assign in Inspector)")]
    [SerializeField] private GameObject warriorPrefab;
    [SerializeField] private GameObject assassinPrefab;
    [SerializeField] private GameObject guardianPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalPlayer = this;
            Debug.Log("[ARNetworkPlayer] Local ARNetworkPlayer spawned!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnCharacterServerRpc(int characterIndex, Vector3 spawnPos, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[ARNetworkPlayer] Server received spawn request: index={characterIndex}, client={rpcParams.Receive.SenderClientId}, pos={spawnPos}");
        
        GameObject prefabToSpawn = null;

        if (characterIndex == 0) prefabToSpawn = warriorPrefab;
        else if (characterIndex == 1) prefabToSpawn = assassinPrefab;
        else if (characterIndex == 2) prefabToSpawn = guardianPrefab;

        if (prefabToSpawn != null)
        {
            Debug.Log($"[ARNetworkPlayer] Spawning {prefabToSpawn.name} at {spawnPos}");
            var go = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            var netObj = go.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
                Debug.Log($"[ARNetworkPlayer] Character spawned successfully with ownership to client {rpcParams.Receive.SenderClientId}");
            }
            else
            {
                Debug.LogError("[ARNetworkPlayer] Spawned prefab has no NetworkObject component!");
            }
        }
        else
        {
            Debug.LogError($"[ARNetworkPlayer] Prefab for index {characterIndex} is NULL! Check Inspector references on ARNetworkPlayer prefab.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartBattleServerRpc()
    {
        Debug.Log("[ARNetworkPlayer] Server executing StartBattleServerRpc. Broadcasting to all clients...");
        StartBattleClientRpc();
    }

    [ClientRpc]
    public void StartBattleClientRpc()
    {
        Debug.Log("[ARNetworkPlayer] Client received StartBattleClientRpc! Transitioning from Lobby to Character Selection...");
        
        if (ARCharacterSelectionManager.Instance != null)
        {
            ARCharacterSelectionManager.Instance.TransitionToSelectionPhase();
        }
        else
        {
            Debug.LogError("[ARNetworkPlayer] ARCharacterSelectionManager Instance not found!");
        }
    }
}
