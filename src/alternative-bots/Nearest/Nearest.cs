using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
using System;
using System.Drawing;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

// ------------------------------------------------------------------
// Nearest
// ------------------------------------------------------------------
// Moves in a circle, taeget the nearest bot.
// ------------------------------------------------------------------
public class Nearest : Bot
{
    // The main method starts our bot
    static void Main(string[] args)
    {
        // Read configuration file from current directory
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Nearest.json");

        // Read the configuration into a BotInfo instance
        var config = builder.Build();
        var botInfo = BotInfo.FromConfiguration(config);

        // Create and start our bot based on the bot info
        new Nearest(botInfo).Start();
    }

    // Constructor taking a BotInfo that is forwarded to the base class
    private Nearest(BotInfo botInfo) : base(botInfo) {}

    // Called when a new round is started -> initialize and do some movement
    public override void Run()
    {
        BodyColor = Color.Blue;
        TurretColor = Color.Blue;
        RadarColor = Color.Black;
        ScanColor = Color.Yellow;

        // AdjustGunForBodyTurn = true;

        // AdjustRadarForGunTurn = true;
        // AdjustRadarForBodyTurn = true;

        // RadarTurnRate = 5;

        // Repeat while the bot is running
        while (IsRunning)
        {
            // Tell the game that when we take move, we'll also want to turn right... a lot
            SetTurnLeft(10_000);
            // Limit our speed to 5
            MaxSpeed = 5;
            // Start moving (and turning)
            Forward(10_000);
        }
        // Fire(1);
    }

    // We scanned another bot -> see distance
    private ScannedBotEvent? target;
    private double nearest = double.MaxValue;
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        double distance = DistanceTo(evt.X, evt.Y);

        if (distance < nearest) {
            nearest = distance;
            target = evt;
        } 
    }

    public override void OnTick(TickEvent tickEvent){
        if (target!=null){
            var angleToTarget = Math.Atan2(target.X - X, target.Y - Y);
            var angleDegrees = angleToTarget*(180/Math.PI);

            if (angleDegrees<0){
                angleDegrees+=360;
            }

            double gunTurn = NormalizeRelativeAngle(angleDegrees - GunDirection);
            SetTurnGunRight(gunTurn);

            if (nearest < 50 && Energy > 50)
                Fire(3);
            else
                Fire(2);

            target = null;
            nearest = double.MaxValue;
        }
        Rescan();
    }

    // public override void OnBulletFired(BulletFiredEvent e){
    //     double targetX = e.Bullet.X;
    //     double targetY = e.Bullet.Y;

    //     double angle = Math.Atan2(targetX-X,targetY-Y);
    //     double angleDegrees = angle*(180.0/Math.PI);

    //     if (angleDegrees < 0){
    //         angleDegrees += 360;
    //     }

    //     // Turn toward the direction
    //     SetTurnRight(NormalizeRelativeAngle(angleDegrees - Direction));
        
    //     Forward(100); // Move closer — adjust distance as needed
    // }

	public override void OnHitWall(HitWallEvent e){
        SetBack(100);
        SetTurnRight(90);
    }

}