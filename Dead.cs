using Godot;
using System;

public partial class Dead : CharacterBody2D
{
    
    private AudioStreamPlayer2D audioPlayer;
    private AudioStream bodyHit = GD.Load<AudioStream>("res://Audio/HitBody.wav");
    private PackedScene Blood = GD.Load<PackedScene>("res://Effects/AlienBlood.tscn");
    
    public override void _Ready()
    {
        audioPlayer = new AudioStreamPlayer2D
        {
            Name = "AudioPlayer",
            Bus = "Master",
            Position = Vector2.Zero,
            MaxDistance = 400,
			VolumeDb = 8

        };
        AddChild(audioPlayer);
        GetNode<Health>("Health").SetUpHealth(0);
        GetNode<Health>("Health").OnHit += ApplyDamage;
        GetTree().CreateTimer(45.0f).Timeout += () => { QueueFree(); };
    }

    public override void _PhysicsProcess(double delta)
    {
        Velocity = Velocity.Lerp(Vector2.Zero,(float)delta*2.0f); 
        MoveAndCollide(Velocity*(float)delta);  
    }

    public void ApplyDamage(Vector2 forceDirection, float force)
	{
		
		Velocity = forceDirection*force*0.5f;
		
		float angle = forceDirection.Angle();

		GpuParticles2D blood = Blood.Instantiate() as GpuParticles2D;
		blood.Position = GlobalPosition;
		blood.Rotation = angle;
		GetTree().Root.GetNode(Utils.WorldPath).AddChild(blood);

		audioPlayer.Stream = bodyHit;
		audioPlayer.PitchScale = 1 + (float)GD.RandRange(-0.1,0.1);
		audioPlayer.Play();
	
	}
}
