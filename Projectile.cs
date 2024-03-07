using Godot;
using System;

public partial class Projectile : CharacterBody2D
{
	public float Speed = 500;
	public int Damage = 10;

	public float Force = 1;
	public Vector2 ForceDirection = Vector2.Zero;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Attack");
		ForceDirection = new((float)Math.Cos(Rotation),(float)Math.Sin(Rotation));
		GetTree().CreateTimer(5).Connect("timeout", new Callable(this,nameof(RemoveSelf)));
	}

	private void RemoveSelf()
	{
		QueueFree();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		
		Velocity = ForceDirection * Speed * (float)delta;
		var collision_data = MoveAndCollide(Velocity);
		if(collision_data == null){
			return;
		}
		
		if(collision_data.GetCollider() is TileMap || collision_data.GetCollider() is StaticBody2D){
			QueueFree();
		}		
	}

	public int HitTarget(Health target)
	{
		GetNode("HitManager").Call("HitTarget",target);
		return Damage;
	}
}
