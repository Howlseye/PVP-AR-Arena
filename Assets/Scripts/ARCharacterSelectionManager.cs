using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TMPro; // Assuming TextMeshPro is used for UI
using Unity.Netcode;

public class ARCharacterSelectionManager : MonoBehaviour
{
    public static ARCharacterSelectionManager Instance;
    public static Transform CurrentTrackedImageTransform;

    [Header("AR References")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Character Prefabs")]
    [SerializeField] private GameObject warriorPrefab;
    [SerializeField] private GameObject assassinPrefab;
    [SerializeField] private GameObject guardianPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private float characterScale = 0.1f;

    [Header("UI References")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private Button selectButton;
    [SerializeField] private GameObject combatPanel;
    [SerializeField] private PinePie.SimpleJoystick.JoystickController virtualJoystick;
    
    [Header("Lobby UI References")]
    [SerializeField] public GameObject lobbyPanel;
    [SerializeField] public TextMeshProUGUI lobbyStatusText;
    [SerializeField] public Button startBattleButton;

    [Header("Action Panels")]
    [SerializeField] private GameObject warriorActionPanel;
    [SerializeField] private GameObject assassinActionPanel;
    [SerializeField] private GameObject guardianActionPanel;

    [Header("Navigation")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button menuButton;

    private GameObject currentPreviewHologram;
    public string currentDetectedCharacter = "";
    private bool isCharacterSelected = false;
    private bool isBattleStarted = false;

    private GameObject boundCharacter = null;

    private void Awake()
    {
        Instance = this;

        // Ensure UI state
        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (combatPanel != null) combatPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        
        HideAllActionPanels();

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuClicked);
        }
        
        if (startBattleButton != null)
        {
            startBattleButton.onClick.AddListener(OnStartBattleClicked);
        }
    }

    private void Update()
    {
        if (lobbyPanel != null && lobbyPanel.activeSelf)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                int playerCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
                if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
                    playerCount = 2; // Client just assumes 2 since it can't see the list

                if (NetworkManager.Singleton.IsHost)
                {
                    if (startBattleButton != null)
                    {
                        startBattleButton.gameObject.SetActive(true);
                        startBattleButton.interactable = (playerCount >= 2);
                    }
                    if (lobbyStatusText != null)
                    {
                        lobbyStatusText.text = $"Players: {playerCount}/2\nWaiting for players...";
                        if (playerCount >= 2) lobbyStatusText.text = $"Players: {playerCount}/2\nReady to start!";
                    }
                }
                else
                {
                    if (startBattleButton != null) startBattleButton.gameObject.SetActive(false);
                    if (lobbyStatusText != null) lobbyStatusText.text = $"Players: {playerCount}/2\nWaiting for Host to start...";
                }
            }
        }
    }

    private void OnStartBattleClicked()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            if (ARNetworkPlayer.LocalPlayer != null)
            {
                Debug.Log("[ARCharacterSelectionManager] Host clicked Start Battle. Sending RPC...");
                ARNetworkPlayer.LocalPlayer.StartBattleServerRpc();
            }
        }
    }

    public void TransitionToCombat()
    {
        Debug.Log("[ARCharacterSelectionManager] Transitioning to Combat HUD!");
        isBattleStarted = true;
        
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (combatPanel != null) combatPanel.SetActive(true);
        
        // Ensure action panels are set correctly based on the character
        HideAllActionPanels();
        if (currentDetectedCharacter.Contains("Warrior") && warriorActionPanel != null) warriorActionPanel.SetActive(true);
        else if (currentDetectedCharacter.Contains("Assassin") && assassinActionPanel != null) assassinActionPanel.SetActive(true);
        else if (currentDetectedCharacter.Contains("Guardian") && guardianActionPanel != null) guardianActionPanel.SetActive(true);
    }

    private void OnBackClicked()
    {
        isCharacterSelected = false;
        isBattleStarted = false;
        
        if (combatPanel != null) combatPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (selectionPanel != null) selectionPanel.SetActive(true);
        if (menuButton != null) menuButton.gameObject.SetActive(true);

        if (currentPreviewHologram != null)
        {
            Destroy(currentPreviewHologram);
        }
        currentDetectedCharacter = "";
        HideAllActionPanels();
    }

    private void OnMenuClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private void HideAllActionPanels()
    {
        if (warriorActionPanel != null) warriorActionPanel.SetActive(false);
        if (assassinActionPanel != null) assassinActionPanel.SetActive(false);
        if (guardianActionPanel != null) guardianActionPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
        }
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        }
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        if (isCharacterSelected) return;

        foreach (var newImage in eventArgs.added)
        {
            HandleTrackedImage(newImage);
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            HandleTrackedImage(updatedImage);
        }
    }

    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
        {
            CurrentTrackedImageTransform = trackedImage.transform;
            string imageName = trackedImage.referenceImage.name;

            if (currentDetectedCharacter != imageName)
            {
                currentDetectedCharacter = imageName;
                UpdatePreviewHologram(trackedImage);
            }
            
            if (selectionPanel != null) selectionPanel.SetActive(true);
        }
        else if (trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.None ||
                 trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Limited)
        {
            if (currentPreviewHologram != null)
            {
                currentPreviewHologram.SetActive(false);
            }
            if (selectionPanel != null) selectionPanel.SetActive(false);
            currentDetectedCharacter = "";
        }
    }

    private void UpdatePreviewHologram(ARTrackedImage trackedImage)
    {
        if (currentPreviewHologram != null)
        {
            Destroy(currentPreviewHologram);
        }

        GameObject prefabToSpawn = null;
        string displayName = "";
        string imageName = trackedImage.referenceImage.name;

        if (imageName.Contains("Warrior"))
        {
            prefabToSpawn = warriorPrefab;
            displayName = "Warrior";
        }
        else if (imageName.Contains("Assassin"))
        {
            prefabToSpawn = assassinPrefab;
            displayName = "Assassin";
        }
        else if (imageName.Contains("Guardian"))
        {
            prefabToSpawn = guardianPrefab;
            displayName = "Guardian";
        }

        if (prefabToSpawn != null)
        {
            currentPreviewHologram = Instantiate(prefabToSpawn, trackedImage.transform);
            
            currentPreviewHologram.transform.localPosition = Vector3.zero;
            currentPreviewHologram.transform.localRotation = Quaternion.identity;
            currentPreviewHologram.transform.localScale = Vector3.one * characterScale;

            if (characterNameText != null)
            {
                characterNameText.text = displayName;
            }
        }
    }

    private void OnSelectClicked()
    {
        if (currentPreviewHologram != null)
        {
            isCharacterSelected = true;

            // Hide selection UI
            if (selectionPanel != null) selectionPanel.SetActive(false);
            if (menuButton != null) menuButton.gameObject.SetActive(false);

            int characterIndex = -1;
            if (currentDetectedCharacter.Contains("Warrior")) characterIndex = 0;
            else if (currentDetectedCharacter.Contains("Assassin")) characterIndex = 1;
            else if (currentDetectedCharacter.Contains("Guardian")) characterIndex = 2;

            bool isMultiplayer = NetworkManager.Singleton != null && 
                                 (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer);

            if (isMultiplayer)
            {
                Debug.Log("[ARCharacterSelectionManager] OnSelectClicked: MULTIPLAYER MODE. Activating Lobby Panel...");
                // MULTIPLAYER MODE
                // Show Lobby Panel
                if (lobbyPanel != null) lobbyPanel.SetActive(true);

                // Destroy local preview hologram because the Server will spawn the official one
                Destroy(currentPreviewHologram);

                if (characterIndex != -1 && ARNetworkPlayer.LocalPlayer != null)
                {
                    ARNetworkPlayer.LocalPlayer.RequestSpawnCharacterServerRpc(characterIndex);
                }
                else
                {
                    Debug.LogError("[ARCharacterSelectionManager] ARNetworkPlayer not found or invalid character!");
                }
            }
            else
            {
                Debug.Log("[ARCharacterSelectionManager] OnSelectClicked: SINGLEPLAYER MODE. Transitioning direct to Combat...");
                // SINGLEPLAYER MODE (Training Scene)
                // Just use the preview hologram as the actual character
                HookupLocalCharacter(currentPreviewHologram);
                TransitionToCombat();
            }
        }
    }

    public void HookupLocalCharacter(GameObject spawnedCharacter)
    {
        boundCharacter = spawnedCharacter;
        
        // Setup Character Controller
        var controller = spawnedCharacter.GetComponent<ARCharacterController>();
        if (controller == null)
        {
            controller = spawnedCharacter.AddComponent<ARCharacterController>();
        }

        if (virtualJoystick != null)
        {
            controller.SetupJoystick(virtualJoystick);
        }

        // Bind buttons for ALL potential action panels, so when they are enabled they work
        BindButtonsForPanel(warriorActionPanel, controller);
        BindButtonsForPanel(assassinActionPanel, controller);
        BindButtonsForPanel(guardianActionPanel, controller);

        Debug.Log($"[ARCharacterSelectionManager] Character Officially Spawned & Hooked Up: {currentDetectedCharacter}");
    }

    private void BindButtonsForPanel(GameObject panel, ARCharacterController controller)
    {
        if (panel != null)
        {
            foreach(var btn in panel.GetComponentsInChildren<Button>(true))
            {
                btn.onClick.RemoveAllListeners();
                if (btn.gameObject.name == "Attack Button")
                {
                    btn.onClick.AddListener(() => controller.Attack());
                }
                else if (btn.gameObject.name == "Attack 2 Button")
                {
                    btn.onClick.AddListener(() => controller.Attack2());
                }
                else if (btn.gameObject.name == "Shield Button")
                {
                    btn.onClick.AddListener(() => controller.Shield());
                }
            }
        }
    }
}
