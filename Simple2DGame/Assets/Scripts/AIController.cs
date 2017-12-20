using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Description:
 * This script handles the Ship AI of non player agents. The agent has a series of states
 * which are set based on context. Motion is determined by setting a series of target postions
 * too navigate towards and missles are fired continuously on sight of the player.
 */

public class AIController : MonoBehaviour
{
    //Using Coroutines to have repeated behaviors
    IEnumerator curRoutine;

    //singleton AI transform reference for outside reference (kinda a cheat)
    public static Transform AITransform;

    // Prefab reference for the missle
    public Transform missle_prefab;

    // Public vars too be tweaked and iterated on
    public float thrust = 10f;
    public float max_speed = 100f;
    public float rot_speed = 10f;
    public float pos_tollerence = 5f;
    public float rate_of_fire = 10f;

    //AI states which determine conditions and actions performed by the agent
    enum State { Searching, Pursuit, Combat };
    State myState = State.Searching;

    //Current target position for the agent
    Vector3 target_position = Vector3.zero;

    //Local reference to rigidbody to make life easier
    Rigidbody2D my_rigid;

	// Use this for initialization
	void Awake ()
    {
        my_rigid = GetComponent<Rigidbody2D>();
        AITransform = GetComponent<Transform>();
    }
    
    // FixedUpdate calls functions which edit physics properties
    void FixedUpdate()
    {
        MoveToTarget();
    }

    // Essentially the movement controller
    void MoveToTarget()
    {
        //Initialize relative direction of the target
        Vector3 target_dir = (target_position - transform.position).normalized;

        // Roates towards the given target
        Vector3 new_dir = Vector3.RotateTowards(transform.up, target_dir, rot_speed * Time.fixedDeltaTime, 0.0f).normalized;
        my_rigid.velocity = new_dir * my_rigid.velocity.magnitude;
        transform.up = new_dir;

        //Adds force too accelerate to max speed
        if (my_rigid.velocity.magnitude < max_speed)  
        {
            my_rigid.AddForce(transform.up*thrust);
        }

        // Caps Max speed
        if (my_rigid.velocity.magnitude > max_speed)
        {
            my_rigid.velocity = my_rigid.velocity.normalized * max_speed;
        }
    }

    // Beggining of AI behaviour
    void Update()
    {
        DrawDebugTarget();
        StateSwitch();
    }

    //For debugging purposes
    void DrawDebugTarget()
    {
        Debug.DrawLine(transform.position, target_position, Color.green);
    }

    // Selection of behaviour based on current state
    void StateSwitch()
    {
        switch (myState)
        {
            case State.Searching:
                Searching();
                break;
            case State.Pursuit:
                Pursue();
                break;
            case State.Combat:
                Combat();
                break;
        }
    }

    void Searching()
    {
        //Standard behaviour; also the starting behaviour
        if (Vector3.Distance(transform.position, target_position) < pos_tollerence)
        {
            SetRandomPosition();
        }
    }

    void Pursue()
    {
        //Grace Period after loosing sight of the player
        if (Vector3.Distance(transform.position, target_position) < pos_tollerence)
        {
            SetRandomPosition();
            myState = State.Searching;
        }
    }

    void Combat()
    {
        //Behaviour with the player in sight
        if (Vector3.Distance(transform.position, target_position) < pos_tollerence)
        {
            SetCombatPosition();
        }
    }

    //Sets the target position to a random point on the arena
    void SetRandomPosition()
    {
        target_position = new Vector3
            (
                Random.Range(GameState.Boundry.min.x + 5f, GameState.Boundry.max.x - 5f),
                Random.Range(GameState.Boundry.min.y + 5f, GameState.Boundry.max.y - 5f),
                0
            );
    }

    //special movement rules while in combat: move towards postion in direction of player's projected position in 1 sec with varience
    void SetCombatPosition()
    {
        Vector3 player_vel = PlayerController.PlayerTransform.GetComponent<Rigidbody2D>().velocity;

        target_position = 
            transform.position 
            + (PlayerController.PlayerTransform.position + player_vel - transform.position)
            + ((Vector3)Random.insideUnitCircle * 3f);
    }

    //Condition to switch from any state to combat
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            myState = State.Combat;
            SetCombatPosition();
            curRoutine = Firing();
            StartCoroutine(curRoutine);
        }
    }

    //Condition to switch from any state to combat
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            myState = State.Pursuit;
            StopCoroutine(curRoutine);
            Pursue();
        }
    }

    //Firing Routine based on shots per second
    IEnumerator Firing()
    {
        while (myState == State.Combat)
        {
            Transform my_missle = Instantiate(missle_prefab, transform.position, transform.rotation);
            my_missle.GetComponent<MissleController>().target = PlayerController.PlayerTransform;
            yield return new WaitForSeconds(1f/rate_of_fire);
        }
    }
}
