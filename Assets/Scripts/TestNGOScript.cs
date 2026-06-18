
using UnityEngine;
using Unity.Netcode;
public class TestNGOScript : MonoBehaviour {
    void Start() { Debug.Log(NetworkManager.Singleton != null); }
}