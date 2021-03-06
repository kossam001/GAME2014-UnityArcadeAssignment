﻿/* PlayerController.cs
 * 
 * Samuel Ko
 * 101168049
 * Last Modified: 2020-10-15
 * 
 * Controls the player character.
 * 
 * 2020-10-06: Added this script.
 * 2020-10-09: Added testing code.  Need removal.
 * 2020-10-15: Added touch controls.  
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterStats stats;
    public GameObject fireball;
    public float size = 0.9f;
    public FireballManager fireballManager;

    // Touch controls
    public GameObject moveButtonLandscape;
    public GameObject moveButtonPortrait;
    private GameObject moveButton;

    public AudioClip shootSE;

    // Start is called before the first frame update
    void Start()
    {
        stats.SetHealth(GameManager.Instance.lives);
    }

    // Update is called once per frame
    void Update()
    {
        if (moveButtonLandscape.activeInHierarchy)
        {
            moveButton = moveButtonLandscape;
        }
        else if (moveButtonPortrait.activeInHierarchy)
        {
            moveButton = moveButtonPortrait;
        }

        Move();
        if (moveButton.GetComponent<MovementButton>().isHolding)
        {
            Turn();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }

        cooldown -= Time.deltaTime;

        if (stats.isDead)
        {
            GameManager.Instance.GameOver();
            stats.ResetStats();
        }    
    }

    private void Move()
    {
        UseKeyboardInput();
        UseTouchInput();
    }

    private void UseTouchInput()
    {
        if (moveButton.GetComponent<MovementButton>().HoldDirection.x != 0 
            || moveButton.GetComponent<MovementButton>().HoldDirection.y != 0)
        {
            GetComponent<Animator>().SetBool("Walk", true);
        }
        else
        {
            GetComponent<Animator>().SetBool("Walk", false);
        }

        transform.position += new Vector3(moveButton.GetComponent<MovementButton>().HoldDirection.x * stats.GetSpeed() * Time.deltaTime,
                                  moveButton.GetComponent<MovementButton>().HoldDirection.y * stats.GetSpeed() * Time.deltaTime,
                                  0.0f);
    }

    private void UseKeyboardInput()
    {
        // Play Animation
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            GetComponent<Animator>().SetBool("Walk", true);
        }
        else
        {
            GetComponent<Animator>().SetBool("Walk", false);
        }

        // moves character by getting axis value - keyboard input 
        transform.position += new Vector3(Input.GetAxis("Horizontal") * stats.GetSpeed() * Time.deltaTime,
                                          Input.GetAxis("Vertical") * stats.GetSpeed() * Time.deltaTime,
                                          0.0f);
    }

    private void Turn()
    {
        // movement vector - calculated using current position minus future position
        //Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal") + transform.position.x, Input.GetAxis("Vertical") + transform.position.y)
        //    - transform.position;

        Vector3 moveDirection = new Vector3(moveButton.GetComponent<MovementButton>().HoldDirection.x + transform.position.x,
            moveButton.GetComponent<MovementButton>().HoldDirection.y + transform.position.y)
            - transform.position;

        // don't update rotation if player is not moving
        if (moveDirection.sqrMagnitude >= 1)
        {
            // rotate player gradually in the direction of movement.
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, 10);
        }
    }

    float cooldown;
    public void Fire()
    {
        if (cooldown > 0)
        {
            return;
        }

        Vector3 audioPos = transform.position;
        audioPos.z = -10;
        AudioSource.PlayClipAtPoint(shootSE, audioPos, 0.1f);

        GameObject fireball = fireballManager.DequeueFireball();
        fireball.transform.position = transform.position;
        fireball.transform.rotation = transform.rotation * Quaternion.Euler(0, 0, -90);
        fireball.GetComponent<FireballBehaviour>().speed = GetComponent<CharacterStats>().GetSpeed() + 1.5f;
        fireball.GetComponent<FireballBehaviour>().owner = gameObject;
        fireball.SetActive(true);
           
        cooldown = stats.GetFirerate();
    }
}
