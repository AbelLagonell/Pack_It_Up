<<<<<<< HEAD
using System;
=======
>>>>>>> origin/main
using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

struct GameManagerFlags {
    public const string GAMESTART = "GAMESTART";
    public const string GAMEOVER = "GAMEOVER";
}

public class GameManager : NetworkComponent {
<<<<<<< HEAD
    [SerializeField] private float globalTimer = 70; //in seconds

    private bool _gameStart;
    private bool _gameOver;
    private bool _gamePaused;
    private int _robberScore = 10;
    private int _informantScore = 0;


=======
    private bool _gameStart = false;
    private bool _gameOver = false;
    private int _robberScore = 10;
    private int _informantScore = 0;

>>>>>>> origin/main
    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case GameManagerFlags.GAMESTART:
                Debug.Log("Started Game");
                if (IsClient) {
                    _gameStart = true;
                    foreach (NetworkPlayerManager npm in GameObject.FindObjectsByType<NetworkPlayerManager>(
<<<<<<< HEAD
                                 FindObjectsSortMode.None)) {
=======
                                  FindObjectsSortMode.None)) {
>>>>>>> origin/main
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
<<<<<<< HEAD
                                 FindObjectsSortMode.None)) {
=======
                                  FindObjectsSortMode.None)) {
>>>>>>> origin/main
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
<<<<<<< HEAD
                GameObject temp = MyCore.NetCreateObject(0, player.Owner, Vector3.zero, Quaternion.identity);
                //TODO create player
            }

            //Setting informant
            //TODO Make this a random gen from 1 to the amount of players that there are and then have that be the informant
            npms[0].SetInformant();
            yield return new WaitForSeconds(1f);

            _gameStart = true;
            SendUpdate(GameManagerFlags.GAMESTART, "1");
            MyCore.StopListening();

=======
                //TODO create player
            }
            
            //Setting informant
            //TODO Make this a random gen from 1 to the amount of players that there are and then have that be the informant
            npms[0].SetInformant();
            yield return new WaitForSeconds(0.5f);
            
            SendUpdate(GameManagerFlags.GAMESTART, "1");
            MyCore.StopListening();
            
>>>>>>> origin/main
            //So that the players can read their roles
            yield return new WaitForSeconds(2f);
            foreach (var player in npms) {
                player.ToggleRoleScreen(false);
<<<<<<< HEAD
                
            }

            while (!_gameOver) {
                if (globalTimer < 0) {
                    //Timer is over here
                    _gameOver = true;
                }

                //TODO do game logic here
                yield return new WaitForSeconds(.1f);
            }

=======
            }
            
            /*while (!_gameOver) {
                //TODO do game logic here
                yield return new WaitForSeconds(.1f);
            }*/
            
            yield return new WaitForSeconds(1f);
            
>>>>>>> origin/main
            //After Game ends but before score screen
            foreach (var player in npms) {
                player.UpdateRScore(_robberScore);
                player.UpdateIScore(_informantScore);
                player.OverrideWinner();
            }
<<<<<<< HEAD

            SendUpdate(GameManagerFlags.GAMEOVER, "1");
            yield return new WaitForSeconds(5f);

=======
            
            SendUpdate(GameManagerFlags.GAMEOVER, "1");
            yield return new WaitForSeconds(15f);
            
>>>>>>> origin/main
            MyId.NotifyDirty();
            StartCoroutine(MyCore.DisconnectServer());
        }
    }

<<<<<<< HEAD
    private void Update() {
        if (_gameStart && !_gamePaused) globalTimer -= Time.deltaTime;
    }
=======
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }
>>>>>>> origin/main
}