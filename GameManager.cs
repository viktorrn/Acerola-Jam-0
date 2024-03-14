using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node2D
{
	private AudioStreamPlayer music;
	private AudioStreamPlayer ambiance;


	public int Nests = 0;
	public int NestsDestroyed = 0;
	public int EnemiesKilled = 0;

	private float musicPitch = 1.0f;

	private bool FirstRun= true;
	private bool fadeOutMusic = false;

	private List<string> Tracks = new List<string>();

	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
	{
		music = GetNode<AudioStreamPlayer>("Music");
		music.Stream = GD.Load<AudioStream>("res://Audio/1.mp3");
		music.Finished += () => {
			PlayNextTrack();
		};

		ambiance = GetNode<AudioStreamPlayer>("Ambiance");
		ambiance.Play();
		ambiance.Finished += () => {
			ambiance.Seek(0);
			ambiance.Play();
		};
		
	}

    public override void _Process(double delta)
	{
		float old = music.PitchScale;
		music.PitchScale = Utils.Lerp(old,musicPitch,0.01f);
		if(!fadeOutMusic)return;
		music.VolumeDb = Utils.Lerp(music.VolumeDb,-80,0.001f);
		if(music.VolumeDb <= -79.5f)
		{
			music.Stop();
			fadeOutMusic = false;
		}
	}


	public void PlayNextTrack()
	{
		if(Tracks.Count == 0) return;
		music.Stream = GD.Load<AudioStream>(Tracks[0]);
		music.VolumeDb = -18;
		Tracks.RemoveAt(0);
		music.Play();
	}

	public void AddNest()
	{
		Nests++;
	}

	public void EnemyKilled()
	{
		EnemiesKilled++;
	}

	public void StartGame()
	{
		Tracks.Clear();
		Tracks.Add("res://Audio/1.mp3");
		Tracks.Add("res://Audio/2.mp3");
		Tracks.Add("res://Audio/3.mp3");
		GetNode<StartLabel>("Control/Label").StartIntro();
		GetNode<GameTimer>("Control/Timer").StartTimer();
		Nests = 0;
		NestsDestroyed = 0;
		EnemiesKilled = 0;
		fadeOutMusic = false;
		GetNode<GameViewPort>("Control/SubViewportContainer/SubViewport").StartGame();
	}

	public void EndGame()
	{
		GetNode<EndScreen>("Control/EndScreen").ShowEndScreen();
		GetNode<GameViewPort>("Control/SubViewportContainer/SubViewport").EndGame();
		GetNode<GameTimer>("Control/Timer").StopTimer();
		fadeOutMusic = true;
	}
	
	public void RemoveNest()
	{
		NestsDestroyed++;
		if(NestsDestroyed == 1)
		{
			GetTree().CreateTimer(2).Timeout += () => {
				PlayNextTrack();
			};
			
		}
	}

	public void OnPlayerTakeDamage(bool wasKilled)
	{
		musicPitch = 0.5f;
		float time = wasKilled ? 5.0f : 0.5f;
		GetTree().CreateTimer(time).Timeout += () => {
			musicPitch = 1.0f;
		};
	}


	
}
