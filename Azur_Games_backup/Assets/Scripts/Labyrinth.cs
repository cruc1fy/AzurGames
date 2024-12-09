using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Labyrinth : MonoBehaviour
{
    //public static Labyrinth instance;

    public float rotationMultiplier = 500f;
    int touchSkip;
    float touchX;
    float touchY;
    float prevAng;
    //public GameObject menu;
    //bool menuHidden = false;
    public List<Ball> balls;
    public List<Fruit> fruits;

    //public GameObject floor;
    public GameObject roof;

    public bool isNeedLoonka;
    public bool isNeedShadows;
    public bool isBoss;

    float activationDelay;
    float rotationMult = 2f;
    float currentAngle;
    float targetAngle;

    private void Awake()
    {
        rotationMult = RemoteSettings.GetFloat("rotationMult", 4f);
        //instance = this;
        /*if(!isNeedShadows)
        {
            var meshe = GetComponentsInChildren<MeshRenderer>();
            foreach(var mesh in meshe)
            {
                mesh.receiveShadows = false;
                mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }*/

        
    }

    private void OnEnable()
    {
        activationDelay = 0.2f;
    }
    // Start is called before the first frame update
    void Start()
    {
        var meshe = GetComponentsInChildren<Collider>();
        foreach (var mesh in meshe)
        {
            mesh.material = Level.instance.gameConfig.labPhysMat;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var ball = other.GetComponent<Ball>();
        if(ball != null && ball.transform.parent == transform)
        {
            ball.transform.SetParent(null, true);
        }

		var fruit = other.GetComponent<Fruit>();
		if (fruit != null && fruit.transform.parent == transform)
		{
			fruit.transform.SetParent(null, true);
		}
	}

    // Update is called once per frame
    void Update()
    {
        if(activationDelay > 0)
        {
            activationDelay -= Time.deltaTime;
            return;
        }

        if (Input.touchCount > 0)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
                return;
        }

        if (Input.GetMouseButton(0))
        {

            var offx = Screen.width / 2 - Input.mousePosition.x;
            var offy = Screen.height / 2 - Input.mousePosition.y;

            var offx2 = Screen.width / 2 - touchX;
            var offy2 = Screen.height / 2 - touchY;

            var dx = Input.mousePosition.x - touchX;
            var dy = Input.mousePosition.y - touchY;

            touchX = Input.mousePosition.x;
            touchY = Input.mousePosition.y;

            var angSpeed = 4f * Mathf.PI / 180f;
            var ang = Mathf.Asin(Vector3.Cross(new Vector3(offx, offy, 0f).normalized, new Vector3(offx2, offy2, 0f).normalized).z);
            if (ang != 0f && touchSkip < 3)
            {
                touchSkip++;
                //Debug.Log("Skip");
                ang = 0;
            }
            /*if(ang > 0)
                        {
                            if(ang > prevAng + 1f)
                            {
                                ang = prevAng + 1;
                            }
                            ang = angSpeed * Time.deltaTime;
                        }
                        else if(ang < 0)
                        {
                            if (ang < prevAng - 1f)
                            {
                                ang = prevAng - 1;
                            }
                            ang = -angSpeed * Time.deltaTime;
                        }*/
            if (ang != 0)
            {
                //Debug.Log("Ang = " + ang + " x1 = " + offx + " y1 = "+offy + " x2 = " + offx2 + " y2 = " + offy2);
            }
            prevAng = ang;
            
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var rot = -rotationMult * ang * 180f / Mathf.PI;// - dx * rotationMultiplier / Screen.width * 0.8f;
                if (Input.mousePosition.y < Screen.height / 2)
                {
                    //rot = -rot;
                }
                targetAngle += rot;
                Level.rotsum += Mathf.Abs(rot);

                Level.instance.playerState.Rotate(rot);

                if(Time.timeScale == 0)
                {
                    Time.timeScale = 1f;
                }
            }
            //Player.instance.rotation += rot;

            /*if (!menuHidden)
            {
                menuHidden = true;
                menu.SetActive(false);
            }*/
        }
        else
        {
            touchSkip = 0;
        }

        if(targetAngle != currentAngle)
        {
            var prevAngle = currentAngle;
            currentAngle = (targetAngle + currentAngle) * 0.5f;
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle - prevAngle) * transform.rotation;
        }
    }
}
