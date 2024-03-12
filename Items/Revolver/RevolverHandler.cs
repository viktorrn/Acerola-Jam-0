using Godot;
using System;

public partial class RevolverHandler : Node2D
{
	public string WeaponName = "Revolver";

    [Export] public float BulletSpeed = 1200.0f;
    [Export] public int BulletOffset = 28;
    [Export]public float FireRate = 0.3f;
    [Export]public float ReloadTime = 1.2f;

    [Export] public float ADSRange = 160.0f;
    

    [Export]public int Damage = 10;
    [Export]public float Force = 100;

    [Export]public int MagSize = 6;
  

    [Export] public float FireForce = 40.0f;

    private PackedScene MuzzleFlash = GD.Load<PackedScene>("res://Effects/GunFire.tscn");

    public int WeaponType = 1; // 0 is primary, 1 is secondary, 3 is granade 
	PackedScene bulletScene = GD.Load<PackedScene>("res://Items/Revolver/RevolverBullet.tscn");

	public void SpawnBullet(Vector2 position, Vector2 direction)
	{	
        
        double angle = direction.Angle();
        Projectile bulletInstance = bulletScene.Instantiate() as Projectile;
        
        bulletInstance.Position = position + direction.Normalized()*BulletOffset;
        bulletInstance.Rotation = (float)angle;
        bulletInstance.ForceDirection = direction.Normalized();
        bulletInstance.Speed = BulletSpeed;
        bulletInstance.Damage = Damage;
        bulletInstance.Force = Force;
        

        GetTree().Root.GetNode(Utils.WorldPath).CallDeferred("add_child",bulletInstance);
        GunFire flash = MuzzleFlash.Instantiate() as GunFire;
        flash.Rotation = (float)angle;
        flash.Position = position + direction.Normalized()*BulletOffset;
        flash.Scale = new Vector2(1.0f,1.0f);
        GetTree().Root.GetNode(Utils.WorldPath).CallDeferred("add_child",flash);
	}

}
