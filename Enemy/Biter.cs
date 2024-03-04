using Godot;
using System;

public partial class Biter : Node2D
{
	private Area2D hitBox;
	public int AttackRange = 40;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		hitBox = GetNode("HitBox") as Area2D;
		hitBox.AddToGroup("Attack");
		hitBox.Monitorable = false;
		hitBox.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public void Attack(CharacterBody2D target)
	{
	
		
		GetTree().CreateTimer(0.4f).Timeout += () => AttackActive(target);
		GetTree().CreateTimer(0.9f).Timeout += () => AttackComplete();
		GetTree().CreateTimer(1.2f).Timeout += () => GetParent().Call("AttackComplete");
		GetTree().CreateTimer(3.0f).Timeout += () => GetParent().Call("AttackCooldownComplete");
	}

	public void  AttackActive(CharacterBody2D target)
	{
		Vector2 direction = (target.GlobalPosition - GlobalPosition).Normalized();
		hitBox.GlobalPosition = GlobalPosition + direction * Math.Clamp(target.GlobalPosition.DistanceTo(GlobalPosition), 0, AttackRange);
		hitBox.Monitorable = true;
		hitBox.Visible = true;
	}

	public void AttackComplete()
	{
		hitBox.Monitorable = false;
		hitBox.Visible = false;
	}
}
