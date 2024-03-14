using Godot;
using System;

public partial class Biter : Node2D
{
	[Export] public int MaxHealth = 10;
	[Export] public int HeadMaxHealth = 5;

	[Export] public float Speed = 130.0f;

	[Export] public float KnockBackResistance = 0.4f;

	[ExportCategory("Attack")]
	[Export] public float TargetRange = 200.0f;
	
	[Export] public float AttackRange = 40;

	[Export] public int AttackReach = 10;

	[Export] public int LunghRange = 180;

	

	[Export] public int Damage = 2;
	[Export] public float Force = 60;
	
	[ExportCategory("Attack Timings")]

	[Export] public float ForeSwing = 0.5f;

	[Export] public float ActiveFrames = 0.2f;

	[Export] public float NLagg = 0.4f;

	[Export] public float AttackOnCooldown = 2.0f;

	bool attackIsActive = false;

	private Player target;
	
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
		target = target as Player;
		GetTree().CreateTimer(ForeSwing/2).Timeout += () => {
			
			InitialVector = target.GlobalPosition - GlobalPosition;

			};
		GetTree().CreateTimer(ForeSwing).Timeout += () => AttackActive(target);
		GetTree().CreateTimer(ForeSwing+ActiveFrames).Timeout += () => AttackComplete();
		GetTree().CreateTimer(ForeSwing+ActiveFrames+NLagg).Timeout += () => GetParent()?.Call("AttackComplete");
		GetTree().CreateTimer(ForeSwing+ActiveFrames+NLagg+AttackOnCooldown).Timeout += () => GetParent()?.Call("AttackCooldownComplete");
	}

	public void  AttackActive(CharacterBody2D target)
	{
		if(target == null) return;
		attackIsActive = true;
		Vector2 direction = (target.GlobalPosition - GlobalPosition).Normalized();
		direction = InitialVector.Normalized();	




		(hitBox as BiteAttack).ForceDirection = direction;
		
		hitBox.GlobalPosition = GlobalPosition + direction * AttackReach;
		hitBox.Rotation = direction.Angle() + (float)Math.PI;
		hitBox.Monitorable = true;
		hitBox.Visible = true;
		hitBox.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
		GetParent<Enemy>().Velocity = direction * LunghRange;
	}

   

    public void AttackComplete()
	{
		if(hitBox == null) return;
		hitBox.Monitorable = false;
		hitBox.Visible = false;
		hitBox.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
		attackIsActive = false;
	}

	public float TargetAngle()
	{
		return (float)Math.PI/2 - GD.Randf()*(float)Math.PI;
	}

	private PackedScene DeadBiter = GD.Load<PackedScene>("res://Enemy/Biter/DeadBiter.tscn");
	public void OnDied(Vector2 scale, Vector2 velocity){
		
		Dead deadBiter = (Dead)DeadBiter.Instantiate();
		deadBiter.GlobalPosition = GlobalPosition;
		deadBiter.Scale = scale;
		deadBiter.Velocity = velocity;
		
		
		GetTree().Root.GetNode(Utils.WorldPath).CallDeferred("add_child",deadBiter);
		QueueFree();
	}


}
