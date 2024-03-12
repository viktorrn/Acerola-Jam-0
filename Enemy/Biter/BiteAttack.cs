using Godot;
using System;

public partial class BiteAttack : Area2D
{
	public float Force = 80;
	public Vector2 ForceDirection = Vector2.Zero;
	private int Damage = 2;
	public override void _Ready()
	{
		AddToGroup("Attack");
	}
	// Called when the node enters the scene tree for the first time.
	public int HitTarget(Node2D enemy)
	{
		return Damage;
	}

}
