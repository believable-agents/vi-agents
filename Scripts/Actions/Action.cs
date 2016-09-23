namespace ViAgents.Actions
{
    using System;

    [Serializable]
    public abstract class Action
    {
        public Sensor sensor;
        public string sensorRequest;

        // public string constraint;
        // public string action;

        public bool waitToFinish;

        public abstract void Execute(object agent);

        public abstract void Pause(object agent);

        public abstract void Resume(object agent, object context);

        public abstract void Abort(object agent);
    }
}

