using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;            // The speed that the player will move at.
    public float shootingAdvisorAngle = 45f;
    public float bulletRange = 100f;

    Vector3 movement, rotation;         // The vector to store the direction of the player's movement and rotation.
    Animator anim;                      // Reference to the animator component.
    Rigidbody playerRigidbody;          // Reference to the player's rigidbody.
    //int floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
    //float camRayLength = 100f;          // The length of the ray from the camera into the scene.
    Ray shootRay = new Ray();
    RaycastHit shootHit;
    int shootableMask;


    void Awake()
    {
        // Create a layer mask for the floor layer.
        //floorMask = LayerMask.GetMask("Floor");

        // Get shootable mask in order to check if enemy is reachable
        shootableMask = LayerMask.GetMask("Shootable");

        // Set up references.
        anim = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
    }


    void FixedUpdate()
    {
        // Move the player around the scene.
        Move();

        // Turn the player.
        Turning();

        // Animate the player.
        Animating();
    }


    void Move()
    {
        // Store the input axes for movement.
        float hInputMove = CrossPlatformInputManager.GetAxis("HorizontalMovement2");
        float vInputMove = CrossPlatformInputManager.GetAxis("VerticalMovement2");

        // Set the movement vector based on the axis input.
        movement.Set(hInputMove, 0f, vInputMove);

        // Normalise the movement vector and make it proportional to the speed per second.
        movement = movement.normalized * speed * Time.deltaTime;

        // Move the player to it's current position plus the movement.
        playerRigidbody.MovePosition(transform.position + movement);
    }


    void Turning()
    {
        // Store the input axes for rotation.
        float hInputTurn = CrossPlatformInputManager.GetAxis("HorizontalRotation2");
        float vInputTurn = CrossPlatformInputManager.GetAxis("VerticalRotation2");

        // Set the rotation vector based on the axis input.
        rotation.Set(hInputTurn, 0f, vInputTurn);
        rotation.y = 0f;

        if (rotation.magnitude > 0.1f)
        {
            Quaternion joysticRotation = Quaternion.LookRotation(rotation.normalized);
            // Create a quaternion (rotation) based on the joysticRotation and the enemy positions.
            Quaternion newRotation =  ShootingAdvisor(joysticRotation);

            //transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.time * 0.008f);
            playerRigidbody.MoveRotation(Quaternion.Lerp(playerRigidbody.rotation, newRotation, Time.time * 0.01f));
        }
    }


    Quaternion ShootingAdvisor(Quaternion joysticRotation)
    {
        // Tune rotation in case of shooting
        //Debug.Log("ShootingAdvisor", gameObject);

        Vector3 playerPosition = transform.position;
        Vector3 closestEnemyVector = new Vector3(float.PositiveInfinity,0f,float.PositiveInfinity);

        bool validEnemyInViewAndRange = false;
        foreach (GameObject enemy in EnemyManager.enemyList.ToArray())
        {
            if (enemy != null)
            {
                EnemyHealth healthComponent = enemy.GetComponent<EnemyHealth>();
                if (healthComponent.currentHealth <= 0)
                {
                    // Remove enemies with 0 health
                    EnemyManager.enemyList.Remove(enemy);
                    Debug.Log("Remove enemy from list.", gameObject);
                }
                else
                {
                    // -------------------------------------------------------------------//
                    // Compute vector between player and enemy
                    Vector3 enemyPosition = enemy.GetComponent<Transform>().position;
                    Vector3 playerEnemyVector = enemyPosition - playerPosition;
                    playerEnemyVector.y = 0f;
                    Quaternion playerEnemyRotation = Quaternion.LookRotation(playerEnemyVector.normalized);

                    // Compute angel between joystic and player-enemy rotation
                    float angle = Quaternion.Angle(joysticRotation, playerEnemyRotation);

                    if (angle <= shootingAdvisorAngle)
                    {
                        // Check if enemy is directly reachable and close enough (bulletRange)
                        Vector3 start = transform.position;
                        start.y = 0.5f;
                        shootRay.origin = start;
                        shootRay.direction = playerEnemyVector.normalized;

                        if (Physics.Raycast(shootRay, out shootHit, bulletRange, shootableMask))
                        {
                            EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();
                            if (enemyHealth != null)
                            {
                                // Enemy in view and in range
                                validEnemyInViewAndRange = true;

                                // Get closest enemy
                                if (playerEnemyVector.magnitude < closestEnemyVector.magnitude)
                                {
                                    closestEnemyVector = playerEnemyVector;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                EnemyManager.enemyList.Remove(enemy);
                Debug.Log("Remove enemy from list.", gameObject);
            }
        }
        
        if (validEnemyInViewAndRange)
            return Quaternion.LookRotation(closestEnemyVector.normalized);
        
        return joysticRotation;
    }


    void Animating()
    {
        // Create a boolean that is true if either of the input axes is non-zero.
        bool walking = movement.x != 0f || movement.y != 0f;

        // Tell the animator whether or not the player is walking.
        anim.SetBool("IsWalking", walking);
    }
}


/*
void ShootingAdvisor()
{
    // Tune rotation in case of shooting
    //Debug.Log("ShootingAdvisor", gameObject);

    Vector3 playerPosition = transform.position;
    Vector3 closestEnemyVector = new Vector3(float.PositiveInfinity, 0f, float.PositiveInfinity);

    foreach (GameObject enemy in EnemyManager.enemyList.ToArray())
    {
        if (enemy != null)
        {
            EnemyHealth healthComponent = enemy.GetComponent<EnemyHealth>();
            if (healthComponent.currentHealth <= 0)
            {
                // Remove enemies with 0 health
                EnemyManager.enemyList.Remove(enemy);
                Debug.Log("Remove enemy from list.", gameObject);
            }
            else
            {
                // Compute vector between player and enemy
                Vector3 enemyPosition = enemy.GetComponent<Transform>().position;
                Vector3 playerEnemyVector = enemyPosition - playerPosition;

                // Get closest enemy
                if (playerEnemyVector.magnitude < closestEnemyVector.magnitude)
                {
                    closestEnemyVector = playerEnemyVector;
                }
            }
        }
        else
        {
            EnemyManager.enemyList.Remove(enemy);
            Debug.Log("Remove enemy from list.", gameObject);
        }
    }

    rotation = closestEnemyVector.normalized;
}
*/
