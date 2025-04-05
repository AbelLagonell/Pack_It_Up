using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

struct PlayerFlags {
    public const string NAME = "NAME";
    public const string CHAR = "CHAR";
    public const string BAG = "BAG";
}


public class Player : Actor {
    [SerializeField] private Text PlayerName;
    [SerializeField] private string PName = "<Default>";
    private NetworkPlayerManager _myNpm;

    public bool hasBag;
    private Bag _currentBag;

    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case PlayerFlags.NAME:
                if (IsServer) {
                    SendUpdate(PlayerFlags.NAME, value);
                }

                if (IsClient) {
                    PlayerName.text = value;
                }

                break;
        }
        //TODO make necessary flags
    }

    public override void NetworkedStart() {
        if(IsLocalPlayer)
        {
            SendCommand(PlayerFlags.NAME, PName);
        }
        foreach (var npm in FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None)) {
            if (npm.Owner == Owner) {
                _myNpm = npm;
                //Get Whatever you need from it
            }
        }
        //TODO Make what happens when they connect to the server
    }

    protected override void OnDeath() {
        //TODO Make do a death thing
    }

    public void AssignBag(Bag bag) {
        _currentBag = bag;
        _currentBag.AssignOwner(gameObject);
        hasBag = true;
    }

    public void AddItem(Item item) {
        _currentBag.AddItem(item);
    }

    public void ReleaseBag() {
        //To be called on release
        hasBag = false;
    }
}