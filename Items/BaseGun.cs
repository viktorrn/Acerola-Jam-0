using Godot;
using System;
using System.Reflection.Metadata;



public partial class BaseGun : CharacterBody2D
{
    public string WeaponName = "BaseGun";

    public int CurrentAmmo = 0;
    public  int MagAmount = 0;
    private int WeaponType = 0; // 0 is primary, 1 is secondary, 3 is granade 


    public bool CanFire = true; 
    public bool IsReloading = false;
    private Node2D Handler;

    private Control prompt;


    override public void _Ready()
    {
        prompt = GetNode("Prompt") as Control;
        Handler = GetNode("Handler") as Node2D;
        try{

            WeaponName = (string)Handler.Get("WeaponName");
            WeaponType = (int)Handler.Get("WeaponType");
            
            MagAmount = (int)Handler.Get("MaxMagAmount");
            CurrentAmmo = (int)Handler.Get("MagSize");

        }catch(Exception){
           GD.Print(this, "![OBS]!: GUN SEEM TO BE MISSING HANDLER NODE!");
        }
    }

    public int FireBullet(Vector2 position, Vector2 direction)
    {
        if(!CanFire || IsReloading || MagAmount <= 0){return -1;}
        if(CurrentAmmo <= 0){return 0;}

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
        if(IsReloading || MagAmount <= 0){return;}
        IsReloading = true;
        MagAmount--;
        GetTree().CreateTimer((float)Handler.Get("ReloadTime")).Connect("timeout", new Callable(this,nameof(OnReloadTimeout)));
 
    }

    private void OnReloadTimeout()
    {
        GD.Print("Reloaded DONE!");
        CurrentAmmo = (int)Handler.Get("MagSize");
        IsReloading = false;
    }   

    public void ShowPrompt()
    {
        prompt.Visible = true;
    }

    public void HidePrompt()
    {
        prompt.Visible = false;
    }

    public void PickUp()
    {
        Visible = false;
        HidePrompt();
    }

    public void Drop()
    {
        Visible = true;
    }
}
