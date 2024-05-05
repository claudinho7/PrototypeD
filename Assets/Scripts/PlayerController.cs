using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Animator _animator;
    private PlayerInput _input;
    
    [Header("Other")]
    public bool npcInteracted;
    private int _currentScene;

    [Header("Enemy")] 
    public EnemyController enemy;
    
    [Header("Stats")]
    public float health;
    public float stamina;
    public float damage;
    private const float MaxHealth = 100;
    private const float MaxStamina = 100;
    private bool _hasIframes;
    
    private float _lastActionTime;
    private bool _inAnimation;
    private bool _isDead;
    
    [Header("UI")]
    public GameObject levelStartPopUp;
    public TextMeshProUGUI levelStartText;
    public Image healthBar;
    public Image staminaBar;
    public TextMeshProUGUI restartTimer;
    public GameObject restartObj;
    public GameObject distanceWarning;
    public GameObject pauseObject;
    private bool _isPaused;

    [Header("Projectile")] 
    public GameObject projectileObj;
    public Transform projectileSpawnLoc;
    
    //cache anim
    private static readonly int Dodge = Animator.StringToHash("Dodge");
    private static readonly int Skill2 = Animator.StringToHash("Skill2");
    private static readonly int Skill1 = Animator.StringToHash("Skill1");
    private static readonly int HeavyAttack = Animator.StringToHash("HeavyAttack");
    private static readonly int LightAttack = Animator.StringToHash("LightAttack");
    private static readonly int Died = Animator.StringToHash("Died");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _input = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        _currentScene = SceneManager.GetActiveScene().buildIndex;
        levelStartPopUp.SetActive(true);
        LevelStartPopUpSwitch();
        
        health = MaxHealth;
        stamina = MaxStamina;
        restartObj.SetActive(false);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Time.time - _lastActionTime >= 3f && stamina < MaxStamina)
        {
            stamina += 10f * Time.deltaTime;
            stamina = Mathf.Min(stamina, MaxStamina);
        }

        //tell the enemy AI when he should dodge
        if (enemy != null)
        {
            enemy.shouldDodge = _inAnimation;
        }

        if (Camera.main == null) return;
        healthBar.GetComponent<Transform>().LookAt(Camera.main.transform); 
        staminaBar.GetComponent<Transform>().LookAt(Camera.main.transform);
        healthBar.fillAmount = health / 100f;
        staminaBar.fillAmount = stamina / 100f;
    }

    private void TakeDamage(float takeDamage)
    {
        if (_hasIframes) return;
        if (_isDead) return;

        health -= takeDamage;

        if (!(health <= 0)) return;
        _animator.SetTrigger(Died);
        StartCoroutine(RestartLevel());
        _isDead = true;
    }

    //Actions
    #region Skills
    private void OnLightAttack()
    {
        if (_inAnimation || !(stamina >= 10)) return;
        _inAnimation = true;
        _animator.SetTrigger(LightAttack);
        stamina -= 10f;
        
        damage = gameObject.name == "PlayerRanged" ? 4f : 8f;

        _lastActionTime = Time.time;
    }
    private void OnHeavyAttack()
    {
        if (_inAnimation || !(stamina >= 20)) return;
        _inAnimation = true;
        _animator.SetTrigger(HeavyAttack);
        stamina -= 20f;
        
        damage = gameObject.name == "PlayerRanged" ? 7f : 12f;
        
        _lastActionTime = Time.time;
    }
    private void OnSkill1()
    {
        if (_inAnimation || !(stamina >= 30)) return;
        _inAnimation = true;
        _animator.SetTrigger(Skill1);
        stamina -= 30f;
        
        damage = gameObject.name == "PlayerRanged" ? 15f : 6f;
        
        _lastActionTime = Time.time;
    }
    private void OnSkill2()
    {
        if (_inAnimation || !(stamina >= 30)) return;
        _inAnimation = true;
        _animator.SetTrigger(Skill2);
        stamina -= 30f;

        damage = gameObject.name == "PlayerRanged" ? 5f : 18f;
        
        _lastActionTime = Time.time;
    }
    private void OnDodge()
    {
        if (_inAnimation || !(stamina >= 25)) return;
        _inAnimation = true;
        _animator.SetTrigger(Dodge);
        stamina -= 25f;
        
        _lastActionTime = Time.time;
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            TakeDamage(enemy.damage);
        }
    }
    
    //triggered from anim event
    public void AnimEnded()
    {
        _inAnimation = false;
    }
    public void IframesStarted()
    {
        _hasIframes = true;
    }
    public void IframesEnded()
    {
        _hasIframes = false;
    }
    
    public void SpawnProjectile()
    {
        if (enemy == null) return;
        
        // Calculate the distance between this object and the monster
        var distanceToMonster = Vector3.Distance(transform.position, enemy.transform.position);

        if (distanceToMonster > 12f)
        {
            StartCoroutine(DistanceWarning());
        }
        else
        {
            var newProjectile = Instantiate(projectileObj, projectileSpawnLoc.transform, true);
            newProjectile.transform.position = projectileSpawnLoc.transform.position;
            newProjectile.transform.SetParent(null);
        }
    }

    //UI Stuff
    #region UI Stuff
    private void OnInteract(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;
        
        if (npcInteracted)
        {
            SceneManager.LoadScene(_currentScene + 1);
            npcInteracted = false;
        }
        else if (levelStartPopUp.activeSelf)
        {
            levelStartPopUp.SetActive(false);
        }
    }

    private void OnPause(InputValue inputValue)
    {
        if (!inputValue.isPressed) return;

        if (!_isPaused)
        {
            _isPaused = true;
            Time.timeScale = 0f;
            pauseObject.SetActive(true);
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Time.timeScale = 1f;
            pauseObject.SetActive(false);
            Cursor.visible = false;
            _isPaused = false;
        }
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void RestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_currentScene);
    }
    
    private void LevelStartPopUpSwitch()
    {
        if (_currentScene is 1 or 3 or 5 or 7)
        {
            if (GameObject.Find("DontDestroyRandomProgression").GetComponent<ProgressionManager>().sceneCounter == 1)
                levelStartText.text =
                    "Step 1. 0% completion.\nExplore the town and find the Mayor to proceed to the next step. Remember the color and how the NPCs respond to you.\n\n Press E to close this PopUp.";
            else if (GameObject.Find("DontDestroyRandomProgression").GetComponent<ProgressionManager>().sceneCounter == 2)
                levelStartText.text =
                    "Step 2. 25% completion.\nExplore the town and find the Mayor to proceed to the next step. Keep an eye out for any differences.\n\n Press E to close this PopUp.";
            else if (GameObject.Find("DontDestroyRandomProgression").GetComponent<ProgressionManager>().sceneCounter == 3)
                levelStartText.text =
                    "Step 3. 50% completion.\nExplore the town and find the Mayor to proceed to the next step. Remember the color and how the NPCs respond to you.\n\n Press E to close this PopUp.";
            else if (GameObject.Find("DontDestroyRandomProgression").GetComponent<ProgressionManager>().sceneCounter == 4)
                levelStartText.text =
                    "Step 4. 75% completion.\nExplore the town and find the Mayor to proceed to the next step. Keep an eye out for any differences.\n\n Press E to close this PopUp.";
            else
                levelStartText.text = levelStartText.text;
        }
    }

    private IEnumerator DistanceWarning()
    {
        var timer = 1f;

        while (timer > 0f)
        {
            distanceWarning.SetActive(true);
            
            yield return new WaitForSeconds(0.25f);
            
            distanceWarning.SetActive(false);
            
            yield return new WaitForSeconds(0.25f);
            
            distanceWarning.SetActive(true);
            
            yield return new WaitForSeconds(0.25f);
            
            distanceWarning.SetActive(false);
            
            yield return new WaitForSeconds(0.25f);

            timer -= 1f;
        }
    }
    #endregion

    private IEnumerator RestartLevel()
    {
        var timer = 10f;
        _input.enabled = false;
        restartObj.SetActive(true);
        
        while (timer > 0f)
        {
            restartTimer.text = "You will restart this level in: " + timer.ToString(CultureInfo.CurrentCulture);
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        SceneManager.LoadScene(_currentScene);
    }
    
    //triggered from enemy controller when it dies
    public IEnumerator NextLevel()
    {
        var timer = 10f;
        _input.enabled = false;
        restartObj.SetActive(true);
        
        while (timer > 0f)
        {
            restartTimer.text = "You will advance to next level in: " + timer.ToString(CultureInfo.CurrentCulture);
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        GameObject.Find("DontDestroyRandomProgression").GetComponent<ProgressionManager>().PickNextScene();
    }
}
