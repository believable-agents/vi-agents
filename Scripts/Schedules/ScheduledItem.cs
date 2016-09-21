namespace ViAgents.Schedules {
	
	[System.Serializable]
	public class ScheduledItem {
		public int startHour;
		public int startMinutes;
		public int endHour;
		public int endMinutes;
		public string action;

		public bool IsActiveAt(int hour, int minute) {
			// the time interval is either at the same day or in two different days
			if (startHour <= endHour) {
				return (hour > startHour || hour == startHour && minute >= startMinutes) &&
					(hour < endHour || hour == endHour && minute <= endMinutes);
			} else {
				return (hour > startHour || hour == startHour && minute >= startMinutes) ||
					(hour < endHour || hour == endHour && minute <= endMinutes);
			}
		}
	}


}

