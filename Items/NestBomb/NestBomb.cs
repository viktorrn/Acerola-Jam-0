using Godot;
using System;

public partial class NestBomb : CharacterBody2D
{
    private Control prompt;

    public string WeaponName = "NestBomb";
    public int WeaponType = 3;

    public bool CanBePickedUp = true;
    public bool CanBeInteracted = false;
    public bool InsideArea = false;

    public override void _Ready()
    {
        prompt = GetNode("Prompt") as Control;
        GetNode<Area2D>("Area2D").AreaEntered += (area) =>
        {
            GD.Print("Inside Area");
           InsideArea = true;
        };

        GetNode<Area2D>("Area2D").AreaExited += (area) =>
        {
            InsideArea = false;
        };  
    }

    public int FireBullet(Vector2 position, Vector2 direction)
    {
        //GlobalPosition = position + direction * 20;
        return 1;
    }



    public void ShowPrompt()
    {
        if(!CanBeInteracted && !CanBePickedUp)return;
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
        if(!InsideArea) return;
        
        CanBePickedUp = false;
        CanBeInteracted = true;
        
    }
    
    public void Interact(){
        if(!CanBeInteracted) return;

        GD.Print("Init Sequence To Go BOOM");
        CanBeInteracted = false;
    }
}
