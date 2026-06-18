using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;

public class DisplayLocalIP : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private string cachedIP = null;
    private int lastPlayerCount = -1;

    private void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        cachedIP = GetLocalIPAddress();
        Debug.Log("[DisplayLocalIP] Initial IP Cached: " + cachedIP);
    }

    private void Update()
    {
        if (textComponent == null) return;

        int playerCount = 1;
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            playerCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
            // The client might only see themselves in ConnectedClientsIds if not host
            if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
            {
                playerCount = 2; // Assume 2 if successfully connected to host
            }
        }
        else
        {
            playerCount = 1; // Single player training
        }

        if (playerCount != lastPlayerCount)
        {
            Debug.Log($"[DisplayLocalIP] Player count changed. Current players: {playerCount}");
            lastPlayerCount = playerCount;
            textComponent.text = $"Room IP: {cachedIP}\nPlayers: {playerCount} / 2";
        }
    }

    private string GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Debug.Log($"[DisplayLocalIP] Found valid IPv4: {ip.ToString()}");
                    return ip.ToString();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[DisplayLocalIP] Error getting IP: " + e.Message);
        }
        return "Unknown/Localhost";
    }
}
