using Godot;
using System;

public partial class GunFire : Node2D
{
	private Timer timer;
	private PointLight2D L1;
	private PointLight2D L2;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<GpuParticles2D>("GunFire").Emitting = true;
		GetNode<GpuParticles2D>("Muzzle").Emitting = true;
		((ParticleProcessMaterial)GetNode<GpuParticles2D>("Muzzle").ProcessMaterial).AngleMax = -GlobalRotationDegrees; //Set("params/angle_min",Rotation);
		((ParticleProcessMaterial)GetNode<GpuParticles2D>("Muzzle").ProcessMaterial).AngleMin = -GlobalRotationDegrees; //Set("params/angle_max",Rotation);

		GetNode<GpuParticles2D>("BulletTrace").Emitting = true;
		L1 = GetNode<PointLight2D>("L1");
		L2 = GetNode<PointLight2D>("L2");

		timer = new()
		{
			OneShot = true,
			WaitTime = 0.5f,
			
		};

		L1.Energy = 0.7f;
		L2.Energy = 0.7f;
		timer.Timeout += () => {QueueFree();};

		AddChild(timer);
		timer.Start();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		L1.Energy = Utils.Lerp(L1.Energy,0,4*(float)delta);
		L2.Energy = Utils.Lerp(L2.Energy,0,4*(float)delta);
	}
}
