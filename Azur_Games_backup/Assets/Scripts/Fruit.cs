using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fruit : MonoBehaviour
{
	public static List<Fruit> fruits = new List<Fruit>();

	public Vector3 lastVel;
	public bool isContactFruit;

	void Start()
	{
		
	}

	void Update()
	{

	}
	private void OnEnable()
	{
		fruits.Add(this);
	}

	private void OnDisable()
	{
		fruits.Remove(this);
	}

	private void OnCollisionEnter(Collision collision)
	{
		//Debug.Log($"Collided with: {collision.gameObject.name}");
		isContactFruit = collision.other.GetComponent<Fruit>() != null;
		 /*if (collision.contactCount > 1) return;

		 var vel = GetComponent<Rigidbody>().velocity.magnitude;
         Debug.Log("Ball Contact " + vel);
         if (vel > 1.5f)
         {
             foreach (var o in Level.instance.playerState.contactObserversFruits)
             {
                 o.OnContactFruit(this, vel);
             }
         }*/
	}
}
