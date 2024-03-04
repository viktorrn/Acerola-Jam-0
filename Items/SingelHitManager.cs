using Godot;
using System;

public partial class SingelHitManager : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public void HitTarget(Node2D target)
	{
		GetParent().QueueFree();
	}
}
