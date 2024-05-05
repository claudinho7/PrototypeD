using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class NpcDialogueBasic : MonoBehaviour
{
    private NpcMovementBasic _npcMovementBasic;
    private PlayerController _playerController;
    
    public GameObject dialoguePopUp;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI chatterText;
    private bool _isChatting;
    private string _randomLine;

    private bool _interacted;
    [SerializeField] private bool isAdvanced;
    [SerializeField] private bool isMayor;

    private void Awake()
    {
        _npcMovementBasic = GetComponent<NpcMovementBasic>();
    }

    private void Start()
    {
        _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void FixedUpdate()
    {
        if (!isAdvanced) return;
        if (!Physics.Raycast(transform.position, (_playerController.transform.position - transform.position).normalized,
                out var hit, 10f)) return;
        if (!hit.collider.CompareTag("Player") || _isChatting) return;
        NpcChatter();
        _isChatting = true;
    }

    private void NpcInteracting()
    {
        if (isAdvanced)
        {
            switch (_npcMovementBasic.npcState)
            {
                case NpcState.Worker:
                    dialogueText.text = "I'm busy, I'm sure you're looking for work as an adventurer with a powerful weapon like that one. You should find the Mayor.";
                    _playerController.npcInteracted = false;
                    break;
                case NpcState.Idle when isMayor:
                    dialogueText.text =
                        "I have heard people talking about a stranger with a fancy weapon walking around. Surely it will be easy killing the monster lurking at the outskirts of our town with that thing.\n\n Press E to GO or leave if you're not ready...";
                    _playerController.npcInteracted = true;
                    break;
                case NpcState.Idle:
                    dialogueText.text = "Sorry, I don't have any tasks for you...\nMaybe you should ask the Mayor, he usually needs help for people like you.";
                    _playerController.npcInteracted = false;
                    break;
                case NpcState.Patrol:
                    dialoguePopUp.SetActive(false);
                    _playerController.npcInteracted = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            switch (_npcMovementBasic.npcState)
            {
                case NpcState.Worker:
                    dialogueText.text = "Go away, I'm busy!";
                    _playerController.npcInteracted = false;
                    break;
                case NpcState.Idle when isMayor:
                    dialogueText.text =
                        "Are you looking for work hero? We need you to slay this monster lurking at the outskirts of our town.\n\n Press E to GO or leave if you're not ready...";
                    _playerController.npcInteracted = true;
                    break;
                case NpcState.Idle:
                    dialogueText.text = "Sorry, I don't have any tasks for you...";
                    _playerController.npcInteracted = false;
                    break;
                case NpcState.Patrol:
                    dialoguePopUp.SetActive(false);
                    _playerController.npcInteracted = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void NpcChatter()
    {
        switch (_npcMovementBasic.npcState)
        {
            case NpcState.Worker:
                var randomIndex = Random.Range(0, 3);
                _randomLine = randomIndex switch
                {
                    0 => "I wonder what my wife is doing while I'm here working...",
                    1 => "If this guys comes to bother me I will snap.",
                    2 => "If i had a weapon as expensive as that one I wouldn't have to work here...",
                    3 => "I can't wait to finish work and get home.",
                    _ => _randomLine
                };
                break;
            case NpcState.Idle when isMayor:
                var randomIndex2 = Random.Range(0, 3);
                _randomLine = randomIndex2 switch
                {
                    0 => "I really need some help right now.",
                    1 => "I wonder where all the adventurers have gone today!?",
                    2 => "Being the Mayor is so hard...",
                    3 => "Is this guy an adventurer? I could use some help now.",
                    _ => _randomLine
                };
                break;
            case NpcState.Idle:
                var randomIndex3 = Random.Range(0, 3);
                _randomLine = randomIndex3 switch
                {
                    0 => "Who is this person? Never seen him here before.",
                    1 => "Why is that weapon so out in the open? Have you no shame?",
                    2 => "Damn is so hot today.",
                    3 => "I should go get a drink.",
                    _ => _randomLine
                };
                break;
            case NpcState.Patrol:
                var randomIndex4 = Random.Range(0, 3);
                _randomLine = randomIndex4 switch
                {
                    0 => "Don't get too close stranger, I'm on duty.",
                    1 => "You look so dangerous, you should sign up and become a soldier like me.",
                    2 => "I wish I had a weapon like that.",
                    3 => "Patrolling the town is so boring...",
                    _ => _randomLine
                };
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        StartCoroutine(ChatterTimer());
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_npcMovementBasic.npcState == NpcState.Patrol) return;
        NpcInteracting();
        dialoguePopUp.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        dialoguePopUp.SetActive(false);
    }

    private IEnumerator ChatterTimer()
    {
        var timer = 10;

        while (timer > 5)
        {
            chatterText.text = _randomLine;
            timer -= 1;

            yield return new WaitForSeconds(1f);
        }

        while (timer is >= 0 and <= 5)
        {
            chatterText.text = "";
            timer -= 1;
            
            yield return new WaitForSeconds(1f);
        }
        
        _isChatting = false;
    }
}
