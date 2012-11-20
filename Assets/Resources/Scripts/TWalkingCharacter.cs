using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TWalkingCharacter : FContainer {
	FAtlasElement[] frameElements;
	FAtlasElement[] crouchFrameElements;
	public FSprite headSprite;
	FSprite bodySprite;
	int frameIndex = 0;
	float yVelocity = 0;
	public float decelAmt = 50f;
	public float frameDuration = 0.1f;
	public bool isJumping = false;
	public bool isWalking = false;
	public bool isCrouched = false;
	public bool isTurning = false;
	float timeSinceLastFrameChange = 0;
	float groundY;
	TweenChain chain;
	//TBorderLayer border;
	
	public TWalkingCharacter(string headImage) {
		FAtlasManager am = Futile.atlasManager;
		
		frameElements = new FAtlasElement[8];
		crouchFrameElements = new FAtlasElement[8];
		
		frameElements[0] = am.GetElementWithName("walkAnim/walk0.png");
		frameElements[1] = am.GetElementWithName("walkAnim/walk1.png");
		frameElements[2] = am.GetElementWithName("walkAnim/walk2.png");
		frameElements[3] = am.GetElementWithName("walkAnim/walk3.png");
		frameElements[4] = am.GetElementWithName("walkAnim/walk4.png");
		frameElements[5] = am.GetElementWithName("walkAnim/walk1.png");
		frameElements[6] = am.GetElementWithName("walkAnim/walk2.png");
		frameElements[7] = am.GetElementWithName("walkAnim/walk3.png");
		
		crouchFrameElements[0] = am.GetElementWithName("squashedWalkAnim/squashedWalk0.png");
		crouchFrameElements[1] = am.GetElementWithName("squashedWalkAnim/squashedWalk1.png");
		crouchFrameElements[2] = am.GetElementWithName("squashedWalkAnim/squashedWalk2.png");
		crouchFrameElements[3] = am.GetElementWithName("squashedWalkAnim/squashedWalk3.png");
		crouchFrameElements[4] = am.GetElementWithName("squashedWalkAnim/squashedWalk4.png");
		crouchFrameElements[5] = am.GetElementWithName("squashedWalkAnim/squashedWalk1.png");
		crouchFrameElements[6] = am.GetElementWithName("squashedWalkAnim/squashedWalk2.png");
		crouchFrameElements[7] = am.GetElementWithName("squashedWalkAnim/squashedWalk3.png");
		
		bodySprite = new FSprite("walkAnim/walk0.png");
		bodySprite.scale = 0.5f;
		bodySprite.anchorY = 0;
		bodySprite.y = -200f;
		AddChild(bodySprite);
		
		headSprite = new FSprite(headImage);
		headSprite.y = 25f;
		headSprite.x -= 5f;
		headSprite.scale = 0.5f;
		headSprite.anchorY = 0;
		headSprite.rotation = -3f;
		AddChild(headSprite);
		
		Tween rotateOut = new Tween(headSprite, 0.3f, new TweenConfig().floatProp("rotation", 3f));
		Tween rotateIn = new Tween(headSprite, 0.3f, new TweenConfig().floatProp("rotation", -3f));
		chain = new TweenChain();
		chain.setIterations(-1);
		chain.append(rotateOut).append(rotateIn);
		Go.addTween(chain);
	}
	
	override public void HandleAddedToStage() {
		base.HandleAddedToStage();
		Futile.instance.SignalUpdate += UpdateAnimation;
		Futile.instance.SignalUpdate += UpdateJump;
	}
	
	override public void HandleRemovedFromStage() {
		base.HandleRemovedFromStage();
		Futile.instance.SignalUpdate -= UpdateAnimation;
		Futile.instance.SignalUpdate -= UpdateJump;
	}
	
	public Rect GetGlobalBoundsRect() {
		Vector2 position = bodySprite.container.LocalToGlobal(new Vector2(bodySprite.x, bodySprite.y));
		float crouchAdj = 1;
		if (isCrouched) crouchAdj = 0.5f;
		Rect rect = new Rect(position.x - 50f, position.y, 100f, 350f * crouchAdj);
		/*if (border != null) border.RemoveFromContainer();
		border = new TBorderLayer(rect.width, rect.height, 3f, Color.black);
		Vector2 rectPosition = bodySprite.container.GlobalToLocal(new Vector2(rect.x, rect.y));
		border.x = rectPosition.x;
		border.y = rectPosition.y;
		bodySprite.container.AddChild(border);*/
		return rect;
	}
	
	public void StartWalking() {
		if (isWalking) return;
		
		isWalking = true;
		
		chain.play();
	}
	
	public void StartCrouching() {
		if (isCrouched) return;
		
		
		isCrouched = true;
	}
	
	public void StopCrouching() {
		if (!isCrouched) return;
		
		headSprite.y = 35f;
		isCrouched = false;
	}
	
	public void StopWalking() {
		if (!isWalking) return;
		
		isWalking = false;
		bodySprite.element = frameElements[0];
		
		//chain.pause();
	}
	
	public void TurnAround() {
		if (isTurning) return;
		
		isTurning = true;
		
		float newScale = this.scaleX * -1;
		Go.to(this, 0.5f, new TweenConfig().floatProp("scaleX", newScale).setEaseType(EaseType.SineInOut).onComplete(DoneTurning));
	}
	
	public void DoneTurning(AbstractTween tween) {
		isTurning = false;	
	}
	
	public void Jump() {
		groundY = this.y;
		yVelocity = 1250f;
		isJumping = true;
	}
	
	public void UpdateAnimation() {
		if (!isWalking) return;
		
		float baseHeadPosition = 35f;
		if (isCrouched) baseHeadPosition -= 125f;
		
		timeSinceLastFrameChange += Time.fixedDeltaTime;
		
		if (timeSinceLastFrameChange > frameDuration) {
			timeSinceLastFrameChange -= frameDuration;
			
			frameIndex = (frameIndex + 1) % 8;
			
			if (frameIndex == 0 || frameIndex == 4) {
				headSprite.y = baseHeadPosition;	
			}
			else if (frameIndex == 1 || frameIndex == 5) {
				headSprite.y = baseHeadPosition - 13f;	
			}
			else if (frameIndex == 2 || frameIndex == 6) {
				headSprite.y = baseHeadPosition + 8f;
			}
			else if (frameIndex == 3 || frameIndex == 7) {
				headSprite.y = baseHeadPosition + 15f;	
			}
			
			if (!isCrouched) bodySprite.element = frameElements[frameIndex];
			else bodySprite.element = crouchFrameElements[frameIndex];
		}
	}
	
	public void UpdateJump() {
		if (!isJumping) return;
		
		yVelocity -= decelAmt;
		float newY = this.y + Time.fixedDeltaTime * yVelocity;
		
		if (newY < groundY) {
			newY = groundY;
			isJumping = false;
		}
		
		this.y = newY;
	}
}
