using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public struct CivFlags
{
    public const string HERO = "HERO";
    public const string DETAIN = "DETAIN";
    public const string NODETAIN = "NODETAIN";
    public const string ATTEMPT = "ATTEMPT";
}
public class Civilian : Actor
{
    [SerializeField] private NavMeshAgent MyAgent;

    [SerializeField] private bool IsHero;
    [SerializeField] private int CivType;
    private int[] Strength = {4,3,2,1};
    private int[] Speed = {8,7,6,10};
    private bool CanEscape, CanAttack;
    private List<GameObject> ClosePlayers;
    private GameObject ClosestPlayer;
    public override void HandleMessage(string flag, string value)
    {
        switch (flag) {
            case CivFlags.DETAIN:
                if(IsClient)
                {
                    //Needs to be detained on all clients. Let server know unit is detained as well?
                    IsDetained = true;
                    IsHero = false;
                    CanEscape = true;
                    MyAnimator.SetBool("detain", true);
                    AudioManager.Instance.PlaySFX("Civ_Detain");
                }
                if(IsServer)
                {
                    SendUpdate(CivFlags.DETAIN,"1");
                }
            break;
            case CivFlags.NODETAIN:
                if(IsClient)
                {
                    IsDetained = false;
                    MyAnimator.SetBool("detain", false);
                    if(CivType == 0 || CivType == 1)
                    {
                        IsHero = true;
                    }
                    CanEscape = false;
                    Debug.Log("Civ Has Escaped");
                    AudioManager.Instance.PlaySFX("Civ_Escape");
                }
                if(IsServer)
                {
                    SendUpdate(CivFlags.NODETAIN,"1");
                }
            break;
            case CivFlags.ATTEMPT:
                if(IsClient)
                {
                    MyAnimator.SetBool("attempt", true);
                    StartCoroutine(WaitAnimate());
                }
                if(IsServer)
                {
                    SendUpdate(CivFlags.ATTEMPT,"1");
                }
            break;
        }
    }

    public override void NetworkedStart()
    {
        CanAttack = true;
        IsDetained = true;
        IsHero = false;
        CanEscape = true;
        MyAgent = GetComponent<NavMeshAgent>();
        //MyAgent.updateRotation = false;
        //MyAgent.updateUpAxis = true;
        MyAgent.speed = Speed[CivType];
        MyAnimator.SetBool("walk", false);
        MyAnimator.SetBool("attack", false);
        MyAnimator.SetBool("detain", true);
        MyAnimator.SetBool("attempt", false);
    }

    public override IEnumerator SlowUpdate()
    {
        while(true) 
        {
            //Try to escape
            if(CanEscape && IsDetained && GameManager._gameStart)
            {
                if(IsServer)
                {
                    IsHero = false;
                    int EscapeCheck = UnityEngine.Random.Range(0,100);
                    Debug.Log("Attempt to Escape, rolled: " + Strength[CivType] * 10 + " / " + EscapeCheck);
                    if(EscapeCheck < Strength[CivType] * 100)
                    {
                        IsDetained = false;
                        MyAnimator.SetBool("detain", false);
                        if(CivType == 0 || CivType == 1)
                        {
                            IsHero = true;
                        }
                        CanEscape = false;
                        Debug.Log("Civ Has Escaped");
                        SendUpdate(CivFlags.NODETAIN, "1");
                    }
                    else
                    {
                        Debug.Log("Civ Wait Begins");
                        CanEscape = false;
                        SendUpdate(CivFlags.ATTEMPT, "1");
                        yield return Wait();
                    }
                }
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
        if(IsServer)
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

                Debug.Log("Searching For Player");
                FindClosestPlayer();
                if(IsHero)
                {
                    if(ClosestPlayer != null)
                    {
                        Debug.Log("Chasing Player");
                        MyAgent.SetDestination(ClosestPlayer.transform.position);
                    }
                    //Add player to close player list, chase closest
                }
                else
                {
                    float targetY;
                    float targetX;
                    if (transform.position.x > ClosestPlayer.transform.position.x)
                    {
                        targetX = ClosestPlayer.transform.position.x + 5;
                    }
                    else
                    {
                        targetX = ClosestPlayer.transform.position.x - 5;
                    }

                    if (transform.position.y > ClosestPlayer.transform.position.y)
                    {
                        targetY = ClosestPlayer.transform.position.y + 5;
                    }
                    else
                    {
                        targetY = ClosestPlayer.transform.position.y - 5;
                    }

                    MyAgent.SetDestination(new UnityEngine.Vector3(targetX, targetY, transform.position.z));
                    //Run
                }
            }
        }
    }

    void OnTriggerExit(Collider c)
    {
        if(IsServer)
        {
            if(c.gameObject.CompareTag("Player"))
            {
                if(ClosePlayers != null)
                {
                    if(ClosePlayers.Count > 0)
                    {
                        ClosePlayers.Remove(c.gameObject);
                    }
                    FindClosestPlayer();
                }  
            }
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if(IsServer)
        {
            if(c.gameObject.CompareTag("Player"))
            {
                if(CanAttack && IsHero)
                {
                    MyAnimator.SetBool("attack", true);
                    SendCommand("DAMAGE",c.gameObject.GetComponent<Player>()._myNpm.Owner.ToString());
                    CanAttack = false;
                    StartCoroutine(WaitAttack());
                }
            }
        }
    }
    
    public void OnAttack()
    {
        //Hero attack player
        MyAnimator.SetBool("attack", true);
        AudioManager.Instance.PlaySFX("Civ_Attack");

    }

    private protected void FindClosestPlayer()
    {
        float Minimum = 100;
        Debug.Log("Hero chasing closest player");
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
        if(!IsDetained && IsServer)
        {
            IsDetained = true;
            IsHero = false;
            CanEscape = true;
            MyAnimator.SetBool("detain", true);
            MyAgent.SetDestination(transform.position);
            HandleMessage(CivFlags.DETAIN,"1");
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
        yield return new WaitForSeconds(30 - Speed[CivType]);
        CanEscape = true;
    }

    private IEnumerator WaitAttack()
    {
        yield return new WaitForSeconds(1);
        MyAnimator.SetBool("attack", false);
        CanAttack = true;
    }

    private IEnumerator WaitAnimate()
    {
        yield return new WaitForSeconds(0.2f);
        MyAnimator.SetBool("attempt", false);
    }
}
