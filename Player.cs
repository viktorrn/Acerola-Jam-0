using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float BulletSpeed = 500.0f;
	public Vector2 LookVector;

	private PackedScene bulletScene;
	private Node2D head;
	private Node2D body;
	private Node2D hand;

	public override void _Ready()
	{
		head = GetNode("Head") as Node2D;
		body = GetNode("Body") as Node2D;
		hand = GetNode("Hand") as Node2D;
		bulletScene = GD.Load<PackedScene>("res://Bullet.tscn");
	}

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public override void _PhysicsProcess(double deltaTime)
	{
		
		Vector2 velocity = Velocity;
		
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("left", "right", "up", "down");
		velocity.X = direction.X * Speed;
		velocity.Y = direction.Y * Speed;

		velocity.Normalized();
		Velocity = velocity*(float)deltaTime;
		var collisionData = MoveAndCollide(Velocity);

		LookVector = GetGlobalMousePosition() - head.GlobalPosition;
		RotateHead();
		RotateWeapon();
		
		if(Input.IsActionJustPressed("LMB")){
			FireBullet();
		}

	}

	private void RotateHead(){

		double angle = Math.Atan2(LookVector.Y,LookVector.X);
		var headSprite = head.GetNode("Sprite2D") as Sprite2D;
		if( LookVector.X < 0 )
		{
			angle += Math.PI;
			angle = Math.Clamp(angle, -Math.PI/3, 5*Math.PI/3 );

		} else {
		
			angle = Math.Clamp(angle, -Math.PI/3, Math.PI/3);
		
		}
		headSprite.FlipH = LookVector.X < 0;
		head.Rotation = (float)angle;	
	}

	private void RotateWeapon()
	{
		double angle = Math.Atan2(LookVector.Y,LookVector.X);
		hand.Scale = new Vector2(1,LookVector.X < 0 ? -1 : 1);
		hand.Rotation = (float)angle;
	}

	private void FireBullet(){
		double lookAngle = Math.Atan2(LookVector.Y,LookVector.X);
		Bullet bulletInstance = bulletScene.Instantiate() as Bullet;
		bulletInstance.Position = Position + new Vector2((float)Math.Cos(lookAngle),(float)Math.Sin(lookAngle))*10;
		bulletInstance.Rotation = (float)lookAngle;
		GetTree().Root.CallDeferred("add_child",bulletInstance);
	}

	public void Kill(){
		GetTree().ReloadCurrentScene();
	}
}
