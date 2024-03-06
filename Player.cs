using Godot;
using System;
using System.Linq;

public partial class Player : CharacterBody2D
{
	[ExportCategory("Camera")]

	[Export] public float CameraOffset = 40;
	[Export] public float WeaponSnapSpeed = 6;


	[ExportCategory("Movement")]
	[Export] public float Speed = 200.0f;
	[Export] public float Acceleration = 6.0f;

	[Export] public float ADSSlowDown = 0.4f;


	[ExportCategory("Health")]
	[Export] public int Health = 4;
	[Export] public int MaxHealth = 4; 


	public Vector2 LookVector;
	public Vector2 DesiredLookVector;

	private System.Collections.Generic.List<CharacterBody2D> itemsInRange = new();


	private Node2D head;
	private Node2D body;
	private Node2D hand;
	private Node2D legs;
	private CharacterBody2D weapon;
	private Health hurtBox;
	private Area2D pickUpBox;

	

	

	private int itemIndex = 0;
	private Godot.Collections.Array<CharacterBody2D> inventory;
	public override void _Ready()
	{
		head = GetNode("Head") as Node2D;
		body = GetNode("Body") as Node2D;
		hand = GetNode("Hand") as Node2D;
		legs = GetNode("Legs") as Node2D;
		hurtBox = GetNode("Hurtbox") as Health;
		pickUpBox = GetNode("PickUp") as Area2D;
		pickUpBox.BodyEntered += OnPickUpBoxBodyEntered;
		pickUpBox.BodyExited += OnPickUpBoxExited;



		hurtBox.SetUpHealth(MaxHealth);
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
		
		DesiredLookVector = GetGlobalMousePosition() - head.GlobalPosition;
		LookVector = LookVector.Lerp(DesiredLookVector, (float)delta*WeaponSnapSpeed);

		
		
		if(inventory[itemIndex] == null) return;
		
		if(Input.IsActionJustPressed("LMB")){
			try
			{
				Node2D weapon = hand.GetNode(inventory[itemIndex].Get("WeaponName").ToString()) as Node2D;
				int result = (int)inventory[itemIndex]?.Call("FireBullet",weapon.GlobalPosition,LookVector);
				if(result == 0)
				{
				GD.Print("Out of ammo");
				}
			}
			catch(Exception e)
			{
				
			}
		}
    }

	private void DropCurrentHeldWeapon(int index){
		if(inventory[index] is null) return;
		inventory[index].GlobalPosition = GlobalPosition;;
		inventory[index]?.Call("Drop");
		inventory[index] = null;
	}

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public override void _PhysicsProcess(double deltaTime)
	{
		

		RotateHead();
		RotateWeapon(deltaTime);
		DisplayWeapon();


		float SlowDown = 0;
		Vector2 direction = Input.GetVector("left", "right", "up", "down");

		if(inventory[itemIndex] != null)
		{
			inventory[itemIndex].Position = hand.GlobalPosition;
			SlowDown = Input.IsActionPressed("RMB") ? ADSSlowDown : SlowDown;
		}
		 
		Vector2 velocity = direction * Speed * (1-SlowDown);

		velocity.Normalized();
		Velocity = Velocity.Lerp(velocity, (float)deltaTime * Acceleration);
		MoveAndSlide();
		//var collisionData = MoveAndCollide(Velocity);
	}



	private void RotateHead(){

		double angle = LookVector.Angle();

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

	private void RotateWeapon(double deltaTime)
	{
		double angle = Math.Atan2(LookVector.Y,LookVector.X);
		hand.Scale = new Vector2(1,LookVector.X < 0 ? -1 : 1);
		hand.Rotation = (float)angle;
	

		if(inventory[itemIndex] == null) return;
		
		Vector2 WeaponPosition = Input.IsActionPressed("RMB") ? Vector2.Zero : new(-2,5);
		Node2D Weapon = hand.GetNode(inventory[itemIndex].Get("WeaponName").ToString()) as Node2D;
		Weapon.Position =  Weapon.Position.Lerp(WeaponPosition, 0.5f);

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
		// could be optimized
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

	public void ApplyDamage(Vector2 forceDirection, float force)
	{
		Velocity = forceDirection * force;
	}

	public Vector2 FocusPosition()
	{
		float Offset = CameraOffset;
		if(inventory[itemIndex] != null)
		{	
			Offset = Input.IsActionPressed("RMB") ? (float)inventory[itemIndex].Get("ADSRange") : Offset;
		}
		return GlobalPosition + LookVector.Normalized() * Math.Clamp(LookVector.Length()/2 , 0, Offset);
	}

}
