using Godot;
using System;

public partial class Biter : Node2D
{
	private Area2D hitBox;
	public int AttackRange = 40;
	public int LunghRange = 220;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		hitBox = GetNode("HitBox") as Area2D;
		hitBox.AddToGroup("Attack");
		hitBox.Monitorable = false;
		hitBox.Visible = false;
		hitBox.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public void Attack(CharacterBody2D target)
	{
		GetTree().CreateTimer(0.5f).Timeout += () => AttackActive(target);
		GetTree().CreateTimer(0.7f).Timeout += () => AttackComplete();
		GetTree().CreateTimer(1.2f).Timeout += () => GetParent()?.Call("AttackComplete");
		GetTree().CreateTimer(3.0f).Timeout += () => GetParent()?.Call("AttackCooldownComplete");
	}

	public void  AttackActive(CharacterBody2D target)
	{
		Vector2 direction = (target.GlobalPosition - GlobalPosition).Normalized();
		
		(hitBox as BiteAttack).ForceDirection = direction;
		
		hitBox.GlobalPosition = GlobalPosition + direction * Math.Clamp(target.GlobalPosition.DistanceTo(GlobalPosition), 0, AttackRange);
		hitBox.Monitorable = true;
		hitBox.Visible = true;
		hitBox.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
		GetParent<CharacterBody2D>().Velocity = direction * LunghRange;
	}

	public void AttackComplete()
	{
		hitBox.Monitorable = false;
		hitBox.Visible = false;
		hitBox.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
	}
}
