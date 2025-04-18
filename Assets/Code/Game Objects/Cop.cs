using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Cop : Actor {
    [SerializeField] private NavMeshAgent MyAgent;
    [SerializeField] private GameObject Bullet;
    private bool CanFire;
    public static bool ChaseStart; //Update this when timer reaches zero from gamemanager to false
    private Vector3 Target;

    public override void HandleMessage(string flag, string value) {

    }

    public override void NetworkedStart() {
        MyAgent = GetComponent<NavMeshAgent>();
        ChaseStart = false;
        MyAgent.speed = 5;
        CanFire = true;
    }

    public override IEnumerator SlowUpdate() {

        while(true)
        {
            if(GameManager._gameStart) //Change this to when timer hits 0
            {
                ChaseStart = true;
            }

            if(ChaseStart && IsServer)
            {
                Debug.Log("Cop: Searching For Player");
                FindClosestPlayer();
                FireBullets();
            }
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    private protected void FindClosestPlayer()
    {
        Debug.Log("Cop Chase Player");
        float Minimum = 100;

        //Find close players 
        foreach (var player in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            MyAnimator.SetBool("walk", true);
            float Distance = UnityEngine.Vector3.Distance(MyAgent.transform.position, player.transform.position);
            foreach (var unit in FindObjectsByType<Player>(FindObjectsSortMode.None))
            {
                if(Distance < Minimum)
                {
                    Minimum = Distance;
                    Target = unit.transform.position;
                }
            }
        }

        MyAgent.SetDestination(Target);
    }

    protected override void OnDeath() {
        //Cop die
        MyCore.NetDestroyObject(NetId);
    }

    public void FireBullets() {
        if (CanFire && IsServer) {
            CanFire = false;
            AudioManager.Instance.PlaySFX("Cop_Fire");
            StartCoroutine(WaitFire());
            Debug.Log("Cop Fire Bullet");
            Transform GunSpawn = transform.GetChild(0).GetChild(0).gameObject.transform;
            MyCore.NetCreateObject(21, NetId, new Vector3(GunSpawn.position.x, GunSpawn.position.y - 2, GunSpawn.position.z), transform.rotation);
        }
    }

    private IEnumerator WaitFire() {
        yield return new WaitForSeconds(2.5f);
        CanFire = true;
    }
}