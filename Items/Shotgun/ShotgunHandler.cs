using Godot;
using System;

public partial class ShotgunHandler : Node2D
{
	public string WeaponName = "Shotgun";
    private int WeaponType = 0; // 0 is primary, 1 is secondary, 3 is granade 

    [Export] public float BulletSpeed = 1000.0f;
    [Export] public int BulletOffset = 30;
    [Export] public float FireRate = 0.4f;
    [Export] public float ReloadTime = 1.0f;

    [Export] public float Spread = 0.05f;
    

    [Export] public int Damage = 4;
    [Export] public float Force = 40;

    [Export] public int MagSize = 8;
    [Export] public int MaxMagAmount = 3;

	PackedScene bulletScene = GD.Load<PackedScene>("res://Items/Shotgun/Slug.tscn");

	public void SpawnBullet(Vector2 position, Vector2 direction)
	{	
        double lookAngle = Math.Atan2(direction.Y,direction.X);
        for(int i = 0; i < 5; i++){
        
            double angle = lookAngle + (i-2)*Spread;
            
            Projectile bulletInstance = bulletScene.Instantiate() as Projectile;
            

            bulletInstance.Position = position + new Vector2((float)Math.Cos(angle),(float)Math.Sin(angle))*BulletOffset;
            bulletInstance.Rotation = (float)angle;
            bulletInstance.Speed = BulletSpeed;
            bulletInstance.Damage = Damage;
            bulletInstance.Force = Force;
            GetTree().Root.CallDeferred("add_child",bulletInstance);
            
        }
	}


}