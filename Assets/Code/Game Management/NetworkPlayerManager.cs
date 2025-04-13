using System;
using System.Collections;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using NETWORK_ENGINE;
using TMPro;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

struct NetworkPlayerManagerFlags {
    public const string READY = "READY";
    public const string RSCORE = "SCORE-ROBBER";
    public const string ISCORE = "SCORE-INFORMANT";
    public const string WINNER = "WINNER";
    public const string ROLE = "ROLE";
    public const string SHOWROLE = "SHOWROLE";
    public const string PAUSE = "PAUSE";
    public const string NAME = "NAME";
    public const string CHAR = "CHAR";
    public const string CHARCHANGE = "CHARCHANGE";
    public const string TIMERSTART = "TIMERSTART";
    public const string SHOWVOTE = "SHOWVOTE";
    public const string GETVOTE = "GETVOTE";
}

public class NetworkPlayerManager : NetworkComponent {
    //Screens
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject startScreenBG;
    [SerializeField] private GameObject scoreScreen;
    [SerializeField] private GameObject robberRoleScreen;
    [SerializeField] private GameObject informantRoleScreen;
    [SerializeField] private GameObject GameUI;
    [SerializeField] private GameObject voteScreen;

    //TEXT
    [SerializeField] private TMP_Text robberScoreText;
    [SerializeField] private TMP_Text informantScoreText;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text timerText;

    //Voting
    public GameObject toggleGroup;
    public GameObject votePrefab;
    public int _characterIndex;

    //Values
    [SerializeField] private int robberScore;
    [SerializeField] private int informantScore;
    [SerializeField] private bool overrideWinner, showRole, showVote;
    [SerializeField] public string playerName;
    [SerializeField] private GameObject lobby;

    public int playerChar = 50;
    public bool isInformant;


    public bool isSpawned = false;
    public bool inGame = true; //Set to false upon death or escape
    public bool ready;

    public float localTimer;
    public bool timerStart;

    public Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        if (startScreen == null) {
            Debug.LogError("No Start Screen chosen");
        }

        if (scoreScreen == null) {
            Debug.LogError("No Score Screen chosen");
        }

        if (informantRoleScreen == null) {
            Debug.LogError("No Informant Role Screen chosen");
        }

        if (robberRoleScreen == null) {
            Debug.LogError("No Robber Role Screen chosen");
        }

        
        //Making sure that it always initializes the same way
        scoreScreen.SetActive(false);
        informantRoleScreen.SetActive(false);
        robberRoleScreen.SetActive(false);
        GameUI.SetActive(false);
        voteScreen.SetActive(false);
        
        startScreen.SetActive(true);
        startScreenBG.SetActive(true);
    }

    private void Update() {
        if (IsLocalPlayer) {
            if (timerStart) {
                GameUI.SetActive(true);
            }

            localTimer = GameManager.GlobalTimer;
            if (localTimer > 0) {
                if (localTimer > 60) {
                    var min = (int)localTimer / 60;
                    var seconds = localTimer % 60;
                    timerText.text = $"{min}:{seconds:00}";
                } else {
                    timerText.text = $"{localTimer:F2}";
                }
            } else {
                timerText.text = "GET OUT";
            }
        }
    }

    public override IEnumerator SlowUpdate() {
        while (IsConnected) {
            if (IsServer)
                if (IsDirty) {
                    SendUpdate(NetworkPlayerManagerFlags.NAME, playerName);
                    SendUpdate(NetworkPlayerManagerFlags.CHAR, playerChar.ToString());
                    SendUpdate(NetworkPlayerManagerFlags.READY, ready.ToString());
                    SendUpdate(NetworkPlayerManagerFlags.RSCORE, robberScore.ToString());
                    SendUpdate(NetworkPlayerManagerFlags.ISCORE, informantScore.ToString());
                    SendUpdate(NetworkPlayerManagerFlags.WINNER, overrideWinner.ToString());
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
            case NetworkPlayerManagerFlags.RSCORE:
                //Should only ever be run on clients
                robberScore = int.Parse(value);
                robberScoreText.text = "Score: " + robberScore;
                break;
            case NetworkPlayerManagerFlags.ISCORE:
                //Should only ever be run on clients
                informantScore = int.Parse(value);
                informantScoreText.text = "Score: " + informantScore;
                break;
            case NetworkPlayerManagerFlags.WINNER:
                //Should only ever be run on clients
                overrideWinner = bool.Parse(value);
                winnerText.text = "Winner are Informants\nEveryone got Arrested";
                break;
            case NetworkPlayerManagerFlags.ROLE:
                //Should only ever be run on clients
                if (IsLocalPlayer)
                    isInformant = bool.Parse(value);
                break;
            case NetworkPlayerManagerFlags.SHOWROLE:
                showRole = bool.Parse(value);
                ToggleRoleScreen(showRole);
                break;
            case NetworkPlayerManagerFlags.NAME:
                playerName = value;
                if (IsServer) {
                    SendUpdate(NetworkPlayerManagerFlags.NAME, value);
                }

                break;
            case NetworkPlayerManagerFlags.CHAR:
                playerChar = int.Parse(value);
                if (playerChar == 50) break;
                GameManager.CharsTaken[int.Parse(value)] = true;
                if (IsServer) {
                    SendUpdate(NetworkPlayerManagerFlags.CHAR, value);
                }

                break;
            case NetworkPlayerManagerFlags.CHARCHANGE:
                //Set previous character selected to false
                //Set current character to true
                break;
            case NetworkPlayerManagerFlags.TIMERSTART:
                timerStart = bool.Parse(value);
                break;
            case NetworkPlayerManagerFlags.GETVOTE:
                if (IsServer) {
                    _characterIndex = int.Parse(value);
                }

                break;

            case NetworkPlayerManagerFlags.SHOWVOTE:
                showVote = bool.Parse(value);
                ShowVotingUI(showVote);
                break;
        }
    }

    public override void NetworkedStart() {
        if (!IsLocalPlayer) {
            startScreen.SetActive(false);
        }
    }

    public void CharSelect(int c) {
        //Update Char Selected Variable
        //Send Command char selected
        //Update char selected on server side so no one picks the same character
        //Red = 0, Orange = 1, Yellow = 2, Green = 3, Blue = 4, Purple = 5, Pink = 6, Black = 7
        if (!GameManager.CharsTaken[c]) {
            startScreenBG.SetActive(false);
            if (IsLocalPlayer) {
                SendCommand(NetworkPlayerManagerFlags.CHAR, c.ToString());
            }
        }
    }

    public void UpdateName() { }

    public void NameSelect(string s) {
        if (IsLocalPlayer) {
            SendCommand(NetworkPlayerManagerFlags.NAME, s);
        }
    }

    public void SpawnChar() {
        //lobby.SetActive(true);
        GameObject temp = MyCore.NetCreateObject(playerChar, Owner, Vector3.zero, Quaternion.identity);
        isSpawned = true;
    }

    public void GameStart() {
        startScreen.SetActive(false);
        //lobby.SetActive(false);
        ToggleRoleScreen(true);
    }

    public void GameEnd() {
        if (overrideWinner) {
            winnerText.text = "Winner are Informants\nEveryone got Arrested";
        } else {
            winnerText.text = "Winner are " + (((robberScore - informantScore) < 0) ? "Informants" : "Robbers");
        }

        if (IsLocalPlayer) scoreScreen.SetActive(true);
    }

    public void UpdateIScore(int score) {
        SendUpdate(NetworkPlayerManagerFlags.ISCORE, score.ToString());
    }

    public void UpdateRScore(int score) {
        SendUpdate(NetworkPlayerManagerFlags.RSCORE, score.ToString());
    }

    public void OverrideWinner() {
        overrideWinner = true;
        SendUpdate(NetworkPlayerManagerFlags.WINNER, overrideWinner.ToString());
    }

    public void SetInformant() {
        isInformant = true;
        SendUpdate(NetworkPlayerManagerFlags.ROLE, isInformant.ToString());
    }

    public bool GetInformant() {
        return isInformant;
    }

    public void ToggleRoleScreen(bool value) {
        if (IsServer) {
            SendUpdate(NetworkPlayerManagerFlags.SHOWROLE, value.ToString());
        }

        if (isInformant)
            informantRoleScreen.SetActive(value);
        else
            robberRoleScreen.SetActive(value);
    }

    public void OnCheckBoxClick(bool value) {
        //Blank out box if character hasn't been picked. Functions properly but visually confuses user.
        if (IsLocalPlayer && playerChar != 50) {
            SendCommand(NetworkPlayerManagerFlags.READY, value.ToString());
        }
    }

    public void OnVoteReady(bool value) {
        if (IsLocalPlayer) {
            SendCommand(NetworkPlayerManagerFlags.READY, value.ToString());
            GetVoted();
        }
    }

    public void MakeVotingUI(bool[] characters) {
        for (int i = 0; i < characters.Length; i++) {
            if (characters[i]) {
                var temp = Instantiate(votePrefab, toggleGroup.transform);
                var voteObject = temp.GetComponent<VotingObject>();
                voteObject.characterIndex = i;
            }
        }
    }

    public void ShowVotingUI(bool value) {
        if (IsServer) {
            showVote = value;
            SendUpdate(NetworkPlayerManagerFlags.SHOWVOTE, value.ToString());
        }
        
        if (IsLocalPlayer) {
            voteScreen.SetActive(value);
        }
    }

    public void GetVoted() {
        RadioGroup rg = toggleGroup.GetComponent<RadioGroup>();
        _characterIndex = rg.GetSelected();
        SendCommand(NetworkPlayerManagerFlags.GETVOTE, _characterIndex.ToString());
    }

    public void SetDetained() {
        player.IsDetained = true;
        player.SetDetained(true);
    }
}