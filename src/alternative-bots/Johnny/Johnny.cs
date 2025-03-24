using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

using System.Drawing;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

public class Johnny : Bot
{
    int gunDirection = 1; // Set gun director (CW or CCW)
    int velocityCounter = 0; // Counter to set change of velocity after OnHitBot
    // The main method starts our bot
    static void Main(string[] args)
    {
        // Read configuration file from current directory
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Johnny.json");

        // Read the configuration into a BotInfo instance
        var config = builder.Build();
        var botInfo = BotInfo.FromConfiguration(config);

        // Create and start our bot based on the bot info
        new Johnny(botInfo).Start();
    }

    // Constructor taking a BotInfo that is forwarded to the base class
    private Johnny(BotInfo botInfo) : base(botInfo) {}

    // Called when a new round is started
    public override void Run()
    {
        BodyColor = Color.Cyan;
        TurretColor = Color.Yellow;
        RadarColor = Color.Cyan;
        ScanColor = Color.DeepPink;
        BulletColor = Color.Yellow;
        
        // Repeat while the bot is running
        while (IsRunning)
        {
            // Gun follows the body
            AdjustGunForBodyTurn = false;

            // SpinBot basic movement. Spins left 'infinitely'
            SetTurnLeft(10000);
            MaxSpeed = 6;
            Forward(10000);
        }
    }

    // We scanned another bot -> fire hard!
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Gun tracking enemy on scan
        AdjustGunForBodyTurn = true;
        // Get angle and distance of enemy            
        var bearing = BearingTo(e.X, e.Y);
        var distance = DistanceTo(e.X, e.Y);

        double absoluteBearing = Direction + bearing; // Get enemy's absolute angle
        double gunTurn = absoluteBearing - GunDirection; // Calculate angle to turn gun

        // Turn gun towards enemy
        SetTurnGunRight(gunTurn);

        // Set SmartFire
        if (distance > 500){
            SetFire(1.5);
        }
        else if (distance > 200 && distance < 500 && Energy > 10){
            SetFire(2.3);
        }
        else if (distance <= 200 && Energy > 10){
            SetFire(3);
        }
        else if (distance < 100 && Energy < 10){
            SetFire(1.1);
        }

        SetForward(10000);

        // Return speed back to original velocity some time after OnHitBot
        if (velocityCounter == 3){
            MaxSpeed = 6;
            velocityCounter = 0;
        }

        // Inverts gun direction per turn
        gunDirection = -gunDirection;
        SetTurnGunRight(450 * gunDirection);
        Go();   
        velocityCounter++;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // Set max speed to flee ram bot
        MaxSpeed = 8;
    }
    public override void OnHitWall(HitWallEvent e){
        // Reverse bot to avoid repeatedly hitting wall
        SetBack(100);
    }
}