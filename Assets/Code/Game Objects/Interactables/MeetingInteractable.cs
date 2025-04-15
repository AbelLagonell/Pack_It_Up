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
    public bool checkReady = false;

    private void Start() { }

    public override IEnumerator SlowUpdate() {
        while (IsConnected) {
            _managers = FindObjectsOfType<NetworkPlayerManager>();
            if (IsServer && checkReady) {
                bool allReady;
                do {
                    allReady = true;
                    foreach (var playerManager in _managers) {
                        if (playerManager.player.IsDetained) continue;
                        if (!playerManager.ready) allReady = false;
                    }

                    yield return new WaitForSeconds(0.1f);
                } while (!allReady);

                //Record everyone's vote
                int[] counts = new int[8];
                foreach (var manager in _managers) {
                    counts[manager._characterIndex]++;
                }

                int max = 0;
                int maxCount = counts[0];
                bool tie = false;
                for (int i = 1; i < counts.Length; i++) {
                    if (counts[i] > maxCount) {
                        maxCount = counts[i];
                        max = i;
                    }

                    for (int j = 0; j < counts[i]; j++) {
                        if (i == j) continue;
                        if (counts[i] == counts[j]) {
                            tie = true;
                        }
                    }
                }

                Debug.Log($"Player {max} has {maxCount} votes");

                //Arresting the character with most votes
                if (maxCount != 0 && !tie) {
                    foreach (var manager in _managers) {
                        manager.ShowVotingUI(false);
                        if (manager.playerChar == max) {
                            manager.SetDetained();
                        }
                    }
                }

                GameManager.GamePaused = false;
                SendUpdate(MeetingInteractableFlags.SHOW, false.ToString());
                var gameManager = FindObjectOfType<GameManager>();
                gameManager.SendUpdate(GameManagerFlags.TIMESCALE, false.ToString());
                checkReady = false;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case MeetingInteractableFlags.SHOW:
                Debug.Log($"Setting UI to {value}");
                _managers = FindObjectsOfType<NetworkPlayerManager>();
                foreach (var player in _managers) {
                    player.ready = false;
                    player.ShowVotingUI(bool.Parse(value));
                }

                //Set up the voting system like the ready basically setting it on ready and voting
                break;
        }
    }

    public override void NetworkedStart() { }

    public override void OnUse() {
        _managers = FindObjectsOfType<NetworkPlayerManager>();
        if (!usable) return;
        usable = false;
        GameManager.GamePaused = true;
        var gameManager = FindObjectOfType<GameManager>();
        gameManager.SendUpdate(GameManagerFlags.TIMESCALE, true.ToString());
        checkReady = true;
        SendUpdate(MeetingInteractableFlags.SHOW, true.ToString());
        foreach (var manager in _managers) {
            manager.ready = false;
        }
    }
}