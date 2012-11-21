using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TParallaxScene : FContainer {
	public float foregroundVelocity = -800f;
	
	private List<TParallaxLayer> layers;
	private bool isUpdating = false;
	
	public TParallaxScene(Color backgroundColor) {
		FSprite background = SquareMaker.Square(Futile.screen.width, Futile.screen.height);
		background.x = Futile.screen.halfWidth;
		background.y = Futile.screen.halfHeight;
		background.color = backgroundColor;
		AddChild(background);
		
		layers = new List<TParallaxLayer>();
	}
	
	public void StartUpdating() {
		isUpdating = true;	
	}
	
	public void StopUpdating() {
		isUpdating = false;	
	}
	
	override public void HandleAddedToStage() {
		base.HandleAddedToStage();
		Futile.instance.SignalUpdate += HandleUpdate;
	}
	
	override public void HandleRemovedFromStage() {
		base.HandleRemovedFromStage();
		Futile.instance.SignalUpdate -= HandleUpdate;
	}
	
	public void HandleUpdate() {
		if (!isUpdating) return;
		
		foreach (TParallaxLayer layer in layers) {
			layer.UpdateWithPreScaledVelocity(foregroundVelocity, Time.fixedDeltaTime);	
		}
	}
	
	public void AddLayerWithImageName(string imageName, float velocityScale, float baseY, bool seamless) {
		TParallaxLayer layer = new TParallaxLayer(imageName, velocityScale, seamless);
		layer.y = baseY;
		AddChild(layer);
		layers.Add(layer);
	}
}
