//
// When adding a new level do the following:
//      Step 1: Create a new floor in the unity scene with tilemap template.
//      Step 2: Add new values for: (Constants.cs)
//              - floorTargets
//              - spawnLocations
//              - anim_times
//              - spawnDirections
//      Step 3: Add a new Map to AgilityMaps.maps
//      Step 4: Add a new reward block to OnTriggerEnter() in Agent script
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Text.RegularExpressions;

public class MoveToTargetAgent : Agent
{

    public class Pair<T, U> {
        public Pair() {
        }

        public Pair(T first, U second) {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }

    };

    public bool maskActions = true;

    // Array transformation from untiy coordinates to map indeces:
    // x +25
    //-z + 25
    public enum Direction { North, East, South, West, NorthEast, NorthWest, SouthEast, SouthWest };
    public Vector3 targetLocation;
    private int[,] map;
    public const int MAP_DIAMETER = 50;
    public int maxFloorReached = 1;

    public static Direction[,] divegrid =
    {
        {Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.NorthWest,Direction.NorthWest,Direction.North,Direction.NorthEast,Direction.NorthEast,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East},
        {Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.North,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East},
        {Direction.West,Direction.West,Direction.West,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.North,Direction.North,Direction.North,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.East,Direction.East,Direction.East},
        {Direction.West,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.North,Direction.North,Direction.North,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.East},
        {Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast},
        {Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast},
        {Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast},
        {Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast},
        {Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast},
        {Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.NorthWest,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.North,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast,Direction.NorthEast},
        {Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,0,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East},
        {Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.SouthWest,Direction.SouthWest,Direction.South,Direction.SouthEast,Direction.SouthEast,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East},
        {Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.West,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.South,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East,Direction.East},
        {Direction.West,Direction.West,Direction.West,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.South,Direction.South,Direction.South,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.East,Direction.East,Direction.East},
        {Direction.West,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.South,Direction.South,Direction.South,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.East},
        {Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast},
        {Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast},
        {Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast},
        {Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast},
        {Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast},
        {Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.SouthWest,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.South,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast,Direction.SouthEast}
    };

    // 5  1  6
    // 3  O  4
    // 7  2  8
    //
    // north = 0
    // northeast = 45
    // east = 90
    // southeast = 135
    // south = 180
    // southwest = 225
    // west = 270
    // northwest = 315

    const int k_NoAction = 0;  // do nothing!
    const int k_Up = 1;
    const int k_Down = 2;
    const int k_Left = 3;
    const int k_Right = 4;
    const int k_UpLeft = 5;
    const int k_UpRight = 6;
    const int k_DownLeft = 7;
    const int k_DownRight = 8;
    const int change_dir_north = 9;
    const int change_dir_northeast = 10;
    const int change_dir_east = 11;
    const int change_dir_southeast = 12;
    const int change_dir_south = 13;
    const int change_dir_southwest = 14;
    const int change_dir_west = 15;
    const int change_dir_northwest = 16;
    const int surge = 17;
    const int powerburst = 18;

    //[SerializeField] private Material winMaterial;
    //[SerializeField] private Material loseMaterial;

    [SerializeField] private Direction agentDirection;
    private Direction last_dir;
    [SerializeField] private int anim_cancel_tick;
    private int dir_toggle = 2;
    private int last_action = -1;
    private bool skip_penalty_toggle = false;
    private AgilityMaps agilitymaps = new AgilityMaps();
    //private bool skip_penalty_toggle_walk = false;
    private List<int> actionsHistory;
    private List<float> distanceHistory;
    private float agentScore;
    private float highestScore;
    private List<int> bestActions;
    [SerializeField] private int Max_Step;
    [SerializeField] private int dive_cooldown;
    [SerializeField] private int surge_cooldown_1;
    [SerializeField] private int surge_cooldown_2;
    [SerializeField] private int powerburst_cooldown;

    public override void Initialize()
    {
        actionsHistory = new List<int>();
        distanceHistory = new List<float>();
        agentScore = 0f;
        highestScore = 0f;
        bestActions = new List<int>();
    }

    public override void OnEpisodeBegin()
    {
        //Debug.Log("episode start!");
        transform.localPosition = Constants.spawnLocations[1]; //Change to change start floor of trainer
        agentDirection = Direction.West;
        transform.localRotation = Quaternion.Euler(0, Constants.spawnDirections[1], 0);
        dive_cooldown = 0;
        surge_cooldown_1 = 0;
        surge_cooldown_2 = 0;
        powerburst_cooldown = 0;
        dir_toggle = dir_toggle - 1;
        anim_cancel_tick = 0;
        map = agilitymaps.GetMap(1);
        actionsHistory.Clear();
        distanceHistory.Clear();
        agentScore = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);

        //Formula for floor# from ypos: Y = -0.0200*X + 5.010
        double floorNum = (transform.localPosition.y*-0.02) + 5.010;
        int floorNumAsInt = (int) floorNum;
        //Debug.Log("floorNum");
        //Debug.Log(floorNumAsInt);

        targetLocation = Constants.floorTargets[floorNumAsInt];
        sensor.AddObservation(targetLocation);
    }

    //get distance away in both dims (i.e. 5 up, 5 right)
    //localpos = pos
    //while (updist (5) and rightdist (5) > 0) or counter <= updist + rightdist (10) try to:
        //move right if tile to the right is 0, decrement rightdist, move localpos
                     //else donothing
        //move up if the tile up is 0, decrement updist, move localpos
                  //else donothing
        //counter += 1n
    public int isDiveable(int mypos_x, int mypos_z, int targetpos_x, int targetpos_z)
    {
        //hard coded boundary condition for now; should never hit given outer loop
        if(Mathf.Abs(targetpos_x) > 24 || Mathf.Abs(targetpos_z) > 24){return 1;} //Map Diameter

        // Debug.Log("LocalPos:");
        // Debug.Log(mypos_x);
        // Debug.Log(mypos_z);

        // Debug.Log("TARGET:");
        // Debug.Log(targetpos_x);
        // Debug.Log(targetpos_z);

        //Map Diameter
        mypos_z = (-1*mypos_z) + 25;
        mypos_x = mypos_x + 25;
        targetpos_z = (-1*targetpos_z) + 25;
        targetpos_x = targetpos_x + 25;

        Pair<int, int> localpos = new Pair<int, int>(mypos_z, mypos_x);
        bool movedVert = false;
        bool movedHori = false;

        var vertDist = targetpos_z - mypos_z;
        var vertMag = 0;
        //mag polarity based on array indeces NOT unity system.
        //Distance based on unity system.
        if(vertDist > 0)
        {
            vertMag = 1; //south
        }
        if(vertDist < 0)
        {
            vertDist = vertDist*-1;
            vertMag = -1; //north
        }
        var horiDist = targetpos_x - mypos_x;
        var horiMag = 0;
        if(horiDist > 0)
        {
            horiMag = 1; //east
        }
        if(horiDist < 0)
        {
            horiDist = horiDist*-1;
            horiMag = -1; //west
        }

        // Debug.Log("POLARITY:");
        // Debug.Log("vert:");
        // Debug.Log(vertDist);
        // Debug.Log(vertMag);
        // Debug.Log("hori:");
        // Debug.Log(horiDist);
        // Debug.Log(horiMag);

        while((vertDist > 0) || (horiDist > 0))
        {
            movedVert = false;
            movedHori = false;
            // Debug.Log("LOCALPOS");
            // Debug.Log(-1*(localpos.First - TILEMAP_RADIUS));
            // Debug.Log(localpos.Second - TILEMAP_RADIUS);
            if((horiDist > 0) && map[localpos.First, (localpos.Second + horiMag)] == 0)
            {
                localpos.Second = localpos.Second + horiMag;
                horiDist = horiDist - 1;
                movedHori = true;
            }
            if((vertDist > 0) && map[(localpos.First+ vertMag), localpos.Second] == 0)
            {
                localpos.First = localpos.First + vertMag;
                vertDist = vertDist - 1;
                movedVert = true;
            }
            //we havent reached the goal square but we cant move e/w or n/s to get closer
            if(!movedVert && !movedHori)
            {
                return 1;
            }
        }
        //we only exit if both vert and hori Dist are 0, meaning we can reach the tile
        return 0;
    }

    //get direction player is facing after a dive
    public Direction getDiveDirection(int offset_x, int offset_z)
    {
        //z, x
        //(0, 0) = (10, 10)
        //(1,1) = (9, 11)
        //(-5, 2) = (15, 12)
        return divegrid[10 - offset_z, 10 + offset_x];
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        // Mask the necessary actions if selected by the user.
        if (maskActions)
        {
            // Prevents the agent from picking an action that would make it collide with a boundary wall
            var positionX = (int)transform.localPosition.x;
            var positionZ = (int)transform.localPosition.z;

            // for now mask accel pot
            actionMask.SetActionEnabled(0, powerburst, false);

            //Debug.Log("start");
            //Debug.Log(positionX);
            //Debug.Log(positionZ);
            //Debug.Log(map);

            //Map Diameter
            var maptranspos_X = positionX + 25;
            var maptranspos_Z = (-1*positionZ) + 25;
            //get bytes of 4 adjacent movement options
            var upmask = map[maptranspos_Z-1, maptranspos_X];
            var downmask = map[maptranspos_Z+1, maptranspos_X];
            var leftmask= map[maptranspos_Z, maptranspos_X-1];
            var rightmask = map[maptranspos_Z, maptranspos_X+1];
            var upleftmask = map[maptranspos_Z-1, maptranspos_X-1];
            var uprightmask = map[maptranspos_Z-1, maptranspos_X+1];
            var downleftmask = map[maptranspos_Z+1, maptranspos_X-1];
            var downrightmask = map[maptranspos_Z+1, maptranspos_X+1];
            //Debug.Log(upmask);
            //Debug.Log(uprightmask);
            //Debug.Log(rightmask);

            //mask accordingly
            if (upmask == 1 || anim_cancel_tick == 1){
                actionMask.SetActionEnabled(0, k_Up, false);
                actionMask.SetActionEnabled(0, k_UpLeft, false);
                actionMask.SetActionEnabled(0, k_UpRight, false);
            }
            if (downmask == 1 || anim_cancel_tick == 1){
                actionMask.SetActionEnabled(0, k_Down, false);
                actionMask.SetActionEnabled(0, k_DownLeft, false);
                actionMask.SetActionEnabled(0, k_DownRight, false);
            }
            if (leftmask == 1 || anim_cancel_tick == 1){
                actionMask.SetActionEnabled(0, k_Left, false);
                actionMask.SetActionEnabled(0, k_DownLeft, false);
                actionMask.SetActionEnabled(0, k_UpLeft, false);
            }
            if (rightmask == 1 || anim_cancel_tick == 1){
                actionMask.SetActionEnabled(0, k_Right, false);
                actionMask.SetActionEnabled(0, k_UpRight, false);
                actionMask.SetActionEnabled(0, k_DownRight, false);
            }
            if (upleftmask == 1){actionMask.SetActionEnabled(0, k_UpLeft, false);}
            if (uprightmask == 1){actionMask.SetActionEnabled(0, k_UpRight, false);}
            if (downleftmask == 1){actionMask.SetActionEnabled(0, k_DownLeft, false);}
            if (downrightmask == 1){actionMask.SetActionEnabled(0, k_DownRight, false);}

            //Dive actions
            List<int> diveActions = new List<int>(new int[441]); //(2n+1)^2 is the number of tiles comprising a square of radius n.

            // all movement, turning and dive actions should be disabled for this tick.
            // Also, disable changing dir twice in a row.
            if (anim_cancel_tick == 1 || ((last_action >= 9) && (last_action <= 16))) {
                actionMask.SetActionEnabled(0, change_dir_north, false);
                actionMask.SetActionEnabled(0, change_dir_northeast, false);
                actionMask.SetActionEnabled(0, change_dir_east, false);
                actionMask.SetActionEnabled(0, change_dir_southeast, false);
                actionMask.SetActionEnabled(0, change_dir_south, false);
                actionMask.SetActionEnabled(0, change_dir_southwest, false);
                actionMask.SetActionEnabled(0, change_dir_west, false);
                actionMask.SetActionEnabled(0, change_dir_northwest, false);
            }
            else {
                actionMask.SetActionEnabled(0, k_NoAction, false); // If not animation cancel tick, dont give agent the option to do nothing.
            }

            //mask all dive actions if cooldown up
            if(dive_cooldown > 0 || anim_cancel_tick == 1)
            {
                for (int i = 0; i < diveActions.Count; i++)
                {
                    actionMask.SetActionEnabled(0, i + 19, false);
                }
            }
            //if not, mask just tiles which cannot be dove to
            else
            {
                for (int zi = -10; zi <= 10; zi++)
                {
                    for (int xi = -10; xi <= 10; xi++)
                    {
                        //outofbounds - hard coded boundary for now
                        if(Mathf.Abs(positionX + xi) > 24 || Mathf.Abs(positionZ + zi) > 24)
                        {
                            diveActions[((xi + 10) + (zi + 10)*21)] = 1;
                        }
                        else
                        {
                            diveActions[((xi + 10) + (zi + 10)*21)] = isDiveable(positionX, positionZ, positionX + xi, positionZ + zi);
                        }
                    }
                }
                // Debug.Log("diveactions");
                // var count=0;
                // foreach (int l in diveActions) {
                //     if (l==0) count++;
                // }
                // Debug.Log(count);
                for (int i = 0; i < diveActions.Count; i++)
                {
                    if (diveActions[i] == 1)
                    {
                        actionMask.SetActionEnabled(0, i + 19, false);
                    }
                }
            }

            //surge
            if(surge_cooldown_1 > 0 && surge_cooldown_2 > 0)
            {
                actionMask.SetActionEnabled(0, surge, false);
            }
            else
            {
                //mask surge if it the tile in front of us is a wall
                switch(agentDirection)
                {
                    case(Direction.North):
                        if(upmask == 1)
                        {
                            actionMask.SetActionEnabled(0, surge, false);
                        }
                        break;
                    case(Direction.South):
                        if(downmask == 1)
                        {
                            actionMask.SetActionEnabled(0, surge, false);
                        }
                        break;
                    case(Direction.West):
                        if(leftmask == 1)
                        {
                            actionMask.SetActionEnabled(0, surge, false);
                        }
                        break;
                    case(Direction.East):
                        if(rightmask == 1)
                        {
                            actionMask.SetActionEnabled(0, surge, false);
                        }
                        break;
                    case(Direction.NorthEast):
                        if(uprightmask == 1)
                        {
                            actionMask.SetActionEnabled(0, surge, false);
                        }
                        break;
                    case(Direction.NorthWest):
                        if(upleftmask == 1)
                        {
                            actionMask.SetActionEnabled(0, surge, false);
                        }
                        break;
                    case(Direction.SouthEast):
                        if(downrightmask == 1)
                        {
                            actionMask.SetActionEnabled(0, surge, false);
                        }
                        break;
                    case(Direction.SouthWest):
                        if(downleftmask == 1)
                        {
                            actionMask.SetActionEnabled(0, surge, false);
                        }
                        break;
                    default:
                        Debug.LogError("Invalid surge direction");
                        break;
                }
            }
        }
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        float distanceToTarget = Vector3.Distance(transform.localPosition, targetLocation);
        dive_cooldown = Mathf.Max(0, dive_cooldown - 1);
        surge_cooldown_1 = Mathf.Max(0, surge_cooldown_1 - 1);
        surge_cooldown_2 = Mathf.Max(0, surge_cooldown_2 - 1);

        var action = actions.DiscreteActions[0];
        actionsHistory.Add(action);
        distanceHistory.Add(distanceToTarget);

        // Reward scaling constants
        // Reward = R_dist + R_tele + R_inert

        // alpha = preference for surging and diving
        float alpha = .005f;
        // beta = penalty scaling for distance away from target
        float beta = -.05f;
        // gamma = inertia coeff. Penalty for average distance not changing much
        float gamma = -.01f;
        // omega = inertia lookback. How long we look back for average distance inertia component.
        int omega = 5;


        // Debug.Log("ACTION!");
        // Debug.Log(action);
        // string result = "ACTIONS: ";
        // foreach (var item in actionsHistory)
        // {
        //     result += item.ToString() + ", ";
        // }
        // Debug.Log(result); //Full action list

        //walks only penalize every 2 ticks
        // if((0 <= last_action && last_action <= 8) && (0 <= action && action <= 8) && skip_penalty_toggle_walk == false){
        //     AddReward(0f);
        //     skip_penalty_toggle_walk = true;
        // }
        //dive-surge and surge-dive penalize as a 1tick penalty
        if (((last_action == 17 && action >= 19) || (last_action >= 19 && action == 17)) && skip_penalty_toggle==false){
            AddReward(0f);
            skip_penalty_toggle = true;
        }
        else{
            //AddReward(-.1f);
            //Debug.Log(-1f*distanceToTarget);
            AddReward(-0.5f); //large static negative reward for existing
            //AddReward(beta*distanceToTarget); //Rdist*floor mod //floor mod = *(.1f*(51f-(float)maxFloorReached))
            //skip_penalty_toggle_walk = false;
            skip_penalty_toggle = false;
        }

        //slight preference for surging Rtele
        // if (action >= 17){
        //     AddReward(alpha); //Rsurge
        // }

        if (anim_cancel_tick == 1){
            anim_cancel_tick = 0;
        }

        //Rineff - penalize if last move is closer to target than this move. Track position for last few steps.
        // if (distanceHistory.Count > 10){
        //     float diff;
        //     List<float> diffs = new List<float>();

        //     for (int i = distanceHistory.Count - 1; i > distanceHistory.Count - omega - 1; i--) {

        //         // absolute difference between
        //         // consecutive numbers
        //         diff = Mathf.Abs(distanceHistory[i] - distanceHistory[i - 1]);
        //         diffs.Add(diff);
        //     }
        //     float avg_dist = diffs.Average();
        //     avg_dist = avg_dist + .01f; // we dont want to divide by 0 in the line below.
        //     AddReward(gamma*(1/avg_dist));
        // }
        //penalize changing dir more than once in a row
        // if ((last_action >= 9) && (last_action <= 16)){
        //     if((action >= 9) && (action <=16)){
        //         AddReward(-.1f);
        //     }
        // }

        switch (action)
        {
            case k_NoAction:
                // do nothing
                break;
            case k_Right:
                transform.localPosition += new Vector3(1f, 0f, 0f);
                transform.localRotation = Quaternion.Euler(0, 90, 0);
                //transform.Rotate(0.0f, 90.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                if(!(dir_toggle == 0 && (last_dir == Direction.NorthEast || last_dir == Direction.SouthEast))){
                    agentDirection = Direction.East;
                }
                //Debug.Log(last_dir);
                //Debug.Log(agentDirection);
                break;
            case k_Left:
                transform.localPosition += new Vector3(-1f, 0f, 0f);
                transform.localRotation = Quaternion.Euler(0, 270, 0);
                //transform.Rotate(0.0f, 270.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                if(!(dir_toggle == 0 && (last_dir == Direction.NorthWest || last_dir ==Direction.SouthWest))){
                    agentDirection = Direction.West;
                }
                //Debug.Log(last_dir);
                //Debug.Log(agentDirection);
                break;
            case k_Up:
                transform.localPosition += new Vector3(0f, 0f, 1f);
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                //transform.Rotate(0.0f, 0.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                if(!(dir_toggle == 0 && (last_dir == Direction.NorthEast || last_dir ==Direction.NorthWest))){
                    agentDirection = Direction.North;
                }
                //Debug.Log(last_dir);
                //Debug.Log(agentDirection);
                break;
            case k_Down:
                transform.localPosition += new Vector3(0f, 0f, -1f);
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                //transform.Rotate(0.0f, 180.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                if(!(dir_toggle == 0 && (last_dir == Direction.SouthWest || last_dir ==Direction.SouthEast))){
                    agentDirection = Direction.South;
                }
                //Debug.Log(last_dir);
                //Debug.Log(agentDirection);
                break;
            case k_UpLeft:
                transform.localPosition += new Vector3(-1f, 0f, 1f);
                transform.localRotation = Quaternion.Euler(0, 315, 0);
                //transform.Rotate(0.0f, 315.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                agentDirection = Direction.NorthWest;
                //Debug.Log(last_dir);
                //Debug.Log(agentDirection);
                break;
            case k_UpRight:
                transform.localPosition += new Vector3(1f, 0f, 1f);
                transform.localRotation = Quaternion.Euler(0, 45, 0);
                //transform.Rotate(0.0f, 45.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                agentDirection = Direction.NorthEast;
                //Debug.Log(last_dir);
                //Debug.Log(agentDirection);
                break;
            case k_DownLeft:
                transform.localPosition += new Vector3(-1f, 0f, -1f);
                transform.localRotation = Quaternion.Euler(0, 225, 0);
                //transform.Rotate(0.0f, 225.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                agentDirection = Direction.SouthWest;
                //Debug.Log(last_dir);
                //Debug.Log(agentDirection);
                break;
            case k_DownRight:
                transform.localPosition += new Vector3(1f, 0f, -1f);
                transform.localRotation = Quaternion.Euler(0, 135, 0);
                //transform.Rotate(0.0f, 135.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                agentDirection = Direction.SouthEast;
                //Debug.Log(last_dir);
                //Debug.Log(agentDirection);
                break;
            case change_dir_north:
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                //transform.Rotate(0.0f, 0.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                agentDirection = Direction.North;
                break;
            case change_dir_northeast:
                transform.localRotation = Quaternion.Euler(0, 45, 0);
                //transform.Rotate(0.0f, 45.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                agentDirection = Direction.NorthEast;
                break;
            case change_dir_east:
                transform.localRotation = Quaternion.Euler(0, 90, 0);
                //transform.Rotate(0.0f, 90.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                agentDirection = Direction.East;
                break;
            case change_dir_southeast:
                transform.localRotation = Quaternion.Euler(0, 135, 0);
                //transform.Rotate(0.0f, 135.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                agentDirection = Direction.SouthEast;
                break;
            case change_dir_south:
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                //transform.Rotate(0.0f, 180.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                agentDirection = Direction.South;
                break;
            case change_dir_southwest:
                transform.localRotation = Quaternion.Euler(0, 225, 0);
                //transform.Rotate(0.0f, 225.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                agentDirection = Direction.SouthWest;
                break;
            case change_dir_west:
                transform.localRotation = Quaternion.Euler(0, 270, 0);
                //transform.Rotate(0.0f, 270.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                agentDirection = Direction.West;
                break;
            case change_dir_northwest:
                transform.localRotation = Quaternion.Euler(0, 315, 0);
                //transform.Rotate(0.0f, 315.0f, 0.0f, Space.World);
                last_dir = agentDirection;
                agentDirection = Direction.NorthWest;
                break;
            //surge
            case surge:
                if(surge_cooldown_1 == 0 && surge_cooldown_2 == 0){surge_cooldown_1 = 17;}
                else if(surge_cooldown_1 > 0 && surge_cooldown_2 == 0){surge_cooldown_2 = 17;}
                else if(surge_cooldown_1 == 0 && surge_cooldown_2 > 0){surge_cooldown_1 = 17;}

                //surge is only masked if the space in front of the agent is a wall.
                //transform local position based on direction
                switch (agentDirection){
                    case Direction.North:
                        //decide how far to move forward here
                        for (int tiles_to_move = 10; tiles_to_move > 0; tiles_to_move--)
                        {
                            if(Mathf.Abs(transform.localPosition.z + tiles_to_move) <= 24 && Mathf.Abs(transform.localPosition.x) <= 24 && (isDiveable((int)transform.localPosition.x, (int)transform.localPosition.z,(int)transform.localPosition.x, (int)transform.localPosition.z + tiles_to_move) == 0))
                            {
                                transform.localPosition += new Vector3(0f, 0, (float)(tiles_to_move));
                                break;
                            }
                        }
                        break;
                    case Direction.East:
                        //decide how far to move forward here
                        for (int tiles_to_move = 10; tiles_to_move > 0; tiles_to_move--)
                        {
                            if(Mathf.Abs(transform.localPosition.z) <= 24 && Mathf.Abs(transform.localPosition.x + tiles_to_move) <= 24 && (isDiveable((int)transform.localPosition.x, (int)transform.localPosition.z,(int)transform.localPosition.x + tiles_to_move, (int)transform.localPosition.z) == 0))
                            {
                                transform.localPosition += new Vector3((float)(tiles_to_move), 0, 0f);
                                break;
                            }
                        }
                        break;
                    case Direction.South:
                        //decide how far to move forward here
                        for (int tiles_to_move = 10; tiles_to_move > 0; tiles_to_move--)
                        {
                            if(Mathf.Abs(transform.localPosition.z - tiles_to_move) <= 24 && Mathf.Abs(transform.localPosition.x) <= 24 && (isDiveable((int)transform.localPosition.x, (int)transform.localPosition.z,(int)transform.localPosition.x, (int)transform.localPosition.z - tiles_to_move) == 0))
                            {
                                transform.localPosition += new Vector3(0f, 0, (float)(-1*tiles_to_move));
                                break;
                            }
                        }
                        break;
                    case Direction.West:
                        //decide how far to move forward here
                        for (int tiles_to_move = 10; tiles_to_move > 0; tiles_to_move--)
                        {
                            if(Mathf.Abs(transform.localPosition.z) <= 24 && Mathf.Abs(transform.localPosition.x - tiles_to_move) <= 24 && (isDiveable((int)transform.localPosition.x, (int)transform.localPosition.z,(int)transform.localPosition.x - tiles_to_move, (int)transform.localPosition.z) == 0))
                            {
                                transform.localPosition += new Vector3((float)(-1*tiles_to_move), 0, 0f);
                                break;
                            }
                        }
                        break;
                    case Direction.NorthEast:
                        //decide how far to move forward here
                        for (int tiles_to_move = 10; tiles_to_move > 0; tiles_to_move--)
                        {
                            if(Mathf.Abs(transform.localPosition.z + tiles_to_move) <= 24 && Mathf.Abs(transform.localPosition.x + tiles_to_move) <= 24 && (isDiveable((int)transform.localPosition.x, (int)transform.localPosition.z,(int)transform.localPosition.x + tiles_to_move, (int)transform.localPosition.z + tiles_to_move) == 0))
                            {
                                transform.localPosition += new Vector3((float)tiles_to_move, 0, (float)tiles_to_move);
                                break;
                            }
                        }
                        break;
                    case Direction.SouthEast:
                        //decide how far to move forward here
                        for (int tiles_to_move = 10; tiles_to_move > 0; tiles_to_move--)
                        {
                            if(Mathf.Abs(transform.localPosition.z - tiles_to_move) <= 24 && Mathf.Abs(transform.localPosition.x + tiles_to_move) <= 24 && (isDiveable((int)transform.localPosition.x, (int)transform.localPosition.z,(int)transform.localPosition.x + tiles_to_move, (int)transform.localPosition.z - tiles_to_move) == 0))
                            {
                                transform.localPosition += new Vector3((float)tiles_to_move, 0, (float)(-1*tiles_to_move));
                                break;
                            }
                        }
                        break;
                    case Direction.NorthWest:
                        //decide how far to move forward here
                        for (int tiles_to_move = 10; tiles_to_move > 0; tiles_to_move--)
                        {
                            if(Mathf.Abs(transform.localPosition.z + tiles_to_move) <= 24 && Mathf.Abs(transform.localPosition.x - tiles_to_move) <= 24 && (isDiveable((int)transform.localPosition.x, (int)transform.localPosition.z,(int)transform.localPosition.x - tiles_to_move, (int)transform.localPosition.z + tiles_to_move) == 0))
                            {
                                transform.localPosition += new Vector3((float)(-1*tiles_to_move), 0, (float)tiles_to_move);
                                break;
                            }
                        }
                        break;
                    case Direction.SouthWest:
                        //decide how far to move forward here
                        for (int tiles_to_move = 10; tiles_to_move > 0; tiles_to_move--)
                        {
                            if(Mathf.Abs(transform.localPosition.z - tiles_to_move) <= 24 && Mathf.Abs(transform.localPosition.x - tiles_to_move) <= 24 && (isDiveable((int)transform.localPosition.x, (int)transform.localPosition.z,(int)transform.localPosition.x - tiles_to_move, (int)transform.localPosition.z - tiles_to_move) == 0))
                            {
                                transform.localPosition += new Vector3((float)(-1*tiles_to_move), 0, (float)(-1*tiles_to_move));
                                break;
                            }
                        }
                        break;
                    default:
                        break;
                }
                break;
            //dive
            case int n when (n >= 19):
                dive_cooldown = 17; //17 tick cooldown

                //decode 441 index to (10,10) grid movement, 19:459 => (-10, -10):(10, 10)
                float vertoffset = (-10 + ((n-19)/21));
                float horizoffset = (-10 + ((n-19)%21));

                // Debug.Log("horiz offset");
                // Debug.Log(horizoffset);
                // Debug.Log("vert offset");
                // Debug.Log(vertoffset);

                transform.localPosition += new Vector3(horizoffset, 0, vertoffset);

                //Todo: change agentDirection based on movement direction
                last_dir = agentDirection;
                if (!(horizoffset == 0 && vertoffset == 0)){agentDirection = getDiveDirection((int)horizoffset, (int)vertoffset);}
                //update rotation according to diveDirection
                switch (agentDirection){
                    case Direction.North:
                        transform.localRotation = Quaternion.Euler(0, 0, 0);
                        //transform.Rotate(0.0f, 0.0f, 0.0f, Space.World);
                        break;
                    case Direction.East:
                        transform.localRotation = Quaternion.Euler(0, 90, 0);
                        //transform.Rotate(0.0f, 90.0f, 0.0f, Space.World);
                        break;
                    case Direction.South:
                        transform.localRotation = Quaternion.Euler(0, 180, 0);
                        //transform.Rotate(0.0f, 180.0f, 0.0f, Space.World);
                        break;
                    case Direction.West:
                        transform.localRotation = Quaternion.Euler(0, 270, 0);
                        //transform.Rotate(0.0f, 270.0f, 0.0f, Space.World);
                        break;
                    case Direction.NorthEast:
                        transform.localRotation = Quaternion.Euler(0, 45, 0);
                        //transform.Rotate(0.0f, 45.0f, 0.0f, Space.World);
                        break;
                    case Direction.SouthEast:
                        transform.localRotation = Quaternion.Euler(0, 135, 0);
                        //transform.Rotate(0.0f, 135.0f, 0.0f, Space.World);
                        break;
                    case Direction.NorthWest:
                        transform.localRotation = Quaternion.Euler(0, 315, 0);
                        //transform.Rotate(0.0f, 315.0f, 0.0f, Space.World);
                        break;
                    case Direction.SouthWest:
                        transform.localRotation = Quaternion.Euler(0, 225, 0);
                        //transform.Rotate(0.0f, 225.0f, 0.0f, Space.World);
                        break;
                    default:
                        break;
                }
                break;
            default:
                Debug.LogError("Invalid action value");
                break;
        }

        if(dir_toggle == 0){dir_toggle = 1;}
        else {dir_toggle = dir_toggle - 1;}

        last_action = action;

        if (Academy.Instance.StepCount == Max_Step)
        {
            OnEpisodeEnd();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut){
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.UpArrow))
        {
        discreteActions[0] = 1;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
        discreteActions[0] = 2;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
        discreteActions[0] = 3;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
        discreteActions[0] = 4;
        }
        if (Input.GetKey(KeyCode.W))
        {
        discreteActions[0] = 10;
        }
        if (Input.GetKey(KeyCode.A))
        {
        discreteActions[0] = 12;
        }
        if (Input.GetKey(KeyCode.S))
        {
        discreteActions[0] = 14;
        }
        if (Input.GetKey(KeyCode.D))
        {
        discreteActions[0] = 16;
        }
    }

    private void OnTriggerEnter(Collider other){

        if (other.TryGetComponent<Target>(out Target target)){
            Match match = Regex.Match(target.gameObject.name, @"^(.*?)(\d+)$");
            string doorornot = match.Groups[1].Value;   // Extracts the text part
            string numberPart = match.Groups[2].Value; // Extracts the numeric part

            if(doorornot.Equals("doortolevel"))
            {
                AddReward(+100f);
                int levelnum = int.Parse(numberPart);
                int anim_time = Constants.anim_times[levelnum - 1];
                transform.localPosition = Constants.spawnLocations[levelnum];
                agentDirection = Direction.West;
                transform.localRotation = Quaternion.Euler(0, Constants.spawnDirections[levelnum - 1], 0);
                dive_cooldown = Mathf.Max(0, dive_cooldown - anim_time);
                surge_cooldown_1 = Mathf.Max(0, surge_cooldown_1 - anim_time);
                surge_cooldown_2 = Mathf.Max(0, surge_cooldown_2 - anim_time);
                dir_toggle = 1;
                anim_cancel_tick = 1;
                map = agilitymaps.GetMap(levelnum);
                if(levelnum > maxFloorReached){
                    maxFloorReached = levelnum;
                    Debug.Log($"Reached floor {maxFloorReached}");
                }
            }

            //final room currently implemented, create a new object for end goal?
            else{
                AddReward(+10000f);
                EndEpisode();
            }
        }
    }

    public void OnEpisodeEnd()
    {
        if (agentScore > highestScore)
        {
            highestScore = agentScore;
            bestActions = new List<int>(actionsHistory);
        }

        if (Academy.Instance.StepCount % 1000 == 0)
        {
            SaveBestActions();
        }
    }

    private void SaveBestActions()
    {
        string path = "best_actions.txt";
        System.IO.File.WriteAllLines(path, bestActions.ConvertAll(a => a.ToString()).ToArray());
    }

}