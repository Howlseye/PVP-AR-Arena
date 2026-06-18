using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class MultiplayerMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_InputField ipInputField;

    private TextMeshProUGUI joinBtnText;

    private void Start()
    {
        if (hostButton != null)
        {
            hostButton.onClick.AddListener(OnHostClicked);
        }

        if (joinButton != null)
        {
            joinButton.onClick.AddListener(OnJoinClicked);
            joinBtnText = joinButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (ipInputField != null && string.IsNullOrEmpty(ipInputField.text))
        {
            ipInputField.text = "127.0.0.1";
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnHostClicked()
    {
        if (NetworkManager.Singleton != null)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.ServerListenAddress = "0.0.0.0";
            }

            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started successfully. Loading GameScene...");
                NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
    }

    private void OnJoinClicked()
    {
        if (NetworkManager.Singleton != null)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null && ipInputField != null)
            {
                string ip = ipInputField.text.Trim(); // Trim whitespace from mobile keyboard
                if (string.IsNullOrEmpty(ip)) ip = "127.0.0.1";
                
                transport.ConnectionData.Address = ip;
            }

            if (joinBtnText != null) joinBtnText.text = "Connecting...";
            joinButton.interactable = false;
            hostButton.interactable = false;

            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started successfully, waiting to sync scene...");
            }
            else
            {
                if (joinBtnText != null) joinBtnText.text = "Failed!";
                joinButton.interactable = true;
                hostButton.interactable = true;
            }
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // Revert UI if connection failed/timed out
        if (joinBtnText != null) joinBtnText.text = "Join Failed (Timeout/Wrong IP)";
        if (joinButton != null) joinButton.interactable = true;
        if (hostButton != null) hostButton.interactable = true;
    }
}
