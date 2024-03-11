using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

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

	[Export] public float AlertDistance = 200.0f;

	public bool StaminaCanRegen = true;


	public Vector2 LookVector;
	public Vector2 DesiredLookVector;

	private List<CharacterBody2D> itemsInRange = new();
	private List<CharacterBody2D> interactionsInRange = new();

	public bool IsAlive = true;
	private Node2D head;
	private Node2D body;
	private Node2D hand;
	private Node2D legs;
	private CharacterBody2D weapon;
	private Health hurtBox;
	private Area2D pickUpBox;
	private Area2D interactionBox;

	private ProgressBar healthBar;
	private ProgressBar staminaBar;

	private ProgressBar AmmoBar;

	private Label MagLabel;

	private Timer staminaRegenTimer;

	private bool SwappingItem = false;

	[Signal] public delegate void OnPlayerDiedEventHandler();
	[Signal] public delegate void OnPlayerFireEventHandler();

	PackedScene Sniper = GD.Load<PackedScene>("res://Items/Sniper/Sniper.tscn");
	PackedScene Shotgun = GD.Load<PackedScene>("res://Items/Shotgun/Shotgun.tscn");
	PackedScene Revolver = GD.Load<PackedScene>("res://Items/Revolver/Revolver.tscn");

	private Area2D AlertArea;

	private float TimePassed = 0.0f;
	

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
		interactionBox = GetNode("Interact") as Area2D;
		interactionBox.BodyEntered += OnInteractionBodyEnterd;
		interactionBox.BodyExited += OnInteractionBodyExited;

	


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

	
		MagLabel = GetNode("Control").GetNode("Mag") as Label;
		
		MagLabel.Visible = false;

		AlertArea = new Area2D
        {
            CollisionMask = 0b100000,
            Monitorable = true,
            Name = "AlertArea"
        };

		CollisionShape2D shape = new()
        {
            Shape = new CircleShape2D { Radius = AlertDistance }
        };
     
		AlertArea.AddChild(shape);

        AddChild(AlertArea);
		

    }

	public void Spawn()
	{
		BaseGun sniper = (BaseGun)Sniper.Instantiate();
		BaseGun shotgun = (BaseGun)Shotgun.Instantiate();
		BaseGun revolver = (BaseGun)Revolver.Instantiate();
		
		AddChild(sniper);
		AddChild(shotgun);
		AddChild(revolver);
	}

	private void AlertEnemies()
	{
		Godot.Collections.Array<Node2D> bodies = AlertArea.GetOverlappingBodies();
		foreach (Node2D body in bodies)
		{
			if (body is Enemy enemy)
			{
				enemy.Alerted();
			}
		}
	}


    public override void _Process(double delta)
    {	
		if(!IsAlive) return;

		DesiredLookVector = GetGlobalMousePosition() - head.GlobalPosition;
		LookVector = LookVector.Lerp(DesiredLookVector, (float)delta*WeaponSnapSpeed);
		
		TimePassed += (float)delta;

		if(TimePassed > 100000.0f) {
			TimePassed = 0.0f;
		}

		UpdateAmmo();
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
		
		foreach(var i in interactionsInRange)
		{
			if(i == null) {interactionsInRange.Remove(i); continue;}
			i?.Call("HidePrompt");
		}
		
		if(interactionsInRange.Count > 0)
		{
			CharacterBody2D interaction = interactionsInRange.First();
			interaction?.Call("ShowPrompt");
			if(Input.IsActionJustPressed("Interact"))
			{
				interaction?.Call("Interact");
			}
		}

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
			// if Is Bomb then make it interactabke
			if(itemIndex == 3 && (bool)inventory[3]?.Get("InsideArea"))	
			{
				interactionsInRange.Add(inventory[3]);
				DropCurrentHeldWeapon(itemIndex);
			} 
			

			return;
		}


		if(Input.IsActionJustPressed("Reload")){
			GD.Print("Reloading");
			if((int)inventory[itemIndex].Get("MagAmount") > 0 && !(bool)inventory[itemIndex].Get("IsReloading"))
			{
				inventory[itemIndex]?.Call("Reload");
			}
			
			return;
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
				return;
				}

				CallDeferred(nameof(AlertEnemies));
			}
			catch(Exception e)
			{
				
			}
		}
    }

	private void UpdateAmmo()
	{
		if(inventory[itemIndex] == null || itemIndex == 3){
		
			MagLabel.Visible = false;
			return;
		}
	
		MagLabel.Visible = true;
		int ammo = (int)inventory[itemIndex].Get("CurrentAmmo");

		MagLabel.Text = Math.Clamp((int)inventory[itemIndex].Get("MagAmount") - 1,0,100).ToString() + " | " + ammo.ToString();
	}
	private void DropCurrentHeldWeapon(int index){
		if(inventory[index] is null) return;
		inventory[index].GlobalPosition = GlobalPosition + new Vector2(0,10).Rotated(LookVector.Angle());
		inventory[index]?.Call("Drop");
		inventory[index] = null;
	}

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public override void _PhysicsProcess(double deltaTime)
	{
		
		float MoveSpeed = Speed;
		Vector2 direction = Vector2.Zero;
		float SlowDown = 0;
		DisplayWeapon();
		RotateBody();
		
		if(IsAlive){
			RotateWeapon(deltaTime);
			
			if(StaminaCanRegen && Stamina < MaxStamina)
			{
				Stamina += (float)deltaTime;
			}

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

			if(Input.IsActionJustReleased("Shift"))
			{
				staminaRegenTimer.Start(StaminaRegenCooldown);
			}
			staminaBar.Value = Stamina;

			direction = Input.GetVector("left", "right", "up", "down");
		}

		Vector2 velocity = direction * MoveSpeed * (1-SlowDown);

		velocity.Normalized();
		Velocity = Velocity.Lerp(velocity, (float)deltaTime * Acceleration);
		MoveAndSlide();
		//var collisionData = MoveAndCollide(Velocity);
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
		} else {
			bombSprite.Visible = false;
		}
	}

	private void RotateWeapon(double deltaTime)
	{
		double angle = Math.Atan2(LookVector.Y,LookVector.X);
		hand.Scale = new Vector2(1,LookVector.X < 0 ? -1 : 1);
		hand.Rotation = (float)angle;
	
		if(inventory[itemIndex] == null) return;
		
		Vector2 WeaponPosition = Input.IsActionPressed("RMB") && !Input.IsActionPressed("Shift") ? Vector2.Zero : new(-2,5);
		
		WeaponPosition = (Input.IsActionPressed("Shift") && Stamina > 0.1f) ? new Vector2(-8+(float)Math.Sin(TimePassed*0.1)*3, 8) : WeaponPosition;
		
		Node2D Weapon = hand.GetNode(inventory[itemIndex].Get("WeaponName").ToString()) as Node2D;
		Weapon.Position =  Weapon.Position.Lerp(WeaponPosition, 0.5f);

	}

	public void Kill(){
		for(int i = 0; i < 4; i++)
		{
			DropCurrentHeldWeapon(i);

		}
		if(!IsAlive) return;	
		IsAlive = false;
		
		GetTree().CreateTimer(5.0f).Timeout += () =>{
			GlobalPosition = new Vector2(-85,310);
			EmitSignal("OnPlayerDied");
			Stamina = MaxStamina;
			IsAlive = true;
			hurtBox.Heal(MaxHealth);
		};
	}

	private void OnPickUpBoxBodyEntered(Node body){
		if(inventory.Contains(body) || !(bool)body.Get("CanBePickedUp")) return;
		itemsInRange.Add(body as CharacterBody2D);
	}

	private void OnPickUpBoxExited(Node body){
		itemsInRange?.Remove(body as CharacterBody2D);
		body?.Call("HidePrompt");
	}

	private void OnInteractionBodyEnterd(Node body){
		if(!(bool)body.Get("CanBeInteracted")) return;
		interactionsInRange.Add(body as CharacterBody2D);
		GD.Print("InteractBody");
	}

	private void OnInteractionBodyExited(Node body){
		interactionsInRange?.Remove(body as CharacterBody2D);
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
			case "Revolver":
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
