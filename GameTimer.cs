using Godot;
using System;

public partial class GameTimer : Label
{
	Timer timer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        timer = new()

        {
            WaitTime = 60 * 10.0f
        };
		AddChild(timer);
		
    }

	public void StartTimer()
	{
		timer.Start();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// format this to min : seconds
		float min = (float)Mathf.Floor(timer.TimeLeft / 60);
		int sec = (int)Math.Floor(timer.TimeLeft) % 60;
		 
		Text = min + " : " + sec;
	}
}
