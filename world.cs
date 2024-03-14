using Godot;
using System;




public partial class world : Node2D
{
    // Called when the node enters the scene tree for the first time.
    private PackedScene Explosion = GD.Load<PackedScene>("res://Effects/Explosion.tscn");
    private PackedScene Laser = GD.Load<PackedScene>("res://Nuke.tscn");
    public override void _Process(double delta)
    {
       
    }

    public void EndMission()
    {
        Player player = GetNode<Player>("Player");
        Vector2 center = player.GlobalPosition;
        NukeBomb bomb = (NukeBomb)Laser.Instantiate();
        AddChild(bomb);
        bomb.GlobalPosition = center;

        GetTree().Root.GetNode<Label>("Game/Control/Label").Text = "Nuklear Warhead inbound...";
        GetTree().CreateTimer(8.0f).Timeout += () => {
            PlayerCamera camera = GetNode<PlayerCamera>("Camera2D");
            camera.ShakeCamera(120, 40, 2, true);
            
            foreach(Enemy enemy in GetTree().GetNodesInGroup("Enemy"))
            {
                enemy.GetNode<Health>("HurtBox").SmiteAttack(1000, Vector2.Zero, 0);
            }
            player.hurtBox.SmiteAttack(1000, Vector2.Zero, 0);
            
            for(int i = 0; i < 10; i++)
            {
                Explosion explosion = (Explosion)Explosion.Instantiate();
                explosion.Scale = new Vector2(4f,4f);
                explosion.GlobalPosition = center + new Vector2(GD.Randf()*120 - 50,GD.Randf()*120 - 50);
                AddChild(explosion);
            }

            for(int i = 0; i < 4; i++)
            {
                GetTree().CreateTimer(i*0.4f).Timeout += () => {
                    Explosion explosion = (Explosion)Explosion.Instantiate();
                    explosion.Scale = new Vector2(4f,4f);
                    explosion.GlobalPosition = center + new Vector2(GD.Randf()*120 - 50,GD.Randf()*120 - 50);
                    AddChild(explosion);
                };
            }

            GetTree().CreateTimer(1.2f).Timeout += () => {
                GetTree().Root.GetNode<GameManager>("Game").EndGame();
            };
 
        };

       
       
    }


}
