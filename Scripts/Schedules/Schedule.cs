namespace ViAgents.Schedules {

	using System.Collections.Generic;
	using UnityEngine;
	using System.Linq;

	[System.Serializable]
	public class Schedule : ScriptableObject {
		public List<ScheduledItem> items;
		
		public ScheduledItem Find(int hours, int minutes) {
			// get random item
			var item = items.Where (w => w.IsActiveAt(hours, minutes)).ToList().Random() as ScheduledItem;

			if (item == null) {
				Debug.LogError("There is not axction scheduled for " + hours + ":" + minutes);
			}
			return item;
		}
	}
}
