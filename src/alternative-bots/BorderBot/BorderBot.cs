using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class BorderBot : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    static void Main(string[] args)
    {
        new BorderBot().Start();
    }

    BorderBot() : base(BotInfo.FromFile("BorderBot.json")) { }

    public override void Run()
    {
        /* Customize bot colors, read the documentation for more information */
        BodyColor = Color.Gray;
        bool kiri = false;
        bool maju = true;
        bool awal = true;
        bool awal2 = true;
        double x = X;
        double y = Y;
        double direction = Direction;
        double width = ArenaWidth;
        double height = ArenaHeight;
        double distanceToWall = 0;
        double angleToWall = 0;
        double angleToBody = 0;
        double wallkiri = x;
        double wallkanan = width - x;
        double wallBawah = y;
        double wallAtas = height - y;
        while (IsRunning) 
        {
            if(awal){
                System.Console.WriteLine("X: " + x + " Y: " + y + " Width: " + width + " Height: " + height);
                System.Console.WriteLine(Direction);
                if(wallkiri < wallkanan && wallkiri < wallBawah && wallkiri < wallAtas){
                    angleToWall = DirectionTo(0, y);
                    distanceToWall = DistanceTo(0, y);
                    System.Console.WriteLine("Kiri "+ angleToWall + " " + distanceToWall);
                }
                else if(wallkanan < wallkiri && wallkanan < wallBawah && wallkanan < wallAtas){
                    angleToWall = DirectionTo(width, y);
                    distanceToWall = DistanceTo(width, y);
                    System.Console.WriteLine("Kanan "+ angleToWall + " " + distanceToWall);
                }
                else if(wallBawah < wallkiri && wallBawah < wallkanan && wallBawah < wallAtas){
                    angleToWall = DirectionTo(x, 0);
                    distanceToWall = DistanceTo(x, 0);
                    System.Console.WriteLine("Bawah "+ angleToWall + " " + distanceToWall);
                }
                else if(wallAtas < wallkiri && wallAtas < wallkanan && wallAtas < wallBawah){
                    angleToWall = DirectionTo(x, height);
                    distanceToWall = DistanceTo(x, height);
                    System.Console.WriteLine("Atas "+ angleToWall + " " + distanceToWall);
                }
                else{
                    angleToWall = DirectionTo(0, y);
                    distanceToWall = DistanceTo(0, y);
                    System.Console.WriteLine("Kiri "+ angleToWall + " " + distanceToWall);
                }

                // disini angleToWall adalah 0,90,180,270 saja.
                angleToWall = angleToWall - Direction;
                if(angleToWall < 0){
                    angleToWall += 360;
                }
                if(angleToWall < 360 - angleToWall){
                    TurnLeft(angleToWall);
                    System.Console.WriteLine("Kanan " + angleToWall);
                }
                else{
                    TurnRight(360 - angleToWall);
                    System.Console.WriteLine("Kiri");
                    System.Console.WriteLine(360 - angleToWall);
                }

                // Bergerak maju menuju tembok
                Forward(distanceToWall-20);  // Bergerak maju hingga mencapai tembok
                TurnRight(90);
                awal = false;
            }
            else{
                AdjustGunForBodyTurn = true; // Separate gun and radar from body   
                if(awal2){
                    // selalu buat gun sejajar dengan body dulu di awal 
                    angleToBody = GunDirection - Direction;
                    if(angleToBody < 0){
                        angleToBody += 360;
                    }
                    if(angleToBody < 360 - angleToBody){
                        TurnGunRight(angleToBody);
                    }
                    else{
                        TurnGunLeft(360 - angleToBody);
                    }
                    awal2 = false;
                }
                else{
                    if(maju){
                        if(DistanceRemaining == 0){
                            SetForward(100);
                            maju = false;
                        }
                    }
                    else {
                        if(DistanceRemaining == 0){
                            SetBack(100);
                            maju = true;
                        }
                    }

                    if(kiri) {
                        if(GunTurnRemaining == 0){
                            SetTurnGunLeft(180);
                            kiri = false;
                        }
                    }
                    else {
                        if(GunTurnRemaining == 0){
                            SetTurnGunRight(180);
                            kiri = true;
                        }
                    }
                }
            }
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        SetFire(3);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Console.WriteLine("Ouch! I hit a bot at " + e.X + ", " + e.Y);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
    }

    /* Read the documentation for more events and methods */
}
