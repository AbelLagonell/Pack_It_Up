using System.Collections;
using UnityEngine;

struct BagFlags {
    public const string MONEY = "MONEY";
    public const string WEIGHT = "WEIGHT";
}

public class Bag : Item {
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

}