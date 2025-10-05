using System;
using System.Linq;
using UnityEngine;

public class Car : PhysicsObject
{
    [SerializeField]
    private Seat[] seats;

    [SerializeField]
    private Wheel[] wheels;
    [SerializeField]
    private WheelSteering[] wheelSteerings;
    [SerializeField]
    private Transform steeringWheel;
    [SerializeField]
    private float steeringWheelAngle;
    [SerializeField]
    private Transform centerOfMass;


    private Quaternion steeringWheelStartRotation;


    private void Awake()
    {
        Rigidbody.centerOfMass = centerOfMass.localPosition;
        steeringWheelStartRotation = steeringWheel.localRotation;
    }

    // same as FixedUpdate
    public override void Input(PlayerInputs playerInputs)
    {
        base.Input(playerInputs);

        foreach (var wheel in wheels)
            wheel.Process(playerInputs.Y);

        foreach (var wheelSteering in wheelSteerings)
            wheelSteering.Process(playerInputs.X);

        steeringWheel.localRotation = steeringWheelStartRotation;
        steeringWheel.Rotate(steeringWheel.forward, steeringWheelAngle * playerInputs.X);
    }

    public Seat GetNearestSeat(Vector3 position)
    {
        return seats.ToDictionary(x => x, y => Vector3.Distance(y.transform.position, position))
            .Where(x => x.Key.SeatableRadius > x.Value)
            .OrderBy(x => x.Value)
            .FirstOrDefault().Key;
    }

    public int GetSeatId(Seat seat)
    {
        return Array.IndexOf(seats, seat);
    }

    public Seat GetSeat(int id)
    {
        return seats[id];
    }


    public override void UpdateState(PredictableState state)
    {
        var serverState = state as RigidbodyState;

        if (serverState == null)
        {
            Debug.LogError("Error while applying server predictable state!");
            return;
        }

        var driver = seats.FirstOrDefault().Player;

        if (driver == null || !NetworkRepository.IsCurrentClientOwnerOfObject(driver))
        {
            Rigidbody.MovePosition(serverState.Position);
            Rigidbody.MoveRotation(serverState.Rotation);
            Rigidbody.velocity = serverState.Velocity;
            Rigidbody.angularVelocity = serverState.RotationVelocity;
            return;
        }


        serverStateTransform.position = serverState.Position;
        serverStateTransform.rotation = serverState.Rotation;


        var localState = States.FirstOrDefault(x => x?.Tick == serverState.Tick);

        if (localState == null)
        {
            //Debug.LogWarning($"Client received server state with tick {serverState.Tick}, " +
            //    $"but clients last state tick was {States.Where(x => x != null)?.OrderByDescending(x => x.Tick).First().Tick}");
            return;
        }

        var error = (serverState.Position - (localState as RigidbodyState).Position).magnitude;

        if (error >= NetworkSettings.MaximumError)
        {
            Reconcilate(serverState);

            return;
        }

        SmoothSync(localState as RigidbodyState, serverState);
    }
}