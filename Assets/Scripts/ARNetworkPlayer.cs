using UnityEngine;
using Unity.Netcode;

public class ARNetworkPlayer : NetworkBehaviour
{
    // Make it accessible locally
    public static ARNetworkPlayer LocalPlayer;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalPlayer = this;
            Debug.Log("[ARNetworkPlayer] Local ARNetworkPlayer spawned!");
        }
    }

    [ServerRpc]
    public void RequestSpawnCharacterServerRpc(int characterIndex, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[ARNetworkPlayer] Server received request to spawn character {characterIndex} from client {rpcParams.Receive.SenderClientId}");
        
        var prefabs = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;
        GameObject prefabToSpawn = null;

        foreach (var p in prefabs)
        {
            if (characterIndex == 0 && (p.Prefab.name.Contains("Warrior") || p.Prefab.name.Contains("Character 1")))
                prefabToSpawn = p.Prefab;
            else if (characterIndex == 1 && (p.Prefab.name.Contains("Assassin") || p.Prefab.name.Contains("Character 2")))
                prefabToSpawn = p.Prefab;
            else if (characterIndex == 2 && (p.Prefab.name.Contains("Guardian") || p.Prefab.name.Contains("Character 3")))
                prefabToSpawn = p.Prefab;
        }

        if (prefabToSpawn != null)
        {
            var go = Instantiate(prefabToSpawn);
            var netObj = go.GetComponent<NetworkObject>();
            netObj.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        }
        else
        {
            Debug.LogError("[ARNetworkPlayer] SpawnCharacterServerRpc: Prefab not found in NetworkManager!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartBattleServerRpc()
    {
        Debug.Log("[ARNetworkPlayer] Server executing StartBattleServerRpc. Broadcasting to all clients...");
        StartBattleClientRpc();
    }

    [ClientRpc]
    private void StartBattleClientRpc()
    {
        Debug.Log("[ARNetworkPlayer] Client received StartBattleClientRpc! Transitioning from Lobby to Combat...");
        
        if (ARCharacterSelectionManager.Instance != null)
        {
            ARCharacterSelectionManager.Instance.TransitionToCombat();
        }
        else
        {
            Debug.LogError("[ARNetworkPlayer] ARCharacterSelectionManager Instance not found!");
        }
    }
}
