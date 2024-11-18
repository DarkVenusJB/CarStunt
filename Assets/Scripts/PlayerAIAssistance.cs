using System;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAIAssistance : MonoBehaviour
{
    private RCC_CarControllerV3 carController;

    public RCC_AIWaypointsContainer waypointsContainer;                 // Waypoints Container.
    public static int currentWaypointIndex = 0;                                            // Current index in Waypoint Container.

    // Steer, Motor inputs. Will feed RCC_CarController with these inputs.
    public float SteerInput = 0f;
    
    // Smoothed steering 
    public bool smoothedSteer = true;
    
    // How many waypoints were passed.
    public int totalWaypointPassed = 0;
    public bool ignoreWaypointNow = false;
    
    // Unity's Navigator.
    private NavMeshAgent navigator;
    
    public RCC_CarControllerV3 CarController 
    {
        get 
        {
            if (carController == null)
                carController = GetComponentInParent<RCC_CarControllerV3>();
            return carController;
        }
    }

    private void Awake() 
    {
        // If Waypoints Container is not selected in Inspector Panel, find it on scene.
        if (!waypointsContainer)
            waypointsContainer = FindObjectOfType(typeof(RCC_AIWaypointsContainer)) as RCC_AIWaypointsContainer;

        // Creating our Navigator and setting properties.
        GameObject navigatorObject = new GameObject("PlayerNavigator");
        navigatorObject.transform.SetParent(transform, false);
        navigator = navigatorObject.AddComponent<NavMeshAgent>();
        navigator.radius = 1;
        navigator.speed = 1f;
        navigator.angularSpeed = 100000f;
        navigator.acceleration = 100000f;
        navigator.height = 1;
        navigator.avoidancePriority = 0;
    }

    private void Update()
    {
        // If not controllable, no need to go further.
        if (!CarController.canControl)
            return;

        // Assigning navigator's position to front wheels of the vehicle
        navigator.transform.localPosition = Vector3.zero;
        navigator.transform.localPosition += Vector3.forward * CarController.FrontLeftWheelCollider.transform.localPosition.z;
    }

    private void FixedUpdate()
    {
        // If not controllable, no need to go further.
        if (!CarController.canControl)
            return;
        
        Navigation();
        FeedRCC();// Calculates steerInput based on navigator.
    }

    private void Navigation()
    {
        float navigatorInput = Mathf.Clamp(transform.InverseTransformDirection(navigator.desiredVelocity).x * 1f, -1f, 1f);

        if (navigatorInput > .4f)
            navigatorInput = 1f;

        if (navigatorInput < -.4f)
            navigatorInput = -1f;
        
         // If our scene doesn't have a Waypoint Container, stop and return with error.
         if (!waypointsContainer)
         {
             Debug.LogError("Waypoints Container Couldn't Found!");
             waypointsContainer = FindObjectOfType(typeof(RCC_AIWaypointsContainer)) as RCC_AIWaypointsContainer;
             return;
         }

         // If our scene has Waypoints Container and it doesn't have any waypoints, stop and return with error.
         if (waypointsContainer && waypointsContainer.waypoints.Count < 1) 
         {
             Debug.LogError("Waypoints Container Doesn't Have Any Waypoints!");
             return;
         }
         
         // Next waypoint and its position.
         RCC_Waypoint currentWaypoint = waypointsContainer.waypoints[currentWaypointIndex];

         // Checks for the distance to next waypoint. If it is less than written value, then pass to next waypoint.
         float distanceToNextWaypoint = Vector3.Distance(transform.position, currentWaypoint.transform.position);

         // Setting destination of the Navigator.
         if (navigator.isOnNavMesh) 
             navigator.SetDestination(waypointsContainer.waypoints[currentWaypointIndex].transform.position);

         //  If distance to the next waypoint is not 0, and close enough to the vehicle, increase index of the current waypoint and total waypoint.
         if (distanceToNextWaypoint != 0 && distanceToNextWaypoint < waypointsContainer.waypoints[currentWaypointIndex].radius) 
         {
             currentWaypointIndex++;
             totalWaypointPassed++;

             // If all waypoints were passed, sets the current waypoint to first waypoint and increase lap.
             if (currentWaypointIndex >= waypointsContainer.waypoints.Count)
                 currentWaypointIndex = 0;
             
             // Setting destination of the Navigator. 
             if (navigator.isOnNavMesh)
                 navigator.SetDestination(waypointsContainer.waypoints[currentWaypointIndex].transform.position);
         }
         
         if (Math.Abs(CarController.inputs.steerInput)  < 0.1f)
         {
             SteerInput = navigatorInput;
             SteerInput = Mathf.Clamp(SteerInput, -1f, 1f) * CarController.direction;
         }
         
         else SteerInput = 0;
    }

    private void FeedRCC()
    {
        // Feeding steerInput of the RCC.
        if (Math.Abs(CarController.inputs.steerInput) < 0.1f)
        {
            if (smoothedSteer)
                CarController.steerInput = Mathf.Lerp(CarController.steerInput, SteerInput, Time.deltaTime * 20f);
            else
                CarController.steerInput = SteerInput;
        }
       
    }
}
