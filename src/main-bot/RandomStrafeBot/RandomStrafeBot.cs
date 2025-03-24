using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class RandomStrafeBot : Bot
{
    private readonly Random rand = new Random();
    private double strafeDirection = 1; 
    private bool isStopped = false;     

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

        AdjustGunForBodyTurn = true; // Memisahkan gun dan radar dari body
        GunTurnRate = MaxGunTurnRate;

        while (IsRunning)
        {
            SetTurnGunLeft(10000); // Continuously turning gun
            if (isStopped)
            {
                isStopped = false;
                strafeDirection = (rand.Next(2) == 0) ? 1 : -1; // 50 50 kemungkinan untuk strafe ke kanan atau kiri
            }
            else
            {
                // jika strafeDirection = 1, akan strafe ke kanan. Sebaliknya jika strafeDirection = -1, akan strafe ke kiri
                SetTurnRight(30 * strafeDirection);
                SetForward(100);

                if (rand.Next(5) == 0) // 20 persen kemungkinan untuk Stop
                {
                    isStopped = true;
                    SetStop(); // stop semua pergerakan yang sedang berjalan
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
        // putar balik lalu maju kembali ke tengah arena
        TurnRight(180);
        Forward(150);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // Jika bisa ditembak, tembak
        double bearing = BearingTo(e.X, e.Y);
        if (Math.Abs(bearing) < 10 && GunHeat == 0)
        {
            Fire(Math.Min(3, Energy - 0.1));
        }
    }

}
