using Godot;
using System;

public partial class GameTimer : Label
{
	Timer timer;
	private bool isRunning = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        timer = new()
        {
            WaitTime = 60*10.0f,
			Autostart = false
        };
		timer.Timeout += () => {
			GetTree().Root.GetNode<world>(Utils.WorldPath).EndMission();
		};
		AddChild(timer);
		timer.Stop();
		isRunning = false;
    }

	public void StopTimer()
	{
		isRunning = false;
		timer.Stop();
	}

	public void StartTimer()
	{
		timer.WaitTime = 10.0f;
		isRunning = true;
		timer.Start();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{ 
		if(!isRunning) return;
		if(((long)timer.TimeLeft) < 4) return;
		Text = timer.TimeLeft.ToString()[0..3];
	}
}
