using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Description:
 * This script handles the majority of player functions. In addition however,
 * player deceloration and collisions are of course handeled by the Rigibody2D and Circle collider
 * respectively.
 */

public class PlayerController : MonoBehaviour
{
    // Prefab reference for the missle
    public Transform missle_prefab;

    // Public vars too be tweaked and iterated on
    public float thrust = 10f;
    public float max_speed = 10f;

    //Local reference to rigidbody to make life easier
    Rigidbody2D my_rigid;

    //singleton player transform reference for outside reference
    public static Transform PlayerTransform;

    // Use this for initialization
    void Awake()
    {
        my_rigid = GetComponent<Rigidbody2D>();
        PlayerTransform = GetComponent<Transform>();
    }

    void Update()
    {
        PlayerInput();
    }

    //Gets input from the player
    void PlayerInput()
    {
        //Fires missle
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Transform my_missle = Instantiate(missle_prefab, transform.position, transform.rotation);
            my_missle.GetComponent<MissleController>().target = AIController.AITransform;
        }
    }

    // FixedUpdate calls functions which edit physics properties
    void FixedUpdate()
    {
        MouseMovement();
    }

    // Majority of functionality
    void MouseMovement()
    {
        //Initialize relative direction of cursor
        Vector2 dir = Input.mousePosition - (new Vector3(Screen.width / 2, Screen.height / 2));
        dir = dir.normalized;

        // Rotates Ship to face cursor
        transform.Find("ShipRenderer").up = dir;

        // Adds Directional thrust on left click from Mouse
        if (Input.GetKey(KeyCode.Mouse0))
        {
            my_rigid.AddForce(dir * thrust);
        }

        //Caps Max Speed
        if (my_rigid.velocity.magnitude > max_speed)
        {
            my_rigid.velocity = my_rigid.velocity.normalized * max_speed;
        }
    }
}
