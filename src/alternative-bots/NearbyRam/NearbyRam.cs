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
    int? nearestId;
    int count = 0;
    int frustration = 0;
    int mundur = 0;
    // int turnDirection = 1;
    double firepower;
    double nearestDistance = double.MaxValue;
    bool maju = false;
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
        BodyColor = Color.Cyan;
        TurretColor = Color.Pink;
        RadarColor = Color.White;
        ScanColor = Color.Black;
        BulletColor = Color.Red;

        // AdjustGunForBodyTurn = true;
        // GunTurnRate = MaxGunTurnRate;

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
        
        firepower = nearestDistance < 100 ? 3 : nearestDistance < 300 ? 2 : 1;
        Fire(firepower);
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

        if (distance < nearestDistance || nearestId == null){
            nearestId = evt.ScannedBotId;
            nearestDistance = distance;
        }

        // if (evt.ScannedBotId == nearestId && maju==false){
        if (evt.ScannedBotId == nearestId){
            var bearing = BearingTo(evt.X, evt.Y);
            SetTurnLeft(bearing);
            SetForward(DistanceTo(evt.X, evt.Y));
            maju = true;
        }
        else count++; // count = jumlah bot yg discan tp bukan nearest
        if(count>=2) {
            nearestId = null;
            nearestDistance = double.MaxValue;
            count = 0;
        }
        // if(frustration>=5){
        //     if (evt.ScannedBotId == nearestId){
        //         var bearing = BearingTo(evt.X, evt.Y);
        //         TurnLeft(bearing);
        //         Forward(DistanceTo(evt.X, evt.Y)/2);
        //         frustration = 0;
        //     }
        // }

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
    
    public override void OnBulletHitBullet(BulletHitBulletEvent bulletHitBulletEvent){
        frustration++;
    }

    public override void OnBulletHitWall(BulletHitWallEvent bulletHitWallEvent){
        frustration++;
    }

    public override void OnBulletHit(BulletHitBotEvent bulletHitBotEvent){
        frustration = 0;
        mundur++;
    }

    public override void OnHitWall(HitWallEvent e){
        Back(100);
        mundur++;
    }

    public virtual void OnHitByBullet(HitByBulletEvent bulletHitBotEvent){
        mundur++;
    }

    public override void OnHitBot(HitBotEvent e){
        if (!e.IsRammed){
            mundur++;
        }
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
