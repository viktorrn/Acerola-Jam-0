using Godot;
using System;
using System.Collections.Generic;

public partial class SimpleNest : StaticBody2D
{

	[Export] public string EnemyType = "Biter";
	[Export] public float SpawnTimer = 15.0f;
	[Export] public float SpawnGap = 1.0f;
	[Export] public int MaxAmount = 3;
	[Export] public int Variant = 0;	

	[Export] public int ammo = 3;

	public bool CanForceSpawn = true;
	[Export] public int ForceSpawnAmount = 3;

	private PackedScene packedScene; 
	private List<Enemy> enemies = new();

	private bool NoEnemies = true;
	private List<Vector2> SpawnLocation = new();
	private Timer timer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ammo = MaxAmount * ammo;
		SpawnLocation.Add(new Vector2(-18,45));
		SpawnLocation.Add(new Vector2(25,-30));
		SpawnLocation.Add(new Vector2(-2,-30));
		SpawnLocation.Add(new Vector2(-25,-30));
      	GetNode<Sprite2D>("Sprite2D").Frame = Variant;
		((Health)GetNode<Area2D>("HurtBox")).OnDied += DestoryNest;

		timer = GetNode<Timer>("SpawnTimer");
		timer.WaitTime = SpawnTimer;
		timer.Timeout += () => { CheckToSpawn(); timer.Start(); };
		AddToGroup("DestructionTerrain");
		packedScene = GD.Load<PackedScene>("res://Enemy/" + EnemyType + "/" + EnemyType + ".tscn");
		GetTree().Root.GetNode<GameManager>("Game").AddNest();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void CheckToSpawn(){
		if(ammo <= 0) {timer.Stop(); return;}
		if(enemies.Count >= MaxAmount) return;
		int missing = MaxAmount - enemies.Count;
		for(int i = 0; i < missing; i++){
			GetTree().CreateTimer(i*SpawnGap).Timeout += SpawnEnemy;
			ammo--;
		}
	}

	public void ForceSpawn(){
		if(!CanForceSpawn) return;
		
		for(int i = 0; i < ForceSpawnAmount; i++){
			GetTree().CreateTimer(i*SpawnGap).Timeout += SpawnEnemy;
		}
	}

	private void SpawnEnemy()
	{
		Enemy enemy = (Enemy)packedScene.Instantiate();
		GetTree().Root.GetNode(Utils.WorldPath).AddChild(enemy);
		enemies.Add(enemy);
		
		Vector2 direction = GlobalPosition.DirectionTo(GlobalPosition + SpawnLocation[Variant]);
		direction = direction.Rotated((float)GD.RandRange(-Math.PI / 2, Math.PI / 2));

		enemy.GlobalPosition = GlobalPosition + SpawnLocation[Variant];
		enemy.Velocity = direction * GD.RandRange(75, 120);
		

		enemy.OnDied += () => { EnemyDied(enemy); };
	}

	public void EnemyDied(Enemy enemy)
	{
		enemies.Remove(enemy);
	}

	public void DestoryNest(){
		timer.Stop();
		GetNode<Sprite2D>("Sprite2D").Frame += 4;

		GetTree().Root.GetNode<GameManager>("Game").RemoveNest();

	}
}
