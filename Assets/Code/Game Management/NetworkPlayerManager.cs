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
    public const string ROLE = "ROLE";
    public const string SHOWROLE = "SHOWROLE";
}

public class NetworkPlayerManager : NetworkComponent {
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject scoreScreen;
    [SerializeField] private GameObject robberRoleScreen;
    [SerializeField] private GameObject informantRoleScreen;
    [SerializeField] private TMP_Text robberScoreText;    //positive
    [SerializeField] private TMP_Text informantScoreText; //negative
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private int robberScore;
    [SerializeField] private int informantScore;
    [SerializeField] private bool overrideWinner, isInformant, showRole;

    public bool ready;

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

        scoreScreen.SetActive(false);
        informantRoleScreen.SetActive(false);
        robberRoleScreen.SetActive(false);
    }

    public override IEnumerator SlowUpdate() {
        while (IsConnected) {
            if (IsServer)
                if (IsDirty) {
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
        }
    }

    public override void NetworkedStart() {
        if (!IsLocalPlayer) {
            startScreen.SetActive(false);
        }
    }

    public void GameStart() {
        startScreen.SetActive(false);
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
        if (IsLocalPlayer) {
            SendCommand(NetworkPlayerManagerFlags.READY, value.ToString());
        }
    }
}