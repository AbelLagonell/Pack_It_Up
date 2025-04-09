using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public struct CivFlags
{
    public const string HERO = "HERO";
    public const string DETAIN = "DETAIN";
}
public class Civilian : Actor
{
    [SerializeField] private NavMeshAgent MyAgent;

    [SerializeField] private bool IsHero;
    [SerializeField] private int CivType;
    private int[] Strength = {4,3,2,1};
    private int[] Speed = {10,9,9,10};
    private int Radius1 = 4;
    private bool CanEscape;
    private NetworkPlayerManager[] Players;
    private List<NetworkPlayerManager> ClosePlayers;
    private NetworkPlayerManager ClosestPlayer;
    public override void HandleMessage(string flag, string value)
    {
        switch (flag) {
            case CivFlags.DETAIN:
                //Needs to be detained on all clients. Let server know unit is detained as well?
                IsDetained = true;
                IsHero = false;
            break;
        }
    }

    public override void NetworkedStart()
    {
        MyAgent = FindObjectOfType<NavMeshAgent>();
        MyAgent.speed = Speed[CivType];
        MyAnimator.SetBool("walk", false);
        MyAnimator.SetBool("attack", false);
        MyAnimator.SetBool("detain", true);

        //Find all Players
        int i = 0;
        foreach (var npm in FindObjectsByType<NetworkPlayerManager>(FindObjectsSortMode.None))
        {
            Players[i] = npm;
        }
    }

    public override IEnumerator SlowUpdate()
    {
        if(!IsDetained)
        {
            //Find Players if not Detained
            CheckPlayers();
        }
        else
        {
            IsHero = false;
            //Try to escape
            if(CanEscape && IsDetained)
            {
                int EscapeCheck = Random.Range(0,100);
                if(EscapeCheck < Strength[CivType] * 5)
                {
                    IsDetained = false;
                    if(CivType == 1 || CivType == 2)
                    {
                        IsHero = true;
                    }
                }
                CanEscape = false;
                StartCoroutine(Wait());
            }
        }
        while(true)
        {
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    protected override void OnDeath()
    {
        //Subract punishment time from global timer then syncronize, don't know exactly how to do it.
        GameManager.GlobalTimer = GameManager.GlobalTimer - 60;
        MyCore.NetDestroyObject(NetId);
    }

    public void CheckPlayers()
    {
        //Find all players within radius
        FindClosestPlayer();

        if(IsHero)
        {
            //Attack closest player if in radius
            if(ClosestPlayer != null)
            {
                MyAnimator.SetBool("walk", true);
                MyAgent.SetDestination(ClosestPlayer.transform.position);
            }
        }
        else 
        {
            //Run away from closest player if in radius
            if(ClosestPlayer != null)
            {
                MyAnimator.SetBool("walk", true);
                float targetZ = transform.position.z;
                float targetX;
                if (transform.position.x > ClosestPlayer.transform.position.x)
                {
                    targetX = ClosestPlayer.transform.position.x + 5;
                }
                else
                {
                    targetX = ClosestPlayer.transform.position.x - 5;
                }

                if (transform.position.z > ClosestPlayer.transform.position.z)
                {
                    targetX = ClosestPlayer.transform.position.z + 5;
                }
                else
                {
                    targetX = ClosestPlayer.transform.position.z - 5;
                }

                MyAgent.SetDestination(new UnityEngine.Vector3(targetX, transform.position.y, targetZ));
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(CompareTag("PLAYER"))
        {
            //Attack
            //Send Message to Damage player
        }
    }

    public void OnAttack()
    {
        //Hero attack player
        MyAnimator.SetBool("attack", true);
    }

    private void FindClosestPlayer()
    {
        float Minimum = 100;

        //Find close players 
        foreach (var player in Players)
        {
            float Distance = UnityEngine.Vector3.Distance(MyAgent.transform.position, player.gameObject.transform.position);
            if(Distance <= Radius1)
            {
                ClosePlayers.Add(player);
                foreach (var unit in ClosePlayers)
                {
                    if(Distance < Minimum)
                    {
                        Minimum = Distance;
                        ClosestPlayer = unit;
                    }
                }
            }

            if(Distance > Radius1)
            {
                ClosePlayers.Remove(player);
            }
        }

        if(ClosePlayers.Count == 0)
        {
            MyAnimator.SetBool("walk", false);
            Minimum = 100;
            ClosestPlayer = null;
            MyAgent.SetDestination(transform.position);
            //Patrol?
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
        yield return new WaitForSecondsRealtime(30 - Speed[CivType]);
        CanEscape = true;
    }
}
