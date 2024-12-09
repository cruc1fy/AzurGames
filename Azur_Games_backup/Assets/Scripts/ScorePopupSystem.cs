using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScorePopupSystem : MonoBehaviour, ICaptureBallObserver, ICaptureFruitObserver
{
    public PlayerState playerState;

    public ScorePopup scorePrefab;   
    public ScorePopup perfectPrefab;   

    List<ScorePopup> scores = new List<ScorePopup>();

    float scoreLifeTime;
    float firstBallTime;
    float firstFruitTime;
    static public bool isFirstBall = true;
    static public bool isFirstFruit = true;

    private void OnEnable()
    {
        playerState.captureBallObservers.Add(this);
        playerState.captureFruitObservers.Add(this);

        scoreLifeTime = RemoteSettings.GetFloat("ScoreLifeTime", 1.6f);
    }

    private void OnDisable()
    {
        playerState.captureBallObservers.Remove(this);
        playerState.captureFruitObservers.Remove(this);
    }

    public void OnCaptureBall(Ball ball)
    {
        if(isFirstBall)
        {
            firstBallTime = Time.time;
            firstFruitTime = Time.time;
            isFirstBall = false;
        }

        float capturedTime = Time.time - firstFruitTime;

        bool isPerfect = playerState.capturedBalls == playerState.totalBallsPerfect - 1 && capturedTime < (playerState.totalBalls / 100f + 1f)*RemoteSettings.GetFloat("PerfectTimeMult", 1f);

        if (!RemoteSettings.GetBool("emit_digits", false) && !isPerfect) return;

        var score = Instantiate(isPerfect ? perfectPrefab : scorePrefab, transform);
        
        score.transform.position = Camera.main.WorldToScreenPoint(ball.transform.position) + Vector3.right * Random.Range(-100f, 100f);

        if (!isPerfect)
        {
            score.tmText.text = (playerState.level + 1).ToString();
            score.speed = Random.Range(RemoteSettings.GetFloat("ScoreJumpSpeedMin", 350f), RemoteSettings.GetFloat("ScoreJumpSpeedMax", 450f));

            scores.Add(score);
        }
        else
        {
            float duration = Random.Range(RemoteSettings.GetFloat("ScoreJumpTimeMin", 0.6f), RemoteSettings.GetFloat("ScoreJumpTimeMax", 0.9f));
            float durationScale = RemoteSettings.GetFloat("ScoreScaleTime", 0.2f);

            var v = score.transform.localPosition;
            v.x = 0f;
            score.transform.localPosition = v;


			DOTween.Sequence()
                .Append(score.transform.DOLocalMove(score.transform.localPosition+ Vector3.up * Random.Range(RemoteSettings.GetFloat("ScoreJumpMin", 550f), RemoteSettings.GetFloat("ScoreJumpMax", 650f)), duration))
                .Append(score.transform.DOScale(1.2f, durationScale))
                .Append(score.transform.DOScale(1f, durationScale))
                .Append(score.transform.DOScale(1.2f, durationScale))
                .Append(score.transform.DOScale(1f, durationScale))
                .Append(score.transform.DOScale(1.2f, durationScale))
                .Append(score.transform.DOScale(1f, durationScale))
                .AppendInterval(RemoteSettings.GetFloat("ScoreDestroyTime", 0.1f))
                .AppendCallback(()=>{ Destroy(score.gameObject); })
            ;
        }
    }

	public void OnCaptureFruit(Fruit fruit)
	{
		if (isFirstFruit)
		{
			firstBallTime = Time.time;
			isFirstFruit = false;
		}

		float capturedTime = Time.time - firstBallTime;/////////////////     ////////////////////////////

		bool isPerfect = playerState.capturedFruits == playerState.totalFruitsPerfect - 1 && capturedTime < (playerState.totalFruits / 100f + 1f) * RemoteSettings.GetFloat("PerfectTimeMult", 1f);

		if (!RemoteSettings.GetBool("emit_digits", false) && !isPerfect) return;

        var score = Instantiate(isPerfect ? perfectPrefab : scorePrefab, transform);

		score.transform.position = Camera.main.WorldToScreenPoint(fruit.transform.position) + Vector3.right * Random.Range(-100f, 100f);


		if (!isPerfect)
		{
			score.tmText.text = (playerState.level + 1).ToString();
			score.speed = Random.Range(RemoteSettings.GetFloat("ScoreJumpSpeedMin", 350f), RemoteSettings.GetFloat("ScoreJumpSpeedMax", 450f));

			scores.Add(score);
		}
		else
		{
			float duration = Random.Range(RemoteSettings.GetFloat("ScoreJumpTimeMin", 0.6f), RemoteSettings.GetFloat("ScoreJumpTimeMax", 0.9f));
			float durationScale = RemoteSettings.GetFloat("ScoreScaleTime", 0.2f);

			var v = score.transform.localPosition;
			v.x = 0f;
			score.transform.localPosition = v;

			DOTween.Sequence()
				.Append(score.transform.DOLocalMove(score.transform.localPosition + Vector3.up * Random.Range(RemoteSettings.GetFloat("ScoreJumpMin", 550f), RemoteSettings.GetFloat("ScoreJumpMax", 650f)), duration))
				.Append(score.transform.DOScale(1.2f, durationScale))
				.Append(score.transform.DOScale(1f, durationScale))
				.Append(score.transform.DOScale(1.2f, durationScale))
				.Append(score.transform.DOScale(1f, durationScale))
				.Append(score.transform.DOScale(1.2f, durationScale))
				.Append(score.transform.DOScale(1f, durationScale))
				.AppendInterval(RemoteSettings.GetFloat("ScoreDestroyTime", 0.1f))
				.AppendCallback(() => { Destroy(score.gameObject); })
			;
		}
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var i = scores.Count;
        while(i > 0)
        {
            i--;
            var score = scores[i];

            score.lifeTime += Time.deltaTime;
            score.transform.localPosition += Vector3.up * score.speed * Time.deltaTime;
            if(score.lifeTime > scoreLifeTime)
            {
                scores.Remove(score);
                Destroy(score.gameObject);
            }
            else if(score.lifeTime > scoreLifeTime - 0.3f)
            {
                var color = score.tmText.color;

                color.a = (scoreLifeTime - score.lifeTime) / 0.3f;

                score.tmText.color = color;
            }
        }
    }
}
