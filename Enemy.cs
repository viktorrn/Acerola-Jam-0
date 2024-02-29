using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	public const float Speed = 150.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.

	public override void _PhysicsProcess(double delta)
	{
		Player player = GetParent().GetNode("Player") as Player;		
		Vector2 velocity = Velocity;

		Vector2 direction = player.Position - Position;

		if(direction.Length() > 30)
		{
			velocity = direction.Normalized()*Speed*(float)delta;
		} else {
			velocity = Vector2.Zero;
		}
		
		Velocity = velocity;
		MoveAndCollide(Velocity);
		
	}
}
