using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TParallaxLayer : FContainer {
	public float velocityScale = 1.0f;
	public float spriteOverlap = 2f;
	
	private bool seamless;
	private List<FSprite> sprites;
	
	public TParallaxLayer(string imageName, float velocityScale, bool seamless) {
		sprites = new List<FSprite>();
		this.velocityScale = velocityScale;
		this.seamless = seamless;
		
		FSprite firstSprite = new FSprite(imageName);
		int quantityNeeded = (int)(Futile.screen.width / (firstSprite.width - spriteOverlap)) + 2;
		if (!this.seamless) quantityNeeded = 1;
		sprites.Add(firstSprite);
		
		for (int i = 1; i < quantityNeeded; i++) {
			FSprite newSprite = new FSprite(imageName);
			sprites.Add(newSprite);
		}
		
		for (int i = 0, xIndex = -1; i < sprites.Count; i++, xIndex++) {
			FSprite sprite = sprites[i];
			sprite.anchorX = sprite.anchorY = 0;
			sprite.y = 0;
			sprite.x = xIndex * (sprite.width - spriteOverlap);
			AddChild(sprite);
		}
	}
	
	public void UpdateWithPreScaledVelocity(float preScaledVelocity, float deltaTime) {
		float scaledVelocity = velocityScale * preScaledVelocity * deltaTime;
		
		foreach (FSprite sprite in sprites) {
			float newX = sprite.x + scaledVelocity;
			if (scaledVelocity > 0) {
				if (newX >= Futile.screen.width) {
					newX -= (sprite.width - spriteOverlap) * sprites.Count;
					if (!this.seamless) newX = -sprite.width;
				}
			}
			else {
				if (newX <= -sprite.width) {
					newX += (sprite.width - spriteOverlap) * sprites.Count;
					if (!this.seamless) newX = Futile.screen.width + sprite.width;
				}
			}
			sprite.x = newX;
		}
	}
}
