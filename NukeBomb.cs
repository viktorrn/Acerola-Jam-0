using Godot;
using System;

public partial class NukeBomb : Node2D
{
	private PointLight2D L1;
	private PointLight2D L2;
	private Sprite2D Circle;
	private Sprite2D Laser;
	private float timePassed = 0;
	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
	{
		L1 = GetNode<PointLight2D>("L1");
		L2 = GetNode<PointLight2D>("L2");
		Circle = GetNode<Sprite2D>("Circle");
		Laser = GetNode<Sprite2D>("Laser");
		Laser.Modulate = new Color(1,1,1,0);
		Circle.Modulate = new Color(1,1,1,0);
		L1.Energy = 0;
		L2.Energy = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timePassed += (float)delta;
		Laser.Modulate =  new Color(1,1,1,Mathf.Clamp(Laser.Modulate.A + 0.2f*(float)delta, 0, 1));
		if(Laser.Modulate.A >= 0.5)
		{
			Circle.Modulate =  new Color(1,1,1,Mathf.Clamp(Circle.Modulate.A + 0.5f*(float)delta, 0, 1));
			L1.Energy = Mathf.Clamp(L1.Energy + 0.5f*(float)delta, 0, 2);
			L2.Energy = Mathf.Clamp(L2.Energy + 0.5f*(float)delta, 0, 2);
			L1.Energy = Math.Clamp(L1.Energy + Mathf.Sin(timePassed*4) * 0.0001f,0,1);
			L2.Energy = Math.Clamp(L2.Energy + Mathf.Sin(timePassed*4) * 0.0001f,0,1);
		}
	}
}
