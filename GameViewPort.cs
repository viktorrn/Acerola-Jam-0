using Godot;
using System;

public partial class GameViewPort : SubViewport
{
	private PackedScene worldScene = GD.Load<PackedScene>("res://world.tscn");
	// Called when the node enters the scene tree for the first time.


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void StartGame()
	{
		// create the world scene
		world world = (world)worldScene.Instantiate();
		GetViewport().AddChild(world);
		// remove the current scene
		
	}

	public void EndGame()
	{
		// remove the world scene
		world world = (world)GetNode("world");
		world.QueueFree();
		// create the current scene
	}
	
}
