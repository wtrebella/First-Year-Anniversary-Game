using UnityEngine;
using System.Collections;

public class TMain : MonoBehaviour {
	
	public static FStage currentScene;
	private static TMain instance_;
	
	public enum SceneType {
		None,
		MergeNamesScene,
		ClickHeartsScene,
		PeopleSceneWithTutorial,
		PeopleSceneWithoutTutorial
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
		
		SwitchToScene(SceneType.MergeNamesScene);
	}
	
	public static void SwitchToScene(SceneType sceneType) {
		FStage oldScene = null;
		
		oldScene = currentScene;
		
		if (sceneType == SceneType.MergeNamesScene) currentScene = new TMergeNamesScene();
		if (sceneType == SceneType.ClickHeartsScene) currentScene = new TClickHeartsScene();
		if (sceneType == SceneType.PeopleSceneWithTutorial) currentScene = new TPeopleScene(true);
		if (sceneType == SceneType.PeopleSceneWithoutTutorial) currentScene = new TPeopleScene(false);
		
		if (oldScene != null) {
			currentScene.alpha = 0;
			Go.to(oldScene, 0.3f, new TweenConfig().floatProp("alpha", 0.0f).onComplete(OnOldSceneCompletedFadingOut));
			Go.to(currentScene, 0.3f, new TweenConfig().floatProp("alpha", 1.0f));
		}	
		
		Futile.AddStage(currentScene);
	}		
	
	public static void OnOldSceneCompletedFadingOut(AbstractTween tween) {
		FStage oldScene = (tween as Tween).target as FStage;
		oldScene.RemoveFromContainer();
	}
	
	public TMain instance {
		get {
			if (instance_ == null) instance_ = this;
			return instance_;
		}
	}
}
