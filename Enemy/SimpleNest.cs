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

	public bool CanForceSpawn = true;
	[Export] public int ForceSpawnAmount = 3;

	private PackedScene packedScene; 
	private List<Enemy> enemies = new List<Enemy>();

	private bool NoEnemies = true;
	private List<Vector2> SpawnLocation = new List<Vector2>();
	private Timer timer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SpawnLocation.Add(new Vector2(-25,35));
		SpawnLocation.Add(new Vector2(25,-20));
		SpawnLocation.Add(new Vector2(-2,-22));
		SpawnLocation.Add(new Vector2(25,20));
      	GetNode<Sprite2D>("Sprite2D").Frame = Variant;
		((Health)GetNode<Area2D>("HurtBox")).OnDied += DestoryNest;

		timer = GetNode<Timer>("SpawnTimer");
		timer.WaitTime = SpawnTimer;
		timer.Timeout += () => { CheckToSpawn(); timer.Start(); };
		AddToGroup("Nest");
		packedScene = GD.Load<PackedScene>("res://Enemy/" + EnemyType + ".tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void CheckToSpawn(){
		if(enemies.Count >= MaxAmount) return;
		int missing = MaxAmount - enemies.Count;
		for(int i = 0; i < missing; i++){
			GetTree().CreateTimer(i*SpawnGap).Timeout += SpawnEnemy;
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
		GetTree().Root.GetNode("world").AddChild(enemy);
		enemies.Add(enemy);
		
		Vector2 direction = GlobalPosition.DirectionTo(SpawnLocation[Variant]);
		direction = direction.Rotated((float)GD.RandRange(-Math.PI / 4, Math.PI / 4));

		enemy.GlobalPosition = GlobalPosition + SpawnLocation[Variant];
		enemy.Velocity = direction * GD.RandRange(50, 75);
		

		enemy.OnDied += () => { EnemyDied(enemy); };
	}

	public void EnemyDied(Enemy enemy)
	{
		enemies.Remove(enemy);
	}

	public void DestoryNest(){
		timer.Stop();
		GetNode<Label>("Label").Visible = true;

	}
}
