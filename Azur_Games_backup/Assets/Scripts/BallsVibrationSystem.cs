using MoreMountains.NiceVibrations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallsVibrationSystem : MonoBehaviour, ICaptureBallObserver
{
    public PlayerState playerState;

    float lastVibroTime;

    private void OnEnable()
    {
        playerState.captureBallObservers.Add(this);
    }

    private void OnDisable()
    {
        playerState.captureBallObservers.Remove(this);
    }

    public void OnCaptureBall(Ball ball)
    {
        if(playerState.vibrate && RemoteSettings.GetBool("Vibro", false) && Time.time > lastVibroTime + RemoteSettings.GetFloat("VibroBallsDelay", 0.1f))
        {
            lastVibroTime = Time.time;
            MMVibrationManager.Haptic(HapticTypes.LightImpact);
        }
    }
}
