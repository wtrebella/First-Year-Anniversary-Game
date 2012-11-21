using UnityEngine;
using System.Collections;

public enum DreamSceneType {
	DreamSceneOne,
	DreamSceneTwo
}

public class TDreamScene : FStage {
	DreamSceneType dreamSceneType;
	
	public TDreamScene(DreamSceneType dreamSceneType) : base("") {
		FSoundManager.PlayMusic("celesta");
		
		TParallaxScene parallaxScene = new TParallaxScene(Color.white);
		parallaxScene.AddLayerWithImageName("Atlases/clouds", 1.0f, 0, true);
		parallaxScene.foregroundVelocity = -100f;
		AddChild(parallaxScene);
		parallaxScene.StartUpdating();
		
		TBorderLayer borderLayer = new TBorderLayer(Futile.screen.width, Futile.screen.height, 5f, new Color(0.2f, 0.2f, 0.2f, 1.0f));
		AddChild(borderLayer);
		
		this.dreamSceneType = dreamSceneType;
		
		TMain.labelDisplayLayer.shouldIncreaseHoldDurationBasedOnStringLength = true;
		TMain.labelDisplayLayer.fontColor = Color.black;
		
		if (this.dreamSceneType == DreamSceneType.DreamSceneOne) {
			TMain.labelDisplayLayer.AddStringsToQueue(new string[] {
				"Suddenly, I fall asleep",
				"In my dream,\nI reminisce about\ncreating the\nperfect last name",
				"Help me piece together\nour last name!"});
		}
		else if (this.dreamSceneType == DreamSceneType.DreamSceneTwo) {
			TMain.labelDisplayLayer.AddStringsToQueue(new string[] {
				"Oh no, all this running\nhas made me fall\nasleep again!",
				"This time, I dream\nabout how much\nI love you",
				"Help me quantify just how\nmuch I love you!"});
		}
		TMain.labelDisplayLayer.defaultHoldDuration = 3.0f;
		TMain.labelDisplayLayer.labelShowType = LabelShowType.SlideFromTop;
		TMain.labelDisplayLayer.labelHideType = LabelHideType.SlideToBottom;
	}
	
	override public void HandleAddedToStage() {
		base.HandleAddedToStage();
		Futile.instance.SignalUpdate += HandleUpdate;
		TMain.labelDisplayLayer.NoStringsLeftToShow += DoneShowingLabels;
	}
	
	override public void HandleRemovedFromStage() {
		base.HandleRemovedFromStage();
		Futile.instance.SignalUpdate -= HandleUpdate;
		TMain.labelDisplayLayer.NoStringsLeftToShow -= DoneShowingLabels;
	}
	
	public void HandleUpdate() {
		
	}
	
	public void DoneShowingLabels() {
		if (this.dreamSceneType == DreamSceneType.DreamSceneOne) {
			TMain.SwitchToScene(TMain.SceneType.MergeNamesScene);
		}
		else if (this.dreamSceneType == DreamSceneType.DreamSceneTwo) {
			TMain.SwitchToScene(TMain.SceneType.ClickHeartsScene);
		}
	}
}
