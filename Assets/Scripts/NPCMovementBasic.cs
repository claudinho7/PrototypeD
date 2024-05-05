using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public enum NpcState
{
    Idle, Patrol, Worker
}

public class NpcMovementBasic : MonoBehaviour
{
    public NpcState npcState;
    private Animator _animator;
    private NavMeshAgent _agent;
    public Transform pointA;
    public Transform pointB;
    private bool _goingToA;

    //animation cache
    private static readonly int IdleState = Animator.StringToHash("IdleState");
    private static readonly int WorkingState = Animator.StringToHash("WorkingState");
    private static readonly int IsIdle = Animator.StringToHash("IsIdle");
    private static readonly int IsPatrol = Animator.StringToHash("IsPatrol");
    private static readonly int IsWorking = Animator.StringToHash("IsWorker");
    private static readonly int Movement = Animator.StringToHash("Movement");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        switch (npcState)
        {
            case NpcState.Idle:
                _animator.SetBool(IsIdle, true);
                StartCoroutine(IdleStateChanger());
                break;
            case NpcState.Patrol:
                _animator.SetBool(IsPatrol, true);
                break;
            case NpcState.Worker:
                _animator.SetBool(IsWorking, true);
                StartCoroutine(WorkingStateChanger());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Update()
    {
        _animator.SetFloat(Movement, _agent.speed);
        if (npcState != NpcState.Patrol || _agent.pathPending || !(_agent.remainingDistance <= 0.2f)) return;
        _agent.SetDestination(_goingToA ? pointB.position : pointA.position);
        _goingToA = !_goingToA;
    }


    private IEnumerator IdleStateChanger()
    {
        while (true)
        {
            var randomIdleState = Random.Range(0, 100); // assuming 0 to 99
            // Set idle state 1 or 2 with 30% probability each
            _animator.SetInteger(IdleState, randomIdleState < 60 ? 0 : Random.Range(1, 3)); // Set idle state 0 with 70% probability
            
            var randomDuration = Random.Range(2, 10);
            for (var i = 0; i < randomDuration; i++)
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private IEnumerator WorkingStateChanger()
    {
        while (true)
        {
            var randomIdleState = Random.Range(0, 100); // assuming 0 to 99
            // Set idle state 1 or 2 with 30% probability each
            _animator.SetInteger(WorkingState, randomIdleState < 60 ? 0 : Random.Range(1, 3)); // Set idle state 0 with 70% probability
            
            var randomDuration = Random.Range(2, 10);
            for (var i = 0; i < randomDuration; i++)
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
