using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Cop : Actor
{
    [SerializeField] private NavMeshAgent MyAgent;
    private bool CanFire;
    private List<NetworkPlayerManager> Players;
    private List<NetworkPlayerManager> Targets;
    private NetworkPlayerManager Target;
    public override void HandleMessage(string flag, string value)
    {
        MyAgent.speed = 5;
    }

    public override void NetworkedStart()
    {
        MyAgent = FindObjectOfType<NavMeshAgent>();
        MyAnimator.SetBool("walk", true);
        //Find all Players, assign all but informant as targets
        int i = 0;
        foreach (var npm in FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None))
        {
            Players[i] = npm;
            if(!npm.isInformant)
            {
                Targets[i] = npm;
            }
        }

        int RandomTarget = Random.Range(0, Targets.Count() - 1);
        Target = Targets[RandomTarget];
    }

    public override IEnumerator SlowUpdate()
    {
        if(!Target.inGame && Target != null)
        {
            NewTarget();
        }

        if(Target != null)
        {
            ChasePlayer();
        }
        yield return new WaitForSeconds(MyCore.MasterTimer);
    }

    public void NewTarget()
    {
        foreach (var player in Targets)
        {
            if(!player.inGame)
            {
                Targets.Remove(player);
            }
        }

        if(Targets.Count() != 0)
        {
            int RandomTarget = Random.Range(0, Targets.Count() - 1);
            Target = Targets[RandomTarget];
        }
        else {
            Target = null;
            MyAnimator.SetBool("walk", false);
        }
    }

    public void ChasePlayer()
    {
        MyAgent.SetDestination(new Vector3(Target.transform.position.x, Target.transform.position.y, Target.transform.position.z));
    }

    protected override void OnDeath()
    {
        //Cop die
        MyCore.NetDestroyObject(NetId);
        StartCoroutine(Wait());
    }

    public void FireBullets()
    {
        if(CanFire)
        {
            //fire bullet
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSecondsRealtime(5);
        CanFire = true;
    }
}
