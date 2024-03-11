using Godot;
using System;

public partial class Explosion : Node2D
{
	private Timer timer;
	private PointLight2D L1;
	private PointLight2D L2;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<GpuParticles2D>("Circle").Emitting = true;
		GetNode<GpuParticles2D>("Explosion").Emitting = true;
		GetNode<GpuParticles2D>("Sparks").Emitting = true;
		L1 = GetNode<PointLight2D>("L1");
		L2 = GetNode<PointLight2D>("L2");
		GD.Print(L1, " ", L2);

        timer = new()
        {
            OneShot = true,
            WaitTime = 1
        };

		timer.Timeout += () => {QueueFree();};
		AddChild(timer);
		timer.Start();
	}
	public override void _PhysicsProcess(double delta)
	{

		L1.Energy = Math.Clamp(4*(float)timer.TimeLeft,0,1);
		L2.Energy = Math.Clamp(4*(float)timer.TimeLeft,0,1);

	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.

}
