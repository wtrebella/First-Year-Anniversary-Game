using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TPeopleScene : FStage, FSingleTouchableInterface {	
	TWalkingCharacter dana;
	TWalkingCharacter whit;
	float universalVelocity = -800f;
	List<FSprite> groundBlocks = new List<FSprite>();
	List<FSprite> farMountainSprites = new List<FSprite>();
	List<FSprite> closeMountainSprites = new List<FSprite>();
	List<THeartToken> heartTokens = new List<THeartToken>();
	List<FSprite> heartShowerHearts;
	List<string> tutorialStrings;
	int tutorialStringIndex = 0;
	bool initiatedSceneSwitch = false;
	float heartTokenDistanceTimer = 0;
	float tutorialTimer = 0;
	float totalDistance = 0;
	FLabel start;
	FLabel goal;
	float spaceBarTimer = 0;
	FLabel tutorialLabel;
	float tutorialLabelTimer = 0;
	float goalDistance = 30000f;
	float endGameWaitTimer = 0;
	FSprite goalProgressBar;
	bool tutorialIsDone = false;
	bool tutorialLabelIsAnimating = false;
	bool foundEachother = false;
	bool tutorialLabelIsShowing = false;
	bool gameIsOver = false;
	int heartTokensSinceLastBrokenOne = 0;
	FSprite cloud;
	
	public TPeopleScene(bool withTutorial) : base("") {
		tutorialIsDone = !withTutorial;
		
		FSoundManager.PlayMusic("jazz");
		
		FSprite background = SquareMaker.Square(Futile.screen.width, Futile.screen.height);
		background.x = Futile.screen.halfWidth;
		background.y = Futile.screen.halfHeight;
		background.color = new Color(0.7f, 0.9f, 1.0f, 1.0f);
		AddChild(background);
		
		SetupParallax();
		InitHeartTokens();
		MakeUIElements();
		
		tutorialLabel = new FLabel("SoftSugar", "");
		tutorialLabel.y = Futile.screen.height - 125f;
		tutorialLabel.x = Futile.screen.halfWidth;
		AddChild(tutorialLabel);
		
		tutorialStrings = new List<string>();
		heartShowerHearts = new List<FSprite>();
		
		start = new FLabel("SoftSugar", "start");
		goal = new FLabel("SoftSugar", "goal");
		start.anchorX = 0;
		goal.anchorX = 1f;
		start.color = goal.color = Color.black;
		start.x = 5f;
		start.scale = goal.scale = 0.35f;
		goal.x = Futile.screen.width - 5f;
		start.y = goal.y = Futile.screen.height - 10f;
		AddChild(start);
		AddChild(goal);
		
		tutorialStrings.Add("Help Whit find his sweetheart!");
		tutorialStrings.Add("Press space to jump");
		tutorialStrings.Add("Hold space to jump higher");
		tutorialStrings.Add("Press down arrow to crouch");
		tutorialStrings.Add("The red bar above shows how\nclose Whit is to his goal");
		tutorialStrings.Add("Avoid the broken hearts!");
		
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
		
		for (int i = 0; i < 3; i++) {
			FSprite groundBlock = new FSprite("ground.psd");
			groundBlock.x = i * (groundBlock.width - 2) + groundBlock.width / 2f;
			groundBlock.y = groundBlock.height / 2f;
			groundBlocks.Add(groundBlock);
			AddChild(groundBlock);
		}
		
		whit = new TWalkingCharacter("whitHead.png");
		whit.x = 130f;
		whit.y = 250f;
		AddChild(whit);
		whit.StartWalking();		
	}
	
	public void DisplayTutorialString(string tutorialString) {
		if (tutorialLabelIsShowing || tutorialLabelIsAnimating) return;
		
		tutorialLabelIsShowing = true;
		tutorialLabelIsAnimating = true;
		tutorialLabel.text = tutorialString;
		tutorialLabel.scale = 0;
		Go.to(tutorialLabel, 0.3f, new TweenConfig()
			.floatProp("scale", 0.9f)
			.setEaseType(EaseType.SineInOut)
			.onComplete(OnTutorialLabelFinishedEnlarging));
	}
	
	public void DismissTutorialLabel() {
		if (!tutorialLabelIsShowing || tutorialLabelIsAnimating) return;
		
		tutorialLabelIsShowing = false;
		Go.to(tutorialLabel, 0.3f, new TweenConfig()
			.floatProp("scale", 0.0f)
			.setEaseType(EaseType.SineInOut)
			.onComplete(OnTutorialLabelFinishedShrinking));
	}
		
	public void OnTutorialLabelFinishedShrinking(AbstractTween tween) {
		tutorialLabelIsAnimating = false;
		tutorialLabelTimer = 0;
		tutorialStringIndex++;
	}
	
	public void OnTutorialLabelFinishedEnlarging(AbstractTween tween) {
		tutorialLabelIsAnimating = false;	
	}
	
	public void SetupParallax() {
		FSprite farMountain1 = new FSprite("mountains0.png");
		FSprite farMountain2 = new FSprite("mountains0.png");
		FSprite farMountain3 = new FSprite("mountains0.png");
		farMountain1.anchorY = farMountain2.anchorY = farMountain3.anchorY = 0;
		farMountain1.y = farMountain2.y = farMountain3.y = 50f;
		farMountain1.x = Futile.screen.halfWidth;
		farMountain2.x = farMountain1.x + farMountain2.width - 2;
		farMountain3.x = farMountain2.x + farMountain3.width - 2;
		farMountainSprites.Add(farMountain1);
		farMountainSprites.Add(farMountain2);
		farMountainSprites.Add(farMountain3);
		AddChild(farMountain1);
		AddChild(farMountain2);
		AddChild(farMountain3);
		
		FSprite closeMountain1 = new FSprite("mountains1.png");
		FSprite closeMountain2 = new FSprite("mountains1.png");
		FSprite closeMountain3 = new FSprite("mountains1.png");
		closeMountain1.anchorY = closeMountain2.anchorY = closeMountain3.anchorY = 0;
		closeMountain1.y = closeMountain2.y = closeMountain3.y = 50f;
		closeMountain1.x = Futile.screen.halfWidth;
		closeMountain2.x = closeMountain1.x + closeMountain2.width - 2;
		closeMountain3.x = closeMountain2.x + closeMountain3.width - 2;
		closeMountainSprites.Add(closeMountain1);
		closeMountainSprites.Add(closeMountain2);
		closeMountainSprites.Add(closeMountain3);
		AddChild(closeMountain1);
		AddChild(closeMountain2);
		AddChild(closeMountain3);
		
		FSprite backgroundFog = SquareMaker.Square(Futile.screen.width, Futile.screen.height);
		backgroundFog.color = Color.black;
		backgroundFog.alpha = 0.5f;
		backgroundFog.x = Futile.screen.halfWidth;
		backgroundFog.y = Futile.screen.halfHeight;
		AddChild(backgroundFog);
		
		cloud = new FSprite("cloud.psd");
		cloud.alpha = 0.6f;
		cloud.x = Futile.screen.halfWidth;
		cloud.y = Futile.screen.height - cloud.height / 2f - 20f;
		AddChild(cloud);
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
	
	public void InitHeartTokens() {
		for (int i = 0; i < 50; i++) {
			THeartToken token = new THeartToken(false);
			token.sprite.isVisible = false;
			AddChild(token.sprite);
			heartTokens.Add(token);
		}
		
		for (int i = 0; i < 10; i++) {
			THeartToken token = new THeartToken(true);
			token.sprite.isVisible = false;
			AddChild(token.sprite);
			heartTokens.Add(token);
		}
	}
	
	public void GameOver() {
		whit.StopWalking();
		whit.StopCrouching();
		whit.headSprite.element = Futile.atlasManager.GetElementWithName("whitSadHead.psd");
		whit.headSprite.y -= 7f;
		gameIsOver = true;
		if (dana != null) dana.StopWalking();
	}
	
	public THeartToken GetUnusedHeartToken(bool isBroken) {
		foreach (THeartToken token in heartTokens) {
			if (token.isBroken == isBroken) {
				if (!token.sprite.isVisible) return token;
			}
		}
		
		Debug.Log("No unused tokens!");
		return null;
	}
	
	public void RecycleHeartToken(THeartToken token) {
		token.sprite.isVisible = false;	
	}
	
	void StartHeartShower() {
		foreach (FSprite heart in heartShowerHearts) {
			heart.x = Random.Range(0, Futile.screen.width);
			heart.y = Futile.screen.height + heart.height + Random.Range(0, 500);
			heart.isVisible = true;
			Go.to(heart, Random.Range(1.0f, 4.0f), new TweenConfig().setIterations(-1).floatProp("rotation", 360 * (RXRandom.Float() < 0.5 ? 1 : -1), true));
		}
	}
	
	public void HandleUpdate() {		
		if (gameIsOver) {
			UpdatePostGameOver();
			return;
		}
		
		if (foundEachother) {
			UpdatePostFoundEachOther();
			return;
		}
				
		if (Input.GetKeyDown(KeyCode.Space) && !whit.isJumping) whit.Jump();
		if (Input.GetKey(KeyCode.Space)) whit.decelAmt = 50f;
		else whit.decelAmt = 250f;
		if (Input.GetKey(KeyCode.DownArrow)) whit.StartCrouching();
		if (Input.GetKeyUp(KeyCode.DownArrow)) whit.StopCrouching();
		UpdateParallax();
		
		if (!tutorialIsDone) {
			UpdateTutorial();
		}
		
		else {
			UpdateUIElements();
			UpdateHeartTokens();
			UpdateHeartTokenCollisions();
			UpdateDana();
		}
	}
	
	public void UpdatePostGameOver() {
		endGameWaitTimer += Time.fixedDeltaTime;
			
		if (endGameWaitTimer > 1.0f && Input.anyKeyDown && !initiatedSceneSwitch) {
			FSoundManager.StopMusic();
			initiatedSceneSwitch = true;
			TMain.SwitchToScene(TMain.SceneType.PeopleSceneWithoutTutorial);
		}
	}
	
	public void UpdatePostFoundEachOther() {
		if (!whit.isJumping && RXRandom.Float() > 0.5f) {
			whit.decelAmt = 150f;
			whit.Jump();	
		}
		
		if (!dana.isJumping && RXRandom.Float() > 0.5f) {
			dana.decelAmt = 150f;
			dana.Jump();
		}
		
		if (!whit.isTurning && RXRandom.Float() > 0.95f) {
			whit.TurnAround();	
		}
		
		if (!dana.isTurning && RXRandom.Float() > 0.95f) {
			dana.TurnAround();	
		}
		
		for (int i = 0; i < heartShowerHearts.Count; i++) {
			FSprite heart = heartShowerHearts[i];
			float newY = heart.y - (float)(i + 50) / 100.0f  * Time.deltaTime * 200.0f;
			if (newY < -heart.height / 2f) newY += Futile.screen.width + heart.height / 2f;
			heart.y = newY;
		}	
	}
	
	public void UpdateTutorial() {
		tutorialLabelTimer += Time.fixedDeltaTime;
		if (tutorialLabelIsAnimating || tutorialLabelIsShowing) tutorialLabelTimer = 0;
		if (tutorialLabelIsAnimating || !tutorialLabelIsShowing) tutorialTimer = 0;
		if (tutorialStringIndex == 0) {
			tutorialTimer += Time.fixedDeltaTime;
			
			if (tutorialTimer >= 3.0f) DismissTutorialLabel();
		}
		else if (tutorialStringIndex == 1 && tutorialLabelIsShowing) {
			if (Input.GetKeyDown(KeyCode.Space)) DismissTutorialLabel();
		}
		else if (tutorialStringIndex == 2 && tutorialLabelIsShowing) {
			if (Input.GetKey(KeyCode.Space)) spaceBarTimer += Time.fixedDeltaTime;
			else if (Input.GetKeyUp(KeyCode.Space)) spaceBarTimer = 0;
			
			if (spaceBarTimer >= 0.5f) DismissTutorialLabel();
		}
		else if (tutorialStringIndex == 3 && tutorialLabelIsShowing) {
			if (Input.GetKeyDown(KeyCode.DownArrow)) DismissTutorialLabel();
		}
		else if ((tutorialStringIndex == 4 || tutorialStringIndex == 5) && tutorialLabelIsShowing) {
			tutorialTimer += Time.fixedDeltaTime;
			if (tutorialTimer >= 3.0f) {
				DismissTutorialLabel();
				if (tutorialStringIndex >= 5) tutorialIsDone = true;
			}
		}
				
		if (tutorialLabelTimer >= 0.8f) {	
			DisplayTutorialString(tutorialStrings[tutorialStringIndex]);
		}
	}
	
	public void UpdateHeartTokenCollisions() {
		foreach (THeartToken token in heartTokens) {
			if (!token.sprite.isVisible) continue;
			if (whit.GetGlobalBoundsRect().CheckIntersect(token.GetGlobalBoundsRect())) {
				FSoundManager.PlayMusic("nooo");
				GameOver();
			}
		}
	}
	
	void MakeUIElements() {
		goalProgressBar = SquareMaker.Square(Futile.screen.width, 20f);
		goalProgressBar.anchorY = 1.0f;
		goalProgressBar.anchorX = 1.0f;
		goalProgressBar.color = new Color(1.0f, 0.4f, 0.4f, 1.0f);
		goalProgressBar.x = Futile.screen.width;
		goalProgressBar.y = Futile.screen.height;
		AddChild(goalProgressBar);
	}
	
	public void PrepareNewHeartTokenForEntry(bool isBroken) {
		THeartToken token;
		token = GetUnusedHeartToken(isBroken);
		
		if (token == null) return;
		
		if (!isBroken) heartTokensSinceLastBrokenOne++;
		else heartTokensSinceLastBrokenOne = 0;
		
		token.sprite.isVisible = true;
		float rand = RXRandom.Float();
		if (rand < 0.5f) token.sprite.y = 150f;
		else if (rand >= 0.5f && rand < 0.8f) token.sprite.y = 350f;
		else token.sprite.y = 550f;
		token.sprite.x = Futile.screen.width + token.sprite.width / 2f;
	}
	
	public void UpdateUIElements() {
		goalProgressBar.width = (1f - totalDistance / goalDistance) * Futile.screen.width;
	}
	
	public void UpdateHeartTokens() {
		heartTokenDistanceTimer += -universalVelocity * Time.fixedDeltaTime;
		totalDistance += -universalVelocity * Time.fixedDeltaTime;
		
		float distanceAmt = Random.Range(800f, 2000f);
		
		if (totalDistance < goalDistance - 1000f) {
			if (heartTokenDistanceTimer >= distanceAmt) {
				PrepareNewHeartTokenForEntry(true);
				
				heartTokenDistanceTimer -= distanceAmt;
			}
		}
		
		foreach (THeartToken token in heartTokens) {
			if (!token.sprite.isVisible) continue;
			token.sprite.x += universalVelocity * Time.fixedDeltaTime;
			if (token.sprite.x < -token.sprite.width / 2f) RecycleHeartToken(token);
		}
	}
	
	public void UpdateDana() {
		if (totalDistance < goalDistance - 1000f) return;
		
		if (dana == null) {
			dana = new TWalkingCharacter("danaHead.png");
			dana.x = Futile.screen.width + 100f;
			dana.y = 250f;
			AddChild(dana);
			dana.StartWalking();
		}
		
		dana.x += Time.fixedDeltaTime * universalVelocity * 0.25f;
		
		if (dana.x < 350f) {
			start.isVisible = goal.isVisible = false;
			FSoundManager.PlayMusic("yay");
			foundEachother = true;
			dana.TurnAround();
			dana.StopWalking();
			whit.StopWalking();
			whit.StopCrouching();
			StartHeartShower();
		}
	}
	
	public void UpdateParallax() {
		foreach (FSprite groundBlock in groundBlocks) {
			float newX = groundBlock.x + universalVelocity * Time.fixedDeltaTime;
			if (newX < -groundBlock.width / 2f) {
				newX += groundBlocks.Count * (groundBlock.width - 2);	
			}
			groundBlock.x = newX;
		}
		
		foreach (FSprite mountain in farMountainSprites) {
			float newX = mountain.x + (universalVelocity * 0.5f) * Time.fixedDeltaTime;
			if (newX < -mountain.width / 2f) {
				newX += farMountainSprites.Count * (mountain.width - 2);	
			}
			mountain.x = newX;
		}
		
		foreach (FSprite mountain in closeMountainSprites) {
			float newX = mountain.x + (universalVelocity * 0.75f) * Time.fixedDeltaTime;
			if (newX < -mountain.width / 2f) {
				newX += closeMountainSprites.Count * (mountain.width - 2);	
			}
			mountain.x = newX;
		}
		
		float cloudX = cloud.x + (universalVelocity * 0.3f) * Time.fixedDeltaTime;
		if (cloudX < -cloud.width / 2f) {
			cloudX += Futile.screen.width + cloud.width + Random.Range(30f, 200f);	
		}
		cloud.x = cloudX;
	}
	
	public bool HandleSingleTouchBegan(FTouch touch) {
		if (gameIsOver && !initiatedSceneSwitch) {
			FSoundManager.StopMusic();
			initiatedSceneSwitch = true;
			TMain.SwitchToScene(TMain.SceneType.PeopleSceneWithoutTutorial);
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
