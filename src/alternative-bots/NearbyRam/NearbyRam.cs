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
    int? nearestId; // id of the nearest bot
    double firepower; // firepower to shoot the bot
    bool maju = false; // boolean to check if the bot is moving forward to other bot or not
    int count = 0; // count how many time bot scanned and there's no bot with the nearestId
    int mundur = 0; // count how many time bot hit wall or bot and need to move backward
    double nearestDistance = double.MaxValue; // distance to the nearest bot
    
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
        BodyColor = Color.Cyan;
        TurretColor = Color.Pink;
        RadarColor = Color.White;
        ScanColor = Color.Black;
        BulletColor = Color.Red;

        // Repeat while the bot is running
        while (IsRunning)
        {
            if(!maju){
                SetTurnLeft(10_000);
                MaxSpeed = 8;
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

        // shot all the bot that is scanned
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

        // change the nearest bot if there's new bot that is nearer
        if (distance < nearestDistance || nearestId == null){
            nearestId = evt.ScannedBotId;
            nearestDistance = distance;
        }

        // ram the nearest bot
        if (evt.ScannedBotId == nearestId){
            var bearing = BearingTo(evt.X, evt.Y);
            SetTurnLeft(bearing);
            SetForward(DistanceTo(evt.X, evt.Y));
            maju = true;
        }
        else count++; // count = jumlah bot yg discan tp bukan nearest
        
        // change the nearest bot if there's no bot with the nearestId
        if(count>=2) {
            nearestId = null;
            nearestDistance = double.MaxValue;
            count = 0;
            Rescan();
        }

        // if the bot is being hit to much, move backward
        // bisa ditaruh di OnHitBot juga
        if (mundur>=3){
            if (ArenaWidth-X == ArenaWidth-5 || ArenaHeight-Y == ArenaHeight-5){
                SetTurnLeft(90);
            } else if (ArenaWidth == ArenaWidth-5 || ArenaHeight == ArenaHeight-5) {
                SetTurnRight(90);
            }
            SetBack(100);
            mundur = 0;
        }
    }

    // if the bullet hit the bot, increase mundur
    public override void OnBulletHit(BulletHitBotEvent e){
        mundur++;
    }

    // if the bot hit the wall, move backward and increase mundur
    public override void OnHitWall(HitWallEvent e){
        Back(100);
        mundur++;
    }

    // if the bot hit by bullet, increase mundur
    public override void OnHitByBullet(HitByBulletEvent e){
        mundur++;
    }

    // if the this API triggered but the bot is not the one who rammed, increase mundur
    public override void OnHitBot(HitBotEvent e){
        if (!e.IsRammed){
            mundur++;
        } else {
            mundur--;
        }
    }

    // reset the nearest bot and nearest distance when the round ended
    public override void OnRoundEnded(RoundEndedEvent e){
        nearestId = null;
        nearestDistance = double.MaxValue;
    }
    
}
