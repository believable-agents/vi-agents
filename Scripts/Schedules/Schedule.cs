namespace ViAgents.Schedules {

	using System.Collections.Generic;
	using System.Linq;

	[System.Serializable]
	public class Schedule {
		public readonly List<ScheduledItem> items;

	    public Schedule(List<ScheduledItem> items)
	    {
	        this.items = items;
	    }

		public ScheduledItem Find(int hours, int minutes) {
			// get random item
			return items.Where (w => w.IsActiveAt(hours, minutes)).ToList().Random() as ScheduledItem;
		}
	}
}
