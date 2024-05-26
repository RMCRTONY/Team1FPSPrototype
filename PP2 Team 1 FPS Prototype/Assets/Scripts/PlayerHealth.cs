using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamage // Has IInteractions
{
    [SerializeField] AudioSource aud;

    [Range(1, 20)][SerializeField] public int HP;

    [Header("Audio")]
    [SerializeField] AudioClip[] audHurt;
    [Range(0, 1)][SerializeField] float audHurtVol;
    [SerializeField] AudioClip audParryPing;
    [Range(0, 1)][SerializeField] float parryPingVol;

    [Header("Warding")]
    [SerializeField] int perfectReturn;
    [SerializeField] float perfectWindow;
    public float timeSinceTriggered;
    public bool acceleratedManaRegen = false;

    public string lastAttackerName = "Unknown"; // Store the last attacker's name

    public int HPOrig;
    public bool isInvincible;
    // Start is called before the first frame update
    void Start()
    {
        HPOrig = HP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void takeDamage(int amount)
    {
        if (isInvincible)
        {
            if (Time.time < timeSinceTriggered + perfectWindow && !acceleratedManaRegen) 
            {
                acceleratedManaRegen = true;
                gameManager.instance.weaponsSystem.manaInUse = false;
                gameManager.instance.weaponsSystem.manaRegenAmount *= perfectReturn;
                aud.PlayOneShot(audParryPing, parryPingVol);
                StartCoroutine(flashParry());
            }
            return;
        }

        HP -= amount;
        aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);
        updatePlayerUI();
        StartCoroutine(flashDamage());

        if (HP <= 0)
        {
            gameManager.instance.youLose(lastAttackerName);
        }
    }

    private void heal(Collider other, iHeal item)
    {
        if (HP == HPOrig) // if health is full, item is not consumed
        {
            return;
        }

        int healthToRestore = item.RestoreHealth();
        int healthGap = HPOrig - HP; // no OverHeal
        if (healthToRestore > healthGap) // done this way to provide the posibility of displaying to player on UI
        {
            HP += healthGap;
        }
        else
        {
            HP += healthToRestore;
        }
        aud.PlayOneShot(item.GetAudioClip(), item.GetVolume());
        Destroy(other.gameObject);
        updatePlayerUI();
        StartCoroutine(flashHeal());
    }

    public void updatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
    }

    IEnumerator flashDamage()
    {
        gameManager.instance.playerDamageScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerDamageScreen.SetActive(false);
    }

    IEnumerator flashHeal() // flashes the screen green
    {
        gameManager.instance.playerHealScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerHealScreen.SetActive(false);
    }

    IEnumerator flashParry()
    {
        gameManager.instance.perfectParryScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.perfectParryScreen.SetActive(false);
    }

    // add ability to pick up health objects by walking into them
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Are you triggered?");
        // if Item is health
        if (other.TryGetComponent(out iHeal item))
        {
            //Debug.Log("HealItem found");
            heal(other, item);
        }

        // Check if the collider belongs to an enemy
        //if (other.CompareTag("Enemy") || other.CompareTag("Boss")) // Add other enemy tags as needed
        //{
        //    //Debug.Log("Collider is enemy's");
        //    // Check if the enemy is attacking
        //    if (other.GetComponent<enemyAI>()?.isAttacking == true || // Check if the enemy is using enemyAI and is attacking
        //        other.GetComponent<bossAI>()?.isAttacking == true ||  // Check if the enemy is using bossAI and is attacking
        //        other.GetComponent<bossBMAI>()?.isAttacking == true) // Check if the enemy is using bossBMAI and is attacking
        //    {
        //        //Debug.Log("Found the AI");
        //        // Get the enemyName directly from the specific boss AI component
        //        if (other.TryGetComponent<enemyAI>(out var enemyAIScript))
        //        {
        //            //Debug.Log("Set Enemy Name.");
        //            lastAttackerName = enemyAIScript.enemyName;
        //        }
        //        else if (other.TryGetComponent<bossAI>(out var bossAIScript))
        //        {
        //            //Debug.Log("Set Boss Name.");
        //            lastAttackerName = bossAIScript.enemyName;
        //        }
        //        else if (other.TryGetComponent<bossBMAI>(out var bossBMAIScript))
        //        {
        //            //Debug.Log("Set Boss Bitch Name.");
        //            lastAttackerName = bossBMAIScript.enemyName;
        //        }
        //        else
        //        {
        //            // Handle cases where the enemy script is not found or doesn't have a public enemyName
        //            lastAttackerName = "Unknown Enemy"; // Or log an error
        //        }
        //    }
        //}
    }
  }
