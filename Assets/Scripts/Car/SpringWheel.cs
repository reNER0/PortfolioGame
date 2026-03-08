using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringWheel : MonoBehaviour
{
    public float gripX;
    public float gripY;
    public float gripDamping;
    public float maxSlip;
    public float slipJumpPercent;
    private Vector3 gripPoint;
    private float lastLateralSpringDistance;

    public Rigidbody vehicleBody;

    //Hit Detection
    RaycastHit hit;
    public LayerMask layerMask;
    public bool isGrounded;

    //Suspension
    public float restLength;
    public float wheelRadius;
    public float currentLength;
    private float lastLength;
    public float springStiffness;
    public float damperStiffness;
    public Vector3 fZ;
    private float suspensionForce;

    //Wheel Motion
    float totalTorque;
    public float wheelWeight;
    public float wheelDamping;
    public float wheelAngularVelocity;
    public Vector3 localVelocity;
    // Estimated angular velocity based on the local velocity of the wheel and its radius
    Vector3 localAngularVelocity;
    Vector3 longitudinalDir;
    Vector3 lateralDir;

    //Lateral Friction
    float slipAngle;
    float muX;
    public Vector3 fX;

    //Longitudinal Friction
    float slipSpeed;
    float muY;
    public Vector3 fY;

    private float driveTorqueInput;   // то, что приходит от мотора/коробки на это колесо (Н·м)
    private float brakeTorqueInput;   // тормоз на это колесо (Н·м)

    private void Awake()
    {
        gripPoint = transform.position;
    }

    public void Process()
    {
        if (Physics.Raycast(transform.position, -transform.up, out hit, restLength + wheelRadius, layerMask)) //Fire a raycast to get the distance between the toplink and the ground
        {
            if(!isGrounded)
                gripPoint = hit.point; //Set the grip point to the point of contact if we just became grounded

            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        if (isGrounded) //If we hit something,
        {
            //Calculate and apply the suspension force (Fz)
            currentLength = hit.distance - wheelRadius;
            CalculateSuspensionForce();
            ApplySuspensionForce();

            //Calculate and apply the friction force (Fx, Fy)
            GetLocalVelocity();
            CalculateLateralFriction();
            CalculateLongitudinalFriction();
            ApplyFrictionForce();

            // GetSimpleTireForce();
            // ApplySimpleTireForce();
        }
        else //If we don't,
        {
            ResetValues(); //Reset values that need resetting
            GetWheelMotionInAir(); //Keep the wheel's ability to spin
        }
    }

    void CalculateSuspensionForce()
    {
        //Hooke's Law
        float springDisplacement = restLength - currentLength;
        float springForce = springDisplacement * springStiffness;

        //Damping Equation
        float springVelocity = (lastLength - currentLength) / Time.fixedDeltaTime;
        float damperForce = springVelocity * damperStiffness;

        suspensionForce = springForce + damperForce;
        fZ = hit.normal.normalized * suspensionForce; //Suspension force acts perpendicular to the contact patch

        lastLength = currentLength; //Set the lastLength for the next frame
    }
    void ApplySuspensionForce()
    {
        vehicleBody.AddForceAtPosition(fZ, transform.position); //Apply the suspension force to the vehicle at the toplink position
    }

    void GetLocalVelocity()
    {
        //Get the velocity of the wheel relative to the ground
        //RB.GetPointVelocity Does Not Update w/ Substeps, If There's A Way To Get This Value Without The Use Of RB Functions, We Can Substep The Whole VP Implementation And Keep The Timestep @ 0.02
        localVelocity = transform.InverseTransformDirection(vehicleBody.GetPointVelocity(hit.point));
        localAngularVelocity = localVelocity / wheelRadius; // omega = v / r

        //Lateral and longitudinal directions of motion of the wheel
        longitudinalDir = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
        lateralDir = Vector3.ProjectOnPlane(transform.right, hit.normal).normalized;
    }

    void CalculateLateralFriction()
    {
        var springVector = gripPoint - hit.point;

        if (springVector.magnitude > maxSlip) 
        {
            // Slip happened
            springVector = Vector3.ClampMagnitude(springVector, maxSlip * (1 - slipJumpPercent));
            gripPoint = hit.point + springVector; //Move the grip point to the end of the spring vector, which is clamped by maxSlip
        }


        var localSpringVector = transform.InverseTransformDirection(springVector);

        var lateralSpringDistance = localSpringVector.x;
        var longitudinalSpringDistance = localSpringVector.z;

        fX = transform.right * (lateralSpringDistance * gripX + gripDamping * (lateralSpringDistance - lastLateralSpringDistance) / Time.fixedDeltaTime);
        fX *= suspensionForce;

        lastLateralSpringDistance = lateralSpringDistance;
    }

    void CalculateLongitudinalFriction()
    {
        int substeps = 5;
        float subDT = Time.fixedDeltaTime / (float)substeps;
        var accumulatedForce = Vector3.zero;
        //float slipSpeedPeak = 4f;
        for (int i = 0; i < substeps; i++)
        {
            float inertia = 0.5f * wheelWeight * wheelRadius * wheelRadius;

            float angularAcceleration = 0;

            // Угловое ускорение от силы мотора
            float torque = driveTorqueInput * wheelRadius;
            var slipLimit = (Math.Abs(localVelocity.z) + 30) / wheelRadius;
            if(Math.Abs(wheelAngularVelocity) < slipLimit)
                angularAcceleration += torque / inertia;

            float brakeTorque = brakeTorqueInput * wheelRadius;
            angularAcceleration -= Math.Sign(wheelAngularVelocity) * brakeTorque / inertia;

            // Новая угловая скорость
            wheelAngularVelocity += angularAcceleration * subDT;

            // Линейная скорость обода колеса
            var linearVelocity = wheelAngularVelocity * wheelRadius;

            gripPoint += longitudinalDir * linearVelocity * subDT;


            var springVector = gripPoint - hit.point;
            bool slipped = springVector.magnitude > maxSlip;

            if (slipped)
            {
                // Slip happened
                springVector = Vector3.ClampMagnitude(springVector, maxSlip * (1 - slipJumpPercent));
                gripPoint = hit.point + springVector; //Move the grip point to the end of the spring vector, which is clamped by maxSlip
            }


            Debug.DrawLine(hit.point, gripPoint, Color.blue);

            var localSpringVector = transform.InverseTransformDirection(springVector);

            var longitudinalSpringDistance = localSpringVector.z;

            //if(slipped)
            //    lastValue = longitudinalSpringDistance;

            //var longitudinalSpringDistance = linearVelocity - localVelocity.z;

            var groundForce = longitudinalSpringDistance * gripY;
            groundForce *= suspensionForce;
            var wheelForce = longitudinalSpringDistance * gripY;
            wheelForce *= suspensionForce;
            wheelForce += wheelWeight * wheelDamping * (longitudinalSpringDistance - lastWheelValue) / subDT;
            //var force = longitudinalSpringDistance * grip - gripDamping * localVelocity.z / subDT;

            // Угловое ускорение от силы земли
            float torqueFromGround = wheelForce * wheelRadius; // момент
            float angularAccelFromGround = torqueFromGround / inertia;

            // Новая угловая скорость
            wheelAngularVelocity -= angularAccelFromGround * subDT;

            accumulatedForce += longitudinalDir * groundForce;
            //fY = longitudinalDir * force;

            lastWheelValue = longitudinalSpringDistance;
        }

        fY = accumulatedForce / (float)substeps;

    }
    private float lastWheelValue;

    void GetWheelMotionInAir()
    {
        int substeps = 5;
        float subDT = Time.fixedDeltaTime / (float)substeps;
        for (int i = 0; i < substeps; i++)
        {
            float driveTorque = driveTorqueInput; //Temp, will come from drivetrain later
            float totalTorque = driveTorque;

            float wheelAngularAcceleration = totalTorque / wheelWeight;
            wheelAngularVelocity += wheelAngularAcceleration * subDT;
        }
    }

    void ApplyFrictionForce()
    {
        vehicleBody.AddForceAtPosition(fX + fY, hit.point);
    }

    public void ApplyTorque(float driveTorque)
    {
        driveTorqueInput = driveTorque;
    }

    public void Brake(float brakeTorque)
    {
        brakeTorqueInput = Mathf.Max(0f, brakeTorque);
    }

    void ResetValues()
    {
        lastLength = currentLength = restLength; //Fully extend suspension

        slipAngle = slipSpeed = 0.0f; //Set wheel slip to zero
        muX = muY = 0.0f; //Set friction coefficients to zero
        fX = fY = fZ = Vector3.zero; //Set forces to zero

        // fZ = simpleTireForce = Vector3.zero; //Set forces to zero
    }


    public float GetWheelSpeed()
    {
        float linearSpeed = wheelAngularVelocity * wheelRadius; // м/с
        return linearSpeed;
    }

    public float GetGroundSpeedKmh()
    {
        return localVelocity.z;
    }
}
