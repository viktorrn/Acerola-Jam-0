using Godot;
using System;

public partial class Projectile : CharacterBody2D
{
	public float Speed = 500;
	public int Damage = 10;

	public float Force = 1;
	public Vector2 ForceDirection;
	private PackedScene Sparks = GD.Load<PackedScene>("res://Effects/WallHit.tscn");
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("Attack");
		//ForceDirection = new((float)Math.Cos(Rotation),(float)Math.Sin(Rotation));
		GetTree().CreateTimer(5).Timeout += QueueFree;
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
			
			// create sparks
			GpuParticles2D sparks = (GpuParticles2D)Sparks.Instantiate();
			sparks.GlobalPosition = GlobalPosition;
			sparks.Rotation = (float)ForceDirection.Angle() + (float)Math.PI;
			GetTree().Root.GetNode(Utils.WorldPath).CallDeferred("add_child",sparks);
			
			QueueFree();
		}		
	}

	public int HitTarget(Health target)
	{
		GetNode("HitManager").Call("HitTarget",target);
		Speed = 0;
		return Damage;
	}
}
