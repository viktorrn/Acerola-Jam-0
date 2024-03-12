using Godot;
using System;

public partial class Health : Area2D
{
	
	private int MaxHealth = 20;
	private int CurrentHealth = 20;

	public bool UsesHitStun = false;
	private bool HitStun = false;
	private float HitStunTime = 0.5f;

	[Export] bool Imortal = false;
	private System.Collections.Generic.List<Area2D> areaList;

	[Signal] public delegate void OnDiedEventHandler();
	[Signal] public delegate void OnHitEventHandler(Vector2 forceDirection,float force);

	[Signal] public delegate void OnHealthChangedEventHandler(int currentHealth);

	public override void _Ready()
	{
		areaList = new System.Collections.Generic.List<Area2D>();
		BodyEntered += HandleAttack;
		AreaEntered += HandleAttack;
		AddToGroup("Health");
	} 
	// Called when the node enters the scene tree for the first time.

	public void SetUpHealth(int maxHealth)
	{
		MaxHealth = maxHealth;
		CurrentHealth = maxHealth;
	}



	public void HandleAttack(Node body)
	{
		if(HitStun) return;

		if(body.IsInGroup("Attack"))
		{

			int damage = (int)body?.Call("HitTarget",this);
			EmitSignal(nameof(OnHit),body.Get("ForceDirection"),body.Get("Force"));

			//areaList.Add(body as Area2D);
			//GetTree().CreateTimer(0.2f).Timeout += () => RemoveArea(body as Area2D);
			if(Imortal) return;
			
			if(UsesHitStun) {
				HitStun = true;
				GetTree().CreateTimer(HitStunTime).Timeout += () => HitStun = false; 
			}
		
			CurrentHealth -= damage;
			CurrentHealth = Math.Clamp(CurrentHealth,0,MaxHealth);
			EmitSignal(nameof(OnHealthChanged),CurrentHealth);
			if(CurrentHealth <= 0)
			{
				EmitSignal(nameof(OnDied));
			}
		}
	}

	public void SmiteAttack(int damage, Vector2 direction, float force)
	{
		EmitSignal(nameof(OnHit),direction,force);
		CurrentHealth -= damage;
		CurrentHealth = Math.Clamp(CurrentHealth,0,MaxHealth);
		EmitSignal(nameof(OnHealthChanged),CurrentHealth);
		if(CurrentHealth <= 0)
		{
			EmitSignal(nameof(OnDied));
		}
	}
	

	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private void RemoveArea(Area2D area)
	{
		areaList?.Remove(area);
	}

	public void Heal(int amount)
	{
		CurrentHealth += Math.Abs(amount);
		Math.Clamp(CurrentHealth,0,MaxHealth);
		EmitSignal(nameof(OnHealthChanged),CurrentHealth);
	}
}
