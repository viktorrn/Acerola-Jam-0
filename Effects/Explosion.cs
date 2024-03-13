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
	

        timer = new()
        {
            OneShot = true,
            WaitTime = 4f
        };

		timer.Timeout += () => {QueueFree();};
		AddChild(timer);
		timer.Start();
	}
	public override void _PhysicsProcess(double delta)
	{

		L1.Energy = Utils.Lerp(L1.Energy,0,4*(float)delta); 
		L2.Energy = Utils.Lerp(L2.Energy,0,4*(float)delta);

	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.

}
