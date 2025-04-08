using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

struct MeetingInteractableFlags {
    public const string SHOW = "SHOW";
}

public class MeetingInteractable : Interactable {
    private NetworkPlayerManager[] _managers;
    private bool checkReady = false;

    private void Start() {
        _managers = FindObjectsOfType<NetworkPlayerManager>();
    }

    public override IEnumerator SlowUpdate() {
            if (IsServer) {
                bool allReady = true;
                do {
                    foreach (var player in _managers) {
                        if (!player.ready) allReady = false;
                    }

                    yield return new WaitForSeconds(1f);
                } while (!allReady);

                //Record everyone's vote
                int votes = 0;
                int character = 0;
                foreach (var manager in _managers) {
                }
                
                foreach (var manager in _managers) {
                }
                
                SendUpdate(MeetingInteractableFlags.SHOW, false.ToString());
                
            }
            
            yield return new WaitForSeconds(1f);
        
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case MeetingInteractableFlags.SHOW:
                if (IsClient) {
                    foreach (var player in _managers) {
                        player.ready = false;
                    }
                }

                //Set up the voting system like the ready basically setting it on ready and voting
                break;
        }
    }

    public override void NetworkedStart() { }

    public override void OnUse() {
        if (usable) return;
        usable = false;
        Time.timeScale = 0;
        GameManager.GamePaused = true;
        checkReady = true;
        SendUpdate(MeetingInteractableFlags.SHOW, "");
    }
}