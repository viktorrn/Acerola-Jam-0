using Godot;
using System;
using System.Collections.Generic;

public partial class StartLabel : Label
{
	private Timer timer;
	private List<string> strings = new List<string>(
		new string[] {
			"Your Objective... Kill and destory",
			"One more thing...",
			"In about T-10 minutes we will blow this entire planet to bits",
			"Do your best soldier"
		}
	); 

	private int index = 0;
	private List<float> times = new List<float>(
			new float[] {
				3.0f,
				5.0f,
				5.0f,
				3.0f,
			}
		);
    public override void _Ready()
    {
      
        timer = new Timer
        {
            WaitTime = 3.0f
			
        };
        timer.Timeout += () => {
			proccedIntro();
		};
	  AddChild(timer);
    }

	public void StartIntro()
	{
		Text = "";
		index = 0;
		timer.Start();
	}
    // Called every frame. 'delta' is the elapsed time since the previous frame.
	public void proccedIntro()
	{
        if(index >= strings.Count) 
		{
			Text = "";
			timer.Stop();
			return;

		}
		
		timer.WaitTime = times[index];
		Text = strings[index];
        index++;
		
		timer.Start();


    }
}


