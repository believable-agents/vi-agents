using ViAgents.Actions;

namespace ViAgents
{
	public class SensorData
	{
		public Sensor Sensor { get; private set; }
		public string SensorRequest { get; private set; }
		public int Priority { get; private set; }
		public float Start { get; private set; }
		public float Finish { get; private set; }
		 
		public SensorData (Sensor sensor, string request, int priority) : this(sensor, request, priority, -1f, -1f)
		{
		}

		public SensorData (Sensor sensor, string request, int priority, float start, float finish)
		{
			Sensor = sensor;
			SensorRequest = request;
			Priority = priority;
			Start = start;
			Finish = finish;
		}

		public bool IsExpired(float currentTime) {
			// if there is no time interval, item never expires
			if (Start < 0)
				return false;

			// we can have interval in the same day or in two different days
			if (Start < Finish) {
				return currentTime < Start || currentTime > Finish;
			} else {
				return currentTime > Start && currentTime < Finish;
			}
		}


		public override string ToString ()
		{
		    if (Start >= 0)
		    {
                return string.Format("[{0}, {2}] {1} ({3:00}:00-{4:00}:00)", Sensor, SensorRequest, Priority, Start, Finish);
            }
            return string.Format("[{0}, {2}] {1}, (no limit)", Sensor, SensorRequest, Priority);
        }
	}
}

