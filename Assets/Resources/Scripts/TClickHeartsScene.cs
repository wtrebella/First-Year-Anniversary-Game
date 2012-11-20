using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TClickHeartsScene : FStage, FSingleTouchableInterface {
	int score = 0;
	int previousScore = 0;
	int currentLabelScore = 0;
	int numHeartsMissed = 0;
	
	float timeSinceLastHeartAdd = 0;
	
	bool gameReadyToStart = false;
	bool gameHasStarted = false;
	bool gameIsOver = false;
	bool gameFullyOver = false;
	bool initiatedSceneSwitch = false;
	bool heartShowerHasStarted = false;
	
	List<FSprite> hearts;
	
	List<FSprite> heartShowerHearts;
	
	FLabel scoreLabel;
	FLabel missedLabel;
	FLabel startLabel1;
	FLabel startLabel2;
		
	public TClickHeartsScene() : base("") {	
		Go.validateTargetObjectsEachTick = false;
		
		FSoundManager.PlaySound("harpDescend");
		
		MakeBackground();
		MakeUIElements();
		
		hearts = new List<FSprite>();
		heartShowerHearts = new List<FSprite>();
		
		for (int i = 0; i < 200; i++) {
			FSprite heart = new FSprite("heart.psd");
			heart.color = new Color(0.75f, Random.Range(0, 0.3f), Random.Range(0, 0.3f), 1.0f);
			heart.isVisible = false;
			heart.alpha = Random.Range(0.6f, 1.0f);
			heart.rotation = Random.Range(0, 359);
			heart.scale = Random.Range(0.1f, 1.0f);
			heartShowerHearts.Add(heart);
			AddChild(heart);
		}
		
		TBorderLayer borderLayer = new TBorderLayer(Futile.screen.width, Futile.screen.height, 5f, new Color(0.2f, 0.2f, 0.2f, 1.0f));
		AddChild(borderLayer);
	}
	
	void MakeBackground() {
		float thickness = 25f;
		float distanceBetween = 10f;
		float borderWidth = Futile.screen.width;
		float borderHeight = Futile.screen.height;
		
		TweenFlow flow = new TweenFlow();
		TweenConfig config = new TweenConfig()
			.floatProp("alpha", 0.15f);
		float delayBetweenTweenStarts = 0.2f;
		
		for (int i = 0; borderWidth > 0 && borderHeight > 0; i++) {
			TBorderLayer layer = new TBorderLayer(borderWidth, borderHeight, 25f, new Color(0.75f, 0.2f, 0.2f, 1.0f));
			layer.x = (distanceBetween + thickness) * i;
			layer.y = (distanceBetween + thickness) * i;
			layer.alpha = 0.0f;
			AddChild(layer);
			borderWidth = borderWidth - distanceBetween * 2f - thickness * 2f;
			borderHeight = borderHeight - distanceBetween * 2f - thickness * 2f;
			flow.insert(delayBetweenTweenStarts * i, new Tween(layer, 0.3f, config));
		}
		
		Go.addTween(flow);
		flow.play();
	}
	
	void MakeUIElements() {
		scoreLabel = new FLabel("SoftSugar", "0");
		scoreLabel.anchorX = 1.0f;
		scoreLabel.anchorY = 1.0f;
		scoreLabel.scale = 0.75f;
		scoreLabel.x = Futile.screen.width + scoreLabel.textRect.width;
		scoreLabel.y = Futile.screen.height - 10f;
		scoreLabel.color = new Color(0.12f, 0.12f, 0.12f, 1.0f);
		AddChild(scoreLabel);
		
		missedLabel = new FLabel("SoftSugar", "Misses left: 0");
		missedLabel.anchorX = 0.0f;
		missedLabel.anchorY = 1.0f;
		missedLabel.scale = 0.75f;
		missedLabel.x = -missedLabel.textRect.width;
		missedLabel.y = Futile.screen.height - 10f;
		missedLabel.color = new Color(0.75f, 0.12f, 0.12f, 1.0f);
		AddChild(missedLabel);
		
		startLabel1 = new FLabel("SoftSugar", "How much do I love you?");
		startLabel2 = new FLabel("SoftSugar", "Click the hearts to find out,\nbut don't miss too many!");
		startLabel1.color = new Color(0.75f, 0.12f, 0.12f, 1.0f);
		startLabel2.color = new Color(0.12f, 0.12f, 0.12f, 1.0f);
		float adj = 40f;
		startLabel1.x = startLabel2.x = Futile.screen.halfWidth;
		startLabel1.y = Futile.screen.halfHeight + adj;
		startLabel2.y = startLabel1.y - startLabel1.textRect.height / 2f - startLabel2.textRect.height / 2f - 10f + adj;
		startLabel1.scale = startLabel2.scale = 0;
		AddChild(startLabel1);
		AddChild(startLabel2);
		
		TweenConfig config1 = new TweenConfig()
			.floatProp("scale", 1.0f)
			.setEaseType(EaseType.SineInOut);
		
		TweenConfig config2 = new TweenConfig()
			.floatProp("scale", 0.5f)
			.setEaseType(EaseType.SineInOut);
		
		float duration = 0.5f;
		
		TweenChain chain = new TweenChain();
		chain.append(new Tween(startLabel1, duration, config1));
		chain.append(new Tween(startLabel2, duration, config2));
		chain.setOnCompleteHandler(OnGameShouldBeReadyToStart);
		
		Go.addTween(chain);
		chain.play();
	}
	
	override public void HandleAddedToStage() {
		base.HandleAddedToStage();
		Futile.touchManager.AddSingleTouchTarget(this);
		Futile.instance.SignalUpdate += HandleUpdate;
	}
	
	override public void HandleRemovedFromStage() {
		base.HandleRemovedFromStage();
		Futile.touchManager.RemoveSingleTouchTarget(this);
		Futile.instance.SignalUpdate -= HandleUpdate;
	}
	
	void StartHeartShower() {
		heartShowerHasStarted = true;
		foreach (FSprite heart in heartShowerHearts) {
			heart.x = Random.Range(0, Futile.screen.width);
			heart.y = Futile.screen.height + heart.height + Random.Range(0, 500);
			heart.isVisible = true;
			Go.to(heart, Random.Range(1.0f, 4.0f), new TweenConfig().setIterations(-1).floatProp("rotation", 360 * (RXRandom.Float() < 0.5 ? 1 : -1), true));
		}
	}
	
	public void FlyInUIElements() {
		Go.to(scoreLabel, 0.3f, new TweenConfig()
			.floatProp("x", Futile.screen.width - 10f)
			.setEaseType(EaseType.SineInOut));
		
		Go.to(missedLabel, 0.3f, new TweenConfig()
			.floatProp("x", 10f)
			.setEaseType(EaseType.SineInOut));
	}
	
	public void FlyOutUIElements() {
		Go.to(scoreLabel, 0.3f, new TweenConfig()
			.floatProp("x", Futile.screen.width + scoreLabel.textRect.width)
			.setEaseType(EaseType.SineInOut));
		
		Go.to(missedLabel, 0.3f, new TweenConfig()
			.floatProp("x", -missedLabel.textRect.width)
			.setEaseType(EaseType.SineInOut));
	}
	
	public void StartGame() {
		Go.to(startLabel1, 0.5f, new TweenConfig()
			.floatProp("scale", 0.0f)
			.setEaseType(EaseType.SineInOut));
		Go.to(startLabel2, 0.5f, new TweenConfig()
			.floatProp("scale", 0.0f)
			.setEaseType(EaseType.SineInOut));
		FlyInUIElements();
		scoreLabel.isVisible = true;
		missedLabel.isVisible = true;
		gameHasStarted = true;
	}
	
	public void HandleUpdate() {		
		if (heartShowerHasStarted) {
			for (int i = 0; i < heartShowerHearts.Count; i++) {
				FSprite heart = heartShowerHearts[i];
				float newY = heart.y - (float)(i + 50) / 100.0f  * Time.deltaTime * 200.0f;
				if (newY < -heart.height / 2f) newY += Futile.screen.width + heart.height / 2f;
				heart.y = newY;
			}
		}
		
		if (!gameHasStarted || gameIsOver) return;
				
		timeSinceLastHeartAdd += Time.deltaTime;
		
		if (currentLabelScore < previousScore) currentLabelScore = previousScore;
		int difference = score - previousScore;
		int amountPerFrame = difference / 11;
		currentLabelScore += amountPerFrame;
		if (currentLabelScore > score) currentLabelScore = score;
		
		scoreLabel.text = string.Format("{0:#,###0}", currentLabelScore);
		missedLabel.text = string.Format("Misses left: {0}", 5 - numHeartsMissed);
		
		if (timeSinceLastHeartAdd >= Random.Range(0.3f, 1.0f) && hearts.Count < 3) {
			timeSinceLastHeartAdd = 0;
			
			AddHeart();
		}
		
		if (numHeartsMissed >= 5 || score >= 1000000) {
			EndGame();
		}
	}
	
	public void EndGame() {
		FlyOutUIElements();
			
		scoreLabel.text = string.Format("{0:#,###0}", score);
		
		for (int i = hearts.Count - 1; i >= 0; i--) {
			FSprite heart = hearts[i];
			foreach (AbstractTween tween in Go.tweensWithTarget(heart)) Go.removeTween(tween);
			heart.RemoveFromContainer();
			hearts.Remove(heart);
		}
		
		if (score >= 1000000) {
			StartHeartShower();
			FSoundManager.PlaySound("happyPiano");
			gameIsOver = true;
			FLabel label = new FLabel("SoftSugar", "I love you times a million!");
			label.color = new Color(0.12f, 0.12f, 0.12f, 1.0f);
			label.x = Futile.screen.halfWidth;
			label.y = Futile.screen.halfHeight;
			label.scale = 0;
			AddChild(label);
			
			TweenConfig config = new TweenConfig()
				.floatProp("scale", 1.0f)
				.setEaseType(EaseType.SineInOut)
				.onComplete(OnWinLabelDoneAppearing);
			
			Go.to(label, 0.5f, config);
		}
		
		if (numHeartsMissed >= 5) {
			gameIsOver = true;
			FSoundManager.PlaySound("sadPiano");
			FLabel topLabel = new FLabel("SoftSugar", "Are you kidding me?!");
			FLabel bottomLabel = new FLabel("SoftSugar", string.Format("I love you way more than x{0:#,###0}!", score));
			topLabel.color = new Color(0.75f, 0.12f, 0.12f, 1.0f);
			bottomLabel.color = new Color(0.12f, 0.12f, 0.12f, 1.0f);
			bottomLabel.x = topLabel.x = Futile.screen.halfWidth;
			float bottomBeginning = 300f;
			float segmentHeight = (Futile.screen.height - bottomBeginning * 2f) / 3f;
			bottomLabel.y = segmentHeight - bottomLabel.textRect.height / 2f + bottomBeginning;
			topLabel.y = segmentHeight * 3f - topLabel.textRect.height / 2f + bottomBeginning;
			bottomLabel.scale = topLabel.scale = 0;
			AddChild(topLabel);
			AddChild(bottomLabel);
			
			TweenConfig config1 = new TweenConfig()
				.floatProp("scale", 1.0f)
				.setEaseType(EaseType.SineInOut);
			TweenConfig config2 = new TweenConfig()
				.floatProp("scale", 0.75f)
				.setEaseType(EaseType.SineInOut);
			
			float duration = 0.5f;
			
			TweenChain chain = new TweenChain();
			chain.append(new Tween(topLabel, duration, config1));
			chain.append(new Tween(bottomLabel, duration, config2));
			chain.setOnCompleteHandler(OnGameShouldBeFullyOver);
			
			Go.addTween(chain);
			chain.play();
		}
	}
	
	public void OnWinLabelDoneAppearing(AbstractTween tween) {
		gameFullyOver = true;
		
		FLabel label = (tween as Tween).target as FLabel;
		
		Tween up = new Tween(label, 0.3f, new TweenConfig()
			.floatProp("scale", 1.1f)
			.setEaseType(EaseType.SineInOut));
			
		Tween down = new Tween(label, 0.3f, new TweenConfig()
			.floatProp("scale", 1.0f)
			.setEaseType(EaseType.SineInOut));
			
		TweenChain upDownChain = new TweenChain();
		upDownChain.setIterations(-1);
		upDownChain.append(up);
		upDownChain.append(down);
			
		Go.addTween(upDownChain);
		upDownChain.play();
	}
	
	public void OnGameShouldBeReadyToStart(AbstractTween tween) {
		gameReadyToStart = true;	
	}
	
	public void OnGameShouldBeFullyOver(AbstractTween tween) {
		gameFullyOver = true;
	}
	
	public void AddHeart() {
		FSprite heart = new FSprite("heart.psd");
		heart.x = Random.Range(heart.width / 2f, Futile.screen.width - heart.width / 2f);
		heart.y = Random.Range(heart.height / 2f, Futile.screen.height - heart.height / 2f - 50f /*UI bar*/);
		heart.scale = 0;
		heart.rotation = Random.Range(0, 359f);
		heart.color = new Color(1.0f, Random.Range(0.0f, 0.3f), Random.Range(0.0f, 0.3f), 1.0f);
		AddChild(heart);
		hearts.Add(heart);
		
		Go.to(heart, Random.Range(1.0f, 5.0f), new TweenConfig()
			.setIterations(-1)
			.floatProp("rotation", 360 * (RXRandom.Float() < 0.5f ? 1 : -1), true));
		
		float inflationDuration = Random.Range(1.0f, 2.0f);
		Tween tweenUp = new Tween(heart, inflationDuration, new TweenConfig()
			.floatProp("scale", Random.Range(0.3f, 1.0f))
			.setEaseType(EaseType.SineInOut));
		
		Tween tweenDown = new Tween(heart, inflationDuration, new TweenConfig()
			.floatProp("scale", 0)
			.onComplete(OnHeartDisappearedWithoutBeingTouched)
			.setEaseType(EaseType.SineInOut));
		
		TweenChain chain = new TweenChain();
		chain.append(tweenUp);
		chain.append(tweenDown);
		
		Go.addTween(chain);
		chain.play();
	}
	
	public void OnHeartDisappearedWithoutBeingTouched(AbstractTween tween) {
		FSprite heart = (tween as Tween).target as FSprite;
		if (heart == null) return;
		KillHeart(heart);
		numHeartsMissed++;
		if (!gameIsOver) FSoundManager.PlaySound("shrink", 0.3f);
	}
	
	public void KillHeart(FSprite heart) {
		Go.killAllTweensWithTarget(heart);
		hearts.Remove(heart);
		heart.RemoveFromContainer();
	}
	
	public bool HandleSingleTouchBegan(FTouch touch) {
		if (!gameHasStarted && gameReadyToStart) {
			StartGame();
			return true;
		}
		if (gameIsOver && !gameFullyOver) return false;
		if (gameFullyOver && !initiatedSceneSwitch) {
			if (score >= 1000000) {
				TMain.SwitchToScene(TMain.SceneType.PeopleSceneGoalThree);
			}
			else {
				TMain.SwitchToScene(TMain.SceneType.ClickHeartsScene);
			}
			initiatedSceneSwitch = true;
		}
		
		for (int i = hearts.Count - 1; i >= 0; i--) {
			FSprite heart = hearts[i];
			
			if (heart.localRect.Contains(heart.GlobalToLocal(touch.position))) {
				previousScore = score;
				FSoundManager.PlaySound("rise");
				score += 40000;
				KillHeart(heart);
				break;
			}
		}
		
		return true;
	}
	
	public void HandleSingleTouchMoved(FTouch touch) {
		
	}
	
	public void HandleSingleTouchEnded(FTouch touch) {
		
	}
	
	public void HandleSingleTouchCanceled(FTouch touch) {

	}
}
