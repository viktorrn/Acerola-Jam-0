using Godot;
using System;

public partial class SniperHandler : Node2D
{
	public string WeaponName = "Sniper";

    [Export] public float BulletSpeed = 2000.0f;
    [Export] public int bulletOffset = 30;
    [Export]public float FireRate = 0.8f;
    [Export]public float ReloadTime = 2.0f;
    

    [Export]public int Damage = 10;
    [Export]public float Force = 100;

    [Export]public int MagSize = 5;
    [Export]public int MaxMagAmount = 3;

    public int WeaponType = 0; // 0 is primary, 1 is secondary, 3 is granade 
	PackedScene bulletScene = GD.Load<PackedScene>("res://Items/Sniper/SniperBullet.tscn");

	public void SpawnBullet(Vector2 position, Vector2 direction)
	{	
		double lookAngle = Math.Atan2(direction.Y,direction.X);
        
        Projectile bulletInstance = bulletScene.Instantiate() as Projectile;
        
        bulletInstance.Position = position + new Vector2((float)Math.Cos(lookAngle),(float)Math.Sin(lookAngle))*30;
        bulletInstance.Rotation = (float)lookAngle;
        bulletInstance.Speed = BulletSpeed;
        bulletInstance.Damage = Damage;
        bulletInstance.Force = Force;
        
        GetTree().Root.CallDeferred("add_child",bulletInstance);
		
	}

}
