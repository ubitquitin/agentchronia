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

public class Constants
{

    public static Dictionary<int, Vector3> floorTargets;
    public static Dictionary<int, Vector3> spawnLocations;
    public static int[] anim_times;
    public static int[] spawnDirections;

    static Constants()
    {
        floorTargets = new Dictionary<int, Vector3>()
        {
            {1, new Vector3(13.0f, 200.5f, 14.0f)},
            {2, new Vector3(12.0f, 150.5f, 14.0f)},
            {3, new Vector3(6.0f, 100.5f, 11.0f)},
            {4, new Vector3(-9.0f, 50.5f, -2.0f)},
            {5, new Vector3(9.0f, 0.5f, -8.0f)},
            {6, new Vector3(-7, -49.5f, -14.0f)},
            {7, new Vector3(-11.0f, -99.5f, 17.0f)},
            {8, new Vector3(15.0f, -149.5f, 1.0f)},
            {9, new Vector3(-7.0f, -199.5f, -18.0f)},
            {10, new Vector3(10.0f, -249.5f, 13.0f)},
            {11, new Vector3(19.0f, -299.5f, 13.0f)},
            {12, new Vector3(-3.0f, -349.5f, 18.0f)},
            {13, new Vector3(13.0f, -399.5f, 3.0f)},
            {14, new Vector3(3.0f, -449.5f, 11.0f)},
            {15, new Vector3(-1.0f, -499.5f, -4.0f)},
            {16, new Vector3(9.0f, -549.5f, 8.0f)},
            {17, new Vector3(17.0f, -599.5f, 4.0f)},
            {18, new Vector3(0.0f, -649.5f, 7.0f)},
            {19, new Vector3(2.0f, -699.5f, 15.0f)},
            {20, new Vector3(6.0f, -749.5f, 0.0f)},
            {21, new Vector3(13.0f, -799.5f, -10.0f)},
            {22, new Vector3(20.0f, -849.5f, 3.0f)},
            {23, new Vector3(-1.0f, -899.5f, -1.0f)},
            {24, new Vector3(-5.0f, -949.5f, -9.0f)},
            {25, new Vector3(7.0f, -999.5f, 5.0f)},
            {26, new Vector3(1.0f, -1049.5f, 1.0f)},
            {27, new Vector3(0.0f, -1099.5f, 24.0f)},
            {28, new Vector3(-14.0f, -1149.5f, 13.0f)},
            {29, new Vector3(-10.0f, -1199.5f, -1.0f)},
            {30, new Vector3(5.0f, -1249.5f, 14.0f)},
            {31, new Vector3(4.0f, -1299.5f, 11.0f)},
            {32, new Vector3(1.0f, -1349.5f, 3.0f)},
            {33, new Vector3(2.0f, -1399.5f, 10.0f)},
            {34, new Vector3(-1.0f, -1449.5f, -2.0f)},
            {35, new Vector3(-6.0f, -1499.5f, 2.0f)},
            {36, new Vector3(-3.0f, -1549.5f, 5.0f)},
            {37, new Vector3(-12.0f, -1599.5f, 12.0f)},
            {38, new Vector3(8.0f, -1649.5f, 4.0f)},
            {39, new Vector3(-3.0f, -1699.5f, 5.0f)},
            {40, new Vector3(-11.0f, -1749.5f, 4.0f)},
            {41, new Vector3(-1.0f, -1799.5f, 19.0f)},
            {42, new Vector3(-13.0f, -1849.5f, 19.0f)},
            {43, new Vector3(-4.0f, -1899.5f, 7.0f)},
            {44, new Vector3(-5.0f, -1949.5f, 5.0f)},
            {45, new Vector3(0.0f, -1999.5f, -7.0f)},
            {46, new Vector3(-17.0f, -2049.5f, -18.0f)},
            {47, new Vector3(-3.0f, -2099.5f, -2.0f)},
            {48, new Vector3(-2.0f, -2149.5f, -2.0f)},
            {49, new Vector3(-8.0f, -2199.5f, -12.0f)},
            {50, new Vector3(0.0f, -2249.5f, 2.0f)},
            {51, new Vector3(0.0f, -2299.5f, 9.0f)},
        };

        spawnLocations = new Dictionary<int, Vector3>()
        {
            {1, new Vector3(14.0f, 200.5f, 13.0f)},
            {2, new Vector3(14.0f, 150.5f, 13.0f)},
            {3, new Vector3(14.0f, 100.5f, 14.0f)},
            {4, new Vector3(14.0f, 50.5f, 14.0f)},
            {5, new Vector3(14.0f, 0.5f, 14.0f)},
            {6, new Vector3(-14.0f, -49.5f, 14.0f)},
            {7, new Vector3(-24.0f, -99.5f, 24.0f)},
            {8, new Vector3(-24.0f, -149.5f, 24.0f)},
            {9, new Vector3(-24.0f, -199.5f, 16.0f)},
            {10, new Vector3(-7.0f, -249.5f, 21.0f)},
            {11, new Vector3(12.0f, -299.5f, 13.0f)},
            {12, new Vector3(-9.0f, -349.5f, 18.0f)},
            {13, new Vector3(-8.0f, -399.5f, -8.0f)},
            {14, new Vector3(-18.0f, -449.5f, -20.0f)},
            {15, new Vector3(-1.0f, -499.5f, -20.0f)},
            {16, new Vector3(4.0f, -549.5f, 7.0f)},
            {17, new Vector3(8.0f, -599.5f, -19.0f)},
            {18, new Vector3(-11.0f, -649.5f, -10.0f)},
            {19, new Vector3(0.0f, -699.5f, 9.0f)},
            {20, new Vector3(0.0f, -749.5f, 0.0f)},
            {21, new Vector3(-17.0f, -799.5f, -2.0f)},
            {22, new Vector3(-13.0f, -849.5f, 2.0f)},
            {23, new Vector3(-5.0f, -899.5f, -3.0f)},
            {24, new Vector3(-9.0f, -949.5f, -10.0f)},
            {25, new Vector3(5.0f, -999.5f, -4.0f)},
            {26, new Vector3(-5.0f, -1049.5f, -6.0f)},
            {27, new Vector3(-1.0f, -1099.5f, 3.0f)},
            {28, new Vector3(4.0f, -1149.5f, -4.0f)},
            {29, new Vector3(10.0f, -1199.5f, -8.0f)},
            {30, new Vector3(7.0f, -1249.5f, -7.0f)},
            {31, new Vector3(7.0f, -1299.5f, -5.0f)},
            {32, new Vector3(2.0f, -1349.5f, -1.0f)},
            {33, new Vector3(16.0f, -1399.5f, 8.0f)},
            {34, new Vector3(3.0f, -1449.5f, -2.0f)},
            {35, new Vector3(-4.0f, -1499.5f, 2.0f)},
            {36, new Vector3(0.0f, -1549.5f, 5.0f)},
            {37, new Vector3(-2.0f, -1599.5f, -2.0f)},
            {38, new Vector3(8.0f, -1649.5f, 2.0f)},
            {39, new Vector3(0.0f, -1699.5f, 4.0f)},
            {40, new Vector3(-8.0f, -1749.5f, 5.0f)},
            {41, new Vector3(19.0f, -1799.5f, -21.0f)},
            {42, new Vector3(-9.0f, -1849.5f, 19.0f)},
            {43, new Vector3(19.0f, -1899.5f, 18.0f)},
            {44, new Vector3(-5.0f, -1949.5f, 7.0f)},
            {45, new Vector3(0.0f, -1999.5f, -5.0f)},
            {46, new Vector3(-3.0f, -2049.5f, -12.0f)},
            {47, new Vector3(-1.0f, -2099.5f, -2.0f)},
            {48, new Vector3(-2.0f, -2149.5f, 2.0f)},
            {49, new Vector3(-2.0f, -2199.5f, -8.0f)},
            {50, new Vector3(-1.0f, -2249.5f, 9.0f)},
            {51, new Vector3(-2.0f, -2299.5f, 10.0f)},
        };

        // So surge can cancel these animations 1t early. So to model this case, I will subtract 1 from all animation times, and mask everything but surge for the tick after animation.
        //If the path directly infront is blocked, and surge isnt an option, all actions except for donothing (0) will be masked.
        anim_times = new int[] {4, 4, 8, 3, 8,
                                6, 5, 4, 8, 6,
                                8, 5, 8, 8, 8,
                                8, 4, 3, 8, 4,
                                3, 8, 8, 4, 5,
                                3, 5, 3, 8, 8,
                                8, 5, 5, 6, 5,
                                3, 4, 9, 8, 5,
                                5, 6, 4, 5, 3,
                                8, 5, 4, 4, 4,
                                4 // last anim time before lap end
                                };

        spawnDirections = new int[] {270,
                                    270, 180, 270, 270, 90,
                                    180, 90, 90, 180, 90,
                                    90, 90, 90, 0, 0,
                                    0, 90, 90, 90, 90,
                                    90, 90, 90, 0, 0,
                                    90, 0, 270, 0, 0,
                                    270, 0, 270, 270, 270,
                                    270, 0, 270, 270, 270,
                                    270, 270, 180, 180, 180,
                                    270, 180, 180, 180, 180,
                                    };
    }
}
