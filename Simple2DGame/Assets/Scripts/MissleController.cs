using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissleController : MonoBehaviour
{
    // Public vars too be tweaked and iterated on
    public float thrust = 40f;
    public float max_speed = 400f;
    public float rot_speed = 10f;
    public float pos_tollerence = 0.5f;

    //Target set by instantient
    public Transform target;

    //Local reference to rigidbody to make life easier
    Rigidbody2D my_rigid;

    // Use this for initialization
    void Start ()
    {
        my_rigid = GetComponent<Rigidbody2D>();
        StartCoroutine(Lock());
	}

    //Update per frame...
    void Update()
    {
        CheckContact();
    }

    //Checks if the missle reached it's destination
    void CheckContact()
    {
        if (Vector3.Distance(transform.position, projected_pos) < pos_tollerence)
        {
            Destroy(gameObject);
        }
    }

    // FixedUpdate calls functions which edit physics properties
    void FixedUpdate()
    {
        Accelerate();
    }

    void Accelerate()
    {
        //Adds force too accelerate to max speed
        if (my_rigid.velocity.magnitude < max_speed)
        {
            my_rigid.AddForce(transform.up * thrust);
        }

        // Caps Max speed
        if (my_rigid.velocity.magnitude > max_speed)
        {
            my_rigid.velocity = my_rigid.velocity.normalized * max_speed;
        }
    }

    Vector3 projected_pos = Vector3.zero;

    //Period of ajustment
    IEnumerator Lock()
    {
        //reduced time to change orientation too 0.2 secs for the sake of gameplay
        float time_remaining = 0.2f;

        float time_to_impact = 1f;

        while (time_remaining > 0f)
        {

            //calculates where the missle will collide with the target
            if (target != null)
            {
                time_to_impact = Vector3.Distance(target.position, transform.position) / max_speed;
                projected_pos = target.position + ((Vector3)target.GetComponent<Rigidbody2D>().velocity) * time_to_impact;
            }
            
            //Initialize relative direction of the projected position
            Vector3 projected_dir = (projected_pos - transform.position).normalized;

            // Rotates towards the given target
            Vector3 new_dir = Vector3.RotateTowards(transform.up, projected_dir, rot_speed * Time.fixedDeltaTime, 0.0f).normalized;
            my_rigid.velocity = new_dir * my_rigid.velocity.magnitude;
            transform.up = new_dir;

            time_remaining -= Time.deltaTime;
            
            yield return null;
        }

        projected_pos = transform.position + ((Vector3)my_rigid.velocity) * (time_to_impact + 1f);
    }

    //Detects if you collided with the enemy
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (target != null)
        {
            if (collision.gameObject.layer == target.gameObject.layer)
            {
                Destroy(gameObject);
            }
        }
    }
}
