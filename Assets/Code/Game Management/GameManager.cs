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
                if (IsClient) {
                    _gameStart = true;
                    foreach (NetworkPlayerManager npm in GameObject.FindObjectsByType<NetworkPlayerManager>(
                                 FindObjectsSortMode.None)) {
                        //TODO Hide Choosing color visuals
                    }
                }
                
                break;
            case GameManagerFlags.GAMEOVER:
                if (IsClient) {
                    //TODO do client stuff
                }

                break;
        }
    }

    public override void NetworkedStart() { }

    public override IEnumerator SlowUpdate() {
        if (IsServer) {
            //Wait for players to get ready
            NetworkPlayerManager[] players;
            bool tempGameStart = true;
            do {
                players = FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None);
                foreach (NetworkPlayerManager player in players) {
                    if (!player.Ready) { tempGameStart = false; }
                }

                yield return new WaitForSeconds(1f);
            } while (!tempGameStart || players.Length < 2);


            //Start Game
            SendUpdate(GameManagerFlags.GAMESTART, "1");
            MyCore.StopListening();


            //Go to each NetworkPlayerManager and look at their options
            //Create the appropriate character for their options
            //GameObject temp = MyCore.NetCreateObject(1,Owner,new Vector3);
            //temp.GetComponent<MyCharacterScript>().team = //set the team;
            players = GameObject.FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None);
            foreach (NetworkPlayerManager player in players) {
                //TODO Set player state 
            }
            
            
            
            while (!_gameOver) {
                //Game states that can be handled by game manager
                //TODO Decrement timer
            }
            
            SendUpdate(GameManagerFlags.GAMEOVER, "1");
            yield return new WaitForSeconds(30f);

            MyId.NotifyDirty();
            StartCoroutine(MyCore.DisconnectServer());
        }

        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }
}