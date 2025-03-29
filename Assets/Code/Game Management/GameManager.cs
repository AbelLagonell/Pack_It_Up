using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEditor;

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
                    Debug.Log("Game Over");
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
        if (IsServer) {
            NetworkPlayerManager[] npms;
            bool allReady = true;
            do {
                npms = FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None);
                allReady = true;
                foreach (var player in npms) {
                    if (!player.ready) allReady = false;
                }

                yield return new WaitForSeconds(1f);
            } while (!allReady || npms.Length < 2);

            npms = FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None);
            foreach (var player in npms) {
                //TODO create player and set up for informant
            }
            
            SendUpdate(GameManagerFlags.GAMESTART, "1");
            MyCore.StopListening();

            /*while (!_gameOver) {
                //TODO do game logic here
                yield return new WaitForSeconds(.1f);
            }*/
            
            yield return new WaitForSeconds(5f);
            
            SendUpdate(GameManagerFlags.GAMEOVER, "1");
            yield return new WaitForSeconds(15f);
            
            MyId.NotifyDirty();
            StartCoroutine(MyCore.DisconnectServer());
        }
    }

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }
}