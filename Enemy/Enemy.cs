using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	public const float Speed = 130.0f;
	private float Stunned = 1.0f;
	private Area2D hitBox;

	private bool Alive = true;
	private bool TakeDamage = false;
	private Vector2 KnockBack = Vector2.Zero;
	
	private CharacterBody2D Target;
	private CharacterBody2D Player;
	private Vector2 TargetPosition;

	public Vector2 SpawnLocation;

	private float FrameDelta = 0.0f;

	public float TargetRange = 200.0f;
	public float AttackRange = 40.0f;

	public bool InAttackState = false;
	public bool AttackOnCooldown = false;
	private bool LookingLeft = false;

	private bool LostTarget = true;

	[Export] public bool DebugDraw = true;

	[Signal] public delegate void OnDiedEventHandler();

	private Vector2 DeisredVelocity = Vector2.Zero;

	private NavigationAgent2D navAgent;

	private Area2D alertArea;

	private Node2D spriteNode;

	private AnimationPlayer animPlayer;

	private float AngleToReach;

	private RayCast2D LOS;

	private Node2D Handler;
	override public void _Ready()
	{
		Alive = true;
		Player = GetTree().Root.GetNode("world").GetNode("Player") as CharacterBody2D;
		TargetPosition = Position;
        navAgent = GetNode("NavAgent") as NavigationAgent2D;
		spriteNode = GetNode("Sprite") as Node2D;
		animPlayer = spriteNode.GetNode("AnimationPlayer") as AnimationPlayer;
		alertArea = GetNode("AlertArea") as Area2D;
		
		Handler = GetNode("Handler") as Node2D;

		
		Health health = GetNode("HurtBox") as Health;
		health.OnHit += ApplyDamage;
		health.OnDied += Kill;

		LOS = GetNode("LineOfSight") as RayCast2D;
		LOS.Position = Vector2.Zero;


		
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
		Velocity = Velocity.Lerp(DeisredVelocity,4*FrameDelta);
		
		MoveAndSlide();
		//MoveAndCollide(Velocity * FrameDelta);
		spriteNode.Scale = new Vector2( LookingLeft ? -1 : 1 , 1.0f);

	}

	public override void _Process(double delta)
	{
		QueueRedraw();
	}

public override void _Draw()
{
	DrawCircle(Vector2.Zero, (float)Handler.Get("AttackRange"), new Color(1,0,0,0.1f));
}

    public void AliveState()
	{
		if(InAttackState) 
		{
			if(Target == null) return;
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
			if(Position.DistanceTo(Player.GlobalPosition) < (float)Handler.Get("TargetRange"))
			{
				LOS.TargetPosition = Player.GlobalPosition - Position;
				LOS.ForceRaycastUpdate();

				if(LOS.IsColliding()) return;

				AngleToReach = (Player.GlobalPosition - Position).Angle() - (float)Handler.Call("TargetAngle");
				Target = Player;
				LostTarget = false;
				(Target as Player).OnPlayerDied += () => {
					Target = null;
					LostTarget = true;
					DeisredVelocity = Vector2.Zero;
				};
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
		if(!AttackOnCooldown && Position.DistanceTo(Target.GlobalPosition) < (float)Handler.Get("AttackRange"))
		{
			AttackState();
			DeisredVelocity = Vector2.Zero;
			Velocity = Vector2.Zero;
		} else {
			float angle = (Player.GlobalPosition - Position).Angle();
			navAgent.TargetPosition = Player.GlobalPosition - new Vector2((float)Handler.Get("AttackRange"),0).Rotated(angle+AngleToReach);
			
			TargetPosition = navAgent.GetNextPathPosition();
			
			Vector2 direction = (TargetPosition - GlobalPosition).Normalized();
			
			DeisredVelocity = direction * (float)Handler.Get("Speed") * (1-Stunned);
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
		AngleToReach = (Player.GlobalPosition - Position).Angle() + (float)Handler.Call("TargetAngle");
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
		EmitSignal(nameof(OnDied));
		CallDeferred(nameof(DisableCollision));
		
		// Alter All Other WithIn Range
		


		//navAgent.
		// play death stuff
	}

	public void DisableCollision()
	{
		CollisionLayer = 0;
		CollisionMask = 0;
	}

	public void ApplyDamage(Vector2 forceDirection, float force)
	{

		TakeDamage = true;
		Velocity += forceDirection*force;
		Velocity = Velocity.Length() > 2000 ? Velocity.Normalized() * 200 : Velocity;
		Target = Player;
	}


}
