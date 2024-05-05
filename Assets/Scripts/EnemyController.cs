using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    private Animator _animator;
    private NavMeshAgent _agent;

    [Header("ThisNPC")]
    public bool isAdvanced;
    
    [Header("Player")]
    public GameObject player;
    public float distanceToPlayer;
    
    [Header("Stats")]
    public float health;
    public float stamina;
    private const float MaxHealth = 100;
    private const float MaxStamina = 100;
    public float damage;
    public bool shouldDodge;

    [Header("Projectile")]
    public Transform projectileSpawnLoc;
    public GameObject projectileObj;
    
    [Header("UI")]
    public Image healthBar;
    public Image staminaBar;
    
    private bool _isMoving;
    private bool _isAttacking;
    private float _lastActionTime;
    private bool _isDead;
    private bool _hasIframes;
    private int _actionCounter;
    
    //animation cache
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Melee1 = Animator.StringToHash("Melee1");
    private static readonly int Melee2 = Animator.StringToHash("Melee2");
    private static readonly int Melee3 = Animator.StringToHash("Melee3");
    private static readonly int Ranged1 = Animator.StringToHash("Ranged1");
    private static readonly int Ranged2 = Animator.StringToHash("Ranged2");
    private static readonly int Ranged3 = Animator.StringToHash("Ranged3");
    private static readonly int Dodge = Animator.StringToHash("Dodge");
    private static readonly int Died = Animator.StringToHash("Died");
    private static readonly int Pushing = Animator.StringToHash("Pushing");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }
    
    private void Start()
    {
        health = MaxHealth;
        stamina = MaxStamina;

        if (!isAdvanced)
        {
            staminaBar.enabled = false;
        }
    }

    private void Update()
    {
        //movement animation
        if (!_isDead)
        {
            _animator.SetFloat(Speed, _agent.velocity.magnitude);
        }
        
        // Calculate the distance between this object and the player
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (!isAdvanced)
        {
            switch (distanceToPlayer)
            {
                case <= 20f and > 10f when (!_isMoving && !_isAttacking) && !_isDead:
                    StartCoroutine(MoveInRanged());
                    break;
                case <= 10f and > 6f when (!_isMoving && !_isAttacking) && !_isDead:
                    if (_actionCounter < 2)
                    {
                        DoRanged1();
                        _actionCounter += 1;
                    }
                    else
                    {
                        DoRanged2();
                        _actionCounter = 0;
                    }
                    break;
                case <= 6f and > 1.1f when (!_isMoving && !_isAttacking) && !_isDead:
                    StartCoroutine(MoveInMelee());
                    break;
                case <= 1.1f when (!_isMoving && !_isAttacking) && !_isDead:
                    if (_actionCounter < 2)
                    {
                        DoMelee1();
                        _actionCounter += 1;
                    }
                    else
                    {
                        DoMelee2();
                        _actionCounter = 0;
                    }
                    break;
            }

            if (Camera.main == null) return;
            healthBar.GetComponent<Transform>().LookAt(Camera.main.transform); 
            healthBar.fillAmount = health / 100f;
        }
        else
        {
            if (player.GetComponent<PlayerController>().health <= 0f) return;
            
            //advanced stuff go here
            switch (distanceToPlayer)
            {
                case <= 20f and > 10f when (!_isMoving && !_isAttacking) && !_isDead:
                    StartCoroutine(MoveInRanged());
                    break;
                case <= 10f and > 6f when (!_isMoving && !_isAttacking) && !_isDead:
                    switch (health)
                    {
                        case > 90f:
                            PickRandomRangedAttack();
                            break;
                        case <= 90f when stamina >= 25f && shouldDodge:
                        {
                            _isAttacking = true;
                            var random = Random.Range(0, 4);

                            if (random > 1)
                            {
                                DoDodge();
                            }
                            else
                            {
                                PickRandomRangedAttack();
                            }

                            break;
                        }
                        default:
                            PickRandomRangedAttack(); 
                            break;
                    }
                    break;
                case <= 6f and > 1.1f when (!_isMoving && !_isAttacking) && !_isDead:
                    if (stamina >= 20)
                    {
                        var random = Random.Range(0, 4);

                        if (random > 1)
                        {
                            DoPushing();
                        }
                        else
                        {
                            StartCoroutine(MoveInMelee());
                        }
                    }
                    else
                    {
                        StartCoroutine(MoveInMelee());
                    }
                    break;
                case <= 1.1f when (!_isMoving && !_isAttacking) && !_isDead:
                    switch (health)
                    {
                        case > 80f:
                            PickRandomMeleeAttack();
                            break;
                        case <= 80f when stamina >= 25f && shouldDodge:
                        {
                            _isAttacking = true;
                            var random = Random.Range(0, 4);

                            if (random > 1)
                            {
                                DoDodge();
                            }
                            else
                            {
                                PickRandomMeleeAttack();
                            }

                            break;
                        }
                        default:
                            PickRandomMeleeAttack();
                            break;
                    }
                    break;
            }

            //stamina replenish
            if (Time.time - _lastActionTime >= 2f && stamina < MaxStamina && !_isDead)
            {
                stamina += 10f * Time.deltaTime;
                stamina = Mathf.Min(stamina, MaxStamina);
            }
            
            //UI
            if (Camera.main == null) return;
            healthBar.GetComponent<Transform>().LookAt(Camera.main.transform); 
            staminaBar.GetComponent<Transform>().LookAt(Camera.main.transform);
            healthBar.fillAmount = health / 100f;
            staminaBar.fillAmount = stamina / 100f;
        }
    }

    private void PickRandomMeleeAttack()
    {
        while (true)
        {
            _isAttacking = true;

            var random = Random.Range(0, 3);
            
            switch (random)
            {
                case 2:
                    switch (stamina)
                    {
                        case >= 40f:
                            DoMelee3();
                            break;
                        case >= 10f:
                            DoMelee1();
                            break;
                        default:
                            StartCoroutine(WaitForStamina());
                            break;
                    }

                    break;
                case 1:
                    switch (stamina)
                    {
                        case >= 20f:
                            DoMelee2();
                            break;
                        case >= 10f:
                            DoMelee1();
                            break;
                        default:
                            StartCoroutine(WaitForStamina());
                            break;
                    }

                    break;
                case 0 when stamina >= 10f:
                    DoMelee1();
                    break;
                case 0:
                    StartCoroutine(WaitForStamina());
                    break;
            }
            break;
        }
    }
    private void PickRandomRangedAttack()
    {
        while (true)
        {
            _isAttacking = true;

            var random = Random.Range(0, 3);
            
            switch (random)
            {
                case 2:
                    switch (stamina)
                    {
                        case >= 40f:
                            DoRanged3();
                            break;
                        case >= 10f:
                            DoRanged1();
                            break;
                        default:
                            StartCoroutine(WaitForStamina());
                            break;
                    }

                    break;
                case 1:
                    switch (stamina)
                    {
                        case >= 20f:
                            DoRanged2();
                            break;
                        case >= 10f:
                            DoRanged1();
                            break;
                        default:
                            StartCoroutine(WaitForStamina());
                            break;
                    }

                    break;
                case 0 when stamina >= 10f:
                    DoRanged1();
                    break;
                case 0:
                    StartCoroutine(WaitForStamina());
                    break;
            }
            break;
        }
    }

    private void TakeDamage(float takeDamage)
    {
        if (_isDead) return;
        
        if (isAdvanced)
        {
            if (_hasIframes) return;
            health -= takeDamage;

            if (!(health <= 0)) return;
            _animator.SetTrigger(Died);
            _isDead = true;
            
            StartCoroutine(player.GetComponent<PlayerController>().NextLevel());
        }
        else
        {
            health -= takeDamage;

            if (!(health <= 0)) return;
            _animator.SetTrigger(Died);
            _isDead = true;

            StartCoroutine(player.GetComponent<PlayerController>().NextLevel());
        }
    }

    //movement
    #region MoveIn
    private IEnumerator MoveInMelee()
    {
        _isMoving = true;
        while (distanceToPlayer > 1f)
        {
            Transform transform1;
            (transform1 = transform).LookAt(player.transform);
            // Calculate the direction from AI to player.
            var position = player.transform.position;
            var directionToPlayer = transform1.position - position;
            directionToPlayer.Normalize();
            // Calculate the destination at player.
            var destination = position + directionToPlayer;
            
            // Set the calculated destination for the NavMesh agent.
            _agent.SetDestination(destination);

            yield return new WaitForSeconds(0.1f);
        }
        _isMoving = false;
    }
    
    private IEnumerator MoveInRanged()
    {
        _isMoving = true;
        while (distanceToPlayer > 10f)
        {
            Transform transform1;
            (transform1 = transform).LookAt(player.transform);
            // Calculate the direction from AI to player.
            var position = player.transform.position;
            var directionToPlayer = transform1.position - position;
            directionToPlayer.Normalize();
            // Calculate the destination 10 units away from the player.
            var destination = position + directionToPlayer * 9f;
            
            // Set the calculated destination for the NavMesh agent.
            _agent.SetDestination(destination);

            yield return new WaitForSeconds(0.1f);
        }
        _isMoving = false;
    }
    #endregion

    //actions
    #region Actions
    private void DoMelee1()
    {
        if (isAdvanced)
        {
            if (!(stamina >= 10f)) return;
            _lastActionTime = Time.time;
                
            _isAttacking = true;
            transform.LookAt(player.transform);
            _animator.SetTrigger(Melee1);
            stamina -= 10f;
        }
        else
        {
            _isAttacking = true;
            transform.LookAt(player.transform);
            _animator.SetTrigger(Melee1);
        }

        damage = 8f;
    }

    private void DoMelee2()
    {
        if (isAdvanced)
        {
            if (!(stamina >= 20)) return;
            _lastActionTime = Time.time;

            _isAttacking = true;
            transform.LookAt(player.transform);
            _animator.SetTrigger(Melee2);
            stamina -= 20f;
        }
        else
        {
            _isAttacking = true;
            transform.LookAt(player.transform);
            _animator.SetTrigger(Melee2);
        }
        
        damage = 15f;
    }

    private void DoMelee3()
    {
        if (!(stamina >= 40)) return;
        _lastActionTime = Time.time;
        
        _isAttacking = true;
        transform.LookAt(player.transform);
        _animator.SetTrigger(Melee3);
        stamina -= 40f;
        
        damage = 7f;
    }
    
    private void DoPushing()
    {
        if (!(stamina >= 20)) return;
        _lastActionTime = Time.time;
        
        _isAttacking = true;
        _animator.SetTrigger(Pushing);
        // Set the calculated destination for the NavMesh agent.
        _agent.speed = 5f;
        _agent.SetDestination(player.transform.position);
        transform.LookAt(player.transform);
        
        stamina -= 20f;
        
        damage = 13f;
    }

    private void DoRanged1()
    {
        if (isAdvanced)
        {
            if (!(stamina >= 10f)) return;
            _lastActionTime = Time.time;
            
            _isAttacking = true;
            transform.LookAt(player.transform);
            _animator.SetTrigger(Ranged1);
            stamina -= 10f;
        }
        else
        {
            _isAttacking = true;
            transform.LookAt(player.transform);
            _animator.SetTrigger(Ranged1);
        }
        damage = 7f;
    }

    private void DoRanged2()
    {
        if (isAdvanced)
        {
            if (!(stamina >= 20f)) return;
            _lastActionTime = Time.time;
            
            _isAttacking = true;
            transform.LookAt(player.transform);
            _animator.SetTrigger(Ranged2);
            stamina -= 20f;
        }
        else
        {
            _isAttacking = true;
            transform.LookAt(player.transform);
            _animator.SetTrigger(Ranged2);
        }
        damage = 5f;
    }

    private void DoRanged3()
    {
        if (!(stamina >= 40f)) return;
        _lastActionTime = Time.time;
        
        _isAttacking = true;
        transform.LookAt(player.transform);
        _animator.SetTrigger(Ranged3);
        stamina -= 40f;
        
        damage = 18f;
    }

    private void DoDodge()
    {
        if (!(stamina >= 25)) return;
        _lastActionTime = Time.time;
        
        _isAttacking = true;
        _animator.SetTrigger(Dodge);
        
        // Calculate the destination point by moving back from the original position
        var transform1 = transform;
        var destination = transform1.position - transform1.forward * 2f;

        // Set the destination for the agent
        _agent.SetDestination(destination);
        
        stamina -= 25f;
    }
    
    public void SpawnProjectile()
    {
        var newProjectile = Instantiate(projectileObj, projectileSpawnLoc.transform, true);
        newProjectile.transform.position = projectileSpawnLoc.transform.position;
        newProjectile.transform.SetParent(null);
    }

    private IEnumerator WaitForStamina()
    {
        var timer = 0;

        while (timer < 4)
        {
            Debug.Log("waiting for stamina");
            
            // Calculate the destination point by moving back from the original position
            var transform1 = transform;
            var destination = transform1.position - transform1.forward * 6f;

            // Set the destination for the agent
            _agent.SetDestination(destination);

            yield return new WaitForSeconds(4f);

            timer = 4;
        }
        
        AnimEnded();
    }
    #endregion
    
    //triggered from anim event
    private void AnimEnded()
    {
        _isAttacking = false;
        _agent.speed = 3f;
    }
    public void IframesStarted()
    {
        _hasIframes = true;
    }
    public void IframesEnded()
    {
        _hasIframes = false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            TakeDamage(player.GetComponent<PlayerController>().damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a line from this object to the player in the scene view
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, player.transform.position);
    }
}
