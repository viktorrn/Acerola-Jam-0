using Godot;
using System;

public partial class SniperHandler : Node2D
{
	public string WeaponName = "Sniper";

    [Export] public float BulletSpeed = 2000.0f;
    [Export] public int bulletOffset = 30;
    [Export]public float FireRate = 0.8f;
    [Export]public float ReloadTime = 2.0f;

    [Export] public float ADSRange = 200.0f;
    

    [Export]public int Damage = 10;
    [Export]public float Force = 100;

    [Export]public int MagSize = 5;
    [Export]public int MaxMagAmount = 3;

    public int WeaponType = 2; // 0 is primary, 1 is secondary, 3 is granade 
	PackedScene bulletScene = GD.Load<PackedScene>("res://Items/Sniper/SniperBullet.tscn");

	public void SpawnBullet(Vector2 position, Vector2 direction)
	{	
        
        double angle = direction.Angle();
        Projectile bulletInstance = bulletScene.Instantiate() as Projectile;
        
        bulletInstance.Position = position + direction.Normalized()*bulletOffset;
        bulletInstance.Rotation = (float)angle;
        bulletInstance.ForceDirection = direction.Normalized();
        bulletInstance.Speed = BulletSpeed;
        bulletInstance.Damage = Damage;
        bulletInstance.Force = Force;
        
        GetTree().Root.GetNode(Utils.WorldPath).CallDeferred("add_child",bulletInstance);
		
	}

}
