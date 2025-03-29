using System.Collections;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using NETWORK_ENGINE;
using TMPro;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Serialization;

struct NetworkPlayerManagerFlags {
    public const string READY = "READY";
    public const string RSCORE = "SCORE-ROBBER";
    public const string ISCORE = "SCORE-INFORMANT";
    public const string WINNER = "WINNER";
}

public class NetworkPlayerManager : NetworkComponent {
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject scoreScreen;
    [SerializeField] private TMP_Text robberScore;    //positive
    [SerializeField] private TMP_Text informantScore; //negative
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private int robbers, informants;
    [SerializeField] private bool overrideWinner = false;

    public bool ready;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        if (startScreen == null) {
            Debug.LogError("No Start Screen chosen");
        }

        if (scoreScreen == null) {
            Debug.LogError("No Score Screen chosen");
        }

        scoreScreen.SetActive(false);
    }

    public override IEnumerator SlowUpdate() {
        while (IsConnected) {
            if (IsServer)
                if (IsDirty) {
                    SendUpdate(NetworkPlayerManagerFlags.READY, ready.ToString());
                    SendUpdate(NetworkPlayerManagerFlags.RSCORE, robbers.ToString());
                    SendUpdate(NetworkPlayerManagerFlags.ISCORE, informants.ToString());
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
                Debug.Log("Recieved" + value);
                robbers = int.Parse(value);
                robberScore.text = "Score: " + robbers;
                break;
            case NetworkPlayerManagerFlags.ISCORE:
                Debug.Log("Recieved" + value);
                informants = int.Parse(value);
                informantScore.text = "Score: " + informants;
                break;
            case NetworkPlayerManagerFlags.WINNER:
                //Should only ever be run on clients
                overrideWinner = bool.Parse(value);
                winnerText.text = "Winner is Informants\nEveryone got Arrested";
                break;
        }
    }

    public override void NetworkedStart() {
        if (!IsLocalPlayer) {
            startScreen.SetActive(false);
        }
    }

    public void GameStart() {
        startScreen.SetActive(false);
    }

    public void GameEnd() {
        if (overrideWinner) {
            winnerText.text = "Winner is Informants\nEveryone got Arrested";
        } else {
            winnerText.text = "Winner is " + (((robbers - informants) < 0) ? "Informants" : "Robbers");
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

    public void OnCheckBoxClick(bool value) {
        if (IsLocalPlayer) {
            SendCommand(NetworkPlayerManagerFlags.READY, value.ToString());
        }
    }
}