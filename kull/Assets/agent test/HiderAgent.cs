using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class HiderAgent : Agent
{
    Rigidbody rBody;
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    //Körs varjegång agenten hittar target eller är utanför banan.
    public Transform Seeker;
    public override void OnEpisodeBegin()
    {

        // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3( 0, 0.5f, 0);
        }

        //Flytta target till en ny position
        //Seeker.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4); 
    }

    //indata för miljön
    public override void CollectObservations(VectorSensor sensor)
    {
        // Seeker and Agent positions
        sensor.AddObservation(Seeker.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    private float turnSmoothVelocity;
    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        float horizontal = actionBuffers.ContinuousActions[0];
        float vertical = actionBuffers.ContinuousActions[1];
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = (Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg);
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, 0.1f);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            rBody.AddForce(direction * forceMultiplier);
        }

        // Rewards
        float distanceToSeeker = Vector3.Distance(this.transform.localPosition, Seeker.localPosition);

        // Reached target
        if (distanceToSeeker < 1.42f)
        {
            SetReward(-1.0f);
            EndEpisode();
        } 
        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            SetReward(-0.2f);
            EndEpisode();
        }
        else {
            SetReward(0.1f);
        }

       
    }

    //test för styrning med keyboard
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

}