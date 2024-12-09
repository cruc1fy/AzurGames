using DG.Tweening;
//using Facebook.Unity;
//using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using AudienceNetwork;
//using Facebook.Unity;
//using UnityEngine.Monetization;


public interface IConsentObserver
{
    void OnConsentDialog();
}

public class BaseAd
{
    public Action<bool> rewardedCallback;
    public Action<bool> interstitialCallback;

    public virtual void Init()
    {

    }

    public virtual bool IsLoadedInterstitial()
    {
        return false;
    }

    public virtual bool IsLoadedVideo()
    {
        return false;
    }

    public virtual void Showinterstitial()
    {

    }

    public virtual void ShowVideo()
    {

    }

    public virtual void SetGDRP(bool value)
    {

    }

    public virtual void LoadInterstitial()
    {

    }

    public virtual void LoadRewarded()
    {

    }
}

public class ApplovinAd : BaseAd, IPurchaseObserver
{
    public override void Init()
    {
        IAP_Manager.purchaseObservers.Add(this);

        DOTween.Sequence().AppendInterval(RemoteSettings.GetFloat("AdsInitDelay", 60f)).AppendCallback(() =>
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                if (sdkConfiguration.ConsentDialogState == MaxSdkBase.ConsentDialogState.Applies)
                {
                    // Show user consent dialog
                    if (PlayerPrefs.GetInt("consent", -1) == -1)
                    {
                        foreach (var observer in AdsAnaliticsManager.consentObservers)
                        {
                            observer.OnConsentDialog();
                        }
                    }
                    else
                    {
                        SetGDRP(PlayerPrefs.GetInt("consent", -1) == 1);
                    }
                }
                else if (sdkConfiguration.ConsentDialogState == MaxSdkBase.ConsentDialogState.DoesNotApply)
                {
                    // No need to show consent dialog, proceed with initialization
                }
                else
                {
                    // Consent dialog state is unknown. Proceed with initialization, but check if the consent
                    // dialog should be shown on the next application initialization
                }
                // AppLovin SDK is initialized, start loading ads
                InitializeInterstitialAds();

                InitializeRewardedAds();
            };

            MaxSdk.SetSdkKey("9m-B8oO0dPizI7yVtZtwO4PcGF6Ap-dzlc8tPXu1ya-EhxkZUaUueuUw8b5xaivJJ1AkZ2lVdkkKeRu81M1gEV");
            MaxSdk.InitializeSdk();

        });
    }

    public override bool IsLoadedInterstitial()
    {
        return MaxSdk.IsInterstitialReady(interstitialAdUnitId);
    }

    public override bool IsLoadedVideo()
    {
        return MaxSdk.IsRewardedAdReady(rewardedAdUnitId);
    }

    public override void Showinterstitial()
    {
        if(PlayerPrefs.GetInt(IAP_Manager.kProductIDConsumable) == 1)
        {
            MaxSdk.HideBanner(bannerAdUnitId);
            return;
        }
        if (MaxSdk.IsInterstitialReady(interstitialAdUnitId))
        {
            MaxSdk.ShowInterstitial(interstitialAdUnitId);
        }
    }

    public override void ShowVideo()
    {
        if (MaxSdk.IsRewardedAdReady(rewardedAdUnitId))
        {
            MaxSdk.ShowRewardedAd(rewardedAdUnitId);
        }
    }

#if UNITY_IPHONE
    string interstitialAdUnitId = "2688aeaf3e5e2108";
#else
    string interstitialAdUnitId = "bb107fd1f5f5a152";
#endif

    public void InitializeInterstitialAds()
    {
        // Attach callback
        MaxSdkCallbacks.OnInterstitialLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.OnInterstitialLoadFailedEvent += OnInterstitialFailedEvent;
        MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent += InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.OnInterstitialHiddenEvent += OnInterstitialDismissedEvent;

        // Load the first interstitial
        LoadInterstitial();

        InitializeBannerAds();
    }

    public override void LoadInterstitial()
    {
        MaxSdk.LoadInterstitial(interstitialAdUnitId);
    }

    private void OnInterstitialLoadedEvent(string adUnitId)
    {
        // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
    }

    private void OnInterstitialFailedEvent(string adUnitId, int errorCode)
    {
        // Interstitial ad failed to load. We recommend re-trying in 3 seconds.
        DOTween.Sequence().AppendInterval(3f).AppendCallback(LoadInterstitial);
    }

    private void InterstitialFailedToDisplayEvent(string adUnitId, int errorCode)
    {
        // Interstitial ad failed to display. We recommend loading the next ad
        foreach (var observer in AdsAnaliticsManager.interstitialObservers)
        {
            observer.OnInterstitialEnd(false);
        }
        AdsAnaliticsManager.instance.ResumeGame();
        interstitialCallback?.Invoke(false);
        LoadInterstitial();
    }

    private void OnInterstitialDismissedEvent(string adUnitId)
    {
        foreach (var observer in AdsAnaliticsManager.interstitialObservers)
        {
            observer.OnInterstitialEnd(true);
        }
        AdsAnaliticsManager.instance.ResumeGame();
        interstitialCallback?.Invoke(true);
        // Interstitial ad is hidden. Pre-load the next ad
        LoadInterstitial();
    }
#if UNITY_IPHONE
    string bannerAdUnitId = "3737c839799d773b"; // Retrieve the id from your account
#else
    string bannerAdUnitId = "8ae2fd12e8f47da3"; // Retrieve the id from your account
#endif


    public void InitializeBannerAds()
    {
        if (PlayerPrefs.GetInt(IAP_Manager.kProductIDConsumable) == 1) return;

        if (RemoteSettings.GetBool("NoBanner", false)) return;

        MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);

        MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, Color.white);
        MaxSdk.ShowBanner(bannerAdUnitId);
    }

    public void OnPurchase(string id)
    {
        MaxSdk.HideBanner(bannerAdUnitId);
    }

#if UNITY_IPHONE
    string rewardedAdUnitId = "833efc18edf23968";
#else
    string rewardedAdUnitId = "ffe0415bc14be5da";
#endif

    public void InitializeRewardedAds()
    {
        // Attach callback
        MaxSdkCallbacks.OnRewardedAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.OnRewardedAdHiddenEvent += OnRewardedAdDismissedEvent;
        MaxSdkCallbacks.OnRewardedAdLoadFailedEvent += OnRewardedAdFailedEvent;
        MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent += RewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.OnRewardedAdReceivedRewardEvent  += RewardedAdReceivedRewardEvent;


        // Load the first RewardedAd
        LoadRewarded();
    }

    private void RewardedAdReceivedRewardEvent(string arg1, MaxSdkBase.Reward arg2)
    {
        AdsAnaliticsManager.instance.ResumeGame();
        rewardedCallback(true);
    }

    public override void LoadRewarded()
    {
        foreach (var observer in AdsAnaliticsManager.rewardedObservers)
        {
            observer.OnRewardedNotReady();
        }

        MaxSdk.LoadRewardedAd(rewardedAdUnitId);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId)
    {
        // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
        foreach(var observer in AdsAnaliticsManager.rewardedObservers)
        {
            observer.OnRewardedReady();
        }
    }

    private void OnRewardedAdDismissedEvent(string adUnitId)
    {
        // Rewarded ad is hidden. Pre-load the next ad
        LoadRewarded();
    }

    private void OnRewardedAdFailedEvent(string adUnitId, int errorCode)
    {
        // Rewarded ad failed to load. We recommend re-trying in 3 seconds.
        DOTween.Sequence().AppendInterval(3f).AppendCallback(LoadRewarded);
    }

    private void RewardedAdFailedToDisplayEvent(string adUnitId, int errorCode)
    {
        // Rewarded ad failed to display. We recommend loading the next ad
        LoadRewarded();
        AdsAnaliticsManager.instance.ResumeGame();
        rewardedCallback(false);
    }

    public override void SetGDRP(bool value)
    {
        MaxSdk.SetHasUserConsent(value);
    }
}


public interface IRewardedObserver
{
    void OnRewardedReady();
    void OnRewardedNotReady();    
}

public interface IInterstitialObserver
{
    void OnInterstitialEnd(bool result);
}

public interface IAnalytics
{
    void Init();
    void SetGDRP(bool value);
    void ShowInterstitial();
    void ShowRewarded();
    void Purchase(string id, string currency, float revenue);

    void LevelStart(int level);
    void LevelFinish(int level, string result, float time);
}

public class YandexAnalytics : IAnalytics
{
    public void Init()
    {
        
    }

    public void SetGDRP(bool value)
    {
        AppMetrica.Instance.SetStatisticsSending(value);
    }

    public void ShowInterstitial()
    {
        AppMetrica.Instance.ReportEvent("video_ads_watch", new Dictionary<string, object>() {
            {"ad_type", "interstitial"},
            {"placement", "1"},
            {"network", "applovin"},
        });
    }

    public void ShowRewarded()
    {
        AppMetrica.Instance.ReportEvent("video_ads_watch", new Dictionary<string, object>() {
            {"ad_type", "rewarded"},
            {"placement", "1"},
            {"network", "applovin"},
        });
    }

    public void Purchase(string id, string currency, float revenue)
    {
        AppMetrica.Instance.ReportEvent("payment_succeed", new Dictionary<string, object>() {
            {"inapp_id", id},
            {"currency", currency},
            {"price", "" + revenue},
        });
    }

    public void Level(int num)
    {
        AppMetrica.Instance.ReportEvent("level_finish", new Dictionary<string, object>() {
            {"level", ""+num},
            {"level_text", "lv"+num},
        });
    }

    public void LevelStart(int level)
    {
        AppMetrica.Instance.ReportEvent("level_start", new Dictionary<string, object>() {
            {"level", ""+level},
        });
    }

    public void LevelFinish(int level, string result, float time)
    {
        AppMetrica.Instance.ReportEvent("level_finish", new Dictionary<string, object>() {
            {"level", ""+level},
            {"result", ""+result},
            {"time", ""+(int)time},
        });
    }
}

public class AdsAnaliticsManager : MonoBehaviour
{
    public static AdsAnaliticsManager instance;

    public static List<IRewardedObserver> rewardedObservers = new List<IRewardedObserver>();
    public static List<IInterstitialObserver> interstitialObservers = new List<IInterstitialObserver>();
    public static List<IConsentObserver> consentObservers = new List<IConsentObserver>();
    public static List<IAnalytics> analytics = new List<IAnalytics>();

    BaseAd[] ads;

    static int interstitialCounter = 0;
                                                                                                           
    void Awake()
    {
        
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            DOTween.Sequence().AppendInterval(1f).AppendCallback(() =>
            {
                analytics.Add(new YandexAnalytics());

#if UNITY_ANDROID
                ///UnityEngine.Advertisements.Advertisement.Initialize("3044022");
#endif
                AppsFlyer.setAppsFlyerKey("r9vNC83N8nYpCzYGigyjUh");

                //AppsFlyer.setIsDebug (true);
#if UNITY_IOS
          
          AppsFlyer.setAppID ("1462267627");
          AppsFlyer.trackAppLaunch ();
#elif UNITY_ANDROID
                AppsFlyer.setAppID(Application.identifier);
                AppsFlyer.init("r9vNC83N8nYpCzYGigyjUh", "AppsFlyerTrackerCallbacks");
#endif

        
                ads = new BaseAd[] {
                //new GoogleAd(),
                //new FacebookAd(),
                //new MoPubAd(),
                //new UnityAd(),
                new ApplovinAd(),
            };

                foreach (var ad in ads)
                {
                    ad.Init();
                }

                /*if (FB.IsInitialized)
                {
                    FB.ActivateApp();


                }
                else
                {
                    //Handle FB.Init
                    FB.Init(() => {
                        FB.ActivateApp();
                    });
                }*/

            });
        }
    }

    public void TrackLevelFinish(int level, string result, float time)
    {
        try
        {
            foreach (var an in analytics)
            {
                an.LevelFinish(level, result, time);
            }
        }
        finally
        {
            //Debug.Log("Crash in TrackLevel");
        }
    }

    public void TrackLevelStart(int level)
    {
        try
        {
            foreach (var an in analytics)
            {
                an.LevelStart(level);
            }
        }
        catch (Exception)
        {
            //Debug.Log("Crash in TrackLevel");
        }
    }

    public void TrackPurchase(string id, string type, float revenue, string currency)
    {
        try
        {
            foreach (var an in analytics)
            {
                an.Purchase(id, currency, revenue);
            }

            AppsFlyer.trackRichEvent(AFInAppEvents.PURCHASE, new Dictionary<string, string>(){
                {AFInAppEvents.CONTENT_ID, id},
                {AFInAppEvents.CONTENT_TYPE, type},
                {AFInAppEvents.REVENUE, "" + revenue},
                {AFInAppEvents.CURRENCY, currency},
            });

            //FB.LogPurchase(revenue, currency);
        }
        finally
        {
            //Debug.Log("Crash in TrackPurchase");
        }
        
    }


    public void PauseGame()
    {
        Time.timeScale = 0.01f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public void ShowInterstitial(Action<bool> callback)
    {
        interstitialCounter++;
        //if (interstitialCounter < 2 || interstitialCounter % 2 == 0) return;
        if (ads == null) return;
        List<BaseAd> loadedAds = new List<BaseAd>();
        foreach(var ad in ads)
        {
            if(ad.IsLoadedInterstitial())
            {
                ad.interstitialCallback = callback;
                loadedAds.Add(ad);
            }
        }
        
        if(loadedAds.Count > 0)
        {
            //PauseGame();
            loadedAds[UnityEngine.Random.Range(0, loadedAds.Count)].Showinterstitial();
        }

        foreach(var an in analytics)
        {
            an.ShowInterstitial();
        }
    }

    

    
    public void ShowRewarded(Action<bool> callback)
    {
        List<BaseAd> loadedAds = new List<BaseAd>();
        foreach (var ad in ads)
        {
            ad.rewardedCallback = callback;
            if (ad.IsLoadedVideo())
            {
                loadedAds.Add(ad);
            }
        }

        if (loadedAds.Count > 0)
        {
            PauseGame();
            loadedAds[UnityEngine.Random.Range(0, loadedAds.Count)].ShowVideo();
        }

        foreach (var an in analytics)
        {
            an.ShowRewarded();
        }
    }

    public void SetGDRP(bool value)
    {
        foreach (var ad in ads)
        {
            ad.SetGDRP(value);
        }

        foreach (var an in analytics)
        {
            an.SetGDRP(value);
        }
    }

    public bool CanShowRewarded()
    {

        if (ads == null) return false;
        foreach(var ad in ads)
        {
            if (ad.IsLoadedVideo()) return true;
        }

        return false;
    }

    public bool CanShowInterstitial()
    {

        if (ads == null) return false;
        foreach (var ad in ads)
        {
            if (ad.IsLoadedInterstitial()) return true;
        }

        return false;
    }

    public void LoadAds()
    {
        if (ads == null) return;

        if (!CanShowInterstitial())
        {
            foreach (var ad in ads)
            {
                ad.LoadInterstitial();
            }
        }
        if (!CanShowRewarded())
        {
            foreach (var ad in ads)
            {
                ad.LoadRewarded();
            }
        }
    }


}
