using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class RandomStrafeBot : Bot
{
    private readonly Random rand = new Random();
    private double strafeDirection = 1; 
    private bool isStopped = false;     
    private bool isAvoidingWall = false;

    static void Main(string[] args)
    {
        new RandomStrafeBot().Start();
    }

    RandomStrafeBot() : base(BotInfo.FromFile("RandomStrafeBot.json")) { }

    public override void Run()
    {
        BodyColor = Color.Black;
        TurretColor = Color.Red;
        RadarColor = Color.Yellow;
        ScanColor = Color.Orange;
        TracksColor = Color.Gray;
        GunColor = Color.Red;

        AdjustGunForBodyTurn = false; // Memisahkan gun dan radar dari body (masih ngebug jg)
        GunTurnRate = MaxGunTurnRate;

        while (IsRunning)
        {
            // avoiding wall nya masih ngebug
            // double distanceToWall = CalculateDistanceToWall();
            // if (distanceToWall < 100) 
            // {
            //     if(!isAvoidingWall){
            //         SetTurnRight(180);
            //         SetForward(100);
            //     }
            //     isAvoidingWall = true;
            // }
            // else 
            if (isStopped)
            {
                isAvoidingWall = false;
                isStopped = false;
                strafeDirection = (rand.Next(2) == 0) ? 1 : -1; // Randomize strafe direction
            }
            else
            {
                isAvoidingWall = false;
                // Strafe movement
                SetTurnRight(30 * strafeDirection);
                SetForward(100);
                // Occasionally stop (stop-and-go strategy)
                if (rand.Next(5) == 0)
                {
                    isStopped = true;
                    SetStop();
                }
            }
            Go();
        }
    }
    public override void OnScannedBot(ScannedBotEvent e) {
        Fire(3);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        distance
        TurnRight(180);
        Forward(150);
    }

    private double CalculateDistanceToWall()
    {
        double width = ArenaWidth;
        double height = ArenaHeight;
        
        double distanceToLeft = X;
        double distanceToRight = width - X;
        double distanceToBottom = Y;
        double distanceToTop = height - Y;
        
        return Math.Min(Math.Min(distanceToLeft, distanceToRight), 
                    Math.Min(distanceToBottom, distanceToTop));
    }

    public override void OnHitBot(HitBotEvent e)
    {
        double bearing = BearingTo(e.X, e.Y);
        if (Math.Abs(bearing) < 10 && GunHeat == 0)
        {
            Fire(Math.Min(3, Energy - 0.1));
        }
    }

}
