using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponsSystem : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] CharacterController controller;
    [SerializeField] Camera cam;
    [SerializeField] AudioSource aud;
    //[SerializeField] GameObject lines;
    //public LineRenderer[] lineRends;
    //private readonly List<RaycastHit> hits = new();

    int primaryDamage;
    float primaryRate;
    int primaryDist;
    int altDamage;
    float altRate;
    float altDist;
    int numOfShots;
    int manaDrain;
    int altManaDrain;

    [Header("Firing Values")]
    [SerializeField] GameObject projectile;
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] Transform primaryFirePos;
    [SerializeField] GameObject altProjectile;
    [SerializeField] Transform altFirePos;
    [SerializeField] GameObject testLantern;
    [SerializeField] GameObject shotGunModel;
    [SerializeField] GameObject shieldModel;
    [SerializeField] GameObject primaryModel;
    public List<AbilityObject> activePrimary; // odd naming convention to allow for player decided loadouts once inventory menu exists
    [SerializeField] GameObject altModel;
    public List<AbilityObject> activeAlt;

    [Header("Mana")]
    [Range(1, 200)][SerializeField] public int manaPool;
    [SerializeField] float manaRegenStutter;
    //[SerializeField] float manaRegenDelay;
    public int manaRegenAmount = 1;
    public int manaRegenOrig;
    public int manaOrig;
    bool manaCool;
    public bool manaInUse;

    private bool isShooting;
    private int selectedPrimary; 
    private int selectedAlt;

    [Header("Movement Ability Values")]
    [SerializeField] int dashSpeed;
    [SerializeField] float dashRate;
    [SerializeField] float dashTime;

    public bool isDashing;
    public bool canDash = false;

    private void Start()
    {
        cam = Camera.main;
        manaOrig = manaPool;
        manaRegenOrig = manaRegenAmount;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.instance.isPaused) // can't do nun
        {
            if (!manaCool && CanFillMana()) // regenerates mana over time
            {
                StartCoroutine(Refill());
            }

            updateManaBar();

            if (activePrimary.Count > 0)
            {
                SelectPrimary();
            }
            if (activeAlt.Count > 0)
            {
                SelectAlt();
            }

            FireWeapons();
        }
     }

    void FireWeapons()
    {
        // Primary fire
        if (Input.GetButtonDown("Fire1") && !isShooting && activePrimary.Count > 0 && manaPool >= manaDrain)
        {
            StartCoroutine(castPrimary());
        }

        // Alt fire
        if (Input.GetButtonDown("Fire2") && activeAlt.Count > 0 && manaPool >= altManaDrain)
        {
            StartCoroutine(castAlt());
        }
    }

    IEnumerator castPrimary()
    {
        isShooting = true;

        if (manaDrain > 0) // if the ability drains mana
        {
            manaInUse = true;
            manaPool -= manaDrain; // drain the mana
        }

        aud.PlayOneShot(activePrimary[selectedPrimary].shootSound, activePrimary[selectedPrimary].shootSoundVol);

        if (activePrimary[selectedPrimary].shootsProjectile)
        {
            // instance projectiles on camera rotation
            Instantiate(projectile, primaryFirePos.position, cam.transform.rotation);
        }
        else
        {
            Instantiate(muzzleFlash, primaryFirePos.transform.position, cam.transform.rotation);
            // its a raycast so do the raycast stuff
            for (int i = 1; i <= numOfShots; i++)
            {
                FireRaycast(i);
            }
            //StartCoroutine(bulletTrails());
        }

        yield return new WaitForSeconds(primaryRate);
        isShooting = false;
        manaInUse = false;
    }

    IEnumerator castAlt() // eventually recieve an enum that indicates kind of spell
    {
        if (activeAlt[selectedAlt].makesImmune && gameManager.instance.playerHealth.isInvincible) // if trying to be invincible and already invincible, exit.
        {
            yield break;
        }

        if (altManaDrain > 0) // if the ability drains mana
        {
            manaInUse = true;
            manaPool -= altManaDrain; // drain the mana
        }

        aud.PlayOneShot(activeAlt[selectedAlt].shootSound, activeAlt[selectedAlt].shootSoundVol);

        if (activeAlt[selectedAlt].shootsProjectile)
        {
            // instance projectiles on camera rotation
            Instantiate(altProjectile, altFirePos.position, cam.transform.rotation);
        }
        else if (activeAlt[selectedAlt].makesImmune)
        {
            gameManager.instance.playerHealth.isInvincible = true;
            gameManager.instance.playerHealth.timeSinceTriggered = Time.time;
            gameManager.instance.invincibleAura.SetActive(true); // indicates that player is invincible
        }
        else if (activeAlt[selectedAlt].isMovement && canDash /*&& !isDashing*/)
        {
            StartCoroutine(dash());
        }

        yield return new WaitForSeconds(altRate);
        gameManager.instance.playerHealth.isInvincible = false;
        gameManager.instance.invincibleAura.SetActive(false);
        manaInUse = false;
    }

    private void FireRaycast(int loop)
    {
        if (loop > 1)
        {
            float maxOffset = 0.5f;
            Vector3 pelletSpread = Vector3.zero;
            Vector3 camDir = cam.transform.forward; // initial aim 
            pelletSpread += cam.transform.right * Random.Range(-maxOffset, maxOffset);
            pelletSpread += cam.transform.up * Random.Range(-maxOffset, maxOffset);

            camDir += pelletSpread.normalized * Random.Range(0f, 0.2f);
            
            if (Physics.Raycast(cam.transform.position, camDir, out RaycastHit hit, primaryDist))
            {

                IDamage dmg = hit.collider.GetComponent<IDamage>();

                if (hit.transform != transform && dmg != null)
                {
                    dmg.takeDamage(primaryDamage);
                }
                Instantiate(activePrimary[selectedPrimary].hitEffect, hit.point, Quaternion.identity); // need those hit effects ALWAYS
            }
            
        }
        else
        {
            Vector3 camDir = cam.transform.forward; // initial aim 
            if (Physics.Raycast(cam.transform.position, camDir, out RaycastHit hit, primaryDist))
            {
                //Debug.DrawLine(cam.transform.position, hit.point, Color.green, 1f);
                //hits.Add(hit); // line renderer setup

                IDamage dmg = hit.collider.GetComponent<IDamage>();

                if (hit.transform != transform && dmg != null)
                {
                    dmg.takeDamage(primaryDamage);
                }
                Instantiate(activePrimary[selectedPrimary].hitEffect, hit.point, Quaternion.identity); // need those hit effects ALWAYS
            }
        }
    }

    public IEnumerator dash()
    {
        if (canDash)
        {
            isDashing = true;

            aud.PlayOneShot(activeAlt[selectedAlt].shootSound, activeAlt[selectedAlt].shootSoundVol);

            Vector3 camDir = cam.transform.forward; // initial aim 
            if (Physics.Raycast(cam.transform.position, camDir, out RaycastHit hit, altDist))
            {
                IDamage dmg = hit.collider.GetComponent<IDamage>();

                if (hit.transform != transform && dmg != null)
                {
                    dmg.takeDamage(altDamage);
                }
            }

            // add dash speed to proper vector direction
            float startTime = Time.time;
            Physics.IgnoreLayerCollision(7, 8, true); // ignores collision with enemies
            while (Time.time < startTime + dashTime)
            {
                controller.Move(dashSpeed * Time.deltaTime * cam.transform.forward); // input the dash
                yield return null;
            }
            Physics.IgnoreLayerCollision(7, 8, false);
            yield return new WaitForSeconds(dashRate);
            isDashing = false;
        }
    }

    public void GetAbilityStats(AbilityObject ability)
    {
        if (gameManager.instance.isSceneCurrentlyLoaded("Dungeon Level") || gameManager.instance.isSceneCurrentlyLoaded("Maze Level"))
            gameManager.instance.messages.DisplayMessage(ability.signature);
        if (ability.isPrimary) // if the ability is a primary ability
        {
            activePrimary.Add(ability); // push to ability wheel
            selectedPrimary = activePrimary.Count - 1;
            ChangePrimary();
        }
        else // secondary/alt ability
        {
            activeAlt.Add(ability); // push to ability wheel
            selectedAlt = activeAlt.Count - 1;
            ChangeAlt();
        }
    }

    void SelectPrimary() // scroll wheel primary rotation
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && selectedPrimary < activePrimary.Count - 1)
        {
            selectedPrimary++;
            ChangePrimary();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedPrimary > 0)
        {
            selectedPrimary--;
            ChangePrimary();
        }
    }

    void ChangePrimary()
    {
        if (activePrimary[selectedPrimary].shootsProjectile) // if the thing fires a projectile prefab
        {
            projectile = activePrimary[selectedPrimary].projectile; // no need to assign unique damage 
        }
        else
        {
            primaryDamage = activePrimary[selectedPrimary].shootDamage; // needs unique damage
            primaryDist = activePrimary[selectedPrimary].shootDist;
            numOfShots = activePrimary[selectedPrimary].numOfShots;
            muzzleFlash = activePrimary[selectedPrimary].muzzleFlash;
        }
        primaryRate = activePrimary[selectedPrimary].shootRate;
        manaDrain = activePrimary[selectedPrimary].manaDrain;

        if (activePrimary[selectedPrimary].preferredHardpoint) // the shotgun is just completely fucked
        {
            shotGunModel.GetComponent<MeshFilter>().sharedMesh = activePrimary[selectedPrimary].abilityModel.GetComponent<MeshFilter>().sharedMesh;
            primaryModel.GetComponent<MeshRenderer>().enabled = false;
            shotGunModel.GetComponent<MeshRenderer>().enabled = true;
            shotGunModel.GetComponent<MeshRenderer>().sharedMaterials = activePrimary[selectedPrimary].abilityModel.GetComponent<MeshRenderer>().sharedMaterials;
        }
        else
        {
            shotGunModel.GetComponent<MeshRenderer>().enabled = false;
            primaryModel.GetComponent<MeshRenderer>().enabled = true;
            primaryModel.GetComponent<MeshFilter>().sharedMesh = activePrimary[selectedPrimary].abilityModel.GetComponent<MeshFilter>().sharedMesh;
            // big Tony blunder: assets have more than one material
            primaryModel.GetComponent<MeshRenderer>().sharedMaterials = activePrimary[selectedPrimary].abilityModel.GetComponent<MeshRenderer>().sharedMaterials;
        }
    }

    void SelectAlt() // Q/E ability selection, infinite scroll
    {
        if (Input.GetButtonDown("Depth Up")) // should be E
        {
            if (selectedAlt >= activeAlt.Count - 1)
            {
                selectedAlt = 0;
            }
            else
            {
                selectedAlt++;
            }
            ChangeAlt();
        }
        else if (Input.GetButtonDown("Depth Down")) // should be Q
        {
            if (selectedAlt <= 0)
            {
                selectedAlt = activeAlt.Count - 1;
            }
            else
            {
                selectedAlt--;
            }
            ChangeAlt();
        }
    }

    void ChangeAlt()
    {
        if (activeAlt[selectedAlt].shootsProjectile) // if the thing fires a projectile prefab
        {
            altProjectile = activeAlt[selectedAlt].projectile; // no need to assign unique damage, all stored in projectile 
        }

        if (activeAlt[selectedAlt].isMovement) // only alt weapons can be Movement abilites currently, sorry
        {
            canDash = true;
            dashSpeed = activeAlt[selectedAlt].dashSpeed;
            altDamage = activeAlt[selectedAlt].dashDamage; // needs unique damage, not stored in projectile
            altDist = activeAlt[selectedAlt].dashDamageDistance;
        }
        else
        {
            canDash = false;
        }

        if (activeAlt[selectedAlt].improvesMana)
        {
            manaRegenAmount = activeAlt[selectedAlt].manaRegenMod;
        }
        else
        {
            manaRegenAmount = manaRegenOrig;
        }

        altRate = activeAlt[selectedAlt].shootRate;
        altManaDrain = activeAlt[selectedAlt].manaDrain;

        if (activeAlt[selectedAlt].preferredHardpoint) // shield looks better flipped
        {
            shieldModel.GetComponent<MeshFilter>().sharedMesh = activeAlt[selectedAlt].abilityModel.GetComponent<MeshFilter>().sharedMesh;
            altModel.GetComponent<MeshRenderer>().enabled = false;
            shieldModel.GetComponent<MeshRenderer>().enabled = true;
            shieldModel.GetComponent<MeshRenderer>().sharedMaterials = activeAlt[selectedAlt].abilityModel.GetComponent<MeshRenderer>().sharedMaterials;
        }
        else
        {
            shieldModel.GetComponent<MeshRenderer>().enabled = false;
            altModel.GetComponent<MeshRenderer>().enabled = true;
            altModel.GetComponent<MeshFilter>().sharedMesh = activeAlt[selectedAlt].abilityModel.GetComponent<MeshFilter>().sharedMesh;
            // big Tony blunder: assets have more than one material
            altModel.GetComponent<MeshRenderer>().sharedMaterials = activeAlt[selectedAlt].abilityModel.GetComponent<MeshRenderer>().sharedMaterials;
        }
    }

    public bool CanFillMana()
    {
        if (manaPool < manaOrig)
        {
            return true;
        }
        manaRegenAmount = manaRegenOrig;
        gameManager.instance.playerHealth.acceleratedManaRegen = false;
        return false;
    }

    IEnumerator Refill()
    {
        manaCool = true;

        manaPool += manaRegenAmount;
        // playerScript.updateManaBar();
        yield return new WaitForSeconds(manaRegenStutter);
        manaCool = false;
    }

    // add ability to pick up mana objects by walking into them
    private void OnTriggerEnter(Collider other)
    {
        // if Item is health
        if (other.TryGetComponent(out iMana item))
        {
            //Debug.Log("HealItem found");
            ManaRestore(other, item);
        }
    }

    private void ManaRestore(Collider other, iMana item)
    {
        if (manaPool == manaOrig) // if mana is full, item is not consumed
        {
            return;
        }

        int manaToRestore = item.RestoreMana();
        int manaGap = manaOrig - manaPool; // no OverMana
        if (manaToRestore > manaGap) // done this way to provide the posibility of displaying to player on UI
        {
            manaPool += manaGap;
        }
        else
        {
            manaPool += manaToRestore;
        }
        aud.PlayOneShot(item.GetAudioClip(), item.GetVolume());
        Destroy(other.gameObject);
        updateManaBar();
    }

    public void updateManaBar()
    {
        gameManager.instance.playerManaBar.fillAmount = (float)manaPool / manaOrig;

        if (gameManager.instance.playerHealth.acceleratedManaRegen )
        {
            gameManager.instance.playerManaBar.color = Color.yellow;
        } else
        {
            gameManager.instance.playerManaBar.color = Color.blue;
        }
    }
}
