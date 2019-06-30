#pragma warning disable 0414 // private field assigned but not used.

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
 
/// <summary>
/// Streaming player input, or receiving data from server. And then feeds the RCC.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Network/UNet/RCC UNet Network")]
public class RCC_UNetNetwork : NetworkBehaviour {
	
	// Network Identity of the vehicle. All networked gameobjects must have this component.
	private NetworkIdentity networkID;

	// Main RCC, Rigidbody, and WheelColliders. 
	private RCC_CarControllerV3 carController;
	private RCC_WheelCollider[] wheelColliders;
	private Rigidbody rigid;

	// Syncing transform, inputs, configurations, and lights of the vehicle. Not using any SyncVar. All synced data is organized via structs and lists.
	#region STRUCTS

	#region Transform

	public struct VehicleTransform{

		public Vector3 position;
		public Quaternion rotation;

		public Vector3 rigidLinearVelocity;
		public Vector3 rigidAngularVelocity;

	}

	public class SyncListVehicleTransform : SyncListStruct<VehicleTransform>{}

	public SyncListVehicleTransform m_Transform = new SyncListVehicleTransform();

	#endregion Transform

	#region Inputs

	public struct VehicleInput{

		public float gasInput;
		public float brakeInput;
		public float steerInput;
		public float handbrakeInput;
		public float boostInput;
		public float clutchInput;
		public float idleInput;
		public int gear;
		public int direction;
		public bool changingGear;
		public float fuelInput;
		public bool engineRunning;
		public bool canGoReverseNow;

	}

	public class SyncListVehicleInput : SyncListStruct<VehicleInput>{}

	public SyncListVehicleInput m_Inputs = new SyncListVehicleInput();

	#endregion Inputs

	#region Configurations

	public struct VehicleConfigurations{

		public int gear;
		public int direction;
		public bool changingGear;
		public bool semiAutomaticGear;
		public float fuelInput;
		public bool engineRunning;
		public float[] cambers;
		public bool applyEngineTorqueToExtraRearWheelColliders;
		public RCC_CarControllerV3.WheelType wheelTypeChoise;
		public float biasedWheelTorque;
		public bool canGoReverseNow;
		public float engineTorque;
		public float brakeTorque;
		public float minEngineRPM;
		public float maxEngineRPM;
		public float engineInertia;
		public bool useRevLimiter;
		public bool useExhaustFlame;
		public bool useClutchMarginAtFirstGear;
		public float highspeedsteerAngle;
		public float highspeedsteerAngleAtspeed;
		public float antiRollFrontHorizontal;
		public float antiRollRearHorizontal;
		public float antiRollVertical;
		public float maxspeed;
		public float engineHeat;
		public float engineHeatMultiplier;
		public int totalGears;
		public float gearShiftingDelay;
		public float gearShiftingThreshold;
		public float clutchInertia;
		public bool NGear;
		public float launched;
		public bool ABS;
		public bool TCS;
		public bool ESP;
		public bool steeringHelper;
		public bool tractionHelper;
		public bool applyCounterSteering;
		public bool useNOS;
		public bool useTurbo;

	}

	public class SyncListVehicleConfigurations : SyncListStruct<VehicleConfigurations>{}

	public SyncListVehicleConfigurations m_Configurations = new SyncListVehicleConfigurations();

	#endregion Configurations

	#region Lights

	public struct VehicleLights{
		
		// Lights.
		public bool lowBeamHeadLightsOn;
		public bool highBeamHeadLightsOn;

		// For Indicators.
		public RCC_CarControllerV3.IndicatorsOn indicatorsOn;

	}

	public class SyncListVehicleLights : SyncListStruct<VehicleLights>{}

	public SyncListVehicleLights m_Lights = new SyncListVehicleLights();

	#endregion Lights

	#endregion

	private float updateTime = 0;

	VehicleInput currentVehicleInputs = new VehicleInput ();
	VehicleTransform currentVehicleTransform = new VehicleTransform ();
	VehicleLights currentVehicleLights = new VehicleLights ();

	bool CB_running = false;

	void Start(){

		// Getting RCC, wheelcolliders, rigidbody, and Network Identity of the vehicle. 
		carController = GetComponent<RCC_CarControllerV3>();
		wheelColliders = GetComponentsInChildren<RCC_WheelCollider> ();
		rigid = GetComponent<Rigidbody>();
		networkID = GetComponent<NetworkIdentity> ();

		// If we are the owner of this vehicle, disable external controller and enable controller of the vehicle. Do opposite if we don't own this.
		if (isLocalPlayer){

			carController.externalController = false;
			carController.SetCanControl(true);

		}else{

			carController.externalController = true;
			carController.SetCanControl(false);

		}

		// Setting name of the gameobject with Network ID.
		gameObject.name = gameObject.name + networkID.netId;

		currentVehicleTransform = new VehicleTransform ();

	}

	void FixedUpdate(){

		// If we are the owner of this vehicle, disable external controller and enable controller of the vehicle. Do opposite if we don't own this.
		carController.externalController = !isLocalPlayer;
		carController.SetCanControl(isLocalPlayer);

		if (isLocalPlayer) {

			// If we are owner of this vehicle, stream all inputs from vehicle.
//			VehicleIsMein ();
			if(!CB_running)
				StartCoroutine(VehicleIsMein());

		} else {

			// If we are not owner of this vehicle, receive all inputs from server.
			VehicleIsClient ();

		}

	}

	IEnumerator VehicleIsMein (){

		CB_running = true;

		currentVehicleInputs.gasInput = carController.gasInput;
		currentVehicleInputs.brakeInput = carController.brakeInput;
		currentVehicleInputs.steerInput = carController.steerInput;
		currentVehicleInputs.handbrakeInput = carController.handbrakeInput;
		currentVehicleInputs.boostInput = carController.boostInput;
		currentVehicleInputs.clutchInput = carController.clutchInput;
		currentVehicleInputs.idleInput = carController.idleInput;
		currentVehicleInputs.gear = carController.currentGear;
		currentVehicleInputs.direction = carController.direction;
		currentVehicleInputs.changingGear = carController.changingGear;
		currentVehicleInputs.fuelInput = carController.fuelInput;
		currentVehicleInputs.engineRunning = carController.engineRunning;
		currentVehicleInputs.canGoReverseNow = carController.canGoReverseNow;

		CmdVehicleInputs (currentVehicleInputs);

		yield return new WaitForSeconds (.02f);

		currentVehicleLights.lowBeamHeadLightsOn = carController.lowBeamHeadLightsOn;
		currentVehicleLights.highBeamHeadLightsOn = carController.highBeamHeadLightsOn;
		currentVehicleLights.indicatorsOn = carController.indicatorsOn;

		CmdVehicleLights (currentVehicleLights);

		yield return new WaitForSeconds (.02f);

		currentVehicleTransform.position = transform.position;
		currentVehicleTransform.rotation = transform.rotation;
		currentVehicleTransform.rigidLinearVelocity = rigid.velocity;
		currentVehicleTransform.rigidAngularVelocity = rigid.angularVelocity;

		CmdVehicleTransform (currentVehicleTransform);

		yield return new WaitForSeconds (.02f);


		CB_running = false;

	}

	void VehicleIsClient(){

		if (m_Inputs != null && m_Inputs.Count >= 1) {

			carController.gasInput = m_Inputs [0].gasInput;
			carController.brakeInput = m_Inputs [0].brakeInput;
			carController.steerInput = m_Inputs [0].steerInput;
			carController.handbrakeInput = m_Inputs [0].handbrakeInput;
			carController.boostInput = m_Inputs [0].boostInput;
			carController.clutchInput = m_Inputs [0].clutchInput;
			carController.idleInput = m_Inputs [0].idleInput;
			carController.currentGear = m_Inputs [0].gear;
			carController.direction = m_Inputs [0].direction;
			carController.changingGear = m_Inputs [0].changingGear;
			carController.fuelInput = m_Inputs [0].fuelInput;
			carController.engineRunning = m_Inputs [0].engineRunning;
			carController.canGoReverseNow = m_Inputs [0].canGoReverseNow;

		}

		if (m_Lights != null && m_Lights.Count >= 1) {

			carController.lowBeamHeadLightsOn = m_Lights [0].lowBeamHeadLightsOn;
			carController.highBeamHeadLightsOn = m_Lights [0].highBeamHeadLightsOn;
			carController.indicatorsOn = m_Lights [0].indicatorsOn;

		}

		if (m_Transform != null && m_Transform.Count >= 1) {

			Vector3 projectedPosition = m_Transform[0].position + m_Transform[0].rigidLinearVelocity * (Time.time - updateTime);

			if(Vector3.Distance(transform.position, m_Transform[0].position) < 15f){
				
				transform.position = Vector3.Lerp (transform.position, projectedPosition, Time.deltaTime * 5f);
				transform.rotation = Quaternion.Lerp (transform.rotation, m_Transform[0].rotation, Time.deltaTime * 5f);

			}else{
				
				transform.position = m_Transform[0].position;
				transform.rotation = m_Transform[0].rotation;

			}

		}

		updateTime = Time.time;

		

	}

	[Command]
	public void CmdVehicleInputs(VehicleInput _input){

		m_Inputs.Insert (0, _input);
		 
	}

	[Command]
	public void CmdVehicleLights(VehicleLights _input){

		m_Lights.Insert (0, _input);

	}

	[Command]
	public void CmdVehicleTransform(VehicleTransform _input){

		m_Transform.Insert(0, _input);

	}

	[Command]
	public void CmdVehicleConfigurations(VehicleConfigurations _input){

		m_Configurations.Insert (0, _input);

	}

}
