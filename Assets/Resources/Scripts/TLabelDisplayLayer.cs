using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum LabelShowType {
	None,
	SlideFromLeft,
	SlideFromTop,
	SlideFromRight,
	SlideFromBottom
}

public enum LabelHideType {
	None,
	SlideToLeft,
	SlideToTop,
	SlideToRight,
	SlideToBottom
}

public class TLabelDisplayLayer : FStage {
	public event Action NoStringsLeftToShow;
	public LabelShowType labelShowType = LabelShowType.SlideFromTop;
	public LabelHideType labelHideType = LabelHideType.SlideToBottom;
	public EaseType easeType = EaseType.SineInOut;
	public float defaultX = Futile.screen.halfWidth;
	public float defaultY = Futile.screen.halfHeight;
	public float inDuration = 0.3f;
	public float outDuration = 0.3f;
	public float defaultHoldDuration = 3.0f;
	public float defaultWaitBetweenStrings = 0.5f;
	public bool shouldIncreaseHoldDurationBasedOnStringLength = false;
	
	private float waitTimer = 0;
	private float fontScale_ = 1.0f;
	private Color fontColor_ = Color.white;
	private FLabel label;
	private bool hasStringsToShow = false;
	private bool labelIsAnimating = false;
	private bool labelIsInPlace = false;
	private List<string> stringQueue;
	
	public TLabelDisplayLayer() : base("") {
		stringQueue = new List<string>();
		label = new FLabel("SoftSugar", "");
		label.isVisible = false;
		AddChild(label);
	}
	
	override public void HandleAddedToStage() {
		base.HandleAddedToStage();
		Futile.instance.SignalUpdate += HandleUpdate;
	}
	
	override public void HandleRemovedFromStage() {
		base.HandleRemovedFromStage();
		Futile.instance.SignalUpdate -= HandleUpdate;
	}
	
	public float fontScale {
		get {return fontScale_;}
		set {
			fontScale_ = value;
			label.scale = fontScale_;
		}
	}
	
	public Color fontColor {
		get {return fontColor_;}
		set {
			fontColor_ = value;
			label.color = fontColor_;
		}
	}
	
	public void HandleUpdate() {		
		if (hasStringsToShow) {
			if (!labelIsAnimating && !labelIsInPlace) {
				waitTimer += Time.fixedDeltaTime;
				if (waitTimer >= defaultWaitBetweenStrings) {
					DisplayNextString();	
				}
			}
			else waitTimer = 0;
		}
	}
	
	public void AddStringsToQueue(string[] stringsToAdd) {
		if (stringsToAdd.Length > 0) hasStringsToShow = true;
		
		for (int i = 0; i < stringsToAdd.Length; i++) {
			stringQueue.Add(stringsToAdd[i]);
		}
		
		waitTimer = defaultWaitBetweenStrings;
	}
	
	public void DisplayNextString() {
		if (stringQueue.Count == 0) {
			hasStringsToShow = false;
			NoStringsLeftToShow();
			return;
		}
		
		float startX = defaultX;
		float startY = defaultY;
		float holdX = defaultX;
		float holdY = defaultY;
		float endX = defaultX;
		float endY = defaultY;
		
		label.text = stringQueue[0];
		label.CreateTextQuads();
		
		if (labelShowType == LabelShowType.SlideFromLeft) {
			startX = -label.textRect.width / 2f;
		}
		else if (labelShowType == LabelShowType.SlideFromRight) {
			startX = Futile.screen.width + label.textRect.width / 2f;
		}
		else if (labelShowType == LabelShowType.SlideFromTop) {
			startY = Futile.screen.height + label.textRect.height / 2f;
		}
		else if (labelShowType == LabelShowType.SlideFromBottom) {
			startY = -label.textRect.height / 2f;
		}
		
		if (labelHideType == LabelHideType.SlideToLeft) {
			endX = -label.textRect.width / 2f;
		}
		else if (labelHideType == LabelHideType.SlideToRight) {
			endX = Futile.screen.width + label.textRect.width / 2f;
		}
		else if (labelHideType == LabelHideType.SlideToTop) {
			endY = Futile.screen.height + label.textRect.height / 2f;
		}
		else if (labelHideType == LabelHideType.SlideToBottom) {
			endY = -label.textRect.height / 2f;
		}
		
		label.x = startX;
		label.y = startY;
		
		Tween inTween = new Tween(label, inDuration, new TweenConfig()
			.floatProp("x", holdX)
			.floatProp("y", holdY)
			.onComplete(DoneMovingLabelIntoPlace)
			.setEaseType(easeType));
	
		float holdDuration = defaultHoldDuration;
		if (shouldIncreaseHoldDurationBasedOnStringLength) holdDuration = Mathf.Max((float)(label.text.Length / 10), 3.0f);
		
		Tween holdTween = new Tween(label, holdDuration, new TweenConfig()
			.floatProp("x", 0, true) // just to fake it into thinking it has a real tween
			.onComplete(StartingToAnimateLabelOut));
		
		Tween outTween = new Tween(label, outDuration, new TweenConfig()
			.floatProp("x", endX)
			.floatProp("y", endY)
			.setEaseType(easeType));
		
		TweenChain chain = new TweenChain();
		chain.append(inTween).append(holdTween).append(outTween);
		chain.setOnCompleteHandler(DoneShowingLabel);
		Go.addTween(chain);
		chain.play();
		
		labelIsAnimating = true;
		label.isVisible = true;
	}
	
	public void DoneShowingLabel(AbstractTween tween) {
		label.isVisible = false;
		labelIsAnimating = labelIsInPlace = false;
		
		stringQueue.Remove(stringQueue[0]);
	}
	
	public void DoneMovingLabelIntoPlace(AbstractTween tween) {
		labelIsAnimating = false;
		labelIsInPlace = true;
	}
		
	public void StartingToAnimateLabelOut(AbstractTween tween) {
		labelIsAnimating = true;
		labelIsInPlace = false;		
	}
}
