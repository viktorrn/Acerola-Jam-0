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
    private List<Health> DestructionTerrain = new();

    private RayCast2D ray;
    private Timer CountDownTimer = new Timer();

    private PackedScene BombExplosion = GD.Load<PackedScene>("res://Effects/Explosion.tscn");

    public override void _Ready()
    {
        prompt = GetNode("Prompt") as Control;
        GetNode<Area2D>("Area2D").AreaEntered += (area) =>
        {
           InsideArea = true;
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

        CountDownTimer.OneShot = true;
        CountDownTimer.WaitTime = 5.0f;
        CountDownTimer.Timeout += FindTargets;
        AddChild(CountDownTimer);

    }

    private void BombAreaEnterd(Area2D area)
    {
        if(!area.IsInGroup("Health")) return;
        if(area.GetParent().IsInGroup("DestructionTerrain")) { DestructionTerrain?.Add(area as Health); return; }
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
        //if(!CanBeInteracted && !CanBePickedUp)return;
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
        prompt.GetNode<Label>("Label").Text = "Start Sequence [E]";
    }
    
    public void Interact(){
        if(!CanBeInteracted) return;
        CanBeInteracted = false;
        CountDownTimer.Start();
    }

    public void FindTargets()
    {
        GD.Print("Init Sequence To Go BOOM");
        BlowUpTargets = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!CountDownTimer.IsStopped()){
            prompt.GetNode<Label>("Label").Text = CountDownTimer.TimeLeft.ToString("0.0");
            prompt.Visible = true;
        }
        if(!BlowUpTargets) return;
        BlowUpTargets = false;

        Node2D explosion = BombExplosion.Instantiate<Node2D>();
        explosion.GlobalPosition = GlobalPosition;
        GetTree().Root.GetNode(Utils.WorldPath).AddChild(explosion);

	
		//Godot.Collections.Array<Node2D> bodies = BombArea.GetOverlappingBodies();
        Godot.Collections.Array<Area2D> areas = (GetNode<Area2D>("BombArea") as Area2D).GetOverlappingAreas();
        
		foreach (Node2D area in areas)
		{   
            try{
                if(area is not Health) continue;
                if(area.GetParent().IsInGroup("DestructionTerrain")) { BlowToBits(area as Health); continue;}
                
                ray.TargetPosition = area.GlobalPosition - GlobalPosition;   
                ray.ForceRaycastUpdate();
            
                
                
                if(ray.GetCollider() == null)
                {
                    BlowToBits(area as Health);
                    continue;
                }
                else{
                    if(ray.GetCollider() is StaticBody2D)
                    {
                        BlowToBits(area as Health);
                    }
                }

            } catch(Exception e){
               
            }
		}

     
        
        GetTree().CreateTimer(0.5f).Timeout += () => {
            BombArea.GetParent().QueueFree();
            QueueFree();

        };

    }


   
    public void BlowToBits(Health target)
    {
   

        Vector2 direction = (target.GlobalPosition - GlobalPosition).Normalized();
        float force = 1500.0f;
        int damage = 1000;
        target.SmiteAttack(damage,direction,force);
    }


}
