
using NodeCanvas.BehaviourTrees;
using NodeCanvas;
using UnityEngine;

namespace ViAgents.Actions
{
	using System;

	[Serializable]
	public class Action
	{
		private static Transform parentObject;

		public Sensor sensor;
		public string sensorRequest;

		public string constraint;
		public string action;

		public bool runForever;
		public bool waitToFinish;

		public BehaviourTree BT;

		void Awake() {
			if (parentObject == null)
			{
				parentObject = GameObject.Find("@RuntimeTrees").transform;
				
				// clear existing trees
				if (parentObject != null && parentObject.childCount > 0)
				{
					for (var i=parentObject.childCount; i>=0; i++) {
						GameObject.DestroyImmediate(parentObject.GetChild(i));
					}
					Debug.Log("Destroyed trees: " + parentObject.childCount);
				}
			}
		}

		public virtual BehaviourTree Execute (ViAgent agent) {
			var bt = agent.GetComponent<BehaviourTreeOwner> ();

			// stop previous actions
			// bt.StopGraph ();

			// get bb
			var bb = bt.blackboard;
		
			// copy all values from the blackboard of the BT
			if (BT.blackboard != null)
			{
				bb.Merge(BT.blackboard);
			}

			// OLD:
			// start new actions
//			bt.BT = GameObject.Instantiate(BT) as BehaviourTree;
//			bt.BT.transform.parent = parentObject;
//
//			bt.blackboard = bb;
//			bt.runForever = runForever;
//			bt.StartGraph (() => agent.ActionFinished(this));

			// new
			bt.blackboard = bb;
//			Debug.Log(agent.name + " rf: " + runForever);

			// stop current action
//			Debug.Log("Starting graph: " + BT.gameObject.name);
//			if (bt.BT != null)
//			{
//				bt.BT.StopGraph();
//			}
//			bt.StopGraph();
//			bt.BT = BT;
//			bt.StartGraph(() => agent.ActionFinished(this));

			// switch to new
		    agent.Log(LogSource.Action, "BT Start");
		    if (BT.name == bt.graph.name)
		    {
                agent.Log(LogSource.Action, "Same graph, restarting");
                bt.StartBehaviour((result) =>
                {
                    agent.Log(LogSource.Action, "BT Finished");
                    agent.ActionFinished(this);
                });
		    }
		    else
		    {
		        bt.SwitchBehaviour(BT, (result) =>
		        {
		            agent.Log(LogSource.Action, "BT Finished");
		            agent.ActionFinished(this);
		        });
		    }
		    bt.repeat = runForever;

			return bt.behaviour;
		}

		public virtual void Pause(ViAgent agent) {
			var bt = agent.GetComponent<BehaviourTreeOwner> ();
			bt.PauseBehaviour ();
		}

		public virtual void Resume(ViAgent agent, BehaviourTree bt) {
			var bto = agent.GetComponent<BehaviourTreeOwner> ();
			bto.behaviour = bt;
			bt.repeat = runForever;
			bt.StartGraph (agent, bto.blackboard, (result) => agent.ActionFinished(this));
		}

		public virtual void Abort (ViAgent agent) {
			var bt = agent.GetComponent<BehaviourTreeOwner> ();
			// stop previous actions
			bt.StopBehaviour();
		}

		public override string ToString ()
		{
			return string.Format ("[{0}: {1}] Tree '{2}'", sensor, sensorRequest, BT.name);
		}
	}
}

