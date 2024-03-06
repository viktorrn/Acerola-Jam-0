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

	private Vector2 TargetPosition;
	public override void _Ready()
	{
		// Hide the mouse
		Target = GetNode<Player>("../Player");
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
	}

	public void SetTargetObject(CharacterBody2D target)
	{
		Target = target;
	}
}
