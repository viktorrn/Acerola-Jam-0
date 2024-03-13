using Godot;
using System;

public partial class EndScreen : Control
{
	public ShaderMaterial EffectShaderMaterial;
	public float DesiredAmount = 0.5f;

	[ExportCategory("Shake")]

	[Export] public float ShakeStrength = 1f;
	[Export] public float ShakeSpeed = 8f;
	[Export] public float ShakeDecay = 0.9f;

	private FastNoiseLite noise = new();
	private float Noise_i = 0;
	private float Shake = 1.0f;

	private GameManager GameManager;
	
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

		EffectShaderMaterial = Material as ShaderMaterial;
		
		GameManager = GetNode<GameManager>("/root/Game");
    }	

	public override void _PhysicsProcess(double delta)
	{
		if(!Visible) return;
		if(Input.IsActionJustPressed("space"))
		{
			HideEndScreen();
			GameManager.StartGame();
		}

		float current = (float)EffectShaderMaterial.GetShaderParameter("amount");
		float newAmount = Utils.Lerp(current,DesiredAmount,(float)delta);
		
		Shake = Utils.Lerp(Shake,0.01f,ShakeDecay * (float)delta);

		Noise_i += ShakeSpeed * (float)delta;
		newAmount = noise.GetNoise1D(Noise_i) * Shake * ShakeStrength + newAmount;


		EffectShaderMaterial.SetShaderParameter("amount",newAmount);
		
	}

	public void ShowEndScreen()
	{
		Visible = true;
		GetNode<Label>("Box/Nests").Text = "Nests Destroyed " +GameManager.NestsDestroyed;
		GetNode<Label>("Box/Enemies").Text = "Enemies Killed " +GameManager.EnemiesKilled; 
	
	}

	public void HideEndScreen()
	{
		Visible = false;
	}

}
