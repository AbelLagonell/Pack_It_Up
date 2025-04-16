using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HitboxSpawner))]
public class Cop : Actor {
    [SerializeField] private NavMeshAgent MyAgent;
    private bool CanFire;
    public static bool ChaseStart; //Update this when timer reaches zero from gamemanager to false
    //private List<GameObject> Players;
    private List<GameObject> Targets;
    private GameObject Target;

    public override void HandleMessage(string flag, string value) {

    }

    public override void NetworkedStart() {
        MyAgent = GetComponent<NavMeshAgent>();
        ChaseStart = false;
        MyAgent.speed = 5;
    }

    public override IEnumerator SlowUpdate() {

        while(true)
        {
            if(IsServer)
            {
                if(Targets != null)
                {
                    foreach(var target in Targets)
                    {
                        if(!target.GetComponent<Player>()._myNpm.inGame)
                        {
                            Targets.Remove(target);
                        }
                        if(!target.GetComponent<Player>()._myNpm.isInformant)
                        {
                            Targets.Remove(target);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    void OnTriggerStay(Collider c)
    {
        if(IsServer)
        {
            if(c.gameObject.CompareTag("Player"))
            {
                Debug.Log("Cop: Adding Player");
                if(Targets == null)
                {
                    Targets = new List<GameObject>
                    {
                        c.gameObject
                    };
                }
                else
                {
                    if(!Targets.Contains(c.gameObject) && !c.gameObject.GetComponent<Player>()._myNpm.isInformant)
                    {
                        Targets.Add(c.gameObject);
                    }

                    foreach(var target in Targets)
                    {
                        if(!target.GetComponent<Player>()._myNpm.inGame)
                        {
                            Targets.Remove(target);
                        }
                        if(!target.GetComponent<Player>()._myNpm.isInformant)
                        {
                            Targets.Remove(target);
                        }
                    }
                }

                Debug.Log("Cop: Searching For Player");
                FindClosestPlayer();

                if(Target != null)
                {
                    Debug.Log("Chasing Player");
                    MyAgent.SetDestination(Target.transform.position);
                }
            }
        }
    }

    private protected void FindClosestPlayer()
    {
        float Minimum = 100;

        //Find close players 
        foreach (var player in Targets)
        {
            MyAnimator.SetBool("walk", true);
            float Distance = UnityEngine.Vector3.Distance(MyAgent.transform.position, player.transform.position);
            foreach (var unit in Targets)
            {
                if(Distance < Minimum)
                {
                    Minimum = Distance;
                    Target = unit;
                }
            }
        }

        if(Targets.Count == 0)
        {
            MyAnimator.SetBool("walk", false);
            Minimum = 100;
            Target = null;
            MyAgent.SetDestination(transform.position);
            //Patrol?
        }
    }

    protected override void OnDeath() {
        //Cop die
        MyCore.NetDestroyObject(NetId);
    }

    public void FireBullets() {
        if (CanFire) {
            CanFire = false;
            AudioManager.Instance.PlaySFX("Cop_Fire");
            StartCoroutine(WaitFire());
        }
    }

    private IEnumerator WaitFire() {
        yield return new WaitForSeconds(2.5f);
        CanFire = true;
    }
}