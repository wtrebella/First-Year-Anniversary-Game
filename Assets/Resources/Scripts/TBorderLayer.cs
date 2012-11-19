using UnityEngine;
using System.Collections;

public class TBorderLayer : FContainer {
	public float thickness;
	public Color color;
	public float width;
	public float height;
	
	public TBorderLayer(float width, float height, float thickness, Color color) {
		this.thickness = thickness;
		this.color = color;
		this.width = width;
		this.height = height;
		
		MakeBorder();
	}
	
	void MakeBorder() {
		if (width < thickness * 2f || height < thickness * 2f) {
			FSprite square = SquareMaker.Square(width, height);
			square.anchorX = square.anchorY = 0.0f;
			square.color = color;
			AddChild(square);
			return;
		}
		
		FSprite left = SquareMaker.Square(thickness, height);
		FSprite right = SquareMaker.Square(thickness, height);
		FSprite bottom = SquareMaker.Square(width - thickness * 2f, thickness);
		FSprite top = SquareMaker.Square(width - thickness * 2f, thickness);
		
		left.anchorX = 0.0f;
		left.anchorY = 0.0f;
		right.anchorX = 1.0f;
		right.anchorY = 0.0f;
		bottom.anchorX = 0.0f;
		bottom.anchorY = 0.0f;
		top.anchorX = 0.0f;
		top.anchorY = 1.0f;
		
		left.x = 0.0f;
		left.y = 0.0f;
		right.x = width;
		right.y = 0.0f;
		bottom.x = thickness;
		bottom.y = 0.0f;
		top.x = thickness;
		top.y = height;
		
		left.color = right.color = bottom.color = top.color = color;
		
		AddChild(left);
		AddChild(right);
		AddChild(top);
		AddChild(bottom);
	}
}
