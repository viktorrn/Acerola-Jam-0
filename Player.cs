using Godot;
using System;
using System.Linq;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float BulletSpeed = 500.0f;
	public Vector2 LookVector;

	private PackedScene bulletScene;
	private Node2D head;
	private Node2D body;
	private Node2D hand;
	private Node2D legs;
	private CharacterBody2D weapon;
	private Area2D hurtBox;
	private Area2D pickUpBox;
	private System.Collections.Generic.List<CharacterBody2D> itemsInRange = new();

	private int itemIndex = 0;
	private Godot.Collections.Array<CharacterBody2D> inventory;
	public override void _Ready()
	{
		head = GetNode("Head") as Node2D;
		body = GetNode("Body") as Node2D;
		hand = GetNode("Hand") as Node2D;
		legs = GetNode("Legs") as Node2D;
		hurtBox = GetNode("Hurtbox") as Area2D;
		pickUpBox = GetNode("PickUp") as Area2D;
		pickUpBox.BodyEntered += OnPickUpBoxBodyEntered;
		pickUpBox.BodyExited += OnPickUpBoxExited;

		inventory = new() { null, null, null, null };
		
		
	}

    public override void _Process(double delta)
    {	
		foreach(var i in itemsInRange)
		{
			i?.Call("HidePrompt");
		}

		if(itemsInRange.Count > 0)
		{
		
			CharacterBody2D item = itemsInRange.First();
			item.Call("ShowPrompt");
			
			if(Input.IsActionJustPressed("Interact"))
			{

				int type = (int)(item != null ? item.Get("weaponType") : -1);
				if(type == -1)
				{
					return;
				}
				
				DropCurrentHeldWeapon(itemIndex);

				inventory[type] = item;
				inventory[type].Call("PickUp");
				
				itemsInRange.Remove(item);
			}
		}
		
		if(Input.IsActionJustPressed("Drop")){
			DropCurrentHeldWeapon(itemIndex);
		}

		if(Input.IsActionJustPressed("Reload")){
			GD.Print("Reloading");
			inventory[itemIndex]?.Call("Reload");
		}
		
    }

	private void DropCurrentHeldWeapon(int index){
		if(inventory[index] is null) return;
		inventory[index]?.Call("Drop");
		inventory[index] = null;
	}

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public override void _PhysicsProcess(double deltaTime)
	{
		if(inventory[itemIndex] != null)
		{
			inventory[itemIndex].Position = hand.GlobalPosition;
		}

		LookVector = GetGlobalMousePosition() - head.GlobalPosition;
		RotateHead();
		RotateWeapon();

		if(Input.IsActionJustPressed("LMB")){
			try
			{
			int result = (int)inventory[itemIndex]?.Call("FireBullet",head.GlobalPosition,LookVector);
			if(result == 0)
			{
				GD.Print("Out of ammo");
			}
			}
			catch(Exception e)
			{
				
			}

		}


		DisplayWeapon();


		Vector2 velocity = Velocity;
		Vector2 direction = Input.GetVector("left", "right", "up", "down");

		velocity.X = direction.X * Speed;
		velocity.Y = direction.Y * Speed;

		velocity.Normalized();
		Velocity = velocity;
		MoveAndSlide();
		//var collisionData = MoveAndCollide(Velocity);


	}

	private void RotateHead(){

		double angle = Math.Atan2(LookVector.Y,LookVector.X);

		if( LookVector.X < 0 )
		{
			angle += Math.PI;
			//angle = Math.Clamp(angle, -Math.PI/3, 5*Math.PI/3 );
			if( LookVector.Y < 0 )
			{
				angle = Math.Clamp(angle, 0, Math.PI/3 );
			} else {
				angle = Math.Clamp(angle, 5*Math.PI/3, 2*Math.PI );
			}

		} else {
		
			angle = Math.Clamp(angle, -Math.PI/3, Math.PI/3);
		}
		
		body.Scale = new Vector2(LookVector.X < 0 ? -1 : 1, 1);
		legs.Scale = new Vector2(LookVector.X < 0 ? -1 : 1, 1);
		
		
		Sprite2D bodySprite = body.GetNode("Sprite2D") as Sprite2D;
		Sprite2D legSprite = legs.GetNode("Sprite2D") as Sprite2D;
		Sprite2D headSprite = head.GetNode("Sprite2D") as Sprite2D;

		int frame = LookVector.Y > 0 ? 0 : 1;
		bodySprite.Frame = frame;
		legSprite.Frame = frame;
		headSprite.Frame = frame;

		head.Scale = new Vector2(LookVector.X < 0 ? -1 : 1, 1);
		head.Rotation = (float)angle;	
	}

	private void RotateWeapon()
	{
		double angle = Math.Atan2(LookVector.Y,LookVector.X);
		hand.Scale = new Vector2(1,LookVector.X < 0 ? -1 : 1);
		hand.Rotation = (float)angle;
	}

	public void Kill(){
		GetTree().ReloadCurrentScene();
	}

	private void OnPickUpBoxBodyEntered(Node body){
		if(inventory.Contains(body)) return;
		itemsInRange.Add(body as CharacterBody2D);
	}

	private void OnPickUpBoxExited(Node body){
		itemsInRange?.Remove(body as CharacterBody2D);
		body.Call("HidePrompt");
	}
	

	private void DisplayWeapon(){
		hand.GetChildren().Cast<Node>().ToList().ForEach(c => (c as Node2D).Visible = false);

		if(inventory[itemIndex] == null) return;
		(hand.GetNode(inventory[itemIndex].Get("WeaponName").ToString()) as Node2D).Visible = true;
		switch(inventory[itemIndex].Get("WeaponName").ToString())
		{
			case "Sniper":
				break;
			case "Shotgun":
				break;
		}
	}
}
