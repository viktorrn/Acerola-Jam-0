using Godot;
using System;

public partial class Bullet : CharacterBody2D
{
	public float Speed = 500;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		Vector2 moveDirection = new((float)Math.Cos(Rotation),(float)Math.Sin(Rotation));
		Velocity = moveDirection * Speed * (float)delta;
		var collision_data = MoveAndCollide(Velocity);

		
	}
}
