using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ViAgents.Personality;
using ViAgents.Actions;
using ViAgents.Physiology;
using ViAgents.Schedules;

namespace ViAgents
{
    public enum LogSource
    {
        Action,
        Queue,
        Sensor
    }

    public struct LogMessage
    {
        public LogSource Source;
        public string Message;

        public LogMessage(LogSource source, string message)
        {
            this.Source = source;
            this.Message = message;
        }
    }

    public class ViAgent : MonoBehaviour
	{
		static DayNightCycle timeControl;

		public List<LogMessage> log = new List<LogMessage>();
		public List<ActionSet> actions;
		public List<StateParameter> state;

		private List<SensorData> workQueue = new List<SensorData> ();
		private SensorData currentItem;
		private SensorData willExecute;
		private Action currentAction;
		private Scheduler scheduler;
		//private string logMessage = "";


        [SerializeField]
		private bool isSleeping;
		public bool IsSleeping {
			get { return this.isSleeping; }
			set { 
				this.Log(LogSource.Action, "AGENT: Sleeping " + value);
				this.isSleeping = value; 
			}
		}

		//public string LogMessage { get { return logMessage; } }
		public List<SensorData> WorkQueue { get { return this.workQueue; } }
		public SensorData CurrentItem { get { return this.currentItem; } }
		public Action CurrentAction { get { return this.currentAction; } }

        [HideInInspector]
        public bool keepLog = false;

		void Awake() {
			// we need to keep time in order to filter actions by expiration
			if (timeControl == null) {
				timeControl = GameObject.FindObjectOfType<DayNightCycle> ();
			}
		}


		public void Log (LogSource source, string message) {
			// Debug.Log(gameObject.name + ": " + message);

			log.Insert(0, new LogMessage(source, string.Format("[{0}] {1}: {2}", 
                timeControl.CurrentTime, source.ToString().ToUpper(), message)));
			if (!keepLog && log.Count > 50)
			{
				log.RemoveAt(50);
			}
			// logMessage = string.Join("\n", this.log.ToArray());
		}

		public void ActionFinished(Action action) {
			this.Log (LogSource.Action, action + " finished");

//			// destroy the tree
//			if (currentItem != null && currentItem.BT != null) {
//				Debug.Log("Destroying: " + currentItem.BT.name);
//				Destroy(currentItem.BT.gameObject);
//			}
			
			// in case we have finished current acttion, we progress with queue
			// this does not have to coincide if we have interrupted previous action
//			Debug.Log(string.Format("{0} == {1} {2} && {3} == {4} {5} && {6} == {7} {8}",
//			                        willExecute, currentItem, willExecute == currentItem,
//			                        action.sensor, currentItem.Sensor, action.sensor == currentItem.Sensor,
//			                        action.sensorRequest, currentItem.SensorRequest, action.sensorRequest == currentItem.SensorRequest));

			if (currentItem == null ||
			    willExecute == currentItem &&
				action.sensor == currentItem.Sensor && 
			    action.sensorRequest == currentItem.SensorRequest) {

				// we mark that current item is done
				currentItem = null;
				currentAction = null;

				// if we have something in queue we process it, otherwise we set that we have nothing to do
				if (workQueue.Count > 0) {
					// try to find first not expired item
					do {
						var item = workQueue[0];
						workQueue.RemoveAt(0);
						if (!item.IsExpired(timeControl.SunTime)) {
							currentItem = item;
							this.Log(LogSource.Queue, "Resume " + currentItem);							
							ExecuteAction(currentItem);
						} else {
							this.Log(LogSource.Queue, "Resume item expired: " + item);
						}
					} while (currentItem == null && workQueue.Count > 0);
				} 
				else
				{
				    scheduler = GetComponent<Scheduler>();
					// check the schedule if we have something to do
					scheduler.Check(true);
				}

			}
		}

		/// <summary>
		/// Sense the specified data and executes tasks in order of their priority 
		/// </summary>
		/// <param name="data">Data.</param>
		public void Sense(SensorData data) {
			// if we have the same request as previous we ignore it
			if (currentItem != null && currentItem.SensorRequest == data.SensorRequest) {
				return;
			}

			this.Log (LogSource.Sensor, string.Format ("[{1}]: Request '{0}' ({2})", data.SensorRequest, data.Sensor, data.Priority));

			// check if current item has expired
			if (currentItem != null && currentItem.IsExpired(timeControl.SunTime))
			{
			    this.Log(LogSource.Queue, "Item expired, force finish: " + currentItem);
			    if (currentItem.BT != null)
			    {
			        currentItem.BT.Stop();
			    }
			}

			// if we have a smaller priority request we stack it in correct place
			if (currentItem != null && data.Priority < currentItem.Priority) {
				// find if it is currently queued
				InsertToQueue(data);
				return;
			}

			// same priority action will be directly terminated, but we can set to wait to finish
			// if current action is not interrumpible, we wait for that action to finish
			if (currentAction != null && currentAction.waitToFinish) {
				this.Log(LogSource.Queue, "Waiting for '" + currentItem + "', then " + data);
				InsertToQueue(data);
				return;
			}
			
			// if we have a higher priority request we stack the current one and execute the new one
			if (currentItem != null && data.Priority > currentItem.Priority) {
				this.Log(LogSource.Queue, "Action '" + currentItem + "' interrupted by " + data);
				//currentItem.BT.PauseGraph();
				InsertToQueue(currentItem);
			}

			// execute the currently requested action
			ExecuteAction (data);
		}

		void InsertToQueue(SensorData data) {
			int startIndex = workQueue.Count;
			
			for (var i=0; i<workQueue.Count; i++) {
				// if we have items with the same priority, try to find if we already queued the item
				if (workQueue[i].Priority == data.Priority) {
					startIndex = i;
					for (var j=i; j<workQueue.Count;j++) {
						// if it is already in queue then 
						if (workQueue[j].SensorRequest == data.SensorRequest) {
							this.Log(LogSource.Queue, "Already queued: " + data);
							return;
						}
					}
					break;
				}
				// if we find items with lower priority, we insert it before the first item with lower priority
				if (workQueue[i].Priority < data.Priority) {
					startIndex = i;
					break;
				}
			}
			this.Log(LogSource.Queue, "Insert " + data);
			workQueue.Insert(startIndex, data);
		}
		
		void ExecuteAction(SensorData data) {
			// find a first action in any action set and execute it
			for (var i=0; i<actions.Count; i++) {
				if (actions[i] == null || actions[i].actions.Count == 0) {
					Debug.LogError("ERROR: " + gameObject.name + " has no actions!");
				}

				var action = actions[i].actions.Find(w => w.sensor == data.Sensor && w.sensorRequest == data.SensorRequest);
				if (action != null) {
					if (action.BT == null) {
						Debug.LogError(string.Format("Action for '{0}:{1}' does not have a behaviour tree", data.Sensor, data.SensorRequest));
						return;
					}

					// set that current item is the requested item
					this.Log(LogSource.Action, action + " started");
					// remember the tree in case we want to execute it again

					//if (data.BT != null) {
					//	data.BT.StartGraph();
					//} else {
					willExecute = data;

					data.BT = action.Execute(this);

					currentItem = data;
					currentAction = action;

					//}
					return;
				}
			}
			Debug.LogError (string.Format("ERROR: {0}'s action for '{0}:{1}' does no exist", name, data.Sensor, data.SensorRequest));
		}

		public Object CreateInstance(Object component) {
			return Instantiate(component);
		}
	}
}
