using Godot;
using System;

public partial class ShotGunPelletRandomizer : Sprite2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Frame = GD.RandRange(0,3);
	}

	
}
