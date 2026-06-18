using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TMPro; // Assuming TextMeshPro is used for UI
using Unity.Netcode;

public class ARCharacterSelectionManager : MonoBehaviour
{
    public static ARCharacterSelectionManager Instance;
    public static Transform CurrentTrackedImageTransform;

    [Header("AR to 3D Transition References")]
    [SerializeField] private UnityEngine.XR.ARFoundation.ARSession arSession;
    [SerializeField] private UnityEngine.XR.ARFoundation.ARCameraBackground arCameraBackground;
    [SerializeField] private GameObject prototypeMap;

    [Header("AR References")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Character Prefabs")]
    [SerializeField] private GameObject warriorPrefab;
    [SerializeField] private GameObject assassinPrefab;
    [SerializeField] private GameObject guardianPrefab;
    
    [Header("Training Mode")]
    [SerializeField] private GameObject trainingDummyPrefab;
    private GameObject spawnedDummy;

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
    public bool isMultiplayerLobbyPhase = false;

    private GameObject boundCharacter = null;

    private void Awake()
    {
        Instance = this;

        // Disable AR Plane Manager to hide yellow/black border detection area
        var planeManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARPlaneManager>();
        if (planeManager != null)
        {
            planeManager.enabled = false;
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
        }

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

    private void Start()
    {
        bool isMultiplayer = NetworkManager.Singleton != null && 
                             (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer);

        if (isMultiplayer)
        {
            isMultiplayerLobbyPhase = true;
            if (lobbyPanel != null) lobbyPanel.SetActive(true);
            Debug.Log("[ARCharacterSelectionManager] Multiplayer Mode: Entering Lobby Phase first.");
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
        Debug.Log("Tombol Start Diklik!");
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            if (ARNetworkPlayer.LocalPlayer != null)
            {
                Debug.Log("[ARCharacterSelectionManager] Host clicked Start Battle. Sending RPC...");
                ARNetworkPlayer.LocalPlayer.StartBattleServerRpc();
            }
            else
            {
                Debug.LogError("[ARCharacterSelectionManager] ERROR: ARNetworkPlayer.LocalPlayer is NULL!");
            }
        }
        else
        {
            Debug.LogWarning("[ARCharacterSelectionManager] Ignore click: Not host or NetworkManager is null.");
        }
    }

    public void TransitionToSelectionPhase()
    {
        Debug.Log("[ARCharacterSelectionManager] Battle started! Transitioning to Selection Phase...");
        isMultiplayerLobbyPhase = false;
        
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        
        if (!string.IsNullOrEmpty(currentDetectedCharacter) && currentPreviewHologram != null)
        {
            if (selectionPanel != null) selectionPanel.SetActive(true);
        }
    }

    public void TransitionToCombat()
    {
        Debug.Log($"[ARCharacterSelectionManager] TransitionToCombat! currentDetectedCharacter={currentDetectedCharacter}");
        isBattleStarted = true;
        
        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (combatPanel != null) combatPanel.SetActive(true);
        
        HideAllActionPanels();
        if (currentDetectedCharacter.Contains("Warrior") && warriorActionPanel != null) warriorActionPanel.SetActive(true);
        else if ((currentDetectedCharacter.Contains("Assassin") || currentDetectedCharacter.Contains("Assasin")) && assassinActionPanel != null) assassinActionPanel.SetActive(true);
        else if (currentDetectedCharacter.Contains("Guardian") && guardianActionPanel != null) guardianActionPanel.SetActive(true);
        
        Debug.Log("[ARCharacterSelectionManager] Combat HUD is now active!");
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
        if (spawnedDummy != null)
        {
            Destroy(spawnedDummy);
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
            
            if (!isMultiplayerLobbyPhase)
            {
                if (selectionPanel != null) selectionPanel.SetActive(true);
            }
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
        Debug.Log($"[ARCharacterSelectionManager] OnSelectClicked! currentPreviewHologram={(currentPreviewHologram != null ? "exists" : "NULL")}, currentDetectedCharacter={currentDetectedCharacter}");
        
        if (currentPreviewHologram != null)
        {
            isCharacterSelected = true;

            if (selectionPanel != null) selectionPanel.SetActive(false);
            if (menuButton != null) menuButton.gameObject.SetActive(false);

            int characterIndex = -1;
            if (currentDetectedCharacter.Contains("Warrior")) characterIndex = 0;
            else if (currentDetectedCharacter.Contains("Assassin") || currentDetectedCharacter.Contains("Assasin")) characterIndex = 1;
            else if (currentDetectedCharacter.Contains("Guardian")) characterIndex = 2;

            Debug.Log($"[ARCharacterSelectionManager] characterIndex={characterIndex}");

            bool isMultiplayer = NetworkManager.Singleton != null && 
                                 (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer);

            if (isMultiplayer)
            {
                Debug.Log("[ARCharacterSelectionManager] OnSelectClicked: MULTIPLAYER MODE. Requesting spawn...");
                Destroy(currentPreviewHologram);

                if (characterIndex != -1 && ARNetworkPlayer.LocalPlayer != null)
                {
                    // IMPORTANT: Activate map FIRST so spawn point positions are valid in world space
                    TransitionTo3DVirtualEnvironment();

                    // Now read spawn points from the ACTIVE map
                    Vector3 randomSpawnPos = new Vector3(0, 1f, 0); // fallback above ground
                    if (prototypeMap != null)
                    {
                        var spawns = new System.Collections.Generic.List<Transform>();
                        foreach(Transform child in prototypeMap.transform)
                        {
                            if (child.name.StartsWith("SpawnPoint")) spawns.Add(child);
                        }
                        Debug.Log($"[ARCharacterSelectionManager] Found {spawns.Count} spawn points in PrototypeMap.");
                        if (spawns.Count > 0)
                        {
                            int idx = Random.Range(0, spawns.Count);
                            randomSpawnPos = spawns[idx].position;
                            Debug.Log($"[ARCharacterSelectionManager] Selected SpawnPoint_{idx+1} at {randomSpawnPos}");
                        }
                        else
                        {
                            Debug.LogWarning("[ARCharacterSelectionManager] No SpawnPoints found! Using fallback position.");
                        }
                    }
                    else
                    {
                        Debug.LogError("[ARCharacterSelectionManager] prototypeMap reference is NULL!");
                    }

                    // Request Server to spawn character at the chosen position
                    Debug.Log($"[ARCharacterSelectionManager] Sending RequestSpawnCharacterServerRpc(index={characterIndex}, pos={randomSpawnPos})");
                    ARNetworkPlayer.LocalPlayer.RequestSpawnCharacterServerRpc(characterIndex, randomSpawnPos);
                }
                else
                {
                    Debug.LogError($"[ARCharacterSelectionManager] FAILED! ARNetworkPlayer.LocalPlayer={(ARNetworkPlayer.LocalPlayer != null ? "exists" : "NULL")}, characterIndex={characterIndex}");
                }
            }
            else
            {
                Debug.Log("[ARCharacterSelectionManager] OnSelectClicked: SINGLEPLAYER MODE. Transitioning direct to Combat...");
                HookupLocalCharacter(currentPreviewHologram);
                SpawnTrainingDummy();
                TransitionToCombat();
            }
        }
        else
        {
            Debug.LogWarning("[ARCharacterSelectionManager] OnSelectClicked but currentPreviewHologram is NULL!");
        }
    }

    private void SpawnTrainingDummy()
    {
        if (trainingDummyPrefab != null && CurrentTrackedImageTransform != null)
        {
            if (spawnedDummy != null) Destroy(spawnedDummy);
            
            spawnedDummy = Instantiate(trainingDummyPrefab, CurrentTrackedImageTransform);
            
            // Set position diagonally forward to ensure it's not hidden behind the character
            spawnedDummy.transform.localPosition = new Vector3(0.25f, 0, 0.3f);
            
            // Make dummy stand up (X=90) and face the player (Y=215)
            spawnedDummy.transform.localRotation = Quaternion.Euler(0, 215, 0) * Quaternion.Euler(90, 0, 0); 
            
            // Apply scale
            spawnedDummy.transform.localScale = Vector3.one * characterScale;
            
            // Force activate all child objects just in case prefab was saved with disabled children
            Transform[] allChildren = spawnedDummy.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                child.gameObject.SetActive(true);
                child.gameObject.layer = LayerMask.NameToLayer("Default"); // Ensure layer is visible
            }
            
            Debug.Log("[ARCharacterSelectionManager] Training Dummy Spawned and Forced Active.");
        }
    }

    public void PlayDummyPushedAnimation()
    {
        if (spawnedDummy != null)
        {
            var anim = spawnedDummy.GetComponent<Animator>();
            if (anim != null)
            {
                anim.Play("pushed", -1, 0f);
            }
        }
    }

    private void TransitionTo3DVirtualEnvironment()
    {
        // 1. Matikan AR Session & AR Camera Background agar pindah ke environment 3D statis
        if (arSession != null) arSession.enabled = false;
        if (arCameraBackground != null) arCameraBackground.enabled = false;
        
        // Disable tracked image manager too
        if (trackedImageManager != null) trackedImageManager.enabled = false;

        // Set tracked image transform null so characters don't snap to markers
        CurrentTrackedImageTransform = null;

        // 2. Munculkan Map
        if (prototypeMap != null) prototypeMap.SetActive(true);

        // 3. Posisikan kamera utama sebagai top-down atau side view
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(0, 15f, -12f);
            Camera.main.transform.LookAt(Vector3.zero);
        }

        Debug.Log("[ARCharacterSelectionManager] Pivoted from AR to 3D Virtual Environment.");
    }

    public void HookupLocalCharacter(GameObject spawnedCharacter)
    {
        Debug.Log($"[ARCharacterSelectionManager] HookupLocalCharacter called! Character={spawnedCharacter.name}, Position={spawnedCharacter.transform.position}");
        boundCharacter = spawnedCharacter;
        
        var controller = spawnedCharacter.GetComponent<ARCharacterController>();
        if (controller == null)
        {
            controller = spawnedCharacter.AddComponent<ARCharacterController>();
        }

        if (virtualJoystick != null)
        {
            controller.SetupJoystick(virtualJoystick);
            Debug.Log("[ARCharacterSelectionManager] Joystick assigned to character controller.");
        }
        else
        {
            Debug.LogWarning("[ARCharacterSelectionManager] virtualJoystick is NULL!");
        }

        BindButtonsForPanel(warriorActionPanel, controller);
        BindButtonsForPanel(assassinActionPanel, controller);
        BindButtonsForPanel(guardianActionPanel, controller);

        Debug.Log($"[ARCharacterSelectionManager] Character Officially Spawned & Hooked Up: {currentDetectedCharacter}");
        
        bool isMultiplayer = NetworkManager.Singleton != null && 
                             (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer);
                             
        if (isMultiplayer)
        {
            // Set scale to 1 instead of 0.1 for 3D world
            spawnedCharacter.transform.localScale = Vector3.one;
            Debug.Log($"[ARCharacterSelectionManager] Set character scale to 1. Position: {spawnedCharacter.transform.position}");
            
            // Adjust camera to follow the character (avoid duplicates)
            if (Camera.main != null)
            {
                var existingFollow = Camera.main.gameObject.GetComponent<CameraFollow3D>();
                if (existingFollow == null)
                {
                    existingFollow = Camera.main.gameObject.AddComponent<CameraFollow3D>();
                }
                existingFollow.target = spawnedCharacter.transform;
                existingFollow.offset = new Vector3(0, 5, -6);
                Debug.Log("[ARCharacterSelectionManager] CameraFollow3D set to follow character.");
            }
            else
            {
                Debug.LogError("[ARCharacterSelectionManager] Camera.main is NULL! Cannot setup camera follow!");
            }

            TransitionToCombat();
        }
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
