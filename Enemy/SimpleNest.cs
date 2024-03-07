using Godot;
using System;
using System.Collections.Generic;

public partial class SimpleNest : StaticBody2D
{

	private PackedScene packedScene = (PackedScene)ResourceLoader.Load("res://Enemy/Enemy.tscn");
	private List<Enemy> enemies = new List<Enemy>();

	private bool NoEnemies = true;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
      
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(NoEnemies)
		{
			NoEnemies = false;

			GetTree().CreateTimer(15).Timeout += () =>
			{
				SpawnEnemy();
				GetTree().CreateTimer(0.5).Timeout += () =>
				{
					SpawnEnemy();
				};
				GetTree().CreateTimer(1).Timeout += () =>
				{
					SpawnEnemy();
				};
			};
		}
		
	}

	private void SpawnEnemy()
	{
		Enemy enemy = (Enemy)packedScene.Instantiate();
		GetTree().Root.GetNode("world").AddChild(enemy);
		enemies.Add(enemy);
		
		Vector2 direction = GlobalPosition.DirectionTo(GetNode<Node2D>("SpawnLocation").GlobalPosition);

		enemy.GlobalPosition = GetNode<Node2D>("SpawnLocation").GlobalPosition;
		enemy.Velocity = direction * 50;
		

		enemy.OnDied += () => { EnemyDied(enemy); };
	}

	public void EnemyDied(Enemy enemy)
	{
		if(!enemies.Remove(enemy)) return;
		if(enemies.Count == 0)
		{
			NoEnemies = true;
		}
	}
}
