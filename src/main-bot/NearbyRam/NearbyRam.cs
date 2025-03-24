using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

using System.Drawing;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

// ------------------------------------------------------------------
// NearbyRam
// ------------------------------------------------------------------
// Scan the nearby bot and ram it
// ------------------------------------------------------------------
public class NearbyRam : Bot
{
    
    // The main method starts our bot
    static void Main(string[] args)
    {
        // Read configuration file from current directory
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("NearbyRam.json");

        // Read the configuration into a BotInfo instance
        var config = builder.Build();
        var botInfo = BotInfo.FromConfiguration(config);

        // Create and start our bot based on the bot info
        new NearbyRam(botInfo).Start();
    }

    // Constructor taking a BotInfo that is forwarded to the base class
    private NearbyRam(BotInfo botInfo) : base(botInfo) {}

    public override void Run()
    {
        // Set the colors of the bot
        BodyColor = Color.Red;
        TurretColor = Color.Pink;
        RadarColor = Color.White;
        ScanColor = Color.Black;
        BulletColor = Color.Red;

        // Repeat while the bot is running
        while (IsRunning)
        {
            if(DistanceRemaining == 0){
                SetTurnLeft(10_000);
                MaxSpeed = 8;
                Forward(10_000);
            }
        }
    }

    // scan the bot and change if there's new nearest bot
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        // shot all the bot that is scanned
        if (DistanceTo(evt.X, evt.Y) < 100) {
            Fire(3);
        }
        else if (DistanceTo(evt.X, evt.Y) < 300){
            Fire(2);
        }
        else {
            Fire(1);
        }
        
        var bearing = BearingTo(evt.X, evt.Y);
        SetTurnLeft(bearing);
        if (DistanceTo(evt.X, evt.Y) < 50) { // ram the nearest bot
            SetForward(DistanceTo(evt.X, evt.Y));
        }
        else {
            SetForward(DistanceTo(evt.X, evt.Y)/2);
        }

    }

    // if the bot hit the wall, move backward
    public override void OnHitWall(HitWallEvent e){
        Back(100);
    }

    // if the bot hit the bot, move forward
    public override void OnHitBot(HitBotEvent e){
        if (e.IsRammed){
            SetForward(DistanceTo(e.X, e.Y));
        }
    }
}
