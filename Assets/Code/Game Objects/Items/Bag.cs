using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

struct BagFlags {
    public const string MONEY = "MONEY";
    public const string WEIGHT = "WEIGHT";
}

public class Bag : Item {
    private GameObject _owner;
    private readonly Vector2 _offsetMult = new Vector2(0.6f, 0.6f);
    public bool _hasOwner;
    public bool isTampered;
    [SerializeField] private float maxWeight = 50;

    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(1f);
    }

    public override void HandleMessage(string flag, string value) {
        switch (flag) {
            case BagFlags.MONEY:
                money = float.Parse(value);
                break;
            case BagFlags.WEIGHT:
                weight = float.Parse(value);
                break;
        }
    }

    public override void NetworkedStart() { }

    public bool AddItem(Item item) {
        if (item.weight + weight >= maxWeight) {
            return false;
        }

        weight += item.weight;
        money += item.money;

        SendUpdate(BagFlags.MONEY, money.ToString());
        SendUpdate(BagFlags.WEIGHT, weight.ToString());
        return true;
    }

    public void AssignOwner(GameObject owner) {
        _hasOwner = true;
        _owner = owner;
    }

    public void ReleaseOwner() {
        _hasOwner = false;
        _owner = null;
    }

    private void Update() {
        if (IsServer)
            if (_hasOwner) {
                transform.position = _owner.transform.position +
                                     _owner.transform.up * _offsetMult.y +
                                     _owner.transform.right * _offsetMult.x;
            }
    }
}