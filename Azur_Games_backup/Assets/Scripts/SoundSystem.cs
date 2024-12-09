using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSystem : MonoBehaviour, ILevelObserver, IRotationObserver, ICaptureBallObserver, ICaptureFruitObserver, IWaveObserver, ISettingsObserver, IUIObserver, IContactBallObserver
{
    public GameConfig gameConfig;
    public PlayerState playerState;

    public int clips = 20;
    int clip = 0;

    AudioSource[] audioClips;

    AudioSource levelSound;
    AudioSource musicSound;

    float angSum;

    public static int ballsPitcher;
    public static int fruitsPitcher;
    float pitchTime;


    float contactBallTime;
    float contactWallTime;

	private static SoundSystem instance;

	private void Awake()
    {
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		audioClips = new AudioSource[clips];
        for (int i = 0; i < clips; i++)
        {
            audioClips[i] = gameObject.AddComponent<AudioSource>();
        }

        levelSound = gameObject.AddComponent<AudioSource>();

        musicSound = gameObject.AddComponent<AudioSource>();
        musicSound.clip = (gameConfig.music);
        musicSound.loop = true;
        musicSound.Play();

        UpdateSetings();

        playerState.levelObservers.Add(this);
        playerState.rotationObservers.Add(this);
        playerState.captureBallObservers.Add(this);
        playerState.captureFruitObservers.Add(this);
        playerState.waveObservers.Add(this);
        playerState.settingsObservers.Add(this);
        playerState.uiObservers.Add(this);
        playerState.contactObservers.Add(this);
    }

    void UpdateSetings()
    {
        int isMusic = PlayerPrefs.GetInt("Music", 1);
        int isSound = PlayerPrefs.GetInt("Sound", 1);

        musicSound.volume = isSound;

        levelSound.volume = isSound;
        foreach(var c in audioClips)
        {
            c.volume = isSound;
        }
    }

    void PlayClip(AudioClip audioClip, float pitch, float volume = 1f)
    {
        var src = audioClips[clip % audioClips.Length];
        clip++;

        src.pitch = pitch;
        src.PlayOneShot(audioClip, volume);
    }


    public void OnLevelComplete()
    {
        PlayClip(gameConfig.finishSound, 1f);
    }

    public void OnLevelStart()
    {
        ballsPitcher = 0;
        fruitsPitcher = 0;
        var theme = gameConfig.levelThemeOrder.Length > 0 ? gameConfig.levelThemeOrder[playerState.level % gameConfig.levelThemeOrder.Length] : playerState.level;

        levelSound.clip = gameConfig.backgroundSound[theme];
        levelSound.loop = true;
        levelSound.Play();
	}

    public void OnRotate(float angle)
    {
        angSum += Mathf.Abs(angle);
        if(angSum > gameConfig.rotationSoundAnglePeriod)
        {
            angSum = 0;
            PlayClip(gameConfig.rotationSound, 1f);
        }
    }

    public void OnCaptureBall(Ball ball)
    {
        if (Time.time > pitchTime + 0.05)
        {
            PlayClip(gameConfig.ballSound, 1f + gameConfig.ballPitchGrow * ballsPitcher);
            

            pitchTime = Time.time;

        }
        ballsPitcher++;
    }

	public void OnCaptureFruit(Fruit fruit)
	{
		if (Time.time > pitchTime + 0.05)
		{
			PlayClip(gameConfig.ballSound, 1f + gameConfig.fruitPitchGrow * ballsPitcher);


			pitchTime = Time.time;

		}
		fruitsPitcher++;
	}

	public void OnWaveComplete()
    {
        //ballsPitcher = 0;
    }

    public void OnChangeSound(bool enable)
    {
        UpdateSetings();
    }

    public void OnChangeMusic(bool enable)
    {
        UpdateSetings();
    }

    public void OnVibration(bool enable)
    {
    }

    void IUIObserver.OnButton()
    {
        PlayClip(gameConfig.buttonSound, 1f);
    }

    public void OnContactBall(Ball ball, float vel)
    {
       /* if (vel > 10f)
        {
            PlayClip(gameConfig.ballContactSound, 1f, 1f);
        }
        else*/ if (vel > gameConfig.ballContactSpeed && vel > 1f)
        {
            vel = 1f;

			if (Time.time > contactBallTime + 0.05)
            {
                PlayClip(gameConfig.ballContactSound, Random.Range(0.85f, 1.35f));//, vel / 10f);
                contactBallTime = Time.time;
            }
        }
    }

	public void OnContactFruit(Fruit fruit, float vel)
	{
		/* if (vel > 10f)
		 {
			 PlayClip(gameConfig.ballContactSound, 1f, 1f);
		 }
		 else*/
		if (vel > gameConfig.ballContactSpeed)
		{
			if (Time.time > contactBallTime + 0.05)
			{
				PlayClip(gameConfig.ballContactSound, Random.Range(0.85f, 1.35f));//, vel / 10f);
				contactBallTime = Time.time;
			}
		}
	}

	private void Update()
    {
        if (Level.instance == null) return;

        var lab = Level.instance.GetActiveLab();
        if (lab == null) return;

        foreach(var ball in Ball.balls)
        {
            if (ball.transform.parent != lab.transform) continue;

            var vel =  ball.GetComponent<Rigidbody>().velocity;

            if (Vector3.Angle(vel, ball.lastVel) > gameConfig.ballContactAngle)
            {
                if (ball.isContactBall)
                {
                    OnContactBall(ball, ball.lastVel.magnitude);
                }
                else
                {
                    OnContactWall(ball, ball.lastVel.magnitude);
                }

            }


            ball.lastVel = vel;
        }

		foreach (var fruit in Fruit.fruits)
		{
			if (fruit.transform.parent != lab.transform) continue;

			var vel = fruit.GetComponent<Rigidbody>().velocity;

			if (Vector3.Angle(vel, fruit.lastVel) > gameConfig.ballContactAngle)
			{
				if (fruit.isContactFruit)
				{
					OnContactFruit(fruit, fruit.lastVel.magnitude);
				}
				else
				{
					OnContactWallFruit(fruit, fruit.lastVel.magnitude);
				}

			}


			fruit.lastVel = vel;
		}
	}

    public void OnContactWall(Ball ball, float vel)
    {
        if (vel > gameConfig.ballContactSpeed)
        {
            if (Time.time > contactWallTime + 0.05)
            {
                PlayClip(gameConfig.wallContactSound, Random.Range(0.85f, 1.35f));//, vel / 10f);
                contactWallTime = Time.time;
            }
        }
    }

	public void OnContactWallFruit(Fruit fruit, float vel)
	{
		if (vel > gameConfig.ballContactSpeed)
		{
			if (Time.time > contactWallTime + 0.05)
			{
				PlayClip(gameConfig.wallContactSound, Random.Range(0.85f, 1.35f));//, vel / 10f);
				contactWallTime = Time.time;
			}
		}
	}
}
