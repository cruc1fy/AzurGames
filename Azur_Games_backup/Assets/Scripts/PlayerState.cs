using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public interface ICaptureBallObserver
{
    void OnCaptureBall(Ball ball);
}

public interface ICaptureFruitObserver
{
	void OnCaptureFruit(Fruit fruit);
}

public interface IContactBallObserver
{
    void OnContactBall(Ball ball, float vel);
    void OnContactWall(Ball ball, float vel);
}

public interface IContactFruitObserver
{
	void OnContactFruit(Fruit fruit, float vel);
	void OnContactWallFruit(Fruit fruit, float vel);
}

public interface IWaveObserver
{
    void OnWaveComplete();
}

public interface ILevelObserver
{
    void OnLevelComplete();
    void OnLevelStart();
}

public interface IRotationObserver
{
    void OnRotate(float angle);
}

public interface ISettingsObserver
{
    void OnChangeSound(bool enable);
    void OnChangeMusic(bool enable);
    void OnVibration(bool enable);
}

public interface IUIObserver
{
    void OnButton();
}



[CreateAssetMenu]
public class PlayerState : ScriptableObject
{
    [System.NonSerialized]
    public int score;

    public int best;
    public bool ads;

    public int level;

    public int totalBalls;
    public int totalFruits;
    public int totalBallsPerfect;
    public int totalFruitsPerfect;
    public int capturedBalls;
    public int capturedFruits;

	private HashSet<Fruit> processedFruits = new HashSet<Fruit>();


	public int totalWaves;
    public int wave;

    public bool vibrate = true;

    private PlayerState instance;


    [System.NonSerialized]
    public List<ICaptureBallObserver> captureBallObservers = new List<ICaptureBallObserver>();
    public List<ICaptureFruitObserver> captureFruitObservers = new List<ICaptureFruitObserver>();
    public List<IWaveObserver> waveObservers = new List<IWaveObserver>();
    public List<ILevelObserver> levelObservers = new List<ILevelObserver>();
    public List<IRotationObserver> rotationObservers = new List<IRotationObserver>();
    public List<ISettingsObserver> settingsObservers = new List<ISettingsObserver>();
    public List<IUIObserver> uiObservers = new List<IUIObserver>();
    public List<IContactBallObserver> contactObservers = new List<IContactBallObserver>();
    public List<IContactFruitObserver> contactObserversFruits = new List<IContactFruitObserver>();
    



    public void CaptureBall(Ball ball)
    {
		capturedBalls++;
        foreach(var observer in captureBallObservers)
        {
            observer.OnCaptureBall(ball);
            ball.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);

		}
		if (capturedBalls == totalBalls - RemoteSettings.GetInt("BallsPrecision", 1))
        {
            if (wave < totalWaves - 1)
            {
                WaveComplete();
            }
            else
            {
                LevelComplete();
            }
        }
    }

	public void CaptureFruit(Fruit fruit)
	{
		capturedFruits++;

		if (processedFruits.Contains(fruit))
		{
			Debug.Log("This fruit was already captured!");
			return;
		}

		foreach (var observer in captureFruitObservers)
		{
			observer.OnCaptureFruit(fruit);
		}
		if (capturedFruits == totalFruits - RemoteSettings.GetInt("BallsPrecision", 1))
		{
			if (wave < totalWaves - 1)
			{
				WaveComplete();
			}
			else
			{
				LevelComplete();
			}
		}
    }

	public void WaveComplete()
    {
        wave++;
        foreach (var observer in waveObservers)
        {
            observer.OnWaveComplete();
        }
    }

    public void LevelComplete()
    {
        foreach (var observer in levelObservers)
        {
            observer.OnLevelComplete();
        }
    }

    public void LevelStart()
	{
		foreach (var observer in levelObservers)
        {
            observer.OnLevelStart();
        }
    }

    public void Rotate(float angle)
    {
        foreach (var observer in rotationObservers)
        {
            observer.OnRotate(angle);
        }
    }

    public void ChangeSound(bool enable)
    {
        foreach (var observer in settingsObservers)
        {
            observer.OnChangeSound(enable);
        }
    }
    
    public void ChangeMusic(bool enable)
    {
        foreach (var observer in settingsObservers)
        {
            observer.OnChangeMusic(enable);
        }
    }

    public void UIButton()
    {
        foreach (var observer in uiObservers)
        {
            observer.OnButton();
        }
    }
    
    public void Perfect()
    {

    }

    public void Save()
    {
        PlayerPrefs.SetInt("best", best);
        PlayerPrefs.SetInt("level", level);
        PlayerPrefs.SetInt("vibrate", vibrate ? 1 : 0);
    }

    public void Load()
    {
        best = PlayerPrefs.GetInt("best");
        level = PlayerPrefs.GetInt("level");
		vibrate = PlayerPrefs.GetInt("vibrate", 1) == 1;
    }

}
