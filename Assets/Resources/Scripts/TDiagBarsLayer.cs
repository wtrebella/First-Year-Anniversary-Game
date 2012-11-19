using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TDiagBarsLayer : FContainer {
	
	List<FSprite> sprites = new List<FSprite>();
	float widthPerBar = 110;
	
	public TDiagBarsLayer() {
		for (int i = 0; i < 24; i++) {
			FSprite sprite = new FSprite("diagBar.png");
			sprite.scale = 1.7f;
			sprite.anchorX = 0;
			sprite.y = Futile.screen.halfHeight;
			sprite.x = Futile.screen.width - widthPerBar * i;
			if (i % 2 == 0) sprite.color = new Color(0.9f, 0.6f, 0.6f, 1.0f);
			else sprite.color = new Color(0.92f, 0.62f, 0.62f, 1.0f);
			sprites.Add(sprite);
			AddChild(sprite);
		}
	}
	
	public override void HandleAddedToStage () {
		base.HandleAddedToStage ();
		Futile.instance.SignalUpdate += HandleUpdate;
	}
	
	public override void HandleRemovedFromStage () {
		base.HandleRemovedFromStage ();
		Futile.instance.SignalUpdate -= HandleUpdate;
	}
	
	public void HandleUpdate() {
		foreach (FSprite sprite in sprites) {
			sprite.x -= 100f * Time.deltaTime;
			
			if (sprite.x + sprite.textureRect.width * sprite.scaleX < 0) {
				sprite.x += widthPerBar * sprites.Count;
			}
		}
	}
}
