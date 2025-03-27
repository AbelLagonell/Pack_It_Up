using System.Collections;
using NETWORK_ENGINE;
using UnityEngine;

public class NetworkPlayerManager : NetworkComponent {

    public bool Ready { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override IEnumerator SlowUpdate() {
        yield return new WaitForSeconds(MyCore.MasterTimer);
    }
    public override void HandleMessage(string flag, string value) {
    }
    public override void NetworkedStart() {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
