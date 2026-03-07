using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public enum DriveTrain
{
    AWD,
    FWD,
    RWD
}

public enum Gear
{
    Drive,
    Reverse,
    Parking
}

[Serializable]
public struct Engine 
{
    public float engineForce;
    public AnimationCurve engineCurve;
    public float maxSpeed;

    public float GetEngineForce(float speed)
    {
        var speedPercent = speed / maxSpeed;

        if (speedPercent > 1)
            return 0;

        return engineCurve.Evaluate(speedPercent) * engineForce;
    }
}

public class Car : PhysicsObject
{
    [SerializeField]
    private Engine engine;

    [SerializeField]
    private DriveTrain driveTrain;

    [SerializeField]
    private float brakeForce;

    [SerializeField]
    private Seat[] seats;

    [SerializeField]
    private SpringWheel[] wheels;
    [SerializeField]
    private WheelSteering[] wheelSteerings;
    [SerializeField]
    private Transform steeringWheel;
    [SerializeField]
    private float steeringWheelAngle;
    [SerializeField]
    private Transform centerOfMass;

    private Quaternion steeringWheelStartRotation;

    private Gear gear = Gear.Parking;


    private void Awake()
    {
        Rigidbody.centerOfMass = centerOfMass.localPosition;
        steeringWheelStartRotation = steeringWheel.localRotation;
    }

    // same as FixedUpdate
    public override void Input(PlayerInputs playerInputs)
    {
        base.Input(playerInputs);

        if (Math.Abs(GetWheelsSpeed()) < 0.25f) 
        {
            if (playerInputs.Y < 0 && Rigidbody.velocity.magnitude < 0.1f)
                gear = Gear.Reverse;
            else
                gear = Gear.Drive;
        }

        float gasInput = 0;
        float brakeInput = 1;
        float steerInput = playerInputs.X;

        switch (gear)
        {
            case Gear.Drive:
                gasInput = Math.Max(playerInputs.Y, 0);
                brakeInput = Math.Max(-playerInputs.Y, 0);
                break;
            case Gear.Reverse:
                gasInput = Math.Min(playerInputs.Y, 0);
                brakeInput = Math.Max(playerInputs.Y, 0);
                break;
        }

        var engineTorque = engine.GetEngineForce(Rigidbody.velocity.magnitude) * gasInput;

        switch (driveTrain)
        {
            case DriveTrain.AWD:
                foreach (var wheel in wheels)
                    wheel.ApplyTorque(engineTorque / wheels.Length);
                break;
            case DriveTrain.FWD:
                foreach (var wheel in wheels.Take(2))
                    wheel.ApplyTorque(engineTorque / 2);
                break;
            case DriveTrain.RWD:
                foreach (var wheel in wheels.Skip(2))
                    wheel.ApplyTorque(engineTorque / (wheels.Length - 2));
                break;
        }

        foreach (var wheel in wheels)
            wheel.Brake(brakeInput * brakeForce);

        foreach (var wheel in wheels)
            wheel.Process();

        foreach (var wheelSteering in wheelSteerings)
            wheelSteering.Process(steerInput);

        steeringWheel.localRotation = steeringWheelStartRotation;
        steeringWheel.Rotate(steeringWheel.forward, steeringWheelAngle * playerInputs.X);
    }

    private float GetWheelsSpeed()
    {
        switch (driveTrain)
        {
            case DriveTrain.AWD:
                return wheels.Average(w => Mathf.Abs(w.GetWheelSpeed()));

            case DriveTrain.FWD:
                return wheels
                    .Take(2)
                    .Average(w => Mathf.Abs(w.GetWheelSpeed()));

            case DriveTrain.RWD:
                return wheels
                    .Skip(2)
                    .Average(w => Mathf.Abs(w.GetWheelSpeed()));

            default:
                return 0;
        }
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


    protected override void FixedUpdate()
    {
        var serverState = lastServerState as RigidbodyState;

        if (serverState == null)
        {
            //Debug.LogError("Error while applying server predictable state!");
            return;
        }

        var driver = seats.FirstOrDefault().Player;

        /*
        if (driver == null || !NetworkRepository.Current.IsCurrentClientOwnerOfObject(driver))
        {
            Rigidbody.MovePosition(serverState.Position);
            Rigidbody.MoveRotation(serverState.Rotation);
            Rigidbody.velocity = serverState.Velocity;
            Rigidbody.angularVelocity = serverState.RotationVelocity;
            return;
        }
        */

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

        if (driver != null && NetworkRepository.Current.IsCurrentClientOwnerOfObject(driver))
        {
            SmoothSync(localState as RigidbodyState, serverState, NetworkSettings.ClientSidePredictionType);
            return;
        }

        SmoothSync(localState as RigidbodyState, serverState, NetworkSettings.ErrorCorrectionType);
    }
}