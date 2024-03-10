using Godot;
using System;

public partial class DestructWall : CharacterBody2D
{
	public override void _Ready()
	{
		Health area = (Health)GetNode<Area2D>("HurtBox");
		area.SetUpHealth(3);
		AddToGroup("DestructionTerrain");
		area.OnDied += () => { QueueFree(); };

	}	

}
