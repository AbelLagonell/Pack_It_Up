using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

struct GameManagerFlags {
    public const string GAMESTART = "GAMESTART";
    public const string GAMEOVER = "GAMEOVER";
}

public class GameManager : NetworkComponent {
    [SerializeField] private float globalTimer = 70; //in seconds

    public static bool[] CharsTaken;
    
    private bool _gameStart;
    private bool _gameOver;
    private bool _gamePaused;
    private int _robberScore = 10;
    private int _informantScore = 0;

    void Start()
    {
        CharsTaken = new bool[8];
        for (int i = 0; i < 8; i++)
        {
            CharsTaken[i] = false;
        }
    }

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
                    if(player.playerChar != 0 && !player.isSpawned)
                    {
                        player.SpawnChar();
                    }
                }

                yield return new WaitForSeconds(1f);
            } while (!allReady || npms.Length < 2);

            npms = FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None);

            //Setting informant
            //TODO Make this a random gen from 1 to the amount of players that there are and then have that be the informant
            npms[0].SetInformant();
            yield return new WaitForSeconds(1f);

            _gameStart = true;
            SendUpdate(GameManagerFlags.GAMESTART, "1");
            MyCore.StopListening();


            //So that the players can read their roles
            yield return new WaitForSeconds(2f);
            foreach (var player in npms) {
                player.transform.GetChild(0).gameObject.SetActive(true);
                player.ToggleRoleScreen(false);
                
            }

            while (!_gameOver) {
                if (globalTimer < 0) {
                    //Timer is over here
                    _gameOver = true;
                }

                //TODO do game logic here
                yield return new WaitForSeconds(.1f);
            }


            //After Game ends but before score screen
            foreach (var player in npms) {
                player.UpdateRScore(_robberScore);
                player.UpdateIScore(_informantScore);
                player.OverrideWinner();
            }

            SendUpdate(GameManagerFlags.GAMEOVER, "1");
            yield return new WaitForSeconds(5f);


            MyId.NotifyDirty();
            StartCoroutine(MyCore.DisconnectServer());
        }
    }

    private void Update() {
        if (_gameStart && !_gamePaused) globalTimer -= Time.deltaTime;
    }

}
