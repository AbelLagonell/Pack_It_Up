using System.Collections;
using NETWORK_ENGINE;
using UnityEngine;
using UnityEngine.Serialization;

struct NetworkPlayerManagerFlags {
    public const string READY = "READY";
}

public class NetworkPlayerManager : NetworkComponent {
    public GameObject startScreen;
    public GameObject scoreScreen;
    public bool ready;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        if (startScreen == null) {
            Debug.LogError("No Start Screen chosen");
        }

        if (scoreScreen == null) {
            Debug.LogError("No Score Screen chosen");
        }

        scoreScreen.SetActive(false);
    }

    public override IEnumerator SlowUpdate() {
        while (IsConnected) {
            if (IsServer)
                if (IsDirty) {
                    SendUpdate(NetworkPlayerManagerFlags.READY, ready.ToString());
                    IsDirty = false;
                }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }

        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case NetworkPlayerManagerFlags.READY:
                ready = bool.Parse(value);
                if (IsServer) SendUpdate(NetworkPlayerManagerFlags.READY, ready.ToString());
                break;
        }
    }

    public override void NetworkedStart() {
        if (!IsLocalPlayer) {
            startScreen.SetActive(false);
        }
    }

    public void GameStart() {
        startScreen.SetActive(false);
        if (IsLocalPlayer) Debug.Log("Game Start: " + MyId.Owner);
    }

    public void GameEnd() {
        if (IsLocalPlayer) scoreScreen.SetActive(true);
        if (IsLocalPlayer) Debug.Log("Game End: " + MyId.Owner);
    }

    public void OnCheckBoxClick(bool value) {
        if (IsLocalPlayer) {
            SendCommand(NetworkPlayerManagerFlags.READY, value.ToString());
        }
    }
}