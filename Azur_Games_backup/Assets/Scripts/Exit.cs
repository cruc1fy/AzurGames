using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    public PlayerState playerState;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
		var ball = other.GetComponent<Ball>();
        if(ball != null)
        {
			playerState.CaptureBall(ball);
			//Destroy(ball.gameObject, 0.1f);
		}   
			var fruit = other.GetComponent<Fruit>();
			if (fruit != null)
			{
				playerState.CaptureFruit(fruit);
			}
        
    }
}
