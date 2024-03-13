using Godot;
using System;

public partial class ShotgunHandler : Node2D
{
	public string WeaponName = "Shotgun";
    private int WeaponType = 0; // 0 is primary, 1 is secondary, 3 is granade 

    [Export] public float BulletSpeed = 1000.0f;
    [Export] public int BulletOffset = 36;
    [Export] public float FireRate = 0.4f;
    [Export] public float ReloadTime = 1.2f;

    [Export] public float ADSRange = 100.0f;

    [Export] public float Spread = 0.03f;

    

    [Export] public int Damage = 4;
    [Export] public float Force = 70;

    [Export] public int MagSize = 8;
  

    [Export] public float FireForce = 80.0f;

    
    private PackedScene MuzzleFlash = GD.Load<PackedScene>("res://Effects/GunFire.tscn");
	private PackedScene bulletScene = GD.Load<PackedScene>("res://Items/Shotgun/Slug.tscn");

	public void SpawnBullet(Vector2 position, Vector2 direction)
	{	
        
        double lookAngle = Math.Atan2(direction.Y,direction.X);
        for(int i = 0; i < 5; i++){
        
            double angle = (i-2)*Spread;
            
            Projectile bulletInstance = bulletScene.Instantiate() as Projectile;
        
            bulletInstance.Position = position + new Vector2((float)Math.Cos(lookAngle + angle),(float)Math.Sin(lookAngle + angle))*BulletOffset;
            bulletInstance.Rotation = (float)angle;
            bulletInstance.ForceDirection = direction.Normalized().Rotated((float)angle);
            bulletInstance.Speed = BulletSpeed;
            bulletInstance.Damage = Damage;
            bulletInstance.Force = Force;
            GetTree().Root.GetNode(Utils.WorldPath).CallDeferred("add_child",bulletInstance);
            

        }
        GunFire flash = MuzzleFlash.Instantiate() as GunFire;
        flash.Rotation = (float)lookAngle;
        flash.Position = position + direction.Normalized()*BulletOffset;
        GetTree().Root.GetNode(Utils.WorldPath).CallDeferred("add_child",flash);

        GetNode<AudioStreamPlayer>("../Pump").PitchScale = 1 + (float)GD.RandRange(-0.1,0.1);
        GetNode<AudioStreamPlayer>("../Pump").Play();
	}


}