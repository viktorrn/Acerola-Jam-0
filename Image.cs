using Godot;
using System;

public partial class Image : Sprite2D
{
	Noise noise = new FastNoiseLite();
	float Noise_i = 0;
	[Export] public float Shake = 1f;
	[Export] public float ShakeSpeed = 1f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Noise_i += ShakeSpeed * (float)delta;
		Offset = new Vector2(
			noise.GetNoise2D(Noise_i,1) * Shake, 
			noise.GetNoise2D(30,Noise_i) * Shake);
	}
}
