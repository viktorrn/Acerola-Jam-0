using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	public const float Speed = 200.0f;
	private float Stunned = 1.0f;
	private Area2D hitBox;

	private bool Alive = true;
	private bool TakeDamage = false;
	private Vector2 KnockBack = Vector2.Zero;
	
	private CharacterBody2D Target;
	private CharacterBody2D Player;
	private Vector2 TargetPosition;
	private float FrameDelta = 0.0f;

	public float TargetRange = 200.0f;
	public float AttackRange = 40.0f;

	public bool InAttackState = false;
	public bool AttackOnCooldown = false;
	private bool LookingLeft = false;

	[Export] public bool DebugDraw = true;

	private Vector2 DeisredVelocity = Vector2.Zero;

	private NavigationAgent2D navAgent;


	private Node2D spriteNode;

	private AnimationPlayer animPlayer;

	override public void _Ready()
	{
		Alive = true;
		Player = GetTree().Root.GetNode("world").GetNode("Player") as CharacterBody2D;
		TargetPosition = Position;
        navAgent = GetNode("NavAgent") as NavigationAgent2D;
		spriteNode = GetNode("Sprite") as Node2D;
		animPlayer = spriteNode.GetNode("AnimationPlayer") as AnimationPlayer;
		
    }
	// Get the gravity from the project settings to be synced with RigidBody nodes.

	public override void _PhysicsProcess(double delta)
	{
		FrameDelta = (float)delta;
		if(Alive){
			AliveState();
		} else {
			(GetNode("Control") as Control).Visible = true;
			DeisredVelocity = Vector2.Zero;
		}
				
		// check if we should stand still or not
		Velocity = Velocity.Lerp(DeisredVelocity,2*FrameDelta);
		
		MoveAndSlide();
		spriteNode.Scale = new Vector2( LookingLeft ? -1 : 1 , 1.0f);

		//MoveAndCollide(Velocity * FrameDelta);
		QueueRedraw();
	}

	public override void _Process(double delta)
	{
	}

public override void _Draw()
{
	DrawCircle(Position,10, new Color(1,0,0,0.01f));
}

    public void AliveState()
	{
		if(InAttackState) 
		{
		
			LookingLeft = (Target.GlobalPosition - Position).X > 0;
			
			return;
		}
		if(TakeDamage)
		{
			StunnedState();
			TakeDamage = false;
		} 
		else
		{
			FindTargetState();
		}
		
		// scale back stunned to 1 as it would 
		Stunned = Math.Clamp(Utils.Lerp(Stunned,0,0.5f*FrameDelta),0,1);
	}

	public void FindTargetState()
	{
		if(Target == null)
		{
			if(Position.DistanceTo(Player.GlobalPosition) < TargetRange)
			{
				navAgent.TargetPosition = Player.GlobalPosition;
				Target = Player;
			}
		} else {
			ChaseState();
		}
	}

	public void StunnedState()
	{
		Stunned = 1.0f;
		
	}

	public void ChaseState()
	{	
		if(!AttackOnCooldown && Position.DistanceTo(Target.GlobalPosition) < AttackRange)
		{
			AttackState();
			DeisredVelocity = Vector2.Zero;
			Velocity = Vector2.Zero;
		} else {
			navAgent.TargetPosition = Target.GlobalPosition;
			TargetPosition = navAgent.GetNextPathPosition();
			
			
			Vector2 direction = (TargetPosition - GlobalPosition).Normalized();
			DeisredVelocity = direction * Speed * (1-Stunned);
			LookingLeft = direction.X > 0;
		}
	}

	public void AttackState()
	{
		InAttackState = true;
		AttackOnCooldown = true;
		animPlayer.Play("Attack");
		GetNode("Handler")?.Call("Attack",Target);
	
	}

	public void AttackComplete()
	{
	
		InAttackState = false;
	}

	public void AttackCooldownComplete()
	{

		AttackOnCooldown = false;
	}

	public void IdleState()
	{
		Velocity = Vector2.Zero;
	}

	public void Kill()
	{
		Alive = false;
		//navAgent.
		// play death stuff
	}

	public void ApplyDamage(Vector2 forceDirection, float force)
	{

		TakeDamage = true;
		Velocity += forceDirection*force;
		Velocity = Velocity.Clamp(Vector2.Zero,new Vector2(300.0f,300.0f));
		Target = Player;
	}
}
