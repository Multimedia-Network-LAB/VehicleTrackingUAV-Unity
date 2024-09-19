using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMover : MonoBehaviour
{
    private TargetWaypoint _targetWaypoint;
    private float moveSpeed = 5f;
    private float distanceToWaypoint = 0.1f;
    
    private Transform currentWaypoint;
    public Vector3 CurrentPosition{get; private set;}

    private void Awake()
    {
        _targetWaypoint = FindObjectOfType<TargetWaypoint>();
        //initializing the current waypoint to the first waypoint
        currentWaypoint = _targetWaypoint.GetNextWayPoint(currentWaypoint);
        transform.position = currentWaypoint.position;
        
        //setting the next waypoint
        currentWaypoint = _targetWaypoint.GetNextWayPoint(currentWaypoint);
        transform.LookAt(currentWaypoint);
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position,currentWaypoint.position) < distanceToWaypoint)
        {
            currentWaypoint = _targetWaypoint.GetNextWayPoint(currentWaypoint);
            transform.LookAt(currentWaypoint);
        }
    }
    
}