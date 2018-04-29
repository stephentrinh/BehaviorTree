using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/***************************************************************************************
*    Title: Behavior Tree
*    Author: Stephen Trinh
*    Date: 4/28/18
*    Code version: 1.0
*    Availability: https://github.com/stephentrinh/BehaviorTree
*
***************************************************************************************/

namespace BehaviorTree
{
    public class BlackboardProperties
    {
        public static BlackboardProperty<Transform> AITransform = new BlackboardProperty<Transform>("AITransform");
        public static BlackboardProperty<NavMeshAgent> Agent = new BlackboardProperty<NavMeshAgent>("Agent");
        public static BlackboardProperty<Transform> Destination = new BlackboardProperty<Transform>("Destination");
        public static BlackboardProperty<Waypoint> CurrentWaypoint = new BlackboardProperty<Waypoint>("CurrentWaypoint");
        public static BlackboardProperty<WaypointList> Waypoints = new BlackboardProperty<WaypointList>("Waypoints");
    }
}