using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HitboxSpawner))]
public class Cop : Actor {
    [SerializeField] private NavMeshAgent MyAgent;
    private bool CanFire, CanAttack;
    public static bool ChaseStart; //Update this when timer reaches zero from gamemanager to false
    private List<NetworkPlayerManager> Players;
    private List<NetworkPlayerManager> Targets;
    private NetworkPlayerManager Target;

    public override void HandleMessage(string flag, string value) {
        MyAgent.speed = 5;
    }

    public override void NetworkedStart() {
        
        MyAgent = GetComponent<NavMeshAgent>();
        MyAnimator.SetBool("walk", true);
        ChaseStart = false;
    }

    public override IEnumerator SlowUpdate() {
        if(GameManager._gameStart && !ChaseStart)
        {
            int i = 0;
            foreach (var npm in FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None)) {
                Players[i] = npm;
                if (!npm.isInformant) {
                    Targets[i] = npm;
                }
            }

            int RandomTarget = Random.Range(0, Targets.Count() - 1);
            Target = Targets[RandomTarget];
            ChaseStart = true;
        }
        
        if (Target == null) {
            NewTarget();
        }
        else
        {
            ChasePlayer();
        }

        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    public void NewTarget() {
        if(Targets != null)
        {
            foreach (var player in Targets) {
                if (!player.inGame) {
                    Targets.Remove(player);
                }
            }

            if (Targets.Count() != 0) 
            {
                int RandomTarget = Random.Range(0, Targets.Count() - 1);
                Target = Targets[RandomTarget];
            } 
            else 
            {
                Target = null;
                MyAnimator.SetBool("walk", false);
            }
        }
        else 
        {
            Target = null;
            MyAnimator.SetBool("walk", false);
        }
    }

    public void ChasePlayer() {
        MyAnimator.SetBool("walk", true);
        MyAgent.SetDestination(new Vector3(Target.transform.position.x, Target.transform.position.y, Target.transform.position.z));
        Debug.Log("All Robbers have escaped or died.");
    }

    protected override void OnDeath() {
        //Cop die
        MyCore.NetDestroyObject(NetId);
    }

    public void FireBullets() {
        if (CanFire) {
            CanFire = false;
            StartCoroutine(WaitFire());
        }
    }

    private IEnumerator WaitFire() {
        yield return new WaitForSeconds(2.5f);
        CanFire = true;
    }
}