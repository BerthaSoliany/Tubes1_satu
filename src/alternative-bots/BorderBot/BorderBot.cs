using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class BorderBot : Bot
{   
    /* A bot that drives along walls, efficiently scans for enemies, and has an improved escape mechanism */
    static void Main(string[] args)
    {
        new BorderBot().Start();
    }
    
    // State flags
    private bool escaping = false;
    private bool kiri = false;
    private bool maju = true;
    private bool awal = true;
    private bool awal2 = true;
    
    // Wall position tracking
    private bool OnLeftWall = false;
    private bool OnRightWall = false;
    private bool OnTopWall = false;
    private bool OnBottomWall = false;
    
    // Movement tracking
    private double distanceToWall = 0;
    private double angleToWall = 0;
    private double angleToBody = 0;
    
    // Enemy tracking
    private double lastEnemyID = -1;
    private double lastEnemyDirection = -1;
    
    // Constants
    private const double WALL_DISTANCE = 20;       // Distance to maintain from walls
    private const double WALL_THRESHOLD = 30;      // Distance to consider being near a wall
    private const double CORNER_THRESHOLD = 100;    // Distance to consider being in a corner
    private const int SCAN_SWEEP = 180;            // Degrees to sweep gun when scanning
    private const double DEFAULT_FIREPOWER = 3;    // Default bullet power

    BorderBot() : base(BotInfo.FromFile("BorderBot.json")) { }

    public override void Run()
    {
        // Initialize state
        ResetState();
        
        // Set bot colors
        BodyColor = Color.Gray;
        GunColor = Color.DarkGray;
        RadarColor = Color.LightGray;
        ScanColor = Color.Yellow;
        BulletColor = Color.Red;
        
        while (IsRunning) 
        {
            
            // Check if we need to reset to initial state
            if(DistanceToWall() > WALL_THRESHOLD && !escaping && !awal){
                ResetState();
            }
            
            if(!awal2){
                // Handle gun scanning
                if(kiri) {
                    if(GunTurnRemaining == 0){
                        SetTurnGunLeft(SCAN_SWEEP);
                        kiri = false;
                    }
                }
                else {
                    if(GunTurnRemaining == 0){
                        SetTurnGunRight(SCAN_SWEEP);
                        kiri = true;
                    }
                }
            }
            // Handle escape completion
            if(escaping){
                if(DistanceRemaining == 0){
                    // Escape complete
                    escaping = false;
                    // Console.WriteLine("Escape complete!");
                    StopReset();
                    // If we're away from walls, go back to a wall
                    if(DistanceToWall() > WALL_THRESHOLD) {
                        ResetState();
                    } else {
                        // We're already near a wall, update our status
                        UpdateWallFlags();
                    }
                }
            }
            // Initial run - find a wall
            else if(awal){
                GoToNearestWall();
                awal = false;
            }
            // Normal operation - patrol along wall
            else {
                if(IsNearCorner()){
                    AwayFromCorner();
                }
                else {
                    PatrolWall();
                }
            }
            
            // Execute pending commands
            Go();
        }
    }
    
    /// <summary>
    /// Reset the bot's state variables to default values
    /// </summary>
    private void ResetState() {
        kiri = false;
        maju = true;
        awal = true;
        awal2 = true;
        escaping = false;
        OnLeftWall = false;
        OnRightWall = false;
        OnTopWall = false;
        OnBottomWall = false;
        lastEnemyID = -1;
        lastEnemyDirection = -1;
    }
    
    /// <summary>
    /// Patrol along the current wall, scanning for enemies
    /// </summary>
    private void PatrolWall() {
        // Enable independent gun movement
        AdjustGunForBodyTurn = true;
        
        // Initialize gun alignment on first run
        if(awal2){
            // Align gun with body initially
            angleToBody = GunDirection - Direction;
            TurnGun(angleToBody);
            awal2 = false;
        }
        else {
            // Handle forward/backward movement along wall
            if(maju){
                if(DistanceRemaining == 0){
                    SetForward(150);
                    maju = false;
                }
            }
            else {
                if(DistanceRemaining == 0){
                    SetBack(150);
                    maju = true;
                }
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Track the enemy position and time
        // if(e.ScannedBotId == lastEnemyID) {
        //     lastEnemyDirection = e.Direction;
        // }
        // // Calculate distance and optimal firepower
        // double distance = DistanceTo(e.X, e.Y);
        // double firepower = CalculateOptimalFirepower(distance, e.Energy);
        
        // // Fire at the enemy
        // SetFire(firepower);
        SetFire(3);
    }
    
    /// <summary>
    /// Calculate the optimal firepower based on distance and enemy energy
    /// </summary>
    private double CalculateOptimalFirepower(double distance, double enemyEnergy) {
        // Base firepower on distance - closer = more power
        double firepower = DEFAULT_FIREPOWER;
        
        if(distance < 100) {
            firepower = 3.0; // Maximum power at close range
        } else if(distance < 200) {
            firepower = 2.5; // Medium-high power at medium range
        } else if(distance < 300) {
            firepower = 2.0; // Medium power at medium-long range
        } else if(distance < 400) {
            firepower = 1.5; // Medium-low power at long range
        } else {
            firepower = 1.0; // Minimum power at extreme range
        }
        
        // Adjust firepower based on our energy and enemy energy
        if(Energy < 20) {
            // Conserve energy if we're low
            firepower = Math.Min(firepower, 1.0);
        }
        
        if(enemyEnergy < 10) {
            // Finish off low-energy enemies
            firepower = Math.Min(3.0, firepower + 0.5);
        }
        
        return firepower;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Interruptible = false;
        // Console.WriteLine($"Hit bot at X:{e.X} Y:{e.Y}, isRammed:{e.IsRammed}");
        lastEnemyID = e.VictimId;
        if(!escaping) {
            EmergencyEscape();
        }
    }
    
    public override void OnHitWall(HitWallEvent e)
    {
        Interruptible = false;
        // Console.WriteLine($"Hit wall at bearing:{e.Bearing}");
        if(!escaping) {
            EmergencyEscape();
        }
    }
    
    /// <summary>
    /// Improved emergency escape mechanism that works in corners and tight spaces
    /// </summary>
    public void EmergencyEscape() {
        // First, stop any current movement to avoid continuing whatever got us stuck
        if(lastEnemyDirection == -1) return;
        if(!escaping) {
            // Console.WriteLine("Emergency escape!");
            StopReset();
        }
        else{
            // Console.WriteLine("Emergency escape in progress!");
        }
        // Set escaping flag
        escaping = true;
        // Debug output
        // Console.WriteLine($"EMERGENCY ESCAPE at X:{X:F1} Y:{Y:F1} Dir:{Direction:F1}");
        // Console.WriteLine($"Wall distances: L:{X:F1} R:{ArenaWidth-X:F1} B:{Y:F1} T:{ArenaHeight-Y:F1}");
        
        // Check if we're in a corner (close to two walls)
        bool inLeftWallArea = X < CORNER_THRESHOLD;
        bool inRightWallArea = (ArenaWidth - X) < CORNER_THRESHOLD;
        bool inBottomWallArea = Y < CORNER_THRESHOLD;
        bool inTopWallArea = (ArenaHeight - Y) < CORNER_THRESHOLD;
        
        int cornersNearby = 0;
        if(inLeftWallArea) cornersNearby++;
        if(inRightWallArea) cornersNearby++;
        if(inBottomWallArea) cornersNearby++;
        if(inTopWallArea) cornersNearby++;
        
        // connect rammedDirection with last enemy position

        double rammedDirection = lastEnemyDirection;
        // Console.WriteLine("Rammed Direction: " + rammedDirection);
        // Get vector away from walls
        double escapeAngle = 0;
        
        // If we're on the top wall, make sure we have a strong escape vector downward
        if((OnTopWall) || (OnBottomWall)){
            // If we got rammed from right side (rammedDirection between 270-360)
            if(rammedDirection > 90 && rammedDirection < 270) {
                // Add component to move left to avoid getting stuck
                escapeAngle = 180;
            }
            // If we got rammed from left side (rammedDirection between 180-270)
            // else if(rammedDirection > 180 && rammedDirection < 270) {
            else{
                // Add component to move right to avoid getting stuck
                escapeAngle = 0;
            }
        }
        
        // // Similar adjustments for other walls
        // else if(OnBottomWall) {
        //     // Add horizontal component based on rammedDirection
        //     if(rammedDirection > 90 && rammedDirection < 270) {
        //         escapeAngle = 180; // Move left if rammed from right
        //     }
        //     // else if(rammedDirection > 90 && rammedDirection < 180) {
        //     else{
        //         escapeAngle = 0; // Move right if rammed from left
        //     }
        // }
        
        else if((OnLeftWall) || (OnRightWall)) {
            // Add vertical component based on rammedDirection
            if(rammedDirection < 180) {
                escapeAngle = 90; // Move up if rammed from below
            }
            // else if(rammedDirection > 0 && rammedDirection < 90) {
            else{
                escapeAngle = 270; // Move down if rammed from above
            }
        }
        
        // else if(OnRightWall) {
        //     // Add vertical component based on rammedDirection
        //     if(rammedDirection > 180 && rammedDirection < 270) {
        //         escapeAngle = 90; // Move up if rammed from below
        //     }
        //     else if(rammedDirection > 90 && rammedDirection < 180) {
        //         escapeAngle = 270; // Move down if rammed from above
        //     }
        // }
        
        else{
            // Default to moving toward center of arena
            escapeAngle = rammedDirection + 180;
            if(escapeAngle > 360) escapeAngle -= 360;
        }
        
        if(escapeAngle < 0) escapeAngle += 360;
        
        // Special case for corners - move diagonally away
        if(cornersNearby >= 2) {
            // Console.WriteLine("CORNER ESCAPE!");
            
            // Find the diagonal that points to the arena center
            if(inLeftWallArea && inBottomWallArea) {
                if((rammedDirection < 225) && (rammedDirection >= 45)){
                    escapeAngle = 90; // Move up
                }
                else{
                    escapeAngle = 0; // Move right
                }
                // escapeAngle = 45; // Move up-right
            }
            else if(inLeftWallArea && inTopWallArea) {
                if((rammedDirection < 315) && (rammedDirection >= 135)){
                    escapeAngle = 180; // Move down
                }
                else{
                    escapeAngle = 0; // Move right
                }
                // escapeAngle = 315; // Move down-right
            }
            else if(inRightWallArea && inBottomWallArea) {
                if((rammedDirection < 315) && (rammedDirection >= 135)){
                    escapeAngle = 180; // Move left
                }
                else{
                    escapeAngle = 90; // Move up
                }
                // escapeAngle = 135; // Move up-left

            }
            else if(inRightWallArea && inTopWallArea) {
                if((rammedDirection < 225) && (rammedDirection >= 45)){
                    escapeAngle = 180; // Move left
                }
                else{
                    escapeAngle = 270; // Move down
                }
                // escapeAngle = 225; // Move down-left
            }
            
            // Console.WriteLine($"Corner escape angle: {escapeAngle:F1}");
        }
        // Calculate the turn angle needed (use shortest path)
        double turnAngle = escapeAngle - Direction;
        // Console.WriteLine("escapeAngle " + escapeAngle);
        // Console.WriteLine("turnAngle " + turnAngle);
        // Console.WriteLine("Direction " + Direction);
        // CRITICAL: Use SetTurnRight/Left and SetForward to make moves non-blocking
        if(escapeAngle == Direction) {
            SetForward(200);
            // Console.WriteLine("Lari Maju");
        }
        else if(escapeAngle == Direction + 180 || escapeAngle == Direction - 180) {
            SetBack(200);
            // Console.WriteLine("Lari Mundur");
        }
        else{
            SetTurnBody(turnAngle);
            SetForward(200);
            // Console.WriteLine("Lari Belok");
            awal2 = true;
            if(turnAngle < 0){
                kiri = false;
            }
            else{
                kiri = true;
            }
        }
        
        // Move IMMEDIATELY while turning - don't wait for turn to complete
        // Use more power for escape moves
        MaxSpeed = 8; // Maximum speed
        
        // // Keep gun moving
        SetTurnGunLeft(10000);
        
        
    }
    
    /// <summary>
    /// StopReset all movement immediately
    /// </summary>
    public void StopReset() {
        SetForward(0);
        SetBack(0);
        SetTurnLeft(0);
        SetTurnRight(0);
        SetTurnGunLeft(0);
    }
    
    /// <summary>
    /// Calculate the minimum distance to any wall
    /// </summary>
    private double DistanceToWall(){
        double distanceToLeft = X;
        double distanceToRight = ArenaWidth - X;
        double distanceToBottom = Y;
        double distanceToTop = ArenaHeight - Y;
        
        return Math.Min(Math.Min(distanceToLeft, distanceToRight), 
                    Math.Min(distanceToBottom, distanceToTop));
    }
    
    private bool IsNearCorner(){
        return (X < CORNER_THRESHOLD || (ArenaWidth - X) < CORNER_THRESHOLD) &&
                (Y < CORNER_THRESHOLD || (ArenaHeight - Y) < CORNER_THRESHOLD);
    }

    private void AwayFromCorner(){
        Console.WriteLine("Away from corner");
        if(X < CORNER_THRESHOLD){
            if(Y < CORNER_THRESHOLD){
                // Bottom left corner
                if((Direction == 0) || (Direction == 90)){
                    SetForward(100);
                }
                else if((Direction == 180) || (Direction == 270)){
                    SetBack(100);
                }
            }
            else{
                // Top left corner
                if((Direction == 0) || (Direction == 270)){
                    SetForward(100);
                }
                else if((Direction == 90) || (Direction == 180)){
                    SetBack(100);
                }
            }
        }
        else{
            if(Y < CORNER_THRESHOLD){
                // Bottom right corner
                if((Direction == 90) || (Direction == 180)){
                    SetForward(100);
                }
                else if((Direction == 0) || (Direction == 270)){
                    SetBack(100);
                }
            }
            else{
                // Top right corner
                if((Direction == 180) || (Direction == 270)){
                    SetForward(100);
                }
                else if((Direction == 0) || (Direction == 90)){
                    SetBack(100);
                }
            }
        }
    }

    /// <summary>
    /// Update the wall position flags based on current position
    /// </summary>
    private void UpdateWallFlags() {
        // Reset all flags
        OnTopWall = false;
        OnBottomWall = false;
        OnLeftWall = false;
        OnRightWall = false;
        
        // Calculate distances to walls
        double distToLeft = X;
        double distToRight = ArenaWidth - X;
        double distToBottom = Y;
        double distToTop = ArenaHeight - Y;
        
        // Set flags based on which wall is closest AND within threshold
        double minDist = Math.Min(Math.Min(distToLeft, distToRight), 
                            Math.Min(distToBottom, distToTop));
        
        if(minDist < WALL_THRESHOLD) {
            if(minDist == distToLeft) OnLeftWall = true;
            else if(minDist == distToRight) OnRightWall = true;
            else if(minDist == distToBottom) OnBottomWall = true;
            else if(minDist == distToTop) OnTopWall = true;
        }
        
        // Debug output
        // Console.WriteLine($"Wall flags: L:{OnLeftWall} R:{OnRightWall} T:{OnTopWall} B:{OnBottomWall}");
    }
    
    /// <summary>
    /// Navigate to the nearest wall
    /// </summary>
    private void GoToNearestWall(){
        UpdateWallFlags();
        
        // // If already at a wall, skip
        // if(OnLeftWall || OnRightWall || OnTopWall || OnBottomWall) {
        //     // Console.WriteLine("Already at a wall");
        //     return;
        // }
        
       // Calculate distances to walls
        double distToLeft = X;
        double distToRight = ArenaWidth - X;
        double distToBottom = Y;
        double distToTop = ArenaHeight - Y;
        
        double minDist = Math.Min(Math.Min(distToLeft, distToRight), 
                            Math.Min(distToBottom, distToTop));

        // Find the closest wall
        if(minDist == distToLeft){
            // Left wall is closest
            angleToWall = DirectionTo(0, Y);
            distanceToWall = DistanceTo(0, Y);
            OnLeftWall = true;
            Console.WriteLine("Going to Left wall: " + angleToWall + " " + distanceToWall);
        }
        else if(minDist == distToRight){
            // Right wall is closest
            angleToWall = DirectionTo(ArenaWidth, Y);
            distanceToWall = DistanceTo(ArenaWidth, Y);
            OnRightWall = true;
            Console.WriteLine("Going to Right wall: " + angleToWall + " " + distanceToWall);
        }
        else if(minDist == distToBottom){
            // Bottom wall is closest
            angleToWall = DirectionTo(X, 0);
            distanceToWall = DistanceTo(X, 0);
            OnBottomWall = true;
            Console.WriteLine("Going to Bottom wall: " + angleToWall + " " + distanceToWall);
        }
        else {
            // Top wall is closest
            angleToWall = DirectionTo(X, ArenaHeight);
            distanceToWall = DistanceTo(X, ArenaHeight);
            OnTopWall = true;
            Console.WriteLine("Going to Top wall: " + angleToWall + " " + distanceToWall);
        }

        // Calculate turn angle
        double turnAngle = angleToWall - Direction;
        TurnBody(turnAngle);

        // Move toward the wall but stop short
        Forward(distanceToWall - WALL_DISTANCE);
        
        // Turn parallel to the wall
        TurnRight(90);
        
        // Reset states for next move sequence
        awal2 = true;
        kiri = false;
    }
    
    /// <summary>
    /// Turn the body by the specified angle, using the shortest path
    /// </summary>
    private void TurnBody(double angle){
        // Normalize angle to 0-360 range
        while(angle < 0){
            angle += 360;
        }
        while(angle >= 360){
            angle -= 360;
        }
        
        // Choose shortest turning direction
        if(angle <= 180){
            TurnLeft(angle);
            Console.WriteLine("Turning left: " + angle);
        }
        else{
            TurnRight(360 - angle);
            Console.WriteLine("Turning right: " + (360 - angle));
        }
    }
    
    /// <summary>
    /// Set the body to turn by the specified angle, using the shortest path
    /// </summary>
    private void SetTurnBody(double angle){
        // Normalize angle to 0-360 range
        while(angle < 0){
            angle += 360;
        }
        while(angle >= 360){
            angle -= 360;
        }
        
        // Choose shortest turning direction
        if(angle <= 180){
            SetTurnLeft(angle);
        }
        else{
            SetTurnRight(360 - angle);
        }
    }
    
    /// <summary>
    /// Turn the gun by the specified angle, using the shortest path
    /// </summary>
    private void TurnGun(double angle){
        // Normalize angle to 0-360 range
        while(angle < 0){
            angle += 360;
        }
        while(angle >= 360){
            angle -= 360;
        }
        
        // Choose shortest turning direction
        if(angle <= 180){
            TurnGunRight(angle);
        }
        else{
            TurnGunLeft(360 - angle);
        }
    }
    
    /// <summary>
    /// Set the gun to turn by the specified angle, using the shortest path
    /// </summary>
    private void SetTurnGun(double angle){
        // Normalize angle to 0-360 range
        while(angle < 0){
            angle += 360;
        }
        while(angle >= 360){
            angle -= 360;
        }
        
        // Choose shortest turning direction
        if(angle <= 180){
            SetTurnGunRight(angle);
        }
        else{
            SetTurnGunLeft(360 - angle);
        }
    }
}