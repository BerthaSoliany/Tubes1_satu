using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

using System.Drawing;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

// ------------------------------------------------------------------
// Nearest
// ------------------------------------------------------------------
// Track the nearest and shot!
// ------------------------------------------------------------------
public class Nearest : Bot
{
    int? nearestId;
    // int turnDirection = 1;
    double firepower;
    double nearestDistance = double.MaxValue;
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


    public override void Run()
    {
        BodyColor = Color.Red;
        TurretColor = Color.White;
        RadarColor = Color.Black;
        ScanColor = Color.Cyan;

        // Repeat while the bot is running
        while (IsRunning)
        {
            SetTurnLeft(10_000);
            MaxSpeed = 5;
            Forward(10_000);
        }
    }

    // scan the bot and change if there's new nearest bot
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        double distance = DistanceTo(evt.X, evt.Y);
        

        if (distance < nearestDistance || nearestId == null){
            nearestId = evt.ScannedBotId;
            nearestDistance = distance;
        }

        if (evt.ScannedBotId == nearestId){
            firepower = nearestDistance < 100 ? 3 : nearestDistance < 300 ? 2 : 1;
            // if (evt.Energy > 16 || nearestDistance<=20)
            //     Fire(3);
            // else if (evt.Energy > 10 || (nearestDistance>20 && nearestDistance<=50))
            //     Fire(2);
            // else if (evt.Energy > 4)
            //     Fire(1);
            // else if (evt.Energy > 2)
            //     Fire(.5);
            // else if (evt.Energy > .4)
            //     Fire(.1);
            Fire(firepower);
        }
    }
    
    public override void OnHitWall(HitWallEvent e){
        SetBack(10);
        TurnRight(10);
    }

    // public override void OnHitBot(HitBotEvent e){
    //     if (e.VictimId == nearestId){
    //         var bearing = BearingTo(e.X, e.Y);
    //         if (bearing >= 0)
    //             turnDirection = 1;
    //         else
    //             turnDirection = -1;

    //         TurnLeft(bearing);
    //         Forward(30);
    //     } else if (e.IsRammed){
    //         TurnLeft(10);
    //     }
    // }

    public override void OnRoundEnded(RoundEndedEvent e){
        nearestId = null;
        nearestDistance = double.MaxValue;
    }

    
}
