using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ViAgents.Personality;
using ViAgents.Actions;
using ViAgents.Physiology;
using ViAgents.Schedules;
using Action = ViAgents.Actions.Action;

namespace ViAgents
{
    public enum LogSource
    {
        Action,
        //Queue,
        Sensor
    }

    public enum LogLevel
    {
        Debug,
        Info
    }

    public struct LogMessage
    {
        public LogSource Source;
        public LogLevel Level;
        private readonly string message;
        private readonly string time;
        private string formattedMessage;

        public LogMessage(LogLevel level, LogSource source, string message, string time)
        {
            this.Level = level;
            this.Source = source;
            this.message = message;
            this.time = time;

            this.formattedMessage = string.Format("[{0}]: {2}", time, Source.ToString().ToUpper(), message);
        }

        public override string ToString()
        {
            return this.formattedMessage;
        }
    }

    public class PriorityPlanningAgent
    {
        public List<LogMessage> log = new List<LogMessage>();
        //public List<StateParameter> state;

        private List<SensorData> workQueue = new List<SensorData>();
        private SensorData currentItem;
        private Action currentAction;
        private string name;
        private bool isSleeping;
        private Func<float> timer; 
        private Action[] actions; 

        // properties

        public bool IsSleeping
        {
            get { return this.isSleeping; }
            set
            {
                this.Log(LogLevel.Info, LogSource.Action, "AGENT: Sleeping " + value);
                this.isSleeping = value;
            }
        }

        public List<SensorData> WorkQueue { get { return this.workQueue; } }
        public SensorData CurrentItem { get { return this.currentItem; } }
        public Action CurrentAction { get { return this.currentAction; } }
        public bool keepLog = false;
        public Action<LogMessage> logger;
        // ctor

        public PriorityPlanningAgent(string name, Action[] actions, Func<float> timer)
        {
            this.name = name;
            this.actions = actions;
            this.timer = timer;
        }

        // methods

        /// <summary>
        /// Returns an action that will be executed next
        /// </summary>
        /// <returns></returns>
        public Action Reason()
        {

            // if current action is not interrumpible, we wait for that action to finish
            if (currentAction != null && currentAction.waitToFinish)
            {
                // this.Log(LogSource.Queue, "Waiting for '" + currentItem + "', then " + data);
                return null;
            }

            // check if current item has expired
            if (currentItem != null && currentItem.IsExpired(this.timer()))
            {
                this.Log(LogLevel.Info, LogSource.Action, "Item expired, force finish: " + currentItem);
                this.ActionFinished(this.currentAction);
            }

            // we may not have anything in the queue
            if (this.workQueue.Count == 0)
            {
                return null;
            }

            // obtain the first item (highest priority) from the queue
            var data = this.workQueue[0];
            
            // if item has lower priority do nothing
            if (currentItem != null && data.Priority < currentItem.Priority)
            {
                return null;
            }

            // this is higher priority item so remove it from the queue as we are going to execute it
            this.workQueue.RemoveAt(0);

            // we have a higher priority request we stack the current one and execute the new one
            if (currentItem != null && data.Priority >= currentItem.Priority)
            {
                this.Log(LogLevel.Info, LogSource.Action, currentItem + "' interrupted by " + data);
                InsertToQueue(currentItem, false);
            }

            // execute the currently requested action
            return ExecuteAction(data);
        }


        public void Log(LogLevel level, LogSource source, string message)
        {
            var time = this.timer();
            var hours = Math.Floor(time);
            var minutes = time - Math.Floor(time);
            minutes = 60*minutes;
            var msg = new LogMessage(
                level,
                source,
                message,
                string.Format("{0:00}:{1:00}", hours, minutes
                )
            );             

            if (logger != null)
            {
                logger(msg);
            }

            log.Insert(0, msg);
            if (!keepLog && log.Count > 50)
            {
                log.RemoveAt(50);
            }
        }

        public void ActionFinished(Action action)
        {
            this.Log(LogLevel.Info, LogSource.Action, action + " finished");

            currentItem = null;
            currentAction = null;

            // you cannot continue resoning here as action has to be executed with VIEW agent
        }

        /// <summary>
        /// Sense the specified data and executes tasks in order of their priority 
        /// </summary>
        /// <param name="data">Data.</param>
        public void Sense(SensorData data)
        {

            // if we have the same request as previous we ignore it
            if (currentItem != null && currentItem.SensorRequest == data.SensorRequest)
            {
                return;
            }

            // insert to priority queue
            this.InsertToQueue(data);
        }



        void InsertToQueue(SensorData data, bool sensorRequest = true)
        {
            // check if item exists in the queue
            if (workQueue.Exists((w) => w.SensorRequest == data.SensorRequest))
            {
                this.Log(LogLevel.Debug, LogSource.Sensor, string.Format("[{1}]: Already queued request'{0}' ({2})", data.SensorRequest, data.Sensor, data.Priority));
                //this.Log(LogLevel.Debug, LogSource.Queue, "Already queued: " + data);
                return;
            }

            if (sensorRequest)
            {
                this.Log(LogLevel.Info, LogSource.Sensor,
                    string.Format("[{1}]: Request '{0}' ({2})", data.SensorRequest, data.Sensor, data.Priority));
            }
            // this.Log(LogLevel.Info, LogSource.Queue, "Inserted " + data);

            // insert into queue
            this.workQueue.Add(data);

            // sort queue
            this.workQueue = this.workQueue.OrderByDescending((w) => w.Priority).ToList();
        }

        private Action ExecuteAction(SensorData data)
        {
            if (actions.Length == 0)
            {
                throw new Exception("ERROR: " + this.name + " has no action sets!");
            }

            // find the action
            var action = actions.FirstOrDefault(w => w.sensor == data.Sensor && w.sensorRequest == data.SensorRequest);
            if (action == null)
            {
                throw new Exception(string.Format("ERROR: {0}'s action for '{1}: {2}' does no exist", name, data.Sensor, data.SensorRequest));
            }

            this.currentItem = data;
            this.currentAction = action;

            return action;
            
            
        }
    }
}
