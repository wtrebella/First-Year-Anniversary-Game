using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TMergeNamesScene : FStage, FSingleTouchableInterface {
	#region Variables
	
	List<FLabel> blackallLetters;
	List<FLabel> tesserLetters;
	List<FLabel> trebellaLetters;
	List<FLabel> unsolidifiedTrebellaLetters;
	
	List<Vector2> trebellaFinalPositions;
	
	List<FSprite> hearts;
	
	FButton whitButton;
	FButton danaButton;
	
	bool initiatedSceneSwitch = false;
	bool tesserDoneAnimatingIn = false;
	bool blackallDoneAnimatingIn = false;
	bool trebellaLettersHaveStartedSolidifying = false;
	bool trebellaLettersDoneSolidifying = false;
	bool trebellaLettersDoneTurningIntoHearts = false;
	bool touchWasUsedCorrectly = false;
	
	bool movedT = false;
	bool movedR = false;
	bool movedB = false;
	
	int numTouchedLs = 0;
	int numTouchedAs = 0;
	int numTouchedEs = 0;
	
	int numTrebellaLettersInPlace = 0;
	int numNonTrebellaLettersLeft = 6;
	int numTrebellaLettersTurnedIntoHearts = 0;
	
	float timeSinceTrebellaLettersFinishedTurningIntoHearts = 0;
	float timeSinceLastSolidifiedLetter = 0;
	int numLettersSolidified = 0;
	
	#endregion
	
	#region Init and Setup
	
	public TMergeNamesScene() : base("Name Destroy Scene") {
		TDiagBarsLayer diagLayer = new TDiagBarsLayer();
		AddChild(diagLayer);
		
		TBorderLayer borderLayer = new TBorderLayer(Futile.screen.width, Futile.screen.height, 5f, new Color(0.2f, 0.2f, 0.2f, 1.0f));
		AddChild(borderLayer);
		
		InitButtons();
		
		trebellaFinalPositions = new List<Vector2>();
		float startingZero = 100f;
		float horizontalDistance = (Futile.screen.width - startingZero * 2f) / 9f;
		for (int i = 0; i < 8; i++) {
			trebellaFinalPositions.Add(new Vector2(startingZero + horizontalDistance * (i + 1), Futile.screen.halfHeight));	
		}
		
		unsolidifiedTrebellaLetters = new List<FLabel>();
		hearts = new List<FSprite>();
		
		for (int i = 0; i < 100; i++) {
			FSprite heart = new FSprite("heart.psd");
			heart.scale = Random.Range(0.1f, 0.3f);
			float secondaryColorAmt = Random.Range(0.0f, 0.3f);
			heart.color = new Color(1.0f, secondaryColorAmt, secondaryColorAmt, 1.0f);
			heart.rotation = Random.Range(0.0f, 359.0f);
			hearts.Add(heart);
		}
		
		List<string>blackallStrings = new List<string>();
		blackallStrings.Add("B");
		blackallStrings.Add("L");
		blackallStrings.Add("A");
		blackallStrings.Add("C");
		blackallStrings.Add("K");
		blackallStrings.Add("A");
		blackallStrings.Add("L");
		blackallStrings.Add("L");
		blackallLetters = InitLetters(blackallStrings, "Burnstown", -30f);
		
		List<string>tesserStrings = new List<string>();
		tesserStrings.Add("T");
		tesserStrings.Add("E");
		tesserStrings.Add("S");
		tesserStrings.Add("S");
		tesserStrings.Add("E");
		tesserStrings.Add("R");
		tesserLetters = InitLetters(tesserStrings, "SoftSugar", Futile.screen.height + 30f);
		
		List<string>trebellaStrings = new List<string>();
		trebellaStrings.Add("T");
		trebellaStrings.Add("R");
		trebellaStrings.Add("E");
		trebellaStrings.Add("B");
		trebellaStrings.Add("E");
		trebellaStrings.Add("L");
		trebellaStrings.Add("L");
		trebellaStrings.Add("A");
		trebellaLetters = InitLetters(trebellaStrings, "Exotica", Futile.screen.height + 30f);
	}

	void InitButtons() {
		float verticalDivision = Futile.screen.height / 3f;

		whitButton = new FButton("whitNormal.png", "whitHover.png", "whitHappy.png", "plip");
		danaButton = new FButton("danaNormal.png", "danaHover.png", "danaHappy.png", "plip");
				
		whitButton.shouldReturnToUpElementAfterTouch = false;
		danaButton.shouldReturnToUpElementAfterTouch = false;
		
		whitButton.SignalRelease += PressedButton;
		danaButton.SignalRelease += PressedButton;
		
		whitButton.x = Futile.screen.halfWidth;
		whitButton.y = verticalDivision;
		
		danaButton.x = Futile.screen.halfWidth;
		danaButton.y = verticalDivision * 2f;

		whitButton.scale = 0.6f;
		danaButton.scale = 0.6f;

		AddChild(whitButton);
		AddChild(danaButton);
	}
	
	List<FLabel> InitLetters(List<string> letterStrings, string fontName, float startingY) {
		float startingWidth = Futile.screen.width * 3f;
		float startingZero = Futile.screen.halfWidth - startingWidth / 2f;
		float horizontalDivision = startingWidth / letterStrings.Count;
		
		List<FLabel> labels = new List<FLabel>();
		
		for (int i = 0; i < letterStrings.Count; i++) {
			FLabel label = new FLabel(fontName, letterStrings[i]);
			label.color = new Color(0.12f, 0.12f, 0.12f, 1.0f);
			labels.Add(label);
			label.y = startingY;
			label.x = startingZero + horizontalDivision * (i + 1);
			AddChild(label);
		}
		
		return labels;
	}
		
	public override void HandleAddedToStage ()
	{
		base.HandleAddedToStage ();
		Futile.touchManager.AddSingleTouchTarget(this);
		Futile.instance.SignalUpdate += HandleUpdate;
	}
	
	public override void HandleRemovedFromStage ()
	{
		base.HandleRemovedFromStage ();
		Futile.touchManager.RemoveSingleTouchTarget(this);
		Futile.instance.SignalUpdate -= HandleUpdate;
	}
	
	#endregion
	
	#region Update Handlers
	
	public void HandleUpdate() {
		if (numTrebellaLettersInPlace == 8 && numNonTrebellaLettersLeft == 0 && !trebellaLettersHaveStartedSolidifying) {
			FSoundManager.PlaySound("success");
			trebellaLettersHaveStartedSolidifying = true;	
		}
		else if (trebellaLettersHaveStartedSolidifying && !trebellaLettersDoneSolidifying) {
			UpdateSolidifyingTrebellaLetters();	
		}
		else if (trebellaLettersDoneTurningIntoHearts) {
			timeSinceTrebellaLettersFinishedTurningIntoHearts += Time.deltaTime;
			if (timeSinceTrebellaLettersFinishedTurningIntoHearts > 1.0f && !initiatedSceneSwitch) {
				initiatedSceneSwitch = true;
				TMain.SwitchToScene(TMain.SceneType.PeopleSceneGoalTwo);
			}
		}
	}
	
	public void UpdateSolidifyingTrebellaLetters() {
		timeSinceLastSolidifiedLetter += Time.deltaTime;
		
		if (timeSinceLastSolidifiedLetter > 0.5f) {
			timeSinceLastSolidifiedLetter -= 0.5f;
			
			FLabel nextLetterToChange = null;
			
			switch (numLettersSolidified) {
			case 0:
				foreach (FLabel letter in unsolidifiedTrebellaLetters) {
					if (letter.text == "T") {
						nextLetterToChange = letter;
						unsolidifiedTrebellaLetters.Remove(letter);
						break;
					}
				}
				break;
			case 1:
				foreach (FLabel letter in unsolidifiedTrebellaLetters) {
					if (letter.text == "R") {
						nextLetterToChange = letter;
						unsolidifiedTrebellaLetters.Remove(letter);
						break;
					}
				}
				break;
			case 2:
				foreach (FLabel letter in unsolidifiedTrebellaLetters) {
					if (letter.text == "E") {
						nextLetterToChange = letter;
						unsolidifiedTrebellaLetters.Remove(letter);
						break;
					}
				}
				break;
			case 3:
				foreach (FLabel letter in unsolidifiedTrebellaLetters) {
					if (letter.text == "B") {
						nextLetterToChange = letter;
						unsolidifiedTrebellaLetters.Remove(letter);
						break;
					}
				}
				break;
			case 4:
				foreach (FLabel letter in unsolidifiedTrebellaLetters) {
					if (letter.text == "E") {
						nextLetterToChange = letter;
						unsolidifiedTrebellaLetters.Remove(letter);
						break;
					}
				}
				break;
			case 5:
				foreach (FLabel letter in unsolidifiedTrebellaLetters) {
					if (letter.text == "L") {
						nextLetterToChange = letter;
						unsolidifiedTrebellaLetters.Remove(letter);
						break;
					}
				}
				break;
			case 6:
				foreach (FLabel letter in unsolidifiedTrebellaLetters) {
					if (letter.text == "L") {
						nextLetterToChange = letter;
						unsolidifiedTrebellaLetters.Remove(letter);
						break;
					}
				}
				break;
			case 7:
				foreach (FLabel letter in unsolidifiedTrebellaLetters) {
					if (letter.text == "A") {
						nextLetterToChange = letter;
						unsolidifiedTrebellaLetters.Remove(letter);
						break;
					}
				}
				break;
			}
			
			FSoundManager.PlaySound("bomb", 2.0f);
			FLabel solidifiedLetter = trebellaLetters[numLettersSolidified];
			solidifiedLetter.x = nextLetterToChange.x;
			solidifiedLetter.y = nextLetterToChange.y;
			nextLetterToChange.RemoveFromContainer();
			
			float duration = 0.2f;
			float distance = 5f;
			
			Tween bounce = new Tween(solidifiedLetter, duration, new TweenConfig()
				.setEaseType(EaseType.SineInOut)
				.floatProp("y", distance, true));
			
			Tween bounceBack = new Tween(solidifiedLetter, duration, new TweenConfig()
				.setEaseType(EaseType.SineInOut)
				.floatProp("y", -distance, true));
			
			TweenChain chain = new TweenChain();
			chain.setIterations(-1, LoopType.RestartFromBeginning);
			chain.append(bounce);
			chain.append(bounceBack);
			
			Go.addTween(chain);
			
			chain.play();
			
			numLettersSolidified++;
			
			if (numLettersSolidified >= 8) trebellaLettersDoneSolidifying = true;
		}
	}
	
	#endregion
	
	#region Touch and Button Handlers
	
	public void PressedButton(FButton button) {
		if (button == whitButton) {
			AnimateLettersIn(blackallLetters, Futile.screen.height / 3f, BlackallDoneAnimatingIn);
		}
		
		else if (button == danaButton) {
			AnimateLettersIn(tesserLetters, Futile.screen.height / 3f * 2f, TesserDoneAnimatingIn);
		}
		
		Go.to(button, 0.3f, new TweenConfig().floatProp("alpha", 0).onComplete(ButtonDoneFading));
	}
	
	public bool HandleSingleTouchBegan(FTouch touch) {
		touchWasUsedCorrectly = false;
		
		if (blackallDoneAnimatingIn && tesserDoneAnimatingIn && !(numTrebellaLettersInPlace == 8 && numNonTrebellaLettersLeft == 0)) HandleTouchOnBlackallAndTesser(touch);
		
		if (trebellaLettersDoneSolidifying && !trebellaLettersDoneTurningIntoHearts) {
			touchWasUsedCorrectly = true;
			foreach (FLabel letter in trebellaLetters) {
				if (letter.textRect.Contains(letter.GlobalToLocal(touch.position))) {
					numTrebellaLettersTurnedIntoHearts++;
					letter.RemoveFromContainer();
					trebellaLetters.Remove(letter);
					ExplodeHeartsFromPoint(touch.position);
					FSoundManager.PlaySound("success");
					break;
				}
			}
			
			if (numTrebellaLettersTurnedIntoHearts == 8) trebellaLettersDoneTurningIntoHearts = true;
		}
		
		return true;
	}
	
	public void HandleSingleTouchMoved(FTouch touch) {
		
	}
	
	public void HandleSingleTouchEnded(FTouch touch) {
		if (!touchWasUsedCorrectly) FSoundManager.PlaySound("error");
	}
	
	public void HandleSingleTouchCanceled(FTouch touch) {
		
	}
	
	public void HandleTouchOnBlackallAndTesser(FTouch touch) {
		FLabel touchedLetter = null;

		for (int i = 0; i < tesserLetters.Count; i++) {
			FLabel label = tesserLetters[i];
			if (label == null) continue;
			Vector2 touchPos = label.GlobalToLocal(touch.position);
			if (label.textRect.Contains(touchPos)) {
				touchedLetter = label;
				break;
			}
		}
		
		for (int i = 0; i < blackallLetters.Count; i++) {
			FLabel label = blackallLetters[i];
			if (label == null) continue;
			Vector2 touchPos = label.GlobalToLocal(touch.position);
			if (label.textRect.Contains(touchPos)) {
				touchedLetter = label;
				break;
			}
		}
		
		if (touchedLetter == null) return;
		
		touchWasUsedCorrectly = true;
		
		TweenChain chain = null;
		float duration = 0.2f;
		float extraRotation = 0f;
		
		// TESSER
		
		// T
		if (touchedLetter == tesserLetters[0]) {
			if (unsolidifiedTrebellaLetters.Contains(touchedLetter)) {
				FSoundManager.PlaySound("error");
				return;
			}
			
			if (!movedT) {
				chain = TweenChainForLetter(touchedLetter, duration, trebellaFinalPositions[0].x, trebellaFinalPositions[0].y, extraRotation);
				movedT = true;
				unsolidifiedTrebellaLetters.Add(touchedLetter);
			}
		}
		
		// E
		if (touchedLetter == tesserLetters[1] || touchedLetter == tesserLetters[4]) {
			if (unsolidifiedTrebellaLetters.Contains(touchedLetter)) {
				FSoundManager.PlaySound("error");
				return;
			}
			
			int index;
			
			if (numTouchedEs == 0) index = 2;
			else if (numTouchedEs == 1) index = 4;
			else index = -1;
				
			if (index != -1) {
				chain = TweenChainForLetter(touchedLetter, duration, trebellaFinalPositions[index].x, trebellaFinalPositions[index].y, extraRotation);
				numTouchedEs++;
				unsolidifiedTrebellaLetters.Add(touchedLetter);
			}
			else {
				int tesserIndex = 0;
				if (touchedLetter == tesserLetters[1]) tesserIndex = 0;
				else if (touchedLetter == tesserLetters[4]) tesserIndex = 4;
				KillLetter(touchedLetter);
				tesserLetters[tesserIndex] = null;
				numNonTrebellaLettersLeft--;
			}
		}
		
		// S
		if (touchedLetter == tesserLetters[2]) {
			KillLetter(touchedLetter);
			numNonTrebellaLettersLeft--;
			tesserLetters[2] = null;
		}
		
		// S
		if (touchedLetter == tesserLetters[3]) {
			KillLetter(touchedLetter);
			numNonTrebellaLettersLeft--;
			tesserLetters[3] = null;
		}
		
		// R
		if (touchedLetter == tesserLetters[5]) {
			if (unsolidifiedTrebellaLetters.Contains(touchedLetter)) {
				FSoundManager.PlaySound("error");
				return;
			}
			
			if (!movedR) {
				chain = TweenChainForLetter(touchedLetter, duration, trebellaFinalPositions[1].x, trebellaFinalPositions[1].y, extraRotation);
				movedR = true;
				unsolidifiedTrebellaLetters.Add(touchedLetter);
			}
		}
		
		// BLACKALL
		
		// B
		if (touchedLetter == blackallLetters[0]) {
			if (unsolidifiedTrebellaLetters.Contains(touchedLetter)) {
				FSoundManager.PlaySound("error");
				return;
			}
			
			if (!movedB) {
				chain = TweenChainForLetter(touchedLetter, duration, trebellaFinalPositions[3].x, trebellaFinalPositions[3].y, extraRotation);
				movedB = true;
				unsolidifiedTrebellaLetters.Add(touchedLetter);
			}
		}
		
		// L
		if (touchedLetter == blackallLetters[1] || touchedLetter == blackallLetters[6] || touchedLetter == blackallLetters[7]) {
			if (unsolidifiedTrebellaLetters.Contains(touchedLetter)) {
				FSoundManager.PlaySound("error");
				return;
			}
			
			int index;
			
			if (numTouchedLs == 0) index = 5;
			else if (numTouchedLs == 1) index = 6;
			else index = -1;
				
			if (index != -1) {
				chain = TweenChainForLetter(touchedLetter, duration, trebellaFinalPositions[index].x, trebellaFinalPositions[index].y, extraRotation);
				numTouchedLs++;
				unsolidifiedTrebellaLetters.Add(touchedLetter);
			}
			else {
				int blackallIndex = 0;
				if (touchedLetter == blackallLetters[1]) blackallIndex = 1;
				else if (touchedLetter == blackallLetters[6]) blackallIndex = 6;
				else if (touchedLetter == blackallLetters[7]) blackallIndex = 7;
				KillLetter(touchedLetter);
				blackallLetters[blackallIndex] = null;
				numNonTrebellaLettersLeft--;
			}
		}
		
		// A
		if (touchedLetter == blackallLetters[2] || touchedLetter == blackallLetters[5]) {
			if (unsolidifiedTrebellaLetters.Contains(touchedLetter)) {
				FSoundManager.PlaySound("error");
				return;
			}
			
			int index;
			
			if (numTouchedAs == 0) index = 7;
			else index = -1;
				
			if (index != -1) {
				chain = TweenChainForLetter(touchedLetter, duration, trebellaFinalPositions[index].x, trebellaFinalPositions[index].y, extraRotation);
				numTouchedAs++;
				unsolidifiedTrebellaLetters.Add(touchedLetter);
			}
			else {
				int blackallIndex = 0;
				if (touchedLetter == blackallLetters[2]) blackallIndex = 2;
				else if (touchedLetter == blackallLetters[5]) blackallIndex = 5;
				KillLetter(touchedLetter);
				blackallLetters[blackallIndex] = null;
				numNonTrebellaLettersLeft--;
			}
		}
		
		// C
		if (touchedLetter == blackallLetters[3]) {
			KillLetter(touchedLetter);
			blackallLetters[3] = null;
			numNonTrebellaLettersLeft--;
		}
		
		// K
		if (touchedLetter == blackallLetters[4]) {
			KillLetter(touchedLetter);
			blackallLetters[4] = null;
			numNonTrebellaLettersLeft--;
		}
		
		if (chain != null) {
			chain.setOnCompleteHandler(OnTrebellaLetterInPlace);
			Go.addTween(chain);
			chain.play();
		}
	}
	
	#endregion
	
	#region Animations
	
	void ExplodeHeartsFromPoint(Vector2 point) {
		int heartsFound = 0;
		
		foreach (FSprite heart in hearts) {
			if (heartsFound >= 50) break;
			
			if (heart.container == null) {
				heartsFound++;
				heart.x = point.x;
				heart.y = point.y;
				AddChild(heart);
				
				float duration = Random.Range(0.1f, 1.5f);
				float xMovement = Random.Range(-600f, 600f);
				float yMovement = Random.Range(-600f, 600f);
				float rotationAmt = Random.Range(-360f, 360f);
				
				Tween tween = new Tween(heart, duration, new TweenConfig()
					.onComplete(OnHeartFinished)
					.floatProp("alpha", 0.0f)
					.floatProp("x", xMovement, true)
					.floatProp("y", yMovement, true)
					.floatProp("rotation", rotationAmt, true)
					.setEaseType(EaseType.SineInOut));
				
				Go.addTween(tween);
				
				tween.play();
			}
		}
	}
	
	void AnimateLettersIn(List<FLabel> letters,
		float finalZero,
		float yPosition,
		float durationPerLetter,
		float overlapBetweenLetterFlyins,
		float extraRotationAmount,
		System.Action<AbstractTween> onCompleteFunction) {
		
		if (overlapBetweenLetterFlyins > durationPerLetter) {
			Debug.Log("overlap can't be greater than duration!");
			return;
		}
		
		float horizontalDivision = (Futile.screen.width - finalZero * 2) / (letters.Count + 1);
		
		TweenFlow flow = new TweenFlow();
		
		for (int i = 0; i < letters.Count; i++) {
			TweenChain chain = TweenChainForLetter(letters[i], durationPerLetter, finalZero + horizontalDivision * (i + 1), yPosition, extraRotationAmount);
			
			flow.insert((durationPerLetter - overlapBetweenLetterFlyins) * i, chain);
		}
		
		if (onCompleteFunction != null) flow.setOnCompleteHandler(onCompleteFunction);
		Go.addTween(flow);
		flow.play();
	}
	
	void AnimateLettersIn(List<FLabel> letters, float yPosition, System.Action<AbstractTween> onCompleteFunction) {
		AnimateLettersIn(letters, 200f, yPosition, 0.5f, 0.3f, 25f, onCompleteFunction);
	}
	
	TweenChain TweenChainForLetter(FLabel letter, float duration, float xFinal, float yFinal, float extraRotationAmount) {	
		float flyInDuration = duration * 3f / 4f;
		float extraRotationDuration = duration - flyInDuration; 
		
		Tween tween = new Tween(letter, flyInDuration, new TweenConfig()
			.onComplete(OnLetterCompleteAnimatingIn)
			.floatProp("x", xFinal)
			.floatProp("y", yFinal)
			.floatProp("rotation", 360f, true)
			.setEaseType(EaseType.SineOut));
			
		Tween afterTween1 = new Tween(letter, extraRotationDuration / 3f, new TweenConfig().floatProp("rotation", extraRotationAmount, true).setEaseType(EaseType.SineOut));
		Tween afterTween2 = new Tween(letter, extraRotationDuration / 3f * 2f, new TweenConfig().floatProp("rotation", -extraRotationAmount, true).setEaseType(EaseType.SineOut));
		
		return new TweenChain().append(tween).append(afterTween1).append(afterTween2);
	}
			
	TweenChain TweenChainForLetter(FLabel letter, float xFinal, float yFinal) {
		return TweenChainForLetter(letter, 0.5f, xFinal, yFinal, 25f);		
	}
	
	void KillLetter(FLabel letter) {
		FSoundManager.PlaySound("shrink", 0.3f);
		
		Tween tween = new Tween(letter, 0.3f, new TweenConfig()
			.onComplete(OnLetterKilled)
			.floatProp("scale", 0)
			.setEaseType(EaseType.SineInOut));
		
		Go.addTween(tween);
		
		tween.play();
	}
		
	#endregion
	
	#region OnCompleteMethods
	
	public void OnHeartFinished(AbstractTween tween) {
		FSprite heart = (tween as Tween).target as FSprite;
		
		heart.alpha = 1.0f;
		heart.RemoveFromContainer();
	}
				
	public void OnLetterKilled(AbstractTween tween) {
		FLabel letter = (tween as Tween).target as FLabel;
		letter.RemoveFromContainer();
	}
	
	public void OnTrebellaDoneExploding(AbstractTween tween) {
			
	}
	
	public void OnLetterCompleteAnimatingIn(AbstractTween tween) {
		FSoundManager.PlaySound("plip", 0.5f);
	}
	
	public void BlackallDoneAnimatingIn(AbstractTween tween) {		
		blackallDoneAnimatingIn = true;
	}
	
	public void TesserDoneAnimatingIn(AbstractTween tween) {
		tesserDoneAnimatingIn = true;
	}
	
	public void ButtonDoneFading(AbstractTween tween) {
		FButton button = (tween as Tween).target as FButton;
		button.RemoveFromContainer();
		if (button == whitButton) whitButton = null;
		else if (button == danaButton) danaButton = null;
	}
	
	public void OnTrebellaLetterInPlace(AbstractTween tween) {
		numTrebellaLettersInPlace++;
	}
	
	#endregion
}
