using Godot;
using System;
using System.Linq;

public partial class Player : CharacterBody2D
{
	[ExportCategory("Camera")]

	[Export] public float CameraOffset = 40;
	[Export] public float WeaponSnapSpeed = 6;


	[ExportCategory("Movement")]
	[Export] public float Speed = 120.0f;
	[Export] public float SprintSpeed = 180.0f;
	[Export] public float Acceleration = 6.0f;

	[Export] public float ADSSlowDown = 0.4f;


	[ExportCategory("Health")]
	[Export] public int Health = 4;
	[Export] public int MaxHealth = 4; 

	[ExportCategory("Stamina")]
	[Export] public float Stamina = 4;
	[Export] public float MaxStamina = 4;

	[Export] public float StaminaRegenCooldown = 1.0f;

	public bool StaminaCanRegen = true;


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

	private ProgressBar healthBar;
	private ProgressBar staminaBar;

	private Timer staminaRegenTimer;

	private bool SwappingItem = false;

	[Signal] public delegate void OnPlayerDiedEventHandler();

	

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
		hurtBox.OnDied += Kill;
		hurtBox.OnHit += ApplyDamage;
		
		inventory = new() { null, null, null, null };
		
		healthBar = GetNode("Control").GetNode("Health") as ProgressBar;
		healthBar.MaxValue = MaxHealth;
		healthBar.Value = Health;
		hurtBox.OnHealthChanged += (int health) => healthBar.Value = health;

		Stamina = MaxStamina;
		staminaBar = GetNode("Control").GetNode("Stamina") as ProgressBar;
		staminaBar.MaxValue = MaxStamina;
        staminaRegenTimer = new Timer
        {
            OneShot = false
        };
		AddChild(staminaRegenTimer);
		staminaRegenTimer.Timeout += () => StaminaCanRegen = true;

    }

    public override void _Process(double delta)
    {	

		foreach(var i in itemsInRange)
		{
			i?.Call("HidePrompt");
		}

		DesiredLookVector = GetGlobalMousePosition() - head.GlobalPosition;
		LookVector = LookVector.Lerp(DesiredLookVector, (float)delta*WeaponSnapSpeed);

		int oldItemIndex = itemIndex;

		if(!SwappingItem)
		{
			if(Input.IsActionJustPressed("1"))
			{
				itemIndex = 0;
			} else if(Input.IsActionJustPressed("2"))
			{
				itemIndex = 1;
			} else if(Input.IsActionJustPressed("3"))
			{
				itemIndex = 2;
				
			} else if(Input.IsActionJustPressed("4"))
			{
				itemIndex = 3;
			}
		}

		if(itemIndex != oldItemIndex)
		{
			SwappingItem = true;
			GetTree().CreateTimer(0.4f).Timeout += () => SwappingItem = false;
		}
		


		if(itemsInRange.Count > 0)
		{
		
			CharacterBody2D item = itemsInRange.First();
			item.Call("ShowPrompt");
			
			if(Input.IsActionJustPressed("Interact"))
			{

				int type = (int)(item != null ? item.Get("WeaponType") : -1);
				if(type == -1)
				{
					return;
				}
				
				DropCurrentHeldWeapon(type);

				inventory[type] = item;
				inventory[type].Call("PickUp");
				
				itemIndex = type;
				SwappingItem = true;
				GetTree().CreateTimer(0.4f).Timeout += () => SwappingItem = false;

				itemsInRange.Remove(item);
			}
		}


		if(inventory[itemIndex] == null || SwappingItem) return;

		if(Input.IsActionJustPressed("Drop")){
			DropCurrentHeldWeapon(itemIndex);
		}


		if(Input.IsActionJustPressed("Reload")){
			GD.Print("Reloading");
			inventory[itemIndex]?.Call("Reload");
		}
		
		if(Input.IsActionPressed("Shift")) return;
		
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
		
		
		RotateBody();
		RotateWeapon(deltaTime);
		DisplayWeapon();
		if(StaminaCanRegen && Stamina < MaxStamina)
		{
			Stamina += (float)deltaTime;
		}

		float SlowDown = 0;
		float MoveSpeed = Speed;
		Vector2 direction = Input.GetVector("left", "right", "up", "down");

		if(inventory[itemIndex] != null)
		{
			inventory[itemIndex].Position = hand.GlobalPosition;
			SlowDown = Input.IsActionPressed("RMB") ? ADSSlowDown : SlowDown;
		}

		if(Input.IsActionPressed("Shift") && Stamina > 0.5f)
		{
			Stamina -= (float)deltaTime;
			MoveSpeed = SprintSpeed;
			StaminaCanRegen = false;
			staminaRegenTimer.Stop();
		}
		
		/*if(Stamina <= 0.5f)
		{
			MoveSpeed *= 0.6f;
		}*/

		if(Input.IsActionJustReleased("Shift"))
		{
			staminaRegenTimer.Start(StaminaRegenCooldown);
		}


		Vector2 velocity = direction * MoveSpeed * (1-SlowDown);

		velocity.Normalized();
		Velocity = Velocity.Lerp(velocity, (float)deltaTime * Acceleration);
		MoveAndSlide();
		//var collisionData = MoveAndCollide(Velocity);
		staminaBar.Value = Stamina;
	}



	private void RotateBody(){

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
		Sprite2D bombSprite = body.GetNode("NestBomb") as Sprite2D;

		Sprite2D legSprite = legs.GetNode("Sprite2D") as Sprite2D;
		Sprite2D headSprite = head.GetNode("Sprite2D") as Sprite2D;

		bombSprite.Visible = false;

		int frame = LookVector.Y > 0 ? 0 : 1;
		bodySprite.Frame = frame;
		legSprite.Frame = frame;
		headSprite.Frame = frame;

		head.Scale = new Vector2(LookVector.X < 0 ? -1 : 1, 1);
		head.Rotation = (float)angle;	

		if(inventory[3] != null && itemIndex != 3)
		{
			bombSprite.Visible = true;
			bombSprite.ZIndex = frame; //Cheese as it will produce the same result even though the logic isnt direcly correct
		}
	}

	private void RotateWeapon(double deltaTime)
	{
		double angle = Math.Atan2(LookVector.Y,LookVector.X);
		hand.Scale = new Vector2(1,LookVector.X < 0 ? -1 : 1);
		hand.Rotation = (float)angle;
	

		if(inventory[itemIndex] == null) return;
		
		Vector2 WeaponPosition = Input.IsActionPressed("RMB") && !Input.IsActionPressed("Shift") ? Vector2.Zero : new(-2,5);
		Node2D Weapon = hand.GetNode(inventory[itemIndex].Get("WeaponName").ToString()) as Node2D;
		Weapon.Position =  Weapon.Position.Lerp(WeaponPosition, 0.5f);

	}

	public void Kill(){
		DropCurrentHeldWeapon(itemIndex);
		GlobalPosition = new Vector2(-85,310);
		EmitSignal("OnPlayerDied");
		hurtBox.Heal(MaxHealth);
		Stamina = MaxStamina;
	}

	private void OnPickUpBoxBodyEntered(Node body){
		if(inventory.Contains(body) || !(bool)body.Get("CanBePickedUp")) return;
		itemsInRange.Add(body as CharacterBody2D);
	}

	private void OnPickUpBoxExited(Node body){
		itemsInRange?.Remove(body as CharacterBody2D);
		body?.Call("HidePrompt");
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
			case "NestBomb":
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
		if(inventory[itemIndex] != null && itemIndex != 3 && !Input.IsActionPressed("Shift"))
		{	
			Offset = Input.IsActionPressed("RMB") ? (float)inventory[itemIndex].Get("ADSRange") : Offset;
		}
		return GlobalPosition + LookVector.Normalized() * Math.Clamp(LookVector.Length()/2 , 0, Offset);
	}

}
