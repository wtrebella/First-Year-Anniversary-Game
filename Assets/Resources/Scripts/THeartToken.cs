using UnityEngine;
using System.Collections;

public class THeartToken {
	public FSprite sprite;
	public bool isBroken = false;
	//TBorderLayer border;
	
	public THeartToken(bool isBroken) {
		this.isBroken = isBroken;
		
		if (isBroken) {
			sprite = new FSprite("brokenHeart.psd");
		}
		else {
			sprite = new FSprite("heart.psd");
		}
		
		sprite.scale = 0.6f;
		sprite.color = new Color(1.0f, Random.Range(0.3f, 0.6f), Random.Range(0.3f, 0.6f), 1.0f);
		sprite.rotation = -10f;
		
		float duration = Random.Range(0.15f, 0.45f);
		Tween rotOut = new Tween(sprite, duration, new TweenConfig().floatProp("rotation", 10f));
		Tween rotIn = new Tween(sprite, duration, new TweenConfig().floatProp("rotation", -10f));
		TweenChain chain = new TweenChain();
		chain.setIterations(-1);
		chain.append(rotOut).append(rotIn);
		Go.addTween(chain);
		chain.play();
	}
	
	public Rect GetGlobalBoundsRect() {
		Vector2 position = sprite.container.LocalToGlobal(new Vector2(sprite.x, sprite.y));
		/*if (border != null) border.RemoveFromContainer();
		border = new TBorderLayer(sprite.width, sprite.height, 3f, Color.black);
		border.x = sprite.container.GlobalToLocal(position).x - sprite.width / 2f;
		border.y = sprite.container.GlobalToLocal(position).y - sprite.width / 2f;
		sprite.container.AddChild(border);*/
		return new Rect(position.x - sprite.width / 2f, position.y - sprite.height / 2f, sprite.width, sprite.height);
	}
}
