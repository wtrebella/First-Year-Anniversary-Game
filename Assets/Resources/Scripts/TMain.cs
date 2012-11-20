using UnityEngine;
using System.Collections;

public class TMain : MonoBehaviour {
	
	public static FStage currentScene;
	private static TMain instance_;
	public static bool goalOneTutorialIsDone = false;
	
	public enum SceneType {
		None,
		MergeNamesScene,
		ClickHeartsScene,
		PeopleSceneGoalOne,
		PeopleSceneGoalTwo,
		PeopleSceneGoalThree
	}
	
	void Start () {
		FutileParams fp = new FutileParams(true, true, false, false);
		fp.AddResolutionLevel(960f, 1.0f, 1.0f, "");
		fp.origin = Vector2.zero;
		fp.backgroundColor = Color.white;
		
		Futile.instance.Init(fp);
		Futile.atlasManager.LoadAtlas("Atlases/MainSheet");
		Futile.atlasManager.LoadAtlas("Atlases/ExtrudersSheet");
		Futile.atlasManager.LoadFont("Burnstown", "Burnstown.png", "Atlases/Burnstown");
		Futile.atlasManager.LoadFont("Exotica", "Exotica.png", "Atlases/Exotica");
		Futile.atlasManager.LoadFont("SoftSugar", "SoftSugar.png", "Atlases/SoftSugar");
		
		SwitchToScene(SceneType.PeopleSceneGoalOne);
	}
	
	public static void SwitchToScene(SceneType sceneType) {
		FStage oldScene = null;
		
		oldScene = currentScene;
		
		if (sceneType == SceneType.MergeNamesScene) currentScene = new TMergeNamesScene();
		if (sceneType == SceneType.ClickHeartsScene) currentScene = new TClickHeartsScene();
		if (sceneType == SceneType.PeopleSceneGoalOne) {
			currentScene = new TPeopleScene(GoalType.GoalOne);
			goalOneTutorialIsDone = true;
		}
		if (sceneType == SceneType.PeopleSceneGoalTwo) currentScene = new TPeopleScene(GoalType.GoalTwo);
		if (sceneType == SceneType.PeopleSceneGoalThree) currentScene = new TPeopleScene(GoalType.GoalThree);
		
		if (oldScene != null) {
			currentScene.alpha = 0;
			Go.to(oldScene, 0.3f, new TweenConfig().floatProp("alpha", 0.0f).onComplete(OnOldSceneCompletedFadingOut));
			Go.to(currentScene, 0.3f, new TweenConfig().floatProp("alpha", 1.0f));
		}	
		
		Futile.AddStage(currentScene);
	}		
	
	public static void OnOldSceneCompletedFadingOut(AbstractTween tween) {
		FStage oldScene = (tween as Tween).target as FStage;
		Futile.RemoveStage(oldScene);
	}
	
	public TMain instance {
		get {
			if (instance_ == null) instance_ = this;
			return instance_;
		}
	}
}
