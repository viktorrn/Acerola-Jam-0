using Godot;
using System;

public partial class Spitter : Node2D
{
	[Export] public int MaxHealth = 8;

	[Export] public float Speed = 90.0f;


	[ExportCategory("Attack")]
	[Export] public float AttackRange = 80.0f;
	[Export] public float TargetRange = 200.0f;
	[Export] public int Damage = 5; 
	[Export] public float Force = 60;
	[Export] public float ProjectileSpeed = 400.0f;

	[ExportCategory("Attack Timings")]

	[Export] public float ForeSwing = 0.5f;

	[Export] public float ActiveFrames = 0.1f;
	[Export] public float NLagg = 0.5f;
	[Export] public float AttackOnCooldown = 1.0f;

	private Vector2 InitialVector;
	private Area2D hitBox;

	private PackedScene packedScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		packedScene = GD.Load<PackedScene>("res://Enemy/Spit.tscn");
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

		Projectile spit = (Projectile)packedScene.Instantiate();
		spit.GlobalPosition = GlobalPosition;
		spit.ForceDirection = direction;
		spit.Speed = ProjectileSpeed;
		spit.Damage = Damage;
		spit.Force = Force;
		GetTree().Root.GetNode("world").AddChild(spit);
	
	}

	public void AttackComplete()
	{
		
	}

	public float TargetAngle()
	{
		return (float)Math.PI/4 - GD.Randf()*(float)Math.PI/2;
	}
}
