using System.Collections;
using System.Linq;
using UnityEngine;
using NETWORK_ENGINE;
using Unity.VisualScripting;

struct GameManagerFlags {
    public const string GAMESTART = "START";
    public const string GAMEPAUSED = "PAUSE";
    public const string GAMEOVER = "END";
    public const string TIMECHANGE = "CHANGE";
    public const string TIMESCALE = "SCALE";
}

public class GameManager : NetworkComponent {
    public static float GlobalTimer = 300; //in seconds
    public static bool[] CharsTaken;
    public static bool GamePaused = true;
    public static bool _gameStart;

    public GameEndCollider gameEndCollider;

    private bool _gameOver;
    private int _robberScore = 10;
    private int _informantScore = 0;
    private NetworkPlayerManager[] _npms;
    private int timeScale = 1;
    private bool _override = false;

    void Start() {
        CharsTaken = new bool[8];
        for (int i = 0; i < 8; i++) {
            CharsTaken[i] = false;
        }
        
        //gameEndCollider = GameObject.FindGameObjectWithTag("GameEnd").GetComponent<GameEndCollider>();
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case GameManagerFlags.GAMESTART:
                Debug.Log("Started Game");
                if (IsServer) {
                    _gameStart = true;
                }

                if (IsClient) {
                    _gameStart = true;
                    foreach (NetworkPlayerManager npm in GameObject.FindObjectsByType<NetworkPlayerManager>(
                                  FindObjectsSortMode.None)) {
                        //TODO Hide Choosing color visuals
                        npm.GameStart();
                        npm.MakeVotingUI(CharsTaken);
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
            case GameManagerFlags.GAMEPAUSED:
                if (IsClient) {
                    GamePaused = bool.Parse(value);
                }

                break;
            case GameManagerFlags.TIMECHANGE:

                if (IsClient) {
                    GlobalTimer += float.Parse(value);
                }

                break;
            case GameManagerFlags.TIMESCALE:
                if (IsClient) {
                    GamePaused = bool.Parse(value);
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
                    if (player.playerChar != 50 && !player.isSpawned) {
                        player.SpawnChar();
                    }
                }

                yield return new WaitForSeconds(1f);
            } while (!allReady || npms.Length < 2);

            _npms = FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None);

            //Setting informant
            //TODO Make this a random gen from 1 to the amount of players that there are and then have that be the informant
            _npms[0].SetInformant();
            yield return new WaitForSeconds(1.5f);

            _gameStart = true;
            SendUpdate(GameManagerFlags.GAMESTART, "1");
            MyCore.StopListening();


            //So that the players can read their roles
            yield return new WaitForSeconds(2f);
            foreach (var player in _npms) {
                player.transform.GetChild(0).gameObject.SetActive(true);
                player.ToggleRoleScreen(false);
                player.SendUpdate(NetworkPlayerManagerFlags.TIMERSTART, true.ToString());
            }

            GamePaused = false;
            SendUpdate(GameManagerFlags.GAMEPAUSED, GamePaused.ToString());


            while (!_gameOver) {
                if (GlobalTimer < 0) {
                    //Timer is over here
                    yield return new WaitForSeconds(.1f);
                }

                var undetained = _npms.Count(manager => !manager.player.IsDetained);
                
                //Make sure that all but the informant is in the end collider
                if ((undetained - 1) == gameEndCollider.AmountOfPlayers())
                    _gameOver = true;
                //Making sure that informant won
                else if ((undetained -1) == 0)
                    _override = true;

                yield return new WaitForSeconds(.1f);
            }

            (_robberScore, _informantScore) = gameEndCollider.TotalScores();

            //After Game ends but before score screen
            foreach (var player in _npms) {
                player.UpdateRScore(_robberScore);
                player.UpdateIScore(_informantScore);
                if (_override) player.OverrideWinner();
            }

            SendUpdate(GameManagerFlags.GAMEOVER, "1");
            yield return new WaitForSeconds(5f);


            MyId.NotifyDirty();
            StartCoroutine(MyCore.DisconnectServer());
        }
    }

    private void Update() {
        if (_gameStart && !GamePaused) GlobalTimer -= Time.deltaTime;
    }

    public void ChangeTime(float time) {
        GlobalTimer += time;
        SendUpdate(GameManagerFlags.TIMECHANGE, time.ToString());
    }
}