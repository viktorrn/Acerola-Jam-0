using Godot;
using System;

public partial class AlienBlood : GpuParticles2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Emitting = true;
        Timer timer = new()
        {
            OneShot = true,
            WaitTime = 0.5f
        };
        timer.Timeout += () => {QueueFree();};
		GetTree().Root.GetNode(Utils.WorldPath).AddChild(timer);
	}

	
}
