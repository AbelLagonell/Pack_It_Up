using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

struct GameManagerFlags {
    public const string GAMESTART = "GAMESTART";
    public const string GAMEOVER = "GAMEOVER";
}

public class GameManager : NetworkComponent {
    private bool _gameStart = false;
    private bool _gameOver = false;

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case GameManagerFlags.GAMESTART:
                Debug.Log("Started Game");
                if (IsClient) {
                    _gameStart = true;
                    foreach (NetworkPlayerManager npm in GameObject.FindObjectsByType<NetworkPlayerManager>(
                                 FindObjectsSortMode.None)) {
                        //TODO Hide Choosing color visuals
                        npm.GameStart();
                    }
                }

                break;
            case GameManagerFlags.GAMEOVER:
                if (IsClient) {
                    _gameOver = true;
                    foreach (NetworkPlayerManager npm in GameObject.FindObjectsByType<NetworkPlayerManager>(
                                 FindObjectsSortMode.None)) {
                        npm.GameEnd();
                    }
                    //TODO do client stuff
                }

                break;
        }
    }

    public override void NetworkedStart() { }

    public override IEnumerator SlowUpdate() {
        while (IsServer && !_gameStart) {
            bool allReady = true;
            var players = FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None);
            allReady = players.Length >= 2;
            foreach (var player in players) {
                if (!player.ready) allReady = false;
            }

            _gameStart = allReady;
            SendUpdate(GameManagerFlags.GAMESTART, _gameStart.ToString());

            yield return new WaitForSeconds(1f);
        }

        while (IsServer) {
            if (IsDirty) {
                SendUpdate(GameManagerFlags.GAMESTART, _gameStart.ToString());
                IsDirty = false;
            }

            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }
}