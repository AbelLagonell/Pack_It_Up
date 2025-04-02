using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : Actor {
    [SerializeField] private TMPro.TMP_Text timerText;
    [SerializeField] private Text PlayerName;
    [SerializeField] private string PName = "<Default>";
    private float localTimer = 61; //Find a way to sync this initially with the game manager
    private NetworkPlayerManager myNPM;
    
    struct PlayerFlags {
        public const string NAME = "NAME";
        public const string CHAR = "CHAR";
    }
    
    private void Start() {
        transform.GetChild(0).gameObject.SetActive(false);
        if (timerText == null) {
            Debug.LogError("Timer Text is not set");
        }
    }

    private void Update() {
        if (localTimer > 0) {
            localTimer -= Time.deltaTime;
            if (localTimer > 60) {
                var min = (int)localTimer / 60;
                var seconds = localTimer % 60;
                timerText.text = $"{min}:{seconds:F2}";
            } else {
                timerText.text = $"{localTimer:F2}";
            }
        } else {
            timerText.text = "GET OUT";
        }
    }

    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag)
        {
            case PlayerFlags.NAME:
                if(IsServer)
                {
                    SendUpdate(PlayerFlags.NAME, value);
                }
                if(IsClient)
                {
                    PlayerName.text = value;
                }
            break;
        }
        //TODO make necessary flags
    }

    public override void NetworkedStart() {
        if (!IsLocalPlayer) {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        foreach (var npm in FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None)) {
            if (npm.Owner == Owner) {
                myNPM = npm;
                //Get Whatever you need from it
            }
        }
        //TODO Make what happens when they connect to the server
    }

    protected override void OnDeath() {
        //TODO Make do a death thing
    }
}
