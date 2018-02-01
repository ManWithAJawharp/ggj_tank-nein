﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class TankMovement : MonoBehaviour
{
    CharacterController controller;

    private enum Gear
    {
        Reverse,
        Free,
        GearOne,
        GearTwo,
        GearThree,
        GearFour,
        GearFive
    }

    private Gear currentGear;

    private bool isCoupled;
    public bool engineStopped;

    private int requiredGear;
    private int previousForwardInput;
    private int currentForwardInput;
    private int currentSidewaysInput;
    private float acceleration;
    public float rotation { get; private set; }

    private Image currentGearCursor;
    private Image requiredGearCursor;
    private Image speedBar;
    [SerializeField] private Transform[] GearCursorPositions;

    private RectTransform rpmPivot;

    [SerializeField] private float[] maxAcceleration;
    [SerializeField] private float[] accelerationPeriod;

    [SerializeField] private float angularVelocity;
    [Range(0, 1)] [SerializeField] private float friction;

    private float maxRpm = 5000f;
    private float displayedSpeed = 0;
    [SerializeField]  private float currentSpeed = 0;
    private Vector3 velocity;


    public float rPM;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        currentGear = Gear.Free;
        currentForwardInput = 0;
        isCoupled = false;
    }

    protected void Start()
    {
        MainManager.Manager.TankMovement = this;
        rpmPivot = UIManager.Manager.UI.transform.Find("rPMPivot").GetComponent<RectTransform>();
        currentGearCursor = UIManager.Manager.UI.transform.Find("GearChanging").Find("CurrentGearCursor").GetComponent<Image>();
        requiredGearCursor = UIManager.Manager.UI.transform.Find("GearChanging").Find("RequiredGearCursor").GetComponent<Image>();
        speedBar = UIManager.Manager.UI.transform.Find("SpeedHud").Find("SpeedBar").GetComponent<Image>();
    }

    void Update()
    {
        //currentspeed for debug purposes
        currentSpeed = velocity.magnitude;

        ForwardMovement();
        Rotation();
        HandleCoupling();
        ShowSpeed();

        // If we're inputting forward (is reversed)
        if (currentForwardInput < 0)
        {
            MoveGearcursorUp();
            StopEngineOnTooHighRpm();
        }
        else
        {
            MoveGearCursorDown();
            StopEngineOnTooLowRpm();
        }
    }

    private void MoveGearCursorDown()
    {
        if (rPM <= Mathf.Abs(accelerationPeriod[(int)currentGear - 1]) && (int)currentGear > (int)Gear.GearOne)
        {
            requiredGearCursor.transform.position = GearCursorPositions[(int)currentGear - 1].position;
        }
    }

    private void MoveGearcursorUp()
    {
        if (rPM >= Mathf.Abs(accelerationPeriod[(int)currentGear]) && (int)currentGear < (int)Gear.GearFive)
        {
            requiredGearCursor.transform.position = GearCursorPositions[(int)currentGear + 1].position;
        }
    }

    private void StopEngineOnTooHighRpm()
    {
        if (rPM >= maxRpm - 0.1f)
        {
            StopEngine();
        }
    }

    private void StopEngineOnTooLowRpm()
    {
        if (rPM <= accelerationPeriod[(int)currentGear] - maxRpm / 5 && currentGear != Gear.Free)
        {
            StopEngine();
        }
    }

    private void ForwardMovement()
    {
        if (!engineStopped)
        {
            currentForwardInput = (int)Input.GetAxis("Controlpad Vertical");
            currentForwardInput = -(int)Input.GetAxis("Vertical");
        }
        else
        {
            currentForwardInput = 0;
        }

        if (currentGear == Gear.Reverse) HandleForwardInput(1);
        else if ((int)currentGear > 1) HandleForwardInput(-1);
        else HandleForwardInput(-1);

        acceleration = Acceleration(rPM);
        velocity = friction * controller.velocity + acceleration * transform.forward;

        controller.SimpleMove(velocity);
    }

    private void HandleForwardInput(int input)
    {
        if (currentForwardInput == input)
        {
            rPM += Time.deltaTime * (1100 - (int)currentGear * 100);

        }
        else rPM -= Time.deltaTime * 1500;

        RotateRpmMeter();
    }

    private float Acceleration(float rPM)
    {
        float rpm = Mathf.Min(rPM, accelerationPeriod[(int)currentGear]);
        return 0.5f * maxAcceleration[(int)currentGear] * (Mathf.Cos(Mathf.PI * rpm / accelerationPeriod[(int)currentGear]) - 1);
    }

    private void Rotation()
    {
        if (!engineStopped)
        {
            currentSidewaysInput = (int)Input.GetAxis("Controlpad Horizontal");
            currentSidewaysInput = -(int)Input.GetAxis("Horizontal");

        }
        else
        {
            currentSidewaysInput = 0;
        }
        rotation = -1f / angularVelocity * currentSidewaysInput;
        controller.transform.eulerAngles += rotation * transform.up;
    }

    private void HandleCoupling()
    {
        if (Input.GetButtonDown("R Button")) isCoupled = true;
        else if (Input.GetButtonUp("R Button")) isCoupled = false;

        if (isCoupled)
        {
            if (Input.GetButtonDown("B Button") && (int)currentGear < maxAcceleration.Length - 1)
            {
                currentGear++;
                rPM -= 2000;
                currentGearCursor.transform.position = GearCursorPositions[(int)currentGear].position;
            }
            else if (Input.GetButtonDown("A Button") && (int)currentGear > 0)
            {
                currentGear--;
                rPM += 2000;
                currentGearCursor.transform.position = GearCursorPositions[(int)currentGear].position;
            }
        }
    }

    private void StopEngine()
    {
        engineStopped = true;
        requiredGearCursor.transform.position = GearCursorPositions[2].position;
        currentGear = Gear.Free;
        currentGearCursor.transform.position = GearCursorPositions[1].position;
    }

    public void StartEngine()
    {
        engineStopped = false;
    }

    private void RotateRpmMeter()
    {
        rPM = Mathf.Clamp(rPM, 0, maxRpm);
        float angle = -(125f / maxRpm) * rPM + 30;
        rpmPivot.eulerAngles = Vector3.forward * angle;
    }

    private void ShowSpeed()
    {
        displayedSpeed = Mathf.Lerp(displayedSpeed, velocity.magnitude, 2 * Time.deltaTime);
        speedBar.fillAmount = displayedSpeed / 10f;
    }
}
