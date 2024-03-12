using Godot;
using System;
using System.Reflection.Metadata;



public partial class BaseGun : CharacterBody2D
{
    public string WeaponName = "BaseGun";

    public int CurrentAmmo = 0;
    public int MaxAmmo = 0;

    private int WeaponType = 0; // 0 is primary, 1 is secondary, 3 is granade 

    public int ADSRange = 10;

    public bool CanBePickedUp = true;

    public bool CanFire = true; 
    public bool IsReloading = false;
    public Node2D Handler;

    private Control prompt;

    private PlayerCamera Camera;

     

    override public void _Ready()
    {
        prompt = GetNode("Prompt") as Control;
        Handler = GetNode("Handler") as Node2D;
        Camera = GetTree().Root.GetNode(Utils.WorldPath).GetNode<PlayerCamera>("Camera2D");
        try{

            WeaponName = (string)Handler.Get("WeaponName");
            WeaponType = (int)Handler.Get("WeaponType");
            
            
            CurrentAmmo = (int)Handler.Get("MagSize");
            ADSRange = (int)Handler.Get("ADSRange");

        }catch(Exception){
           GD.Print(this, "![OBS]!: GUN SEEM TO BE MISSING HANDLER NODE!");
        }
    }

    public int FireBullet(Vector2 position, Vector2 direction)
    {
        if(!CanFire || IsReloading ){return -1;}
        if(CurrentAmmo <= 0){return 0;}

        Camera.ShakeCamera((float)Handler.Get("FireForce"));        

        CurrentAmmo--;
        CanFire = false;

        GetNode("Handler").Call("SpawnBullet",position,direction);
       
        GetTree().CreateTimer((float)Handler.Get("FireRate")).Connect("timeout", new Callable(this, nameof(OnFireTimeout)));
        return 1;
    }

    private void OnFireTimeout()
    {
        CanFire = true;
    }

    public void Reload()
    {
        if(IsReloading){return;}
        IsReloading = true;
        GetTree().CreateTimer((float)Handler.Get("ReloadTime")).Timeout += OnReloadTimeout; 
 
    }

    private void OnReloadTimeout()
    {
        CurrentAmmo = (int)Handler.Get("MagSize");
        IsReloading = false;
    }   

    public void ShowPrompt()
    {
        if(!CanBePickedUp) return;
        prompt.Visible = true;
    }

    public void HidePrompt()
    {
        prompt.Visible = false;
    }

    public void PickUp()
    {
        GetNode<Sprite2D>("Sprite2D").Visible = false;
        HidePrompt();
    }

    public void Drop()
    {
        GetNode<Sprite2D>("Sprite2D").Visible = true;
       
    }

}
