using Godot;
using System;

public partial class Biter : Node2D
{
	[Export] public int MaxHealth = 10;
	[Export] public int HeadMaxHealth = 5;

	[Export] public float Speed = 130.0f;

	[ExportCategory("Attack")]
	[Export] public float TargetRange = 200.0f;
	
	[Export] public float AttackRange = 40;

	[Export] public int AttackReach = 10;

	[Export] public int LunghRange = 220;

	

	[Export] public int Damage = 5;
	[Export] public float Force = 60;
	
	[ExportCategory("Attack Timings")]

	[Export] public float ForeSwing = 0.5f;

	[Export] public float ActiveFrames = 0.2f;

	[Export] public float NLagg = 0.5f;

	[Export] public float AttackOnCooldown = 3.0f;
	
	private Area2D hitBox;
	private Vector2 InitialVector;
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
		InitialVector = target.GlobalPosition - GlobalPosition;
		GetTree().CreateTimer(ForeSwing).Timeout += () => AttackActive(target);
		GetTree().CreateTimer(ForeSwing/2).Timeout += () => {InitialVector = target.GlobalPosition - GlobalPosition;};
		GetTree().CreateTimer(ForeSwing+ActiveFrames).Timeout += () => AttackComplete();
		GetTree().CreateTimer(ForeSwing+ActiveFrames+NLagg).Timeout += () => GetParent()?.Call("AttackComplete");
		GetTree().CreateTimer(ForeSwing+ActiveFrames+NLagg+AttackOnCooldown).Timeout += () => GetParent()?.Call("AttackCooldownComplete");
	}

	public void  AttackActive(CharacterBody2D target)
	{
		Vector2 direction = (target.GlobalPosition - GlobalPosition).Normalized();
		direction = InitialVector.Normalized();	




		(hitBox as BiteAttack).ForceDirection = direction;
		
		hitBox.GlobalPosition = GlobalPosition + direction * AttackReach;
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

	public float TargetAngle()
	{
		return (float)Math.PI/2 - GD.Randf()*(float)Math.PI;
	}


}
