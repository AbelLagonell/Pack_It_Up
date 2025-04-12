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
    private bool CanEscape;
    private List<GameObject> ClosePlayers;
    private GameObject ClosestPlayer;
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
        IsDetained = false;
        IsHero = true;
        MyAgent = GetComponent<NavMeshAgent>();
        //MyAgent.updateRotation = false;
        //MyAgent.updateUpAxis = true;
        MyAgent.speed = Speed[CivType];
        MyAnimator.SetBool("walk", false);
        MyAnimator.SetBool("attack", false);
        MyAnimator.SetBool("detain", true);
    }

    public override IEnumerator SlowUpdate()
    {
        while(true) 
        {
            //Try to escape
            if(CanEscape && IsDetained)
            {
                IsHero = false;
                int EscapeCheck = Random.Range(0,100);
                if(EscapeCheck < Strength[CivType] * 5)
                {
                    IsDetained = false;
                    MyAnimator.SetBool("detain", false);
                    if(CivType == 1 || CivType == 2)
                    {
                        IsHero = true;
                    }
                }
                CanEscape = false;
                StartCoroutine(Wait());
            }
            yield return new WaitForSeconds(MyCore.MasterTimer);
        }
    }

    protected override void OnDeath()
    {
        //Subract punishment time from global timer then syncronize, don't know exactly how to do it.
        GameManager.GlobalTimer = GameManager.GlobalTimer - 60;
        MyCore.NetDestroyObject(NetId);
    }

    void OnTriggerEnter(Collider c)
    {
        if(c.gameObject.CompareTag("Player") && !IsDetained)
        {
            Debug.Log("Adding Player");
            if(ClosePlayers == null)
            {
                ClosePlayers = new List<GameObject>
                {
                    c.gameObject
                };
            }
            else
            {
                if(!ClosePlayers.Contains(c.gameObject))
                {
                    ClosePlayers.Add(c.gameObject);
                }
            }

            Debug.Log("Searching Player");
            FindClosestPlayer();
            if(IsHero)
            {
                if(ClosestPlayer != null)
                {
                    Debug.Log("Chasing Player");
                    MyAgent.SetDestination(ClosestPlayer.transform.position);
                    // Make NPC look at player transform.rotation = 
                }
                //Add player to close player list, chase closest
            }
            else
            {
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
                //Run
            }
        }
    }

    void OnTriggerExit(Collider c)
    {
        if(c.CompareTag("Player"))
        {
            if(ClosePlayers.Count > 0)
            {
                ClosePlayers.Remove(c.gameObject);
            }
            FindClosestPlayer();
        }
    }

    /*
        void OnCollisionEnter(Collision collision)
        {
            if(CompareTag("PLAYER"))
            {
                //Attack
                //Send Message to Damage player
            }
        }
    */
    public void OnAttack()
    {
        //Hero attack player
        MyAnimator.SetBool("attack", true);
    }

    private void FindClosestPlayer()
    {
        float Minimum = 100;

        //Find close players 
        foreach (var player in ClosePlayers)
        {
            MyAnimator.SetBool("walk", true);
            float Distance = UnityEngine.Vector3.Distance(MyAgent.transform.position, player.transform.position);
            foreach (var unit in ClosePlayers)
            {
                if(Distance < Minimum)
                {
                    Minimum = Distance;
                    ClosestPlayer = unit;
                }
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

    public void Detain()
    {
        if(!IsDetained)
        {
            IsDetained = true;
            IsHero = false;
            MyAnimator.SetBool("detain", true);
            MyAgent.SetDestination(transform.position);
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
