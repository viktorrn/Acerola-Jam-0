using Godot;
using System;

public partial class Health : Area2D
{
	
	private int MaxHealth = 20;
	private int CurrentHealth = 20;
	private System.Collections.Generic.List<Area2D> areaList;

	public override void _Ready()
	{
		areaList = new System.Collections.Generic.List<Area2D>();
		BodyEntered += HandleAttack;
		AreaEntered += HandleAttack;
	} 
	// Called when the node enters the scene tree for the first time.

	public void SetUpHealth(int maxHealth)
	{
		MaxHealth = maxHealth;
		CurrentHealth = maxHealth;
	}



	public void HandleAttack(Node body)
	{

		if(body.IsInGroup("Attack"))
		{
			if(areaList.Contains(body as Area2D)){return;}
			
			int damage = (int)body?.Call("HitTarget",this);
			GetParent().Call("ApplyDamage",body.Get("ForceDirection"),body.Get("Force"));
			
			areaList.Add(body as Area2D);
			GetTree().CreateTimer(0.5f).Timeout += () => RemoveArea(body as Area2D);

			CurrentHealth -= damage;
			if(CurrentHealth <= 0)
			{
				GetParent().Call("Kill");
			}
		}
	}

	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private void RemoveArea(Area2D area)
	{
		areaList?.Remove(area);
	}
}
