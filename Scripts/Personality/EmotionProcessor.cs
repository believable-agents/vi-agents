//using System;
//using Vwbt.ConnectionServer;
//using System.Collections.Generic;
//
//namespace ViAgents.Personality
//{
//	public class EmotionProcessor
//	{
//		public EmotionProcessor ()
//		{
//		}
//
//		internal List<Reaction> ProcessText(string text) 
//		{
//			var parsed = ParseEvent (text);
//			return GenerateEvents (parsed.Item1, parsed.Item2);
//			// emotion.evaluateEvent(message, sourceAvatar);
//		}
//
//		#region "Event pre-processing"
//
//		/// <summary>
//		/// This method would parse an event message and transform into a generalised action with
//		/// intensity. But for the demonstrator, we will just use a hard-coded string.
//		/// </summary>
//		/// <param name="eventString">The string to be generalised into a certain action</param>
//		/// <returns>A parsed string of a generalised action with intensity and other info</returns>
//		private Tuple<EmotionTrigger,double> ParseEvent(string eventString)
//		{
//			string[] e = eventString.Split(' ');
//			double intensity = 0;
//
//			var emotion = (EmotionTrigger) Enum.Parse (typeof(EmotionTrigger), e [0]);
//
//			//if a paramater has been supplied, try and cast to double
//			if (e.Length > 1)
//				Double.TryParse(e[1], out intensity);
//
//			return new Tuple<EmotionTrigger, double>(emotion, intensity);
//		}
//
//		/// <summary>
//		/// Convert an event string into a set of events to be evaluated for their emotional
//		/// effects.
//		/// </summary>
//		/// <param name="classifiedString">A string that was generated after the system translates an
//		/// action, message, or event into a string representating a generalised action/event with relevant
//		/// additional parameters</param>
//		/// <returns></returns>
//		private List<Reaction> GenerateEvents(EmotionTrigger trigger, double intensity)
//		{
//			//In future, event messages may have more parameters to correctly model an event, but for now, we
//			//just use an arbitrary number to indicate the intensity of an action
//
//
//			List<Reaction> events = new List<Reaction>();
//
//			//Events are generalised into a set of actions we believe are possible in second life
//			//In a real implementation, classifiedStrings need to parsed and transformed into one of
//			//the below actions; but for the demonstrator, we will just use these hard-coded strings
//			//The numbers used to mutiply with intensity below are arbritrary. They are made different/
//			//less intense according to the difference in perceived intensity of the event.
//			if (trigger == EmotionTrigger.Insult)
//			{
//				ActionEvent personInsultsMe = new ActionEvent(EventType.Action, false, "other", intensity * 0.5);
//				events.Add(personInsultsMe);
//
//				ConsequenceEvent meIsInsulted = new ConsequenceEvent(EventType.Consequence, false, "me", intensity * 0.6, false, true);
//				events.Add(meIsInsulted);
//
//				events.Add(new ActionConsequenceEvent(EventType.ActionConsequence, false, meIsInsulted, personInsultsMe));
//			}
//			else if (trigger == EmotionTrigger.Praise)
//			{
//				ActionEvent personPraisesMe = new ActionEvent(EventType.Action, true, "other", intensity * 0.6);
//				events.Add(personPraisesMe);
//
//				ConsequenceEvent meIsPraised = new ConsequenceEvent(EventType.Consequence, true, "me", intensity * 0.5, false, true);
//				events.Add(meIsPraised);
//
//				events.Add(new ActionConsequenceEvent(EventType.ActionConsequence, true, meIsPraised, personPraisesMe));
//			}
//			else if (trigger == EmotionTrigger.Hit)
//			{
//				ActionEvent personHitsMe = new ActionEvent(EventType.Action, false, "other", intensity);
//				ConsequenceEvent meIsHurt = new ConsequenceEvent(EventType.Consequence, false, "me", intensity * 1.1, false, true);
//				ConsequenceEvent meWillBeHurtMore = new ConsequenceEvent(EventType.Consequence, false, "me", intensity * 0.5, true, true);
//
//				events.Add(personHitsMe);
//				events.Add(meIsHurt);
//				events.Add(new ActionConsequenceEvent(EventType.ActionConsequence, false, meIsHurt, personHitsMe));
//				events.Add(meWillBeHurtMore);
//			}
//			else if (trigger == EmotionTrigger.Heal)
//			{
//				ActionEvent personHealsMe = new ActionEvent(EventType.Action, true, "other", intensity);
//				ConsequenceEvent meIsHealed = new ConsequenceEvent(EventType.Consequence, true, "me", intensity * 1.1, false, true);
//
//				events.Add(personHealsMe);
//				events.Add(meIsHealed);
//				events.Add(new ActionConsequenceEvent(EventType.ActionConsequence, true, meIsHealed, personHealsMe));
//			}
//			else if (trigger == EmotionTrigger.Ease)
//			{
//				ActionEvent meEase = new ActionEvent(EventType.Action, true, "me", intensity);
//				ConsequenceEvent meIsEased = new ConsequenceEvent(EventType.Consequence, true, "me", intensity * 0.5, false, true);
//
//				events.Add(meEase);
//				events.Add(meIsEased);
//				events.Add(new ActionConsequenceEvent(EventType.ActionConsequence, true, meIsEased, meEase));
//			}
//			else if (trigger == EmotionTrigger.Unease)
//			{
//				ActionEvent meUnEase = new ActionEvent(EventType.Action, false, "me", intensity);
//				ConsequenceEvent meIsUnEased = new ConsequenceEvent(EventType.Consequence, false, "me", intensity * 0.5, false, true);
//
//				events.Add(meUnEase);
//				events.Add(meIsUnEased);
//				events.Add(new ActionConsequenceEvent(EventType.ActionConsequence, false, meIsUnEased, meUnEase));
//			}
//
//			return events;
//		}
//
//		#endregion
//	}
//}
//
