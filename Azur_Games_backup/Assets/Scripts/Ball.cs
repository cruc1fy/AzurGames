using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public static List<Ball> balls = new List<Ball>();

    public Vector3 lastVel;
    public bool isContactBall;
    private void OnEnable()
    {
        balls.Add(this);
	}

    private void OnDisable()
    {
        balls.Remove(this);
		
	}

    private void OnCollisionEnter(Collision collision)
    {
		isContactBall = collision.other.GetComponent<Ball>() != null;
       // if (collision.contactCount > 1) return;

        /* var vel = GetComponent<Rigidbody>().velocity.magnitude;
         Debug.Log("Ball Contact " + vel);
         //if (vel > 1.5f)
         {
             foreach (var o in Level.instance.playerState.contactObservers)
             {
                 o.OnContactBall(this, vel);
             }
         }*/
    }
}
