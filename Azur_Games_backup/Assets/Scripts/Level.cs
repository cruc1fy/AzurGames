
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Purchasing;
using UnityEngine.SocialPlatforms;
using System.Diagnostics.Eventing.Reader;
using Unity.RemoteConfig;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Net;
using UnityEditorInternal;










#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif


#if UNITY_IOS
using Unity.Notifications.iOS;
using UnityEngine.iOS;
#endif

public class Level : MonoBehaviour, IWaveObserver, ILevelObserver, ICaptureFruitObserver, ICaptureBallObserver, IPurchaseObserver, IRewardedObserver, IConsentObserver, IInterstitialObserver
{
	public static Level instance;
	public PlayerState playerState;
	public GameConfig gameConfig;

	public float size = 2f;

	int activeLab;

	List<Labyrinth> labyrinths = new List<Labyrinth>();

	public Labyrinth penek; /*Resources.Load<Labyrinth("Prefabs/penek1");*/

	List<Ball> balls = new List<Ball>();
	List<Fruit> fruits = new List<Fruit>();

	public Text levelText;

	bool inMove;

	public Image backgroundImage;
	public GradientBackground gradientBackground;

	public Text scoreText;

	public GameObject exitTubePrefab;

	public GameObject menu;
	bool menuHidden = false;

	public GameObject board1;
	public GameObject board2;


	public GameObject NoAdsButton;
	public GameObject BannerBack;

	public GameObject loonka;

	public GameObject SkipButton;

	public Labyrinth[] mazes;

	static float adsTime;

	int bonusBalls;

	public GameObject[] pointsOff;
	public GameObject[] pointsOn;

	public Text ballsCountText;

	public GameObject gameMenu;

	ParticleSystem contactFX;
	ParticleSystem[] contactFXchildren;

	static bool renewNotifications;

	float levelStartTime;

	public Camera cam;

	static readonly string[] phrases = { "Hard Ball to Swallow", "A Piece of Cake", "Balls Goes Up Balls Come Down", "Balls Doesn't Grow On Trees", "Two Down, One to Go", "Down For The Count" };

	public int currentLevel;

	struct userAttributes { }
	struct appAttributes { }


	private void Awake()
	{
		currentLevel = PlayerPrefs.GetInt("level"); ;
		instance = this;
	}

	private void OnEnable()
	{
		AdsAnaliticsManager.rewardedObservers.Add(this);
		AdsAnaliticsManager.consentObservers.Add(this);
		IAP_Manager.purchaseObservers.Add(this);

		playerState.waveObservers.Add(this);
		playerState.levelObservers.Add(this);
		playerState.captureBallObservers.Add(this);
		playerState.captureFruitObservers.Add(this);
	}

	private void OnDisable()
	{
		AdsAnaliticsManager.rewardedObservers.Remove(this);
		AdsAnaliticsManager.consentObservers.Remove(this);
		IAP_Manager.purchaseObservers.Remove(this);

		playerState.waveObservers.Remove(this);
		playerState.levelObservers.Remove(this);
		playerState.captureBallObservers.Remove(this);
		playerState.captureFruitObservers.Remove(this);
	}

	void OnDestroy()
	{
		ConfigManager.FetchCompleted -= OnRemoteSettingsLoaded;
	}

	void OnRemoteSettingsLoaded(ConfigResponse response)
	{
		if (response.requestOrigin == ConfigOrigin.Remote)
		{
			Debug.Log("Remote settings loaded successfully.");
		}
		else if (response.requestOrigin == ConfigOrigin.Cached)
		{
			Debug.Log("Loaded from cache.");
		}
		else
		{
			Debug.LogError("Failed to load settings. Using default values.");
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		playerState.totalFruits = fruits.Count;

		ConfigManager.FetchCompleted += OnRemoteSettingsLoaded;

		ConfigManager.FetchConfigs(new userAttributes(), new appAttributes());


		Application.targetFrameRate = 60;

#if UNITY_IOS
        /*var delta = Vector2.up * (Screen.height - Screen.safeArea.height - Screen.safeArea.y);
        BannerBack.GetComponent<RectTransform>().sizeDelta += delta;
        GameMenu.instance.GetComponent<RectTransform>().sizeDelta -= delta;
        GameMenu.instance.GetComponent<RectTransform>().anchoredPosition += 0.5f*delta;
        FinishMenu.instance.GetComponent<RectTransform>().sizeDelta -= delta * 2f;
        FinishMenu.instance.GetComponent<RectTransform>().anchoredPosition += 1.0f * delta;
        SuckMenu.instance.GetComponent<RectTransform>().sizeDelta -= delta * 2f;
        SuckMenu.instance.GetComponent<RectTransform>().anchoredPosition += 1.0f * delta;
        */       
#endif
		rotsum = 0f;

		playerState.Load();

		playerState.LevelStart();

		var targetAspect = 1080f / 1920f;
		var currentAspect = Screen.width / (float)Screen.height;

		if (targetAspect > currentAspect)
			Camera.main.transform.position = Vector3.back * 16f * targetAspect / currentAspect;

		SkipButton.SetActive(AdsAnaliticsManager.instance.CanShowRewarded());

		if (PlayerPrefs.GetInt(IAP_Manager.kProductIDNonConsumable) == 1)
		{
			NoAdsButton.gameObject.SetActive(false);
			BannerBack.SetActive(false);
		}

		//if (RemoteSettings.GetBool("NoBanner"))
		{
			BannerBack.SetActive(false);
		}

		if (PlayerPrefs.GetInt("rate") == 0 && (playerState.level == RemoteSettings.GetInt("RateFirst", 4) || playerState.level == RemoteSettings.GetInt("RateSecond", 9)))
		{
			RateMenu.instance.Show();
		}
		else if (CanShowAds())
		{
			ShowAds((bool result) => { });
		}
		else
		{
			AdsAnaliticsManager.instance.LoadAds();
			// BannerBack.SetActive(false);
		}

		PlayerPrefs.SetInt("SkipAds", 0);

		levelStartTime = Time.time;

		DOTween.Sequence().AppendInterval(5f).AppendCallback(() =>
		{
			AdsAnaliticsManager.instance.TrackLevelStart(playerState.level);
		});

		if (!renewNotifications)
		{
			renewNotifications = true;
			//StartCoroutine(SpawnNotifications());
			PlayerPrefs.Save();
		}
		ScorePopupSystem.isFirstBall = true;

		scoreText.text = playerState.score.ToString();

		var theme = gameConfig.levelThemeOrder.Length > 0 ? gameConfig.levelThemeOrder[playerState.level % gameConfig.levelThemeOrder.Length] : playerState.level;

		if (gameConfig.useImages)
		{
			backgroundImage.sprite = AssetPath.Load<Sprite>(gameConfig.backgroundsStr[theme % gameConfig.backgroundsStr.Length]);
		}
		else
		{
			gradientBackground.SetColor(gameConfig.gradient3[theme % gameConfig.gradient1.Length], gameConfig.gradient1[theme % gameConfig.gradient3.Length]);
		}


		gameConfig.labMaterial.SetColor("_MainColor", gameConfig.colors[theme % gameConfig.colors.Length]);
		gameConfig.labMaterial.SetColor("_RimColor", gameConfig.colors[theme % gameConfig.colors.Length]);
		gameConfig.tubeMaterial.SetColor("_MainColor", gameConfig.tubeColors[theme % gameConfig.tubeColors.Length]);
		gameConfig.ballMaterial.color = gameConfig.ballColors[theme % gameConfig.ballColors.Length];
		gameConfig.ballMiddleMaterial.color = gameConfig.ballMiddleColors[theme % gameConfig.ballMiddleColors.Length];
		gameConfig.ballBigMaterial.color = gameConfig.ballBigColors[theme % gameConfig.ballBigColors.Length];
		gameConfig.capMaterial.SetColor("_MainColor", gameConfig.capColors[theme % gameConfig.capColors.Length]);
		gameConfig.capOutMaterial.SetColor("_MainColor", gameConfig.capOutColors[theme % gameConfig.capOutColors.Length]);

		Shader.SetGlobalColor("_GlobalShadowColor", gameConfig.shadowColors[theme % gameConfig.shadowColors.Length]);

		Instantiate(gameConfig.particles[theme % gameConfig.particles.Length]);
		contactFX = Instantiate(gameConfig.contactFx[theme % gameConfig.contactFx.Length]);
		contactFXchildren = contactFX.GetComponentsInChildren<ParticleSystem>();

		playerState.totalWaves = gameConfig.wavesCount;
		playerState.wave = 0;

		var ballsLevel = RemoteSettings.GetInt("ExtraBallsLevel", 3);
		var fruitsLevel = RemoteSettings.GetInt("ExtraBallsLevel", 3);
		playerState.totalBalls = ((playerState.level + ballsLevel - 1) / ballsLevel) * RemoteSettings.GetInt("ballsPerLevel", 15) + RemoteSettings.GetInt("StartBallsCount", 20);
		playerState.totalFruits = ((playerState.level + fruitsLevel - 1) / fruitsLevel) * RemoteSettings.GetInt("ballsPerLevel", 15) + RemoteSettings.GetInt("StartBallsCount", 20);
		if (playerState.totalBalls > RemoteSettings.GetInt("maxBalls", 450)) playerState.totalBalls = RemoteSettings.GetInt("maxBalls", 450);
		if (playerState.totalFruits > RemoteSettings.GetInt("maxBalls", 450)) playerState.totalFruits = RemoteSettings.GetInt("maxBalls", 450);

		var isBonusLevel = PlayerPrefs.GetInt("BonusLevel", 0) == 1;
		if (PlayerPrefs.GetInt("BonusLevel", 0) == 1)
		{
			PlayerPrefs.SetInt("BonusLevel", 0);

			playerState.totalBalls += (int)(playerState.totalBalls * RemoteSettings.GetFloat("ExtraBallsPercent", 0.33f));
		}
#if UNITY_EDITOR
		//playerState.totalBalls = 1;
#endif

		playerState.totalBallsPerfect = playerState.totalBalls;
		playerState.totalFruitsPerfect = playerState.totalFruits;

		playerState.capturedBalls = 0;
		playerState.capturedFruits = 0;

		levelText.text = "Level " + (playerState.level + 1);
		if (isBonusLevel)
		{
			levelText.text += " Bonus";
		}

		var isBossLevel = false;


		Random.InitState(playerState.level);
		var labs = gameConfig.labyrinthsStr;

		for (int i = 0; i < gameConfig.wavesCount; i++)
		{
			Labyrinth lab;

			var labNum = playerState.level * gameConfig.wavesCount + i;
			if (labNum < labs.Length)
			{
				lab = AssetPath.Load<Labyrinth>(labs[labNum]);
			}
			else
			{
				/*if (Random.value < RemoteSettings.GetFloat("FunLevelChance", 0.1f))
                {
                    lab = gameConfig.fanLabyrinths[Random.Range(0, gameConfig.fanLabyrinths.Length)];
                }
                else*/
				{
					lab = mazes[Random.Range(0, mazes.Length)];

					lab.GetComponentInChildren<MazeGenerator>().Draw();
				}
			}
			if (lab.isNeedLoonka)
			{
				loonka.SetActive(true);

			}
			if (lab.isBoss) isBossLevel = true;

			Labyrinth lab1 = penek;
			Quaternion rotateValue = Quaternion.Euler(0, 0, 0);

			if (PlayerPrefs.GetInt("level") != 1)
			{
				lab1 = lab;
				
				
			}
			else
			{
				if (i == 0)
				{
					lab1 = lab;
					Debug.Log($"lab1 is {lab1}");
					rotateValue *= Quaternion.Euler(0, 0, 0);
				}
				else if (i == 1)
				{
					lab1 = lab;
					Debug.Log($"lab1 is {lab1}");
					rotateValue *= Quaternion.Euler(0, 0, 0);
				}
				else if (i == 2)
				{
					lab1 = penek;
					rotateValue *= Quaternion.Euler(360f, 0f, -90f);
				}
			}

			float spawnValue = (lab1 == penek) ? 2.1f : i;
			var newLab = SpawnLab(lab1, spawnValue);

			/*if (PlayerPrefs.GetInt("level") == 1 && playerState.wave == 2)
			{

			}*/
			if (i == 2)
			{
				rotateValue = Quaternion.Euler(0f, 270f, -90f);
				newLab.transform.rotation = Quaternion.identity * rotateValue;
			}

			//}


			if (i == 0)
			{
				int currentLevel = PlayerPrefs.GetInt("level");
				if (currentLevel == 3)
				{
					SpawnFruits(newLab, playerState.totalBalls);
				}
				else 
				{
					SpawnBalls(newLab, playerState.totalBalls);
				}

				newLab.transform.rotation = Quaternion.identity;
			}
		}

		if (isBossLevel)
		{
			levelText.text = "Boss Level " + (playerState.level + 1);
		}

		SpawnExit(gameConfig.wavesCount);

		ActivateLab(activeLab);
		ActivateRoof(activeLab);

		labyrinths[2].transform.rotation = Quaternion.Euler(0f, 0f, 0f);

		ActivateBalls(activeLab);
		ActivateFruits(activeLab);
	}

	static private IEnumerator SpawnNotifications()
	{
#if UNITY_ANDROID
        var c = new AndroidNotificationChannel()
        {
            Id = "channel_id",
            Name = "Default Channel",
            Importance = Importance.High,
            Description = "Generic notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(c);

        AndroidNotificationCenter.CancelAllNotifications();

        for (int i = 1; i <= 3; i++)
        {
            var target = RemoteSettings.GetInt("NotificationTime" + i, i * 24);

            var not = new AndroidNotification("Balls, Balls, Balls", phrases[Random.Range(0, phrases.Length)], System.DateTime.Now.AddHours(target));
            not.SmallIcon = "icon_0";
            not.LargeIcon = "icon_1";
            AndroidNotificationCenter.SendNotification(not, "channel_id");
        }
#endif
#if UNITY_IOS
        bool granted = false;
        using (var req = new AuthorizationRequest(AuthorizationOption.AuthorizationOptionAlert | AuthorizationOption.AuthorizationOptionBadge, true))
        {
            while (!req.IsFinished)
            {
                yield return null;
            };

            string res = "\n RequestAuthorization: \n";
            res += "\n finished: " + req.IsFinished;
            res += "\n granted :  " + req.Granted;
            res += "\n error:  " + req.Error;
            res += "\n deviceToken:  " + req.DeviceToken;
            Debug.Log(res);

            granted = req.Granted;
        }
        if(granted)
        {
            for (int i = 1; i <= 3; i++)
            {
                iOSNotificationCenter.RemoveScheduledNotification("_notification_0" + i);

                var timeTrigger = new iOSNotificationTimeIntervalTrigger()
                {
                    TimeInterval = new System.TimeSpan(RemoteSettings.GetInt("NotificationTime" + i, i * 24), 0, 0),
                    Repeats = false
                };

                var notification = new iOSNotification()
                {
                    // You can optionally specify a custom Identifier which can later be 
                    // used to cancel the notification, if you don't set one, an unique 
                    // string will be generated automatically.
                    Identifier = "_notification_0" + i,
                    Title = "Balls, Balls, Balls",
                    Body = phrases[Random.Range(0, phrases.Length)],
                    Subtitle = "",
                    ShowInForeground = true,
                    ForegroundPresentationOption = (PresentationOption.NotificationPresentationOptionAlert | PresentationOption.NotificationPresentationOptionSound),
                    CategoryIdentifier = "category_a",
                    ThreadIdentifier = "thread1",
                    Trigger = timeTrigger,
                };

                iOSNotificationCenter.ScheduleNotification(notification);
            }
        }
#endif
		yield return null;
	}

	private void SpawnFruits(Labyrinth newLab, int max)
	{
		gameConfig.apple = Resources.Load<GameObject>("Prefabs/apple").GetComponent<Fruit>();
		gameConfig.orange = Resources.Load<GameObject>("Prefabs/Orange").GetComponent<Fruit>();
		gameConfig.lime = Resources.Load<GameObject>("Prefabs/Lime").GetComponent<Fruit>();
		gameConfig.water = Resources.Load<GameObject>("Prefabs/Cube").GetComponent<Fruit>();
		var totalMax = RemoteSettings.GetInt("maxFruits", 450);
		var hasDifferentFruits = RemoteSettings.GetBool("hasDifferentFruits", false);

		var deep = 2;

		var sq = (int)Mathf.Sqrt(max / 5) + 1;

		var count = 0;
		int fruitType = 0;
		for (var d = -deep; d <= deep; d++)
		{
			for (var j = 0; j < sq * sq; j++)
			{
				switch(fruitType)
				{
					case 0:
						fruitType = 1;
						break;
					case 1:
						fruitType = 2;
						break;
					case 2:
						fruitType = 0;
						break;
				}

				//int fruitType = count % 10;
				/*if (fruitType < 7 || !hasDifferentFruits) fruitType = 0;
				else if (fruitType < 9) fruitType = 1;
				else if (fruitType < 10) fruitType = 2;*/
				if (count >= max) continue;

				var fruit = Instantiate(fruitType == 2 ? gameConfig.apple : fruitType == 1 ? gameConfig.orange : gameConfig.lime, newLab.transform);

				float scale;
				fruit.transform.localPosition = Vector3.right * (j % sq - sq / 2) * 0.1f + Vector3.down * (j / sq - sq / 2) * 0.1f + Vector3.forward * d * 0.1f;

				if (PlayerPrefs.GetInt("level") == 1)
				{
					scale = ConfigManager.appConfig.GetFloat(fruitType == 0 ? "fruitSizePenek3" : fruitType == 1 ? "fruitSizePenek2" : "fruitSizePenek1", 1f);
				}
				else
				{
					scale = ConfigManager.appConfig.GetFloat(fruitType == 0 ? "fruitSize3" : fruitType == 1 ? "fruitSize2" : "fruitSize1", 1f);
				}
					
					fruit.transform.localScale *= (scale - 1f) * (totalMax - max) / totalMax + 1f;

					fruits.Add(fruit);

				

				count++;
				ballsCountText.text = "x " + fruits.Count;
			}
		}
	}

	private void SpawnBalls(Labyrinth newLab, int max)
	{
		var totalMax = RemoteSettings.GetInt("maxBalls", 450);
		var hasDifferentBalls = RemoteSettings.GetBool("hasDifferentBalls", false);
		var deep = 2;

		var sq = (int)Mathf.Sqrt(max / 5) + 1;
		var count = 0;
		for (var d = -deep; d <= deep; d++)
		{
			for (var j = 0; j < sq * sq; j++)
			{
				float scaleFactor = Random.Range(0f, 1f);
				int ballSize = count % 10;
				if (ballSize < 7 || !hasDifferentBalls) ballSize = 0;
				else if (ballSize < 9) ballSize = 1;
				else if (ballSize < 10) ballSize = 2;
				if (count >= max) continue;
				var ball = Instantiate(ballSize == 2 ? gameConfig.ballBig : ballSize == 1 ? gameConfig.ballMid : gameConfig.ball, newLab.transform);
				ball.transform.localPosition = Vector3.right * (j % sq - sq / 2) * 0.1f + Vector3.down * (j / sq - sq / 2) * 0.1f + Vector3.forward * d * 0.1f;

				var scale = RemoteSettings.GetFloat(ballSize == 0 ? "ballSize0" : ballSize == 1 ? "ballSize1" : "ballSize2", 1f);
				ball.transform.localScale *= (scale - 1f) * (totalMax - max) / totalMax + 1f;

				balls.Add(ball);
				count++;
			}
		}

		ballsCountText.text = "x " + balls.Count;
	}

	public bool CanShowAds()
	{
		return playerState.level > RemoteSettings.GetInt("AdsMinimumLevel", 5) && playerState.score > 0 && PlayerPrefs.GetInt("SkipAds") == 0 && Time.time > adsTime + RemoteSettings.GetInt("adsInterval", 45);
	}

	public void ShowAds(System.Action<bool> callback)
	{
		if (CanShowAds())
		{
			if (AdsAnaliticsManager.instance.CanShowInterstitial())
			{
				adsTime = Time.time;
				AdsAnaliticsManager.instance.ShowInterstitial(callback);
				return;
			}
		}
		else if (activeLab == 2)
		{

		}

		callback(false);
	}

	private void SpawnBonusBalls()
	{
		SpawnBalls(labyrinths[activeLab], playerState.totalBalls / 3);
	}

	private void SpawnBonusFruits()
	{
		//SpawnFruits(labyrinths[activeLab], playerState.totalFruits / 3);
	}

	private void ActivateLab(int v)
	{
		for (int i = 0; i < gameConfig.wavesCount; i++)
		{

			var lab = labyrinths[i];
			lab.enabled = i == v;

			if (v != -1)
			{
				pointsOn[i].SetActive(i <= v);
				pointsOff[i].SetActive(i > v);
			}
		}
	}

	private void ActivateRoof(int v)
	{
		for (int i = 0; i < gameConfig.wavesCount; i++)
		{
			var lab = labyrinths[i];
			lab.roof.SetActive(i == v);

		}
	}

	private void ActivateBalls(int v)
	{
		for (int i = 0; i < 3; i++)
		{
			var lab = labyrinths[i];

			foreach (var ball in lab.balls)
			{
				ball.gameObject.SetActive(i == v);
			}
		}
	}

	private void ActivateFruits(int v)
	{
		for (int i = 0; i < 3; i++)
		{
			var lab = labyrinths[i];

			foreach (var fruit in lab.fruits)
			{
				fruit.gameObject.SetActive(i == v);
			}
		}
	}

	/*private Labyrinth SpawnLab(Labyrinth lab, int i)
	{
		var newLab = Instantiate(lab, transform);
		newLab.transform.position = Vector3.down * i * size;

		labyrinths.Add(newLab);

		return newLab;
	}*/

	private Labyrinth SpawnLab(Labyrinth lab, float i)
	{
		var newLab = Instantiate(lab, transform);
		newLab.transform.position = Vector3.down * i * size;


		labyrinths.Add(newLab);
		if (playerState.wave != 0)
		{
			newLab.transform.DORotateQuaternion(Quaternion.Euler(90f, 0f, 0f), 0.8f);
		}
		
		

		return newLab;
	}

	private void SpawnExit(int i)
	{
		var newLab = Instantiate(exitTubePrefab, transform);
		newLab.transform.position = Vector3.down * i * size;


	}
	public static float rotsum = 0f;
	// Update is called once per frame
	void Update()
	{
		
		if (rotsum > 20f && Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			if (Input.touchCount > 0)
			{
				if (EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
					return;
			}
			if (!menuHidden)
			{
				menuHidden = true;
				menu.SetActive(false);
			}
		}
	}

	float vibrateTime;

	Sequence sequence;

	public void OnWaveComplete()
	{
		if (inMove) return;
		var cap = playerState.capturedBalls;
		var capFruits = playerState.capturedFruits;
		//playerState.capturedBalls = 0;
		ActivateLab(-1);
		var al = activeLab;
		bool isVoronka = loonka.activeSelf;

		if (Time.time > vibrateTime + 5f)
		{
			vibrateTime = Time.time;
			if (playerState.vibrate && RemoteSettings.GetBool("Vibro", false))
			{
				//Handheld.Vibrate();
			}
		}
		if (!inMove && sequence != null)
		{
			sequence.Kill();
		}

		sequence = DOTween.Sequence().AppendInterval(RemoteSettings.GetFloat("FinishDelay", 1.3f)).AppendCallback(() =>
		{
			inMove = true;
			playerState.capturedBalls = 0;
			playerState.capturedFruits = 0;
			board1.SetActive(false);
			board2.SetActive(false);
			loonka.SetActive(false);
			activeLab++;
			foreach (var ball in balls)
			{
				Vector3 originalScale = ball.transform.lossyScale;
				if (playerState.wave == 1)
				{
					ball.transform.SetParent(labyrinths[activeLab].transform, true);
					ball.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
				}
				else if (playerState.wave == 2)
				{
					ball.transform.SetParent(labyrinths[activeLab].transform, true);
				}
				

				var vecDist = ball.transform.localPosition;
					if (vecDist.magnitude > 0.4f)
					{
						ball.transform.localPosition = vecDist / vecDist.magnitude * 0.4f;
					}
				//ball.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
				
			}

			foreach (var fruit in fruits)
			{
				Vector3 originalScale = fruit.transform.lossyScale;
				fruit.transform.SetParent(labyrinths[activeLab].transform, true);
				fruit.transform.localScale = originalScale;
				var vecDist = fruit.transform.localPosition;
				if (vecDist.magnitude > 0.4f)
				{
					fruit.transform.localPosition = vecDist / vecDist.magnitude * 0.4f;
				}
			}

			if (playerState.capturedBalls >= playerState.totalBalls)
			{
				playerState.Perfect();
			}

			playerState.totalBalls = cap;
			playerState.totalFruits = capFruits;

			//ActivateBalls(activeLab);
			ActivateRoof(activeLab);

			if (PlayerPrefs.GetInt("level") == 1)
			{
				
				//for (int i = 0; i < gameConfig.wavesCount; i++)
				//{
					labyrinths[al].transform.DORotateQuaternion(Quaternion.Euler(0f, 0f, 0f), 0.8f);
					labyrinths[al].transform.DOLocalMove(labyrinths[al].transform.position + Vector3.up * 3f, 0.8f);
				//}
			}
			else
			{
				Debug.Log("Executed!");
				//labyrinths[al].transform.DORotateQuaternion(Quaternion.Euler(0f, 0f, 0f), 0.8f);
				labyrinths[al].transform.DORotateQuaternion(Quaternion.Euler(-90f, 0f, 0f), 0.8f);
				
				labyrinths[al].transform.DOLocalMove(labyrinths[al].transform.position + Vector3.up * 3f, 0.8f);
			}

		})
		.Append(
			transform.DOMove(Vector3.up * size * (activeLab + 1), 1.5f)
		)
		.AppendCallback(() => {
			if (labyrinths[al + 1].isBoss)
			{
				//Camera.main.transform.DOMoveZ(-16f, 0.8f);
				labyrinths[al + 1].transform.DOScale(0.65f, 0.8f);
			}

		})
		.AppendCallback(() =>
		{
			if (PlayerPrefs.GetInt("level") == 1)
			{
				if (labyrinths[2].transform.position != new Vector3(180f, 180f, 90f))
				{
					labyrinths[2].transform.DORotateQuaternion(Quaternion.Euler(180f, 180f, 90f), 0.8f);
				}

				//labyrinths[1].transform.DORotateQuaternion(Quaternion.Euler(90f, 0f, 0f), 0.8f);

				if (playerState.wave == 0)
				{
					//labyrinths[1].transform.DORotateQuaternion(Quaternion.Euler(270f, 0f, 270f), 0.8f);
				}
				if (playerState.wave == 1)
				{
					labyrinths[1].transform.DORotateQuaternion(Quaternion.Euler(0f, 0f, 0f), 0.8f);
				}
				if (playerState.wave == 2)
				{
					labyrinths[2].transform.DORotateQuaternion(Quaternion.Euler(270f, 0f, 270f), 0.8f);
				}

			}
			else
			{
				if (playerState.wave == 1)
				{
					labyrinths[1].transform.DORotateQuaternion(Quaternion.Euler(0f, 0f, 0f), 0.8f);
					labyrinths[2].transform.DORotateQuaternion(Quaternion.Euler(90f, 0f, 0f), 0.8f);
				}
				else if (playerState.wave == 2) 
				{
					Debug.Log(labyrinths[2]);
					labyrinths[2].transform.DORotateQuaternion(Quaternion.Euler(0f, 0f, 0f), 0.8f);
				}
				//labyrinths[al + 1].transform.DORotateQuaternion(Quaternion.Euler(90f, 90f, 0f), 0.8f);
			}
		}
			
		).AppendCallback(() => {
			ActivateLab(activeLab);
			inMove = false;
			board1.SetActive(true);
			board2.SetActive(true);
			loonka.SetActive(isVoronka);
			ScorePopupSystem.isFirstBall = true;

			SoundSystem.ballsPitcher = 0;
			//ShowAds();
		});


	}
	
	public void OnLevelComplete()
	{
		ActivateLab(-1);

		gameMenu.SetActive(false);

		AdsAnaliticsManager.instance.TrackLevelFinish(playerState.level, "win", Time.time - levelStartTime);
	}

	public void OnCaptureBall(Ball ball)
	{
		if (inMove) return;
		if (activeLab < labyrinths.Count - 1)
		{
			ball.transform.SetParent(labyrinths[activeLab + 1].transform, true);
			if (playerState.wave == 2)
			{
				ball.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
			}
		}

		playerState.score += playerState.level + 1;
		playerState.Save();

		UpdateScore();


		/*if(contactFX.emission.burstCount > 0)
        {
            contactFX.Emit(Random.Range(contactFX.emission.GetBurst(0).minCount, contactFX.emission.GetBurst(0).maxCount+1));
        }*/

		foreach (var fx in contactFXchildren)
		{
			if (fx.emission.burstCount > 0)
			{
				fx.Emit(Random.Range(fx.emission.GetBurst(0).minCount, fx.emission.GetBurst(0).maxCount + 1));
			}
		}

		/*if (RemoteSettings.GetBool("Sound", true))
        {
            AudioSource.PlayClipAtPoint(gameConfig.GetWeigtedBallSound(), cam.transform.position);
        }*/
	}

	public void OnCaptureFruit(Fruit fruit)
	{
		if (inMove) return;
		if (activeLab < labyrinths.Count - 1)
		{
			fruit.transform.SetParent(labyrinths[activeLab + 1].transform, true);
		}

		playerState.score += playerState.level + 1;

		playerState.Save();

		UpdateScore();


		/*if(contactFX.emission.burstCount > 0)
        {
            contactFX.Emit(Random.Range(contactFX.emission.GetBurst(0).minCount, contactFX.emission.GetBurst(0).maxCount+1));
        }*/

		foreach (var fx in contactFXchildren)
		{
			if (fx.emission.burstCount > 0)
			{
				fx.Emit(Random.Range(fx.emission.GetBurst(0).minCount, fx.emission.GetBurst(0).maxCount + 1));
			}
		}

		/*if (RemoteSettings.GetBool("Sound", true))
        {
            AudioSource.PlayClipAtPoint(gameConfig.GetWeigtedBallSound(), cam.transform.position);
        }*/
	}

	public Labyrinth GetActiveLab()
	{
		if (activeLab < 0) return null;
		return labyrinths[activeLab];
	}

	private void UpdateScore()
	{
		scoreText.text = playerState.score.ToString();
	}

	public void OnPurchase(string id)
	{
		if (id == IAP_Manager.kProductIDConsumable)
		{
			NoAdsButton.gameObject.SetActive(false);
			BannerBack.SetActive(false);
		}
	}

	public void OnRewardedReady()
	{
		SkipButton.SetActive(true);
	}

	public void OnRewardedNotReady()
	{
		SkipButton.SetActive(false);
	}

	public void OnConsentDialog()
	{
		/*PlatformDialog.SetButtonLabel("Yes", "No");
        PlatformDialog.Show(
            "GDRP Consent Dialog",
            "Can we use your data for relevant ads?",
            PlatformDialog.Type.OKCancel,
            () => {
                Debug.Log("Yes");
                AdsAnaliticsManager.instance.SetGDRP(true);
            },
            () => {
                Debug.Log("No");
                AdsAnaliticsManager.instance.SetGDRP(false);
            }
        );*/
	}

	public void OnInterstitialEnd(bool result)
	{
		if (result)
		{
			adsTime = Time.time;
		}
	}

	public void OnLevelStart()
	{
		
	}
}
