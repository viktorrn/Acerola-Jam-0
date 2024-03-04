using Godot;
using System;

public partial class BiteAttack : Area2D
{
	public float Force = 0;
	public Vector2 ForceDirection = Vector2.Zero;
	public override void _Ready()
	{
		AddToGroup("Attack");
	}
	// Called when the node enters the scene tree for the first time.
	public int HitTarget(Node2D enemy)
	{
		GD.Print("Bite Attack");
		return 5;
	}

	public float ApplyDamage(Vector2 forceDirection, float force)
	{
		GD.Print("Apply Damage");
		return 0;
	}
}
