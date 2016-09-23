namespace ViAgents.Schedules
{
    public class ScheduleManager
    {

        private const int SchedulerPriority = 10;

        #region Fields
        private readonly Schedule schedule;
        private readonly PriorityPlanningAgent agent;
        #endregion

        public ScheduleManager(PriorityPlanningAgent agent, Schedule schedule)
        {
            this.agent = agent;
            this.schedule = schedule;
        }

        public void Check(int hours, int minutes)
        {
            // Debug.Log ("Checking scedule at " + hours + ":" + minutes);
            var item = schedule.Find(hours, minutes);
            if (item == null)
            {
                return;
            }

            this.agent.Sense(new SensorData(
               Sensor.Schedule,
               item.action,
               SchedulerPriority,
               item.startHour + item.startMinutes / 60,
               item.endHour + item.endMinutes / 60));
        }
    }
}
