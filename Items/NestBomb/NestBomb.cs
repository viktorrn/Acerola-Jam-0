using Godot;
using System;

public partial class NestBomb : CharacterBody2D
{

    private Control prompt;

    public string WeaponName = "NestBomb";
    public int WeaponType = 3;

    public bool CanBePickedUp = true;
    public bool ReadyToBeArmed = false;

    public override void _Ready()
    {
        prompt = GetNode("Prompt") as Control;
        GetNode<Area2D>("Area2D").AreaEntered += (area) =>
        {
           GD.Print("Inside the area");
        };
    }

    public int FireBullet(Vector2 position, Vector2 direction)
    {
        //GlobalPosition = position + direction * 20;
        return 1;
    }



     public void ShowPrompt()
    {
        if(!CanBePickedUp && !ReadyToBeArmed) return;
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
