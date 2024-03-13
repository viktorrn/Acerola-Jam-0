using Godot;
using System;

public partial class Logo : Sprite2D
{
	private Noise noise = new FastNoiseLite();
	private float Noise_i = 0;
	[Export] public float Shake = 0.01f;
	[Export] public float ShakeSpeed = 1f;

	// Called when the node enters the scene tree for the first time.

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 pos = new Vector2(970,409);
		Noise_i += ShakeSpeed * (float)delta;
		Position = new Vector2(
			pos.X + noise.GetNoise2D(Noise_i,1) * Shake, 
			pos.Y + noise.GetNoise2D(30,Noise_i) * Shake);
		
	}
}
