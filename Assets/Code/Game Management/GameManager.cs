using System.Collections;
using System.Linq;
using UnityEngine;
using NETWORK_ENGINE;
using System.Threading;

struct GameManagerFlags {
    public const string GAMESTART = "START";
    public const string GAMEPAUSED = "PAUSE";
    public const string GAMEOVER = "END";
    public const string TIMECHANGE = "CHANGE";
    public const string TIMESCALE = "SCALE";
    public const string TIMEOUT = "TIMEOUT";
}

public class GameManager : NetworkComponent {
    public static float GlobalTimer; //in seconds
    public static bool[] CharsTaken;
    public static bool GamePaused = true;
    public static bool _gameStart;
    public static bool _timeOut;

    public float TotalGameTime = 20;
    public GameEndCollider gameEndCollider;
    public int minPlayers = 1;

    private bool _gameOver;
    private int _robberScore = 10;
    private int _informantScore = 0;
    private NetworkPlayerManager[] _npms;
    private bool _override = false;

    void Start() {
        CharsTaken = new bool[8];
        for (int i = 0; i < 8; i++) {
            CharsTaken[i] = false;
        }

        GlobalTimer = TotalGameTime;
        Debug.Log(GlobalTimer);
        //gameEndCollider = GameObject.FindGameObjectWithTag("GameEnd").GetComponent<GameEndCollider>();
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case GameManagerFlags.GAMESTART:
                Debug.Log("Started Game");
                if (IsServer) {
                    _gameStart = true;
                    SendUpdate(GameManagerFlags.GAMESTART, "1");
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
        
        gameEndCollider = (GameEndCollider)FindAnyObjectByType(typeof(GameEndCollider));
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
            } while (!allReady || npms.Length < minPlayers);

            _npms = FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None);
            //Setting informant
            _npms[Random.Range(0, _npms.Length)].SetInformant();
            yield return new WaitForSeconds(1f);

            _gameStart = true;
            SendUpdate(GameManagerFlags.GAMESTART, "1");
            MyCore.NotifyGameStart();


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
                var undetained = _npms.Count(manager => !manager.player.IsDetained);

                //If ever the informant gets everyone
                if (undetained == 1 && minPlayers != 1) {
                    _gameOver = true;
                    _override = true;
                }

                if (GlobalTimer < 0) {
                    //RIGHT HERE
                    gameEndCollider.MyCollider.enabled = true;
                    _timeOut = true;
                    SendUpdate(GameManagerFlags.TIMEOUT, "1");
                    //Make sure that all but the informant is in the end collider
                    if ((undetained - 1) == gameEndCollider.AmountOfPlayers(false))
                        _gameOver = true;
                    //! We can make it so that even if they are detained but are all there it still good but idk
                }


                yield return new WaitForSeconds(.1f);
            }

            yield return new WaitForSeconds(1f);

            (_robberScore, _informantScore) = gameEndCollider.TotalScores();

            //After Game ends but before score screen
            foreach (var player in _npms) {
                player.UpdateRScore(_robberScore);
                player.UpdateIScore(_informantScore);
                if (_override) player.OverrideWinner();
                else player.SendUpdate(NetworkPlayerManagerFlags.PLAYEND, (_robberScore > _informantScore).ToString());
            }


            //Game Over
            SendUpdate(GameManagerFlags.GAMEOVER, "1");

            int send = 0;
            if (_override) send = -1;
            else if (_robberScore > _informantScore) send = 0;
            else if (_robberScore < _informantScore) send = 1;

            foreach (NetworkPlayerManager npm in _npms) {
                npm.SendUpdate(NetworkPlayerManagerFlags.GAMEEND, send.ToString());
            }

            yield return new WaitForSeconds(10f);


            MyId.NotifyDirty();
            MyCore.UI_Quit();
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