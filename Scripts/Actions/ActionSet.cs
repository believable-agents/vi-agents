using System;
using UnityEngine;
using System.Collections.Generic;


namespace ViAgents.Actions
{
	public class ActionSet : ScriptableObject
	{
		public ActionSet() {
			actions = new List<Action> ();
		}

		public List<Action> actions;
	}
}

