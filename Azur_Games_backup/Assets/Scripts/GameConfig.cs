using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

[CreateAssetMenu]
public class GameConfig : ScriptableObject
{
    [AssetPath.Attribute(typeof(Labyrinth))]
    public string[] labyrinthsStr;
    [AssetPath.Attribute(typeof(Labyrinth))]
    public string[] fanLabyrinthsStr;

	// public Labyrinth[] labyrinths;
	// public Labyrinth[] fanLabyrinths;

	public Fruit apple;
    public Fruit orange;
    public Fruit lime;
    public Fruit water;

    public Ball ball;
    public Ball ballBig;
    public Ball ballMid;

    public int wavesCount = 3;

    [AssetPath.Attribute(typeof(Sprite))]
    public string[] backgroundsStr;
    //public Sprite[] backgrounds;
    public bool useImages;

    public Material labMaterial;
    public Color[] colors;

    public Material ballMaterial;
    public Material ballMiddleMaterial;
    public Material ballBigMaterial;
    public Color[] ballColors;
    public Color[] ballBigColors;
    public Color[] ballMiddleColors;

    public Color[] gradient1;
    public Color[] gradient2;
    public Color[] gradient3;

    public Sprite[] fireworks;


    public Material capMaterial;
    public Material capOutMaterial;

    public Color[] capColors;
    public Color[] capOutColors;


    public Color[] shadowColors;

    public GameObject[] particles;


    public Material tubeMaterial;
    public Color[] tubeColors;

    public ParticleSystem[] contactFx;

    public int[] levelThemeOrder;
    public PhysicMaterial labPhysMat;

    
    public AudioClip[] backgroundSound;
    public AudioClip music;

    public AudioClip rotationSound;
    public float rotationSoundAnglePeriod;

    public AudioClip ballSound;
    public float ballPitchGrow;
    public float fruitPitchGrow;

    public AudioClip buttonSound;
    public AudioClip finishSound;
    public AudioClip ballContactSound;
    public float ballContactAngle = 20f;
    public float ballContactSpeed = 1f;
    public AudioClip wallContactSound;

	//public float[] ballSoundsWeight;


		/*public AudioClip GetWeigtedBallSound()
		{
			var sum = 0f;
			foreach(var w in ballSoundsWeight)
			{
				sum += w;
			}

			var rnd = Random.Range(0f, sum);

			sum = 0f;
			var i = -1;
			foreach (var w in ballSoundsWeight)
			{
				i++;
				sum += w;
				if (sum >= rnd) return ballSounds.Length > i ? ballSounds[i] : ballSounds[0];
			}

			return ballSounds[0];
		}*/
}

struct BallsContactSettings
{

}