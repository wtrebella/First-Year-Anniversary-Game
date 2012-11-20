using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GoalType {
	GoalOne,
	GoalTwo,
	GoalThree
}

public class TPeopleScene : FStage, FSingleTouchableInterface {	
	#region Variables
	// characters
	TWalkingCharacter dana;
	TWalkingCharacter whit;
	
	// goal stuff
	FSprite faceCoin;
	FSprite bigHeartCoin;
	
	// parallax stuff
	List<FSprite> groundBlocks = new List<FSprite>();
	List<FSprite> farMountainSprites = new List<FSprite>();
	List<FSprite> closeMountainSprites = new List<FSprite>();
	FSprite cloud;
	
	// UI stuff
	FSprite goalProgressBar;
	FLabel start;
	FLabel goal;
		
	// tutorial stuff
	List<string> tutorialStrings;
	FLabel tutorialLabel;
	int tutorialStringIndex = 0;
	float tutorialLabelHiddenTimer = 0;
	float tutorialLabelShowingTimer = 0;
	float spaceBarTimer = 0;
	bool tutorialLabelIsAnimating = false;
	bool tutorialLabelIsShowing = false;
	bool tutorialIsDone = true;
	
	// game stuff
	FContainer everythingContainer;
	GoalType goalType;
	List<THeartToken> heartTokens = new List<THeartToken>();
	List<FSprite> heartShowerHearts;
	float universalVelocity = -800f;
	float heartTokenDistanceTimer = 0;
	float totalDistance = 0;
	float goalDistance;
	bool foundEachother = false;
	
	// final note stuff
	List<string> finalNoteStrings;
	List<FLabel> finalNoteLabels;
	FLabel currentFinalNoteLabel;
	float finalNoteShowingTimer = 0;
	float finalNoteHiddenTimer = 0;
	bool finalNoteShowing = false;
	bool finalNoteAnimating = false;
	bool doneWithNote = false;
	int finalNoteStringIndex = 0;
	
	// endgame stuff
	bool initiatedSceneSwitch = false;
	float endGameWaitTimer = 0;
	bool gameIsOver = false;
	
	#endregion
	
	#region Constructor and Setup
	public TPeopleScene(GoalType goalType) : base("") {	
		FSprite background = SquareMaker.Square(Futile.screen.width, Futile.screen.height);
		background.color = Color.black;
		background.x = Futile.screen.halfWidth;
		background.y = Futile.screen.halfHeight;
		AddChild(background);
		
		everythingContainer = new FContainer();
		AddChild(everythingContainer);
		
		this.goalType = goalType;
		tutorialIsDone = TMain.goalOneTutorialIsDone;
		
		if (this.goalType == GoalType.GoalOne) goalDistance = 20000f;
		else if (this.goalType == GoalType.GoalTwo) goalDistance = 30000f;
		else if (this.goalType == GoalType.GoalThree) goalDistance = 50000f;
		
		FSoundManager.PlayMusic("jazz");
		
		SetupParallax();
		SetupHeartTokens();
		SetupUIElements();
		SetupTutorial();
		SetupHeartShower();		
		SetupFinalNote();
		
		if (this.goalType == GoalType.GoalTwo) {
			FLabel label = new FLabel("SoftSugar", "It's getting harder!");
			label.x = Futile.screen.halfWidth;
			label.y = Futile.screen.height - 100f;
			everythingContainer.AddChild(label);
			Tween tween = new Tween(label, 0.3f, new TweenConfig()
				.setDelay(3.0f)
				.floatProp("y", Futile.screen.height + 200f)
				.setEaseType(EaseType.SineInOut));
			Go.addTween(tween);
			tween.play();
		}
		else if (this.goalType == GoalType.GoalThree) {
			FLabel label = new FLabel("SoftSugar", "Will Whit ever\nfind a wife?");
			label.x = Futile.screen.halfWidth;
			label.y = Futile.screen.height - 100f;
			everythingContainer.AddChild(label);
			Tween tween = new Tween(label, 0.3f, new TweenConfig()
				.setDelay(3.0f)
				.floatProp("y", Futile.screen.height + 200f)
				.setEaseType(EaseType.SineInOut));
			Go.addTween(tween);
			tween.play();
		}
		
		whit = new TWalkingCharacter("whitHead.png");
		whit.x = 130f;
		whit.y = 250f;
		everythingContainer.AddChild(whit);
		whit.StartWalking();
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
	
	void SetupFinalNote() {
		finalNoteStrings = new List<string>();
		finalNoteLabels = new List<FLabel>();
		for (int i = 0; i < 3; i++) {
			FLabel label = new FLabel("SoftSugar", "");
			label.color = Color.white;
			label.scale = 0.8f;
			label.y = Futile.screen.height + 100f;
			label.x = 650f;
			label.isVisible = false;
			everythingContainer.AddChild(label);
			finalNoteLabels.Add(label);
		}
		
		finalNoteStrings.Add("Yay, we found\neach other!");
		finalNoteStrings.Add("It's a good thing\nbecause you're\nthe best thing\nthat has ever\nhappened to me!");
		finalNoteStrings.Add("Thanks for being\nso wonderful");
		finalNoteStrings.Add("Thanks for being\nso hilarious");
		finalNoteStrings.Add("Thanks for making\nmy dreams come true");
		finalNoteStrings.Add("I love you!!!");
	}
	
	void SetupUIElements() {
		goalProgressBar = SquareMaker.Square(Futile.screen.width, 20f);
		goalProgressBar.anchorY = 1.0f;
		goalProgressBar.anchorX = 1.0f;
		goalProgressBar.color = new Color(1.0f, 0.4f, 0.4f, 1.0f);
		goalProgressBar.x = Futile.screen.width;
		goalProgressBar.y = Futile.screen.height;
		everythingContainer.AddChild(goalProgressBar);
		
		start = new FLabel("SoftSugar", "start");
		goal = new FLabel("SoftSugar", "goal");
		start.anchorX = 0;
		goal.anchorX = 1f;
		start.color = goal.color = Color.black;
		start.x = 5f;
		start.scale = goal.scale = 0.35f;
		goal.x = Futile.screen.width - 5f;
		start.y = goal.y = Futile.screen.height - 10f;
		everythingContainer.AddChild(start);
		everythingContainer.AddChild(goal);
	}
	
	public void SetupHeartShower() {
		heartShowerHearts = new List<FSprite>();

		for (int i = 0; i < 200; i++) {
			FSprite heart = new FSprite("heart.psd");
			heart.color = new Color(0.75f, Random.Range(0, 0.3f), Random.Range(0, 0.3f), 1.0f);
			heart.isVisible = false;
			heart.alpha = Random.Range(0.6f, 1.0f);
			heart.rotation = Random.Range(0, 359);
			heart.scale = Random.Range(0.1f, 1.0f);
			heartShowerHearts.Add(heart);
			everythingContainer.AddChild(heart);
		}
	}
	
	public void SetupTutorial() {
		tutorialLabel = new FLabel("SoftSugar", "");
		tutorialLabel.y = Futile.screen.height - 100f;
		tutorialLabel.x = Futile.screen.halfWidth;
		everythingContainer.AddChild(tutorialLabel);
		
		tutorialStrings = new List<string>();
		
		tutorialStrings.Add("Help Whit find his sweetheart!");
		tutorialStrings.Add("Press space to jump");
		tutorialStrings.Add("Hold space to jump higher");
		tutorialStrings.Add("Press down arrow to crouch");
		tutorialStrings.Add("The red bar above shows how\nclose Whit is to his goal");
		tutorialStrings.Add("Avoid the broken hearts!");
	}
	
	public void SetupHeartTokens() {
		for (int i = 0; i < 50; i++) {
			THeartToken token = new THeartToken(false);
			token.sprite.isVisible = false;
			everythingContainer.AddChild(token.sprite);
			heartTokens.Add(token);
		}
		
		for (int i = 0; i < 10; i++) {
			THeartToken token = new THeartToken(true);
			token.sprite.isVisible = false;
			everythingContainer.AddChild(token.sprite);
			heartTokens.Add(token);
		}
	}
	
	public void SetupParallax() {
		FSprite background = SquareMaker.Square(Futile.screen.width, Futile.screen.height);
		background.x = Futile.screen.halfWidth;
		background.y = Futile.screen.halfHeight;
		background.color = new Color(0.7f, 0.9f, 1.0f, 1.0f);
		everythingContainer.AddChild(background);
		
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
		everythingContainer.AddChild(farMountain1);
		everythingContainer.AddChild(farMountain2);
		everythingContainer.AddChild(farMountain3);
		
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
		everythingContainer.AddChild(closeMountain1);
		everythingContainer.AddChild(closeMountain2);
		everythingContainer.AddChild(closeMountain3);
		
		FSprite backgroundFog = SquareMaker.Square(Futile.screen.width, Futile.screen.height);
		backgroundFog.color = Color.black;
		backgroundFog.alpha = 0.5f;
		backgroundFog.x = Futile.screen.halfWidth;
		backgroundFog.y = Futile.screen.halfHeight;
		everythingContainer.AddChild(backgroundFog);
		
		cloud = new FSprite("cloud.psd");
		cloud.alpha = 0.6f;
		cloud.x = Futile.screen.halfWidth;
		cloud.y = Futile.screen.height - cloud.height / 2f - 20f;
		everythingContainer.AddChild(cloud);
		
		for (int i = 0; i < 3; i++) {
			FSprite groundBlock = new FSprite("ground.psd");
			groundBlock.x = i * (groundBlock.width - 2) + groundBlock.width / 2f;
			groundBlock.y = groundBlock.height / 2f;
			groundBlocks.Add(groundBlock);
			everythingContainer.AddChild(groundBlock);
		}
	}
	
	#endregion
	
	#region Tutorial
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
		tutorialLabelHiddenTimer = 0;
		tutorialStringIndex++;
	}
	
	public void OnTutorialLabelFinishedEnlarging(AbstractTween tween) {
		tutorialLabelIsAnimating = false;	
	}
	
	public void UpdateTutorial() {
		tutorialLabelHiddenTimer += Time.fixedDeltaTime;
		if (tutorialLabelIsAnimating || tutorialLabelIsShowing) tutorialLabelHiddenTimer = 0;
		if (tutorialLabelIsAnimating || !tutorialLabelIsShowing) tutorialLabelShowingTimer = 0;
		if (tutorialStringIndex == 0) {
			tutorialLabelShowingTimer += Time.fixedDeltaTime;
			
			if (tutorialLabelShowingTimer >= 3.0f) DismissTutorialLabel();
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
			tutorialLabelShowingTimer += Time.fixedDeltaTime;
			if (tutorialLabelShowingTimer >= 3.0f) {
				DismissTutorialLabel();
				if (tutorialStringIndex >= 5) tutorialIsDone = true;
			}
		}
				
		if (tutorialLabelHiddenTimer >= 0.8f) {	
			DisplayTutorialString(tutorialStrings[tutorialStringIndex]);
		}
	}
	
	#endregion
	
	#region Updates
	
	public void HandleUpdate() {		
		if (gameIsOver) {
			UpdatePostGameOver();
			return;
		}
		
		if (foundEachother) {
			UpdatePostFoundEachOther();
			UpdateFinalNote();
			return;
		}
				
		if (Input.GetKeyDown(KeyCode.Space) && !whit.isJumping) whit.Jump();
		if (Input.GetKey(KeyCode.Space)) {
			if (whit.isCrouched) whit.decelAmt = 100f;
			else whit.decelAmt = 50f;
		}
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
			UpdateGoal();
		}
	}
	
	public void UpdateFinalNote() {
		if (doneWithNote) return;
		
		if (finalNoteAnimating) {
			finalNoteHiddenTimer = finalNoteShowingTimer = 0;	
		}
		else if (finalNoteShowing) {
			finalNoteShowingTimer += Time.fixedDeltaTime;
			finalNoteHiddenTimer = 0;
			
			if (finalNoteShowingTimer >= Mathf.Max((float)(finalNoteStrings[finalNoteStringIndex - 1].Length / 10), 3.0f)) {
				AnimateFinalNoteLabelOut();
				if (finalNoteStringIndex >= finalNoteStrings.Count) {
					doneWithNote = true;
					Go.to(everythingContainer, 8.0f, new TweenConfig().floatProp("alpha", 0).onComplete(OnSceneFadedOut));
				}
			}
		}
		else {
			finalNoteHiddenTimer += Time.fixedDeltaTime;
			finalNoteShowingTimer = 0;
			
			if (finalNoteHiddenTimer >= 1.0f) {				
				AnimateFinalNoteLabelIn();
			}
		}
	}
	
	public void UpdateHeartTokens() {
		heartTokenDistanceTimer += -universalVelocity * Time.fixedDeltaTime;
		totalDistance += -universalVelocity * Time.fixedDeltaTime;
		
		float distanceAmt = Random.Range(900f, 2000f);
		if (goalType == GoalType.GoalTwo) distanceAmt += 500f;
		if (goalType == GoalType.GoalOne) distanceAmt += 1000f;
		
		
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
	
	public void UpdateGoal() {
		if (totalDistance < goalDistance - 1000f || initiatedSceneSwitch) return;
		
		if (goalType == GoalType.GoalOne) {
			if (faceCoin == null) {
				faceCoin = new FSprite("danaHappy.png");
				faceCoin.x = Futile.screen.width + 100f;
				faceCoin.y = 250f;
				everythingContainer.AddChild(faceCoin);
				everythingContainer.AddChild(whit); // move him to top
				
				Tween tween1 = new Tween(faceCoin, 0.5f, new TweenConfig()
					.floatProp("scaleX", -1.0f)
					.setEaseType(EaseType.SineInOut));
				
				Tween tween2 = new Tween(faceCoin, 0.5f, new TweenConfig()
					.floatProp("scaleX", 1.0f)
					.setEaseType(EaseType.SineInOut));
				
				TweenChain chain = new TweenChain();
				chain.setIterations(-1);
				chain.append(tween1);
				chain.append(tween2);
				Go.addTween(chain);
				chain.play();
			}
			
			faceCoin.x += Time.fixedDeltaTime * universalVelocity;
			
			if (faceCoin.x < 100f) {
				initiatedSceneSwitch = true;
				FSoundManager.StopMusic();
				FSoundManager.PlaySound("success");
				TMain.SwitchToScene(TMain.SceneType.MergeNamesScene);
			}
		}
		
		else if (goalType == GoalType.GoalTwo) {
			if (bigHeartCoin == null) {
				bigHeartCoin = new FSprite("heart.psd");
				bigHeartCoin.scale = 2.0f;
				bigHeartCoin.x = Futile.screen.width + 100f;
				bigHeartCoin.color = new Color(1.0f, 0.2f, 0.2f, 1.0f);
				bigHeartCoin.y = 250f;
				everythingContainer.AddChild(bigHeartCoin);
				everythingContainer.AddChild(whit); // move to top
				
				Tween tween1 = new Tween(bigHeartCoin, 0.5f, new TweenConfig()
					.floatProp("scaleX", -1.0f)
					.setEaseType(EaseType.SineInOut));
				
				Tween tween2 = new Tween(bigHeartCoin, 0.5f, new TweenConfig()
					.floatProp("scaleX", 1.0f)
					.setEaseType(EaseType.SineInOut));
				
				TweenChain chain = new TweenChain();
				chain.setIterations(-1);
				chain.append(tween1);
				chain.append(tween2);
				Go.addTween(chain);
				chain.play();
			}
			
			bigHeartCoin.x += Time.fixedDeltaTime * universalVelocity;
			
			if (bigHeartCoin.x < 100f) {
				initiatedSceneSwitch = true;
				FSoundManager.StopMusic();
				FSoundManager.PlaySound("success");
				TMain.SwitchToScene(TMain.SceneType.ClickHeartsScene);
			}
		}
		
		else if (goalType == GoalType.GoalThree) {
			if (dana == null) {
				dana = new TWalkingCharacter("danaHead.png");
				dana.x = Futile.screen.width + 100f;
				dana.y = 250f;
				everythingContainer.AddChild(dana);
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
			float newX = mountain.x + (universalVelocity * 0.15f) * Time.fixedDeltaTime;
			if (newX < -mountain.width / 2f) {
				newX += farMountainSprites.Count * (mountain.width - 2);	
			}
			mountain.x = newX;
		}
		
		foreach (FSprite mountain in closeMountainSprites) {
			float newX = mountain.x + (universalVelocity * 0.3f) * Time.fixedDeltaTime;
			if (newX < -mountain.width / 2f) {
				newX += closeMountainSprites.Count * (mountain.width - 2);	
			}
			mountain.x = newX;
		}
		
		float cloudX = cloud.x + (universalVelocity * 0.2f) * Time.fixedDeltaTime;
		if (cloudX < -cloud.width / 2f) {
			cloudX += Futile.screen.width + cloud.width + Random.Range(30f, 200f);	
		}
		cloud.x = cloudX;
	}

	public void UpdatePostGameOver() {
		endGameWaitTimer += Time.fixedDeltaTime;
			
		if (endGameWaitTimer > 1.0f && Input.anyKeyDown && !initiatedSceneSwitch) {
			FSoundManager.StopMusic();
			initiatedSceneSwitch = true;
			if (goalType == GoalType.GoalOne) TMain.SwitchToScene(TMain.SceneType.PeopleSceneGoalOne);
			else if (goalType == GoalType.GoalTwo) TMain.SwitchToScene(TMain.SceneType.PeopleSceneGoalTwo);
			else if (goalType == GoalType.GoalThree) TMain.SwitchToScene(TMain.SceneType.PeopleSceneGoalThree);
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
	
	public void UpdateHeartTokenCollisions() {
		foreach (THeartToken token in heartTokens) {
			if (!token.sprite.isVisible) continue;
			if (whit.GetGlobalBoundsRect().CheckIntersect(token.GetGlobalBoundsRect())) {
				FSoundManager.PlayMusic("nooo");
				GameOver();
			}
		}
	}
	
	public void UpdateUIElements() {
		goalProgressBar.width = (1f - totalDistance / goalDistance) * Futile.screen.width;
	}
	
	#endregion
	
	#region Final Note
	
	public void AnimateFinalNoteLabelIn() {
		finalNoteAnimating = true;
		
		foreach (FLabel label in finalNoteLabels) {
			if (!label.isVisible) {
				currentFinalNoteLabel = label;
				break;
			}
		}
		
		if (currentFinalNoteLabel == null) {
			Debug.Log("Oops, no unused labels");
			return;
		}
		
		currentFinalNoteLabel.isVisible = true;
		currentFinalNoteLabel.text = finalNoteStrings[finalNoteStringIndex];
		
		finalNoteStringIndex++;
		
		currentFinalNoteLabel.y = Futile.screen.height + 100f;
		Go.to(currentFinalNoteLabel, 0.5f, new TweenConfig()
			.floatProp("y", Futile.screen.halfHeight)
			.setEaseType(EaseType.SineInOut)
			.onComplete(OnFinalNoteLabelFinishedAppearing));
	}
	
	public void AnimateFinalNoteLabelOut() {
		finalNoteAnimating = true;
		
		Go.to(currentFinalNoteLabel, 0.5f, new TweenConfig()
			.floatProp("y", -100f)
			.setEaseType(EaseType.SineInOut)
			.onComplete(OnFinalNoteLabelFinishedHiding));
	}
	
	public void OnFinalNoteLabelFinishedHiding(AbstractTween tween) {
		finalNoteAnimating = false;
		finalNoteShowing = false;
		currentFinalNoteLabel.isVisible = false;
		currentFinalNoteLabel = null;
	}
	
	public void OnFinalNoteLabelFinishedAppearing(AbstractTween tween) {
		finalNoteAnimating = false;
		finalNoteShowing = true;
	}
	
	#endregion
	
	#region Heart Tokens
	
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
	
	public void PrepareNewHeartTokenForEntry(bool isBroken) {
		THeartToken token;
		token = GetUnusedHeartToken(isBroken);
		
		if (token == null) return;
		
		token.sprite.isVisible = true;
		float rand = RXRandom.Float();
		if (rand < 0.5f) token.sprite.y = 150f;
		else if (rand >= 0.5f && rand < 0.8f) token.sprite.y = 350f;
		else token.sprite.y = 550f;
		token.sprite.x = Futile.screen.width + token.sprite.width / 2f;
	}
	
	#endregion
	
	#region Endgame
	
	public void GameOver() {
		whit.StopWalking();
		whit.StopCrouching();
		whit.headSprite.element = Futile.atlasManager.GetElementWithName("whitSadHead.psd");
		whit.headSprite.y -= 7f;
		gameIsOver = true;
		if (dana != null) dana.StopWalking();
	}
	
	public void OnSceneFadedOut(AbstractTween tween) {
			
	}
	
	void StartHeartShower() {
		foreach (FSprite heart in heartShowerHearts) {
			heart.x = Random.Range(0, Futile.screen.width);
			heart.y = Futile.screen.height + heart.height + Random.Range(0, 500);
			heart.isVisible = true;
			Go.to(heart, Random.Range(1.0f, 4.0f), new TweenConfig().setIterations(-1).floatProp("rotation", 360 * (RXRandom.Float() < 0.5 ? 1 : -1), true));
		}
	}
	
	#endregion
	
	#region Touches
	
	public bool HandleSingleTouchBegan(FTouch touch) {
		if (gameIsOver && !initiatedSceneSwitch) {
			FSoundManager.StopMusic();
			initiatedSceneSwitch = true;
			if (goalType == GoalType.GoalOne) TMain.SwitchToScene(TMain.SceneType.PeopleSceneGoalOne);
			else if (goalType == GoalType.GoalTwo) TMain.SwitchToScene(TMain.SceneType.PeopleSceneGoalTwo);
			else if (goalType == GoalType.GoalThree) TMain.SwitchToScene(TMain.SceneType.PeopleSceneGoalThree);
		}
		
		return true;
	}
	
	public void HandleSingleTouchMoved(FTouch touch) {
		
	}
	
	public void HandleSingleTouchEnded(FTouch touch) {
		
	}
	
	public void HandleSingleTouchCanceled(FTouch touch) {
		
	}
	#endregion
}
