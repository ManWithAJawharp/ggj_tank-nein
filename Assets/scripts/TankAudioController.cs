﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

public class TankAudioController : MonoBehaviour 
{
#if true
	CharacterController controller;

	[SerializeField, FMODUnity.EventRef] private string brakeOneShotEvent;
	[SerializeField, FMODUnity.EventRef] private string brakeInstanceEvent;
	[SerializeField, FMODUnity.EventRef] private string engineEvent;
	[SerializeField, FMODUnity.EventRef] private string ambienceEvent;
	
	[SerializeField] private string brakeParam;
	[SerializeField] private string engineSpeedParam;
	[SerializeField] private string engineClutchParam;
	[SerializeField] private string ambienceSpeedParam;
	
	private EventInstance brakeInstance;
	private EventInstance ambienceInstance;
	private EventInstance engineInstance;
	private EventInstance brakeOneshotInstance;

	private ParameterInstance paramTankEngineSpeed;
	private ParameterInstance paramTankEngineClutch;
	private ParameterInstance paramAmbinceSpeed;
	private ParameterInstance paramBrake;

	private void OnEnable() {
		brakeInstance = FMODUnity.RuntimeManager.CreateInstance(brakeInstanceEvent);
		ambienceInstance = FMODUnity.RuntimeManager.CreateInstance(ambienceEvent);
		engineInstance = FMODUnity.RuntimeManager.CreateInstance(engineEvent);
		
		brakeInstance.getParameter(brakeParam, out paramBrake);
		engineInstance.getParameter(engineSpeedParam, out paramTankEngineSpeed);
		engineInstance.getParameter(engineClutchParam, out paramTankEngineClutch);
		ambienceInstance.getParameter(ambienceSpeedParam, out paramAmbinceSpeed);


		brakeInstance.start();
		ambienceInstance.start();
		engineInstance.start();

		controller = GetComponent<CharacterController>();
	}

	
	// Update is called once per frame
	void Update () {

		paramTankEngineSpeed.setValue(controller.velocity.magnitude);
		paramAmbinceSpeed.setValue(controller.velocity.magnitude);

		if (Input.GetButtonDown("L Button")){
			paramBrake.setValue(1f);
			FMODUnity.RuntimeManager.PlayOneShot(brakeOneShotEvent, transform.position);
		}
		if (Input.GetButtonUp("L Button")){
			paramBrake.setValue(0);
		}
	}
	#endif
}