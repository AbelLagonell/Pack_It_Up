using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

struct MeetingInteractableFlags {
    public const string SHOW = "SHOW";
}

public class MeetingInteractable : Interactable {
    [SerializeField] private GameObject votingUI;
    private NetworkPlayerManager[] _managers;

    private void Start() {
        votingUI.SetActive(false);
        _managers = FindObjectsOfType<NetworkPlayerManager>();
    }

    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(1f);
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case MeetingInteractableFlags.SHOW:
                votingUI.SetActive(true);
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
        SendUpdate(MeetingInteractableFlags.SHOW, "");
    }
}