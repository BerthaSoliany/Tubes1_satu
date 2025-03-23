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
    int? nearestId; // id of the nearest bot
    double firepower; // firepower to shoot the bot
    bool maju = false; // boolean to check if the bot is moving forward to other bot or not
    int count = 0; // count how many time bot scanned and there's no bot with the nearestId
    int frustration = 0; // increase when bullet hit wall or bullet hit bullet
    double nearestDistance = double.MaxValue; // distance to the nearest bot
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
        // Set the colors of the bot
        BodyColor = Color.Red;
        TurretColor = Color.White;
        RadarColor = Color.Black;
        ScanColor = Color.Cyan;
        BulletColor = Color.Orange;

        // Set the gun to turn independently of the body
        AdjustGunForBodyTurn = true;
        GunTurnRate = MaxGunTurnRate;

        // Repeat while the bot is running
        while (IsRunning)
        {
            if(!maju){
                SetTurnLeft(10_000);
                MaxSpeed = 5;
                Forward(10_000);
            }
            else{
                maju = false;
            }
        }
    }

    // scan the bot and change if there's new nearest bot
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        double distance = DistanceTo(evt.X, evt.Y);
        
        // change the nearest bot if there's new bot that is nearer
        if (distance < nearestDistance || nearestId == null){
            nearestId = evt.ScannedBotId;
            nearestDistance = distance;
        }

        // shot the nearest bot
        if (evt.ScannedBotId == nearestId){
            if (nearestDistance < 100) {
                firepower = 3;
            }
            else if (nearestDistance < 300){
                firepower = 2;
            }
            else {
                firepower = 1;
            }
            Fire(firepower);
        }
        else count++; // increase count if the scanned bot is not the nearest bot

        // change the nearest bot if there's no bot with the nearestId
        if(count>=3) {
            nearestId = null;
            nearestDistance = double.MaxValue;
            count = 0;
        }

        // move to the nearest bot if the bot frustrate (bullet not hit a bot)
        if(frustration>=3){
            if (evt.ScannedBotId == nearestId){
                var bearing = BearingTo(evt.X, evt.Y);
                TurnLeft(bearing);
                Forward(DistanceTo(evt.X, evt.Y)/2);
                frustration = 0;
            }
        }
    }

    // frustation increment happen if the bullet hit bullet or wall
    public override void OnBulletHitBullet(BulletHitBulletEvent bulletHitBulletEvent){
        frustration++;
    }

    public override void OnBulletHitWall(BulletHitWallEvent bulletHitWallEvent){
        frustration++;
    }

    // reset frustration if the bullet hit a bot
    public override void OnBulletHit(BulletHitBotEvent bulletHitBotEvent){
        frustration = 0;
    }

    // if the bot hit the wall, move back
    public override void OnHitWall(HitWallEvent e){
        Back(100);
    }

    // if the bot hit the bot, move back
    public override void OnHitBot(HitBotEvent e)
    {
        nearestId = e.VictimId;
        SetBack(150);
        SetTurnLeft(95);
        SetForward(100);
        Rescan();
    }

    // reset nearestId and nearestDistance if the round ended
    public override void OnRoundEnded(RoundEndedEvent e){
        nearestId = null;
        nearestDistance = double.MaxValue;
    }
}
