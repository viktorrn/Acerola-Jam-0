using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class NestBomb : CharacterBody2D
{
    private Control prompt;

    public string WeaponName = "NestBomb";
    public int WeaponType = 3;

    public bool CanBePickedUp = true;
    public bool CanBeInteracted = false;
    public bool InsideArea = false;
    public bool BlowUpTargets = false;

    private Area2D BombArea;

    private List<Area2D> Targets = new();
    private List<Health> Nests = new();

    private RayCast2D ray;

    public override void _Ready()
    {
        prompt = GetNode("Prompt") as Control;
        GetNode<Area2D>("Area2D").AreaEntered += (area) =>
        {
            GD.Print("Inside Area");
           InsideArea = true;
           GD.Print(area);
           BombArea = area;
        };

        GetNode<Area2D>("Area2D").AreaExited += (area) =>
        {
            InsideArea = false;
            BombArea = null;
        };  

        GetNode<Area2D>("BombArea").AreaEntered += BombAreaEnterd;
        AddToGroup("Attack");
        AddToGroup("Items");
        ray = GetNode("Ray") as RayCast2D;
        ray.Position = Vector2.Zero;
    
    }

    private void BombAreaEnterd(Area2D area)
    {
        if(!area.IsInGroup("Health")) return;
        if(area.GetParent().IsInGroup("Nest")) { Nests?.Add(area as Health); return; }
        if(Targets.Contains(area as Health)) return;
        Targets?.Add(area as Health);
    }

    private void BombAreaExited(Area2D area)
    {
        Targets?.Remove(area as Health);
    }

    public static int FireBullet(Vector2 position, Vector2 direction)
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
        CanBeInteracted = false;
        GetTree().CreateTimer(5f).Timeout += FindTargets;
    }

    public void FindTargets()
    {
        GD.Print("Init Sequence To Go BOOM");
        BlowUpTargets = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!BlowUpTargets) return;
        BlowUpTargets = false;

        foreach(Health target in Targets)
        {
            ray.TargetPosition = target.GlobalPosition - GlobalPosition;   
            ray.ForceRaycastUpdate();
            if(ray.IsColliding()) continue;
            BlowToBits(target);
            //CallDeferred(nameof(BlowToBits),target);   
        }

        foreach(Health target in Nests)
        {
            BlowToBits(target);
        }

        BombArea.GetParent().QueueFree();
        QueueFree();

    }

   

    public void BlowToBits(Health target)
    {
        Vector2 direction = (target.GlobalPosition - GlobalPosition).Normalized();
        float force = 1500.0f;
        int damage = 1000;
        target.SmiteAttack(damage,direction,force);
    }

    public int HitTarget(Health target)
    {
        return 1000;
    }


}
