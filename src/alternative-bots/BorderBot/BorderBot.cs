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
    private bool kiri = false; // boolean for radar sweeping direction
    private bool maju = true; // boolean for oscillating movement
    private bool awal = true; // boolean for position state of the bot. false if on a wall, otherwise true.
    private bool awal2 = true; // boolean for adjusting gun direction to body direction 
    
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
            if(DistanceToWall() > WALL_THRESHOLD && !escaping && !awal){ // on the middle of the arena, but not escaping
                ResetState();
            }
            
            if(!awal2){ // no need to reset gun direction
                ScanSweepRadar();
            }
            if(escaping){
                if(DistanceRemaining == 0){ // escape finished
                    escaping = false;
                    StopReset();
                    if(DistanceToWall() > WALL_THRESHOLD) {
                        ResetState(); // -> GoToNearestWall()
                    } else {
                        UpdateWallFlags();
                    }
                }
            }
            else if(awal){ // initial state, go to nearest wall
                GoToNearestWall();
                awal = false;
            }
            else { // already at a wall
                if(IsNearCorner()){
                    AwayFromCorner(); // avoid corner
                }
                else {
                    PatrolWall(); // osilasi di dinding
                }
            }
            
            Go();
        }
    }

    private void ScanSweepRadar() { // pergerakan gun dan radar, 180 derajat bolak-balik, dapat scanning seluruh arena.
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

    private void ResetState() { // reset all value to default
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
    
    private void PatrolWall() { // when bot on wall, align gun with body then oscillate
        // Enable independent gun movement
        AdjustGunForBodyTurn = true;
        
        if(awal2){
            // Align gun with body initially
            angleToBody = GunDirection - Direction;
            TurnGun(angleToBody);
            awal2 = false;
        }
        else {
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
        if(e.ScannedBotId == lastEnemyID) { // save the direction of enemy that ramming this bot.
            lastEnemyDirection = e.Direction;
        }
        SetFire(3);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Interruptible = false;
        lastEnemyID = e.VictimId; // save the id of the enemy that ramming this bot.
        if(!escaping) {
            EmergencyEscape();
        }
    }
    
    public override void OnHitWall(HitWallEvent e)
    {
        Interruptible = false;
        if(!escaping) {
            EmergencyEscape();
        }
    }

    public void EmergencyEscape() { // escape from the enemy, to the counter direction from the rammed direction
        if(lastEnemyDirection == -1) return;
        if(!escaping) {
            StopReset(); // prioritas escape
        }
        escaping = true;
        
        bool inLeftWallArea = X < CORNER_THRESHOLD;
        bool inRightWallArea = (ArenaWidth - X) < CORNER_THRESHOLD;
        bool inBottomWallArea = Y < CORNER_THRESHOLD;
        bool inTopWallArea = (ArenaHeight - Y) < CORNER_THRESHOLD;
        
        int cornersNearby = 0;
        if(inLeftWallArea) cornersNearby++;
        if(inRightWallArea) cornersNearby++;
        if(inBottomWallArea) cornersNearby++;
        if(inTopWallArea) cornersNearby++;
        
        // hubungkan rammedDirection with last enemy position
        double rammedDirection = lastEnemyDirection;
        double escapeAngle = 0;
        
        if((OnTopWall) || (OnBottomWall)){
            // If rammed from right 
            if(rammedDirection > 90 && rammedDirection < 270) {
                escapeAngle = 180; // move left
            }
            // If rammed from left
            else{
                escapeAngle = 0; // move right
            }
        }
        
        else if((OnLeftWall) || (OnRightWall)) {
            // If rammed from below
            if(rammedDirection < 180) {
                escapeAngle = 90; // Move up 
            }
            // If rammed from above
            else{ 
                escapeAngle = 270; // Move down
            }
        }
        
        else{
            // Default to moving toward center of arena
            escapeAngle = rammedDirection + 180;
            if(escapeAngle > 360) escapeAngle -= 360;
        }
        
        if(escapeAngle < 0) escapeAngle += 360;
        
        // corner handling
        if(cornersNearby >= 2) {
            if(inLeftWallArea && inBottomWallArea) { // corner kiri bawah
                if((rammedDirection < 225) && (rammedDirection >= 45)){
                    escapeAngle = 90; // Move up
                }
                else{
                    escapeAngle = 0; // Move right
                }
            }
            else if(inLeftWallArea && inTopWallArea) { // corner kiri atas
                if((rammedDirection < 315) && (rammedDirection >= 135)){
                    escapeAngle = 270; // Move down
                }
                else{
                    escapeAngle = 0; // Move right
                }
            }
            else if(inRightWallArea && inBottomWallArea) { // corner kanan bawah
                if((rammedDirection < 315) && (rammedDirection >= 135)){
                    escapeAngle = 180; // Move left
                }
                else{
                    escapeAngle = 90; // Move up
                }
            }
            else if(inRightWallArea && inTopWallArea) { // corner kanan atas
                if((rammedDirection < 225) && (rammedDirection >= 45)){
                    escapeAngle = 180; // Move left
                }
                else{
                    escapeAngle = 270; // Move down
                }
            }
        }
        double turnAngle = escapeAngle - Direction;
        if(escapeAngle == Direction) { // searah, lari maju.
            SetForward(200);
        }
        else if(escapeAngle == Direction + 180 || escapeAngle == Direction - 180) { // berlawanan, lari mundur.
            SetBack(200);
        }
        else{ // corner atau not on wall, lari belok.
            SetTurnBody(turnAngle);
            SetForward(200);
            awal2 = true;
            // ini untuk nanti ketika ke wall lagi, adjust gun ke body
            if(turnAngle < 0){
                kiri = false;
            }
            else{
                kiri = true;
            }
        }

        MaxSpeed = 8; // Maximum speed

        // // Keep gun moving
        SetTurnGunLeft(10000);
    }
    
    public void StopReset() { // stop all movement
        SetForward(0);
        SetBack(0);
        SetTurnLeft(0);
        SetTurnRight(0);
        SetTurnGunLeft(0);
    }
    
    private double DistanceToWall(){ // returns the distance to closest wall
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

    private void AwayFromCorner(){ // move away from corner
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

    private void UpdateWallFlags() { // update bot's position state
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
    }
    
    private void GoToNearestWall(){ // like it said, go to nearest wall
        UpdateWallFlags();
        
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

        double turnAngle = angleToWall - Direction;
        TurnBody(turnAngle);

        Forward(distanceToWall - WALL_DISTANCE);
        
        TurnRight(90);
        
        awal2 = true;
        kiri = false;
    }
    
    // helper functions
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
