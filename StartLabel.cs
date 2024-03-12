using Godot;
using System;

public partial class StartLabel : Label
{


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed("up"))
		{
			GetTree().CreateTimer(10.0f).Timeout += () => {
				Text = "Objective... Kill and destory";
				GetTree().CreateTimer(7.0f).Timeout += ()=>{QueueFree();};
			};
		}
	}
}
