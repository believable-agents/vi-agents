using System;
using UnityEngine;
using System.Threading;

using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;


namespace ViAgents.Physiology
{
	[RequireComponent(typeof(ViAgent))]
	public class PhysiologyModel : MonoBehaviour
	{
		[Range(0f,4f)]
		public float speedMultiplier = 1;

		[Range(0f,5f)]
		public float hungerModifier = 1;
		[Range(0f,5f)]
		public float thirstModifier = 1;
		[Range(0f,5f)]
		public float energyModifier = 1; 

		public float hunger;
		public float thirst;
		public float energy;

        public float hungerT;
        public float thirstT;
        public float energyT;

		float sleepMultiplier = 0.0f;

		static DayNightCycle dayNight;
		static Transform player;

		ViAgent agent;

		// decay rates
		// we get hungry twice a day
		static float hungerDecay;
		static float thirstDecay;
		static float energyDecay;

		static float updateRateInSeconds = 1;

        // aram code
        GameObject hungerBar ;
        GameObject thirstBar;
        GameObject energyBar;
      

//		static bool play = true;
//
//#if UNITY_EDITOR
//		static PhysiologyModel() {
//			EditorPlayMode.PlayModeChanged += (current, changed) => {
//				if (changed.Equals (PlayModeState.Playing)) {
//					play = true;		
//				} else {
//					play = false;
//				}
//			};
//		}
//#endif

		// properties

		public bool IsThirsty { get { return this.thirst >= 50; } }

	    public bool IsHungry
	    {
	        get
	        {
                Debug.Log(gameObject.name + " is checking for hunger " + this.hunger);
	            return this.hunger >= 50;
	        }
	    }

	    public bool IsTired { get { return this.energy <= 50; } }  

		public float Hunger { 
			get { return this.hunger; }  
			set { this.hunger = value; }
		}

		public float Thirst { 
			get { return this.thirst; }  
			set { this.thirst = value; }
		}

		public float Energy { 
			get { return this.energy; }  
			set { this.energy = value; }
		}

		void Awake() {
			// initialise multithreader
//			var mt = Loom.Current;

			this.hunger = 0;
			this.thirst = 0;
			this.energy = 100;
			this.agent = GetComponent<ViAgent> ();
			if (this.agent == null) {
				Debug.LogError (gameObject.name + " has no ViAgent component assigned!");
			}

			// find the time control component
			if (dayNight == null) {
				dayNight = GameObject.Find("DayNight").GetComponent<DayNightCycle>();

				var dayInSeconds = dayNight.DayInMinutes * 60;

				// decay is calculated so that in some time interval in part of the day
				// 50 is the treshold value
				// increment = (50 / timeToFillInSeconds) * updateRateInSeconds;

				// agents get hungry two times
				hungerDecay = (50 / (dayInSeconds / 2)) * updateRateInSeconds;
				// agents get thirsty four times
				thirstDecay =  (50 / (dayInSeconds / 4)) * updateRateInSeconds;
				// agents get tired three times
				energyDecay =  (50 / (dayInSeconds / 3)) * updateRateInSeconds;

				// initialise player
				player = GameObject.FindGameObjectWithTag("Player").transform;
			}

            // ----------aram code
            hungerBar = GameObject.Find("HungerValue");
            thirstBar = GameObject.Find("ThirstValue");
            energyBar = GameObject.Find("EnergyValue");

            hungerT = hunger / 100f;
            thirstT = thirst / 100f;
            energyT = energy / 100f;

            hungerBar.transform.localScale = new Vector3(hungerT, 1f, 1f);
            thirstBar.transform.localScale = new Vector3(thirstT, 1f, 1f);
            energyBar.transform.localScale = new Vector3(energyT, 1f, 1f);

            //-----------

//			var timer = new System.Timers.Timer ();
//			timer.Interval = updateRateInSeconds * 1000;
//			timer.Elapsed += UpdateModifiers;
//			timer.Start ();
		}

		float fixedTime = 0f;

		void Update() {




			fixedTime += Time.deltaTime;

            if (fixedTime < 1f)
            {
                return;
            }

			// we have different metabolism at night than during the day
			if (this.agent.IsSleeping) {
				this.hunger += hungerDecay * fixedTime * speedMultiplier * hungerModifier * sleepMultiplier;
				this.thirst += thirstDecay * fixedTime * speedMultiplier * thirstModifier * sleepMultiplier;
				this.energy += energyDecay * fixedTime * speedMultiplier * energyModifier * 4;


			} else {
				this.hunger += hungerDecay * fixedTime * speedMultiplier * hungerModifier;
				this.thirst += thirstDecay * fixedTime * speedMultiplier * thirstModifier;
				this.energy -= energyDecay * fixedTime * speedMultiplier * energyModifier;
			}


			// clamp values     //aram --  from 0 to 100
			this.hunger = Mathf.Clamp (this.hunger, 0, 100);
			this.thirst = Mathf.Clamp (this.thirst, 0, 100);
			this.energy = Mathf.Clamp (this.energy, 0, 100);

			 // aram ----- code  )
            /* we need to add if drinking and if eating  */

            hungerT = hunger / 100f;
            thirstT = thirst / 100f;
            energyT = energy / 100f;

            hungerBar.transform.localScale = new Vector3(hungerT, 1f, 1f);
            thirstBar.transform.localScale = new Vector3(thirstT, 1f, 1f);
            energyBar.transform.localScale = new Vector3(energyT, 1f, 1f);        

            //


			// set fixed time back to 0
			fixedTime = 0f;
			
			// the importance of physiological aspects is that 
			if (this.energy < 50) {
				NotifyAgent("tired", 98);
			}
			if (this.hunger > 50) {
				NotifyAgent("hungry", 99);
			}
			if (this.thirst > 50) {
				NotifyAgent("thirsty", 100);
			}


		}
        


/*		public void UpdateModifiers(object sender, System.Timers.ElapsedEventArgs args) {
			if (!play)
				return;

			// we have different metabolism at night than during the day
			if (this.agent.IsSleeping) {
				this.hunger += hungerDecay * speedMultiplier * hungerModifier * sleepMultiplier;
				this.thirst += thirstDecay * speedMultiplier * thirstModifier * sleepMultiplier;
				this.energy += energyDecay * speedMultiplier * energyModifier * 4;
			} else {
				this.hunger += hungerDecay * speedMultiplier * hungerModifier;
				this.thirst += thirstDecay * speedMultiplier * thirstModifier;
				this.energy -= energyDecay * speedMultiplier * energyModifier;
			}

			// clamp values
			this.hunger = Mathf.Clamp (this.hunger, 0, 100);
			this.thirst = Mathf.Clamp (this.thirst, 0, 100);
			this.energy = Mathf.Clamp (this.energy, 0, 100);


			// the importance of physiological aspects is that 
			if (this.energy < 50) {
				Loom.QueueOnMainThread(() => NotifyAgent("tired", 98));
			}
			if (this.hunger > 50) {
				Loom.QueueOnMainThread(() => NotifyAgent("hungry", 99));
			}
			if (this.thirst > 50) {
				Loom.QueueOnMainThread(() => NotifyAgent("thirsty", 100));
			}
		}
		*/

		void NotifyAgent(string state, int priority) {
			if (this == null) {
				return;
			}
			if (this.agent == null) {
				this.agent = gameObject.GetComponent<ViAgent>();
				Debug.Log("Getting agent");
				return;
			}
			this.agent.Sense(new SensorData(Sensor.Physiology, state, priority, -1, -1));
		}

        //void OnGUI()
        //{
        //    // draw only when player is close
        //    if (player != null && (!Settings.Main.ShowPhysiologyGUI || distanceToPlayer > DrawDistance || Vector3.Angle(_player.forward, _transform.position - _player.position) > 110))
        //        return;

        //    //		// also do not show objects behind walls
        //    //		RaycastHit hit;
        //    //		meRaisedPosition = _transform.position;
        //    //		meRaisedPosition.y += 1;
        //    //		playerRaisedPosition = _player.position;
        //    //		playerRaisedPosition.y += 1;
        //    //
        //    //		if (Physics.Raycast (meRaisedPosition, playerRaisedPosition - meRaisedPosition, out hit)) {
        //    //			if (hit.transform.gameObject.tag != "Player") {
        //    //				Debug.Log (hit.transform.gameObject.name);
        //    //				return;
        //    //			}
        //    //		} else {
        //    //			Debug.Log ("Not hit");
        //    //			return;
        //    //		}

        //    // COMFORT
        //    // draw outside box
        //    GUI.Box(new Rect(pos.x - 1, pos.y - 1, size.x + 2, size.y + 2), "");

        //    // Constrain all drawing to be within a pixel area .
        //    GUI.BeginGroup(new Rect(pos.x, pos.y, comfortProgress, size.y));

        //    // Define progress bar texture within customStyle under Normal > Background
        //    GUI.Box(new Rect(0, 0, size.x, size.y), "", Settings.Main.ComfortStyle);

        //    // Always match BeginGroup calls with an EndGroup call
        //    GUI.EndGroup();



        //    // HUNGER
        //    GUI.Box(new Rect(pos.x - 1, pos.y + size.y + 1, size.x + 2, size.y + 2), "");

        //    // Constrain all drawing to be within a pixel area .
        //    GUI.BeginGroup(new Rect(pos.x, pos.y + size.y + 2, hungerProgress, size.y));

        //    // Define progress bar texture within customStyle under Normal > Background
        //    GUI.Box(new Rect(0, 0, size.x, size.y), "", Settings.Main.HungerStyle);

        //    // Always match BeginGroup calls with an EndGroup call
        //    GUI.EndGroup();
        //} 
	}
}

