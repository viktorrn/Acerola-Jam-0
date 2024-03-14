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
	public Health hurtBox;
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

	private PackedScene Blood = GD.Load<PackedScene>("res://Effects/Blood.tscn");

	private Area2D AlertArea;

	private int TimePassed = 0;

	private Vector2 PlayerInitialPosition = new Vector2(700,350);
	private Vector2 PlayerSpawnLocation = new Vector2(700,350);

	private Vector2 WeaponPosition = new Vector2(-5,2);

	private int itemIndex = 0;
	private Godot.Collections.Array<CharacterBody2D> inventory;
	private PlayerCamera camera;

	private GameEffects gameEffects;

	private bool IsActive = false;

	private AudioStreamPlayer DeathSound;

	private AudioStream Death = GD.Load<AudioStream>("res://Audio/Death.wav");
	//private AudioStream Hit = GD.Load<AudioStream>("res://Audio/Hit.wav");

	//private AudioStream Step = GD.Load<AudioStream>("res://Audio/Step.wav");

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

		PlayerSpawnLocation = PlayerInitialPosition;
		camera = GetTree().Root.GetNode(Utils.WorldPath).GetNode<PlayerCamera>("Camera2D");
		gameEffects = GetTree().Root.GetNode<GameEffects>("Game/Control/SubViewportContainer");

		hurtBox.SetUpHealth(MaxHealth);
		hurtBox.OnDied += Kill;
		hurtBox.OnHit += ApplyDamage;
		hurtBox.UsesHitStun = true;
		
		inventory = new() { null, null, null, null };
		
		healthBar = GetNode("Control").GetNode("Health") as ProgressBar;
		healthBar.MaxValue = MaxHealth;
		healthBar.Value = Health;
		
		DeathSound = GetNode<AudioStreamPlayer>("Death");

		hurtBox.OnHealthChanged += (int hp) => { 
			int health = hurtBox.CurrentHealth;
			healthBar.Value = health; 
			camera.ShakeCamera(120);  
			float decay = health <= 0 ? 0.9f : 2.0f*health/MaxHealth;
			gameEffects.SetShakeAmount(4*(1 - health/MaxHealth),decay);
			GetTree().Root.GetNode<GameManager>("Game")?.Call("OnPlayerTakeDamage", health <= 0);
			};

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
		CallDeferred(nameof(Spawn));

		GetNode<Area2D>("Respawn").AreaEntered += (Area2D area) => { 
			PlayerSpawnLocation = area.GlobalPosition;
			area.QueueFree(); 
			};
		
    }

	public void Spawn()
	{
		GlobalPosition = PlayerSpawnLocation;
		
		Stamina = MaxStamina;
		IsAlive = true;
		hurtBox.Heal(MaxHealth);

		BaseGun sniper = (BaseGun)Sniper.Instantiate();
		BaseGun shotgun = (BaseGun)Shotgun.Instantiate();
		BaseGun revolver = (BaseGun)Revolver.Instantiate();
		
		GetTree().Root.GetNode(Utils.WorldPath).AddChild(sniper);
		GetTree().Root.GetNode(Utils.WorldPath).AddChild(shotgun);
		GetTree().Root.GetNode(Utils.WorldPath).AddChild(revolver);
		
		inventory[0] = shotgun;
		inventory[1] = revolver;
		inventory[2] = sniper;

		inventory[0].Visible = false;
		inventory[1].Visible = false;
		inventory[2].Visible = false;
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


		GetNode<Control>("Prompt").GetNode<Label>("Label").Text = "";
		if(inventory[itemIndex] == null || SwappingItem) return;

		
			
		if(Input.IsActionJustPressed("Reload")){
			if(!(bool)inventory[itemIndex].Get("IsReloading"))
			{
				inventory[itemIndex]?.Call("Reload");
			} 
			return;
		}

		if((bool)inventory[itemIndex].Get("IsReloading")){
			GetNode<Control>("Prompt").GetNode<Label>("Label").Text = "Reloading...";
		}

		if(itemIndex == 3 && (bool)inventory[3]?.Get("InsideArea"))	
		{
			GetNode<Control>("Prompt").GetNode<Label>("Label").Text = "Place [X]";
			if(Input.IsActionJustPressed("Drop")){
				interactionsInRange.Add(inventory[3]);
				DropCurrentHeldWeapon(itemIndex);
				return;
			}
		} 
		
		if(Input.IsActionPressed("Shift")) return;
		
		if(Input.IsActionJustPressed("LMB")){
			try
			{
				Node2D weapon = hand.GetNode(inventory[itemIndex].Get("WeaponName").ToString()) as Node2D;
				int result = (int)inventory[itemIndex]?.Call("FireBullet",weapon.GlobalPosition,LookVector);
				if(itemIndex == 2)
				{
					inventory[itemIndex]?.Call("Reload");
				}
				if(result == 0)
				{
				inventory[itemIndex]?.Call("Reload");
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

		MagLabel.Text = ammo.ToString();
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
		if(Velocity.Length() > 90.0f)
		{
			if(!GetNode<AudioStreamPlayer2D>("Walk").Playing)
			{
				GetNode<AudioStreamPlayer2D>("Walk").PitchScale = 0.4f + (float)GD.RandRange(-0.05,0.1);
				if(Input.IsActionPressed("Shift"))
				{
					GetNode<AudioStreamPlayer2D>("Walk").PitchScale = 1.0f + (float)GD.RandRange(-0.1,0.1);
				}
				GetNode<AudioStreamPlayer2D>("Walk").Seek(0);
				GetNode<AudioStreamPlayer2D>("Walk").Play();

			}

			//StepSound.Seek(0);
			//StepSound.Play();
		}

		TimePassed = Velocity.Length() > 90.0f ? TimePassed + 1 : 0;
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

		float timeX = (float)Math.Sin(TimePassed*0.2);
		float timeY = (float)Math.Sin(TimePassed*0.2);
		
		WeaponPosition = new Vector2(-2 + timeX*1.2f, 5 + timeY*0.8f);
		WeaponPosition =  Input.IsActionPressed("Shift") ? new Vector2(-4 + timeX*3, 8 + timeY) : WeaponPosition;
		WeaponPosition = Input.IsActionPressed("RMB") && !Input.IsActionPressed("Shift") ? Vector2.Zero : WeaponPosition;
		
		
		Node2D Weapon = hand.GetNode(inventory[itemIndex].Get("WeaponName").ToString()) as Node2D;
		Weapon.Position =  Weapon.Position.Lerp(WeaponPosition, 0.5f);

	}

	public void Kill(){
		
		if(!IsAlive) return;	
		IsAlive = false;
		
		inventory[0]?.QueueFree();
		inventory[0] = null;
		inventory[1]?.QueueFree();
		inventory[1] = null;
		inventory[2]?.QueueFree();
		inventory[2] = null;
		DropCurrentHeldWeapon(3);
		
		DeathSound.Seek(0);
		DeathSound.Play();

		GetTree().CreateTimer(5.0f).Timeout += () =>{
			EmitSignal("OnPlayerDied");
			Spawn();
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
		float angle = forceDirection.Angle();
		Velocity = forceDirection * force;
		GpuParticles2D blood = Blood.Instantiate() as GpuParticles2D;
		blood.Position = GlobalPosition;
		blood.OneShot = true;
		blood.Rotation = angle;
		GetTree().Root.GetNode(Utils.WorldPath).AddChild(blood);
		GetNode<AudioStreamPlayer>("PlayerHit").Play();


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
