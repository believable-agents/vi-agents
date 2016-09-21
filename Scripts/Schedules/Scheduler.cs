// ------------------------------------------------------------------------------
using System;

namespace ViAgents.Schedules {

	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using ViAgents.Personality;
	using ViAgents.Actions;
	using ViAgents.Physiology;
	using ViAgents;

	[RequireComponent(typeof(ViAgent))]
	public class Scheduler : MonoBehaviour {

		private const int SchedulerPriority = 10;
		static DayNightCycle timeControl;

		#region Fields
		public Schedule schedule;

		ViAgent agent;
		ScheduledItem currentItem;
		#endregion

		void Start() {
			agent = gameObject.GetComponent<ViAgent> ();
			if (agent == null) {
				Debug.LogError("Scheduler works only with ViAgent");
			}
			if (schedule == null) {
				Debug.LogError("Agent has no schedule!");
			}
//			Debug.Log ("Attaching from: " + gameObject.name);

			if (timeControl == null) {
				timeControl = GameObject.Find (DayNightCycle.GameObjectName).GetComponent<DayNightCycle> ();
			}
			timeControl.HourChanged += CheckSchedule;

			// control the schedule
			CheckSchedule (timeControl, timeControl.Hour, timeControl.Minute);
		}

//		void Start() {
//			// on start send current scheduled item to agent
//			Debug.Log (this.agent + "; " + timeControl.Hour + ":" + timeControl.Minute + " = " + schedule.Find (timeControl.Hour, timeControl.Minute));
//			var scheduledItem = schedule.Find (timeControl.Hour, timeControl.Minute);
//			this.agent.Sense (
//				new SensorData(
//					Sensor.Schedule, 
//					scheduledItem.action, 
//					SchedulerPriority, 
//					scheduledItem.startHour + scheduledItem.startMinutes / 60,
//					scheduledItem.endHour + scheduledItem.endMinutes / 60));
//		}

		public void Check(bool currentFinished = false) {
		    if (currentFinished)
		    {
		        currentItem = null;
		    }
			StartCoroutine (Check (timeControl, timeControl.Hour, timeControl.Minute));
		}

		void CheckSchedule (DayNightCycle sender, int hours, int minutes)
		{
			Console.WriteLine ("YES");
			StartCoroutine(Check(sender, hours, minutes)); 
		}

		IEnumerator Check (DayNightCycle sender, int hours, int minutes) {
//			Debug.Log ("Checking scedule at " + hours + ":" + minutes);
			var item = schedule.Find (hours, minutes);
			if (item == null) {
				Debug.LogError(gameObject.name + " has nothing planned for this hour!");
				yield break;
			}
			if (item != currentItem) {
				currentItem = item;
				Loom.QueueOnMainThread(() =>
					this.agent.Sense (new SensorData(
						Sensor.Schedule, 
						currentItem.action, 
						SchedulerPriority, 
						currentItem.startHour + currentItem.startMinutes / 60,
						currentItem.endHour + currentItem.endMinutes / 60)));
			} 
		}
	}

}
