using Godot;
using System;

public partial class PlayerCamera : Camera2D
{

	[ExportCategory("Target")]
	[Export] public CharacterBody2D Target;

	[Export] public bool PositionMode = true;
	

	[ExportCategory("Smoothing")]
	[Export] public bool EnableSmoothing = true;
	[Export] public int SmoothingDistance = 12;

	[ExportCategory("Shake")]

	[Export] public float ShakeStrength = 20.0f;
	[Export] public float ShakeSpeed = 0.2f;
	[Export] public float ShakeDecay = 0.25f;

	private FastNoiseLite noise = new();
	private float Noise_i = 0;
	private float Shake = 100.0f;

	private bool PriorityShake = false;

	private Vector2 TargetPosition;
	public override void _Ready()
	{
		// Hide the mouse
		Target = GetNode<Player>("../Player");
		
		noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		// Follow the player
		if(EnableSmoothing)
		{
			float weight = (float)SmoothingDistance / 100;
			if(PositionMode)
			{
				TargetPosition = (Vector2)Target.Call("FocusPosition");
				GlobalPosition = GlobalPosition.Lerp(TargetPosition,weight);
			} else
			{
				GlobalPosition = GlobalPosition.Lerp(Target.GlobalPosition,weight);
			}
		}else
		{
			GlobalPosition = Target.GlobalPosition;
		}

		Shake = Utils.Lerp(Shake,0,ShakeDecay * (float)delta);
		Noise_i += ShakeSpeed * (float)delta;
		// Shake the camera
		Offset = new Vector2( 
			noise.GetNoise2D(1,Noise_i)*Shake, 
			noise.GetNoise2D(100,Noise_i)*Shake
			);
		
		if(Shake < 0.01f)
		{
			PriorityShake = false;
		}
	}

	public void ShakeCamera(float strength, float speed = 8, float decay = 8, bool priority = false)
	{
		if(PriorityShake) return;
		ShakeSpeed = speed;
		ShakeDecay = decay;
		Shake = strength;
		PriorityShake = priority;
	}

	public void SetTargetObject(CharacterBody2D target)
	{
		Target = target;
	}
}
