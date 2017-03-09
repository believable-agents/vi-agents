using System.Globalization;

namespace ViAgents.Personality
{
	using System;

	public class PersonalAction
	{
		public string ActionName;
		public ActionType ActionType;
	    public string Goal;
		//These factors are valid for the range -1 to 1
		//Neurotic factors:
		public double Temptation; //(impulsiveness) low = resist urge, medium = sometimes tempted, high=easily tempted
		//Extraversion factors:
		public double Gregariousness; //low = prefer alone, medium = alone/others, high=prefers company
		public double Assertiveness; //low = in background, medium = in foreground, high = a leader
		public double Excitement; //(excitement seeking) low = low need for thrills, medium = occasionally need for thrills, high = crave thrills
		//Openess factors:
		public double Familiarity; //low = likes the familiar, medium = a mixture, high = variety
		//Agreeableness factors:
		//public double Straightforwardness; //low = guarded, medium = tactful, high = frank
		public double Altruism; //low = uninvolved, medium = willing to help others, high = eager to help
		public double Compliance; //low = aggressive, medium = approachable, high = defers
		public double Modesty; //low = superior, medium = equal, high = humble
		//Conscientiousness:
		public double Correctness; //low=don't care about correctness, medium=covers priorities, high=governed by conscience
		
		public PersonalAction() { }
		
		public PersonalAction(double temptation, double gregariousness, double assertiveness, double excitement, double familiarity, double straightforwardness,
		                      double altruism, double compliance, double modesty, double correctness)
		{
			Temptation = temptation;
			Gregariousness = gregariousness;
			Assertiveness = assertiveness;
			Excitement = excitement;
			Familiarity = familiarity;
			//_straightforwardness = straightforwardness;
			Altruism = altruism;
			Compliance = compliance;
			Modesty = modesty;
			Correctness = correctness;
		}

		
		/// <summary>
		/// Calculates the utility of an action based both on the personality of a bot
		/// and it's current emotional state and the attributes of the action
		/// </summary>
		/// <param name="agent">The agent evaluating the utility of an action</param>
		/// <returns>The utility of an action</returns>
		public double Utility(PersonalityModel personality)
		{
			double utility = 0;
			EmotionTree emotions = personality.Emotions;
			
			// We believe that emotions and personality have a two-way relationship. Personality affects ones emotions
			// but emotions also effect one's personality. For example, you are less likely to be agreeable if you are
			// angry. The first step here is to calculate a personality value based on the agent's emotions. Then normalise
			// to a value between -1 and 1.
			double o = normalise(emotionalAffectedOpenness(personality.Openness, emotions), 0, 1, -1, 1);
			double c = normalise(emotionalAffectedConscientiousness(personality.Conscientiousness, emotions), 0, 1, -1, 1);
			double i = normalise(personality.Introversion, 0, 1, -1, 1);
			double a = normalise(emotionalAffectedAgreeableness(personality.Agreeableness, emotions), 0, 1, -1, 1);
			double n = normalise(emotionalAffectedNeuroticism(personality.Neuroticism, emotions), 0, 1, -1, 1);
			
			//Debug output - print out the original personality attributes and their processed value
			Console.WriteLine("===========================" + this.ToString() + "==============================");
			Console.WriteLine("Normalising variables...");
			Console.WriteLine(String.Format("Openness {0}->{1}, Conscientiousness {2}->{3}, Introversion {4}->{5}, Agreeableness {6}->{7}, Neuroticism {8}->{9}",
			                                personality.Openness.ToString(), o.ToString(), personality.Conscientiousness.ToString(), c.ToString(), personality.Introversion.ToString(),
			                                i.ToString(), personality.Agreeableness.ToString(), a.ToString(), personality.Neuroticism.ToString(), n.ToString()));
			
			//For each attribute of the action, work out the utility associated with it.
			utility += utilityAddition(o, Familiarity);
			utility += utilityAddition(c, Correctness);
			//The personality model uses the opposite of extraversion, so reverse the values
			utility += utilityAddition(i, -1 * Gregariousness);
			utility += utilityAddition(i, -1 * Assertiveness);
			utility += utilityAddition(i, -1 * Excitement);
			//Console.Write("Straightforwardness bonus:");
			//utility += utilityAddition(a, _straightforwardness) ;
			utility += utilityAddition(a, Altruism);
			utility += utilityAddition(a, Compliance);
			utility += utilityAddition(a, Modesty);
			utility += utilityAddition(n, Temptation);
			
			Console.WriteLine("Final utility: " + utility.ToString());
			return utility; 
		}
		
		/// <summary>
		/// Utility is calculated as the absolute value of the difference between a personality value
		/// And a related attribute value scaled from 2 (the maximum difference)->0 (the minimum difference) to -1->1. 
		/// </summary>
		/// <param name="personalityValue"></param>
		/// <param name="actionValue"></param>
		/// <returns></returns>
		private double utilityAddition(double personalityValue, double actionValue)
		{
			return normalise(Math.Abs(personalityValue - actionValue), 2, 0, -1, 1);
		}
		
		/// <summary>
		/// This transforms a value between oldmin to oldmax to a value between newmax and newmin
		/// </summary>
		/// <param name="d">The value to be transformed</param>
		/// <param name="oldmax">The old maximum value</param>
		/// <param name="oldmin">The old min value</param>
		/// <param name="newmax">The new max value</param>
		/// <param name="newmin">The new min value</param>
		/// <returns></returns>
		private double normalise(double d, double oldmax, double oldmin, double newmax, double newmin)
		{
			Console.WriteLine("Normalisation: " + (((d - oldmin) / (oldmax - oldmin)) * (newmax - newmin) + newmin).ToString());
			return ((d - oldmin) / (oldmax - oldmin)) * (newmax - newmin) + newmin;
		}
		
		/// <summary>
		/// Calculates the actual value of neuroticism after considering the current emotional state
		/// </summary>
		/// <param name="neuroticism">The raw neuroticism value of the bot</param>
		/// <param name="emotions">The emotions of the bot</param>
		/// <returns>The new value of neuroticism</returns>
		private double emotionalAffectedNeuroticism(double neuroticism, EmotionTree emotions)
		{
			//Emotions here are based on neuroticism's effect on emotions
			//the /2 at the end of the following line is worked out from the number of emotions invovled divided by 2.
			//because the maximum range of the addition is +- (num of emotions /2)
			double emotionalBonus = (emotions[EmotionType.Distress] - emotions[EmotionType.Joy] - emotions[EmotionType.Gratification] + emotions[EmotionType.Remorse]) / 2;
			return cleanedDouble(neuroticism + emotionalBonus);
		}
		
		/// <summary>
		/// Calculates the actual value of openness after considering the current emotional state
		/// </summary>
		/// <param name="openness">The raw openness value of the bot</param>
		/// <param name="emotions">The emotions of the bot</param>
		/// <returns>The new value of openness</returns>
		private double emotionalAffectedOpenness(double openness, EmotionTree emotions)
		{
			//Emotions here are based on the emotions affected by openness
			//the /2 at the end of the following line is worked out from the number of emotions invovled divided by 2.
			//because the maximum range of the addition is +- (num of emotions /2)
			double emotionalBonus = (emotions[EmotionType.Pride] + emotions[EmotionType.Shame] + emotions[EmotionType.Admiration] + emotions[EmotionType.Reproach]) / 2;
			return cleanedDouble(openness - emotionalBonus);
		}
		
		/// <summary>
		/// Calculates the actual value of conscientiousness after considering the current emotional state
		/// </summary>
		/// <param name="conscientiousness">The raw conscientiousness value of the bot</param>
		/// <param name="emotions">The emotions of the bot</param>
		/// <returns>The new value of conscientiousness</returns>
		private double emotionalAffectedConscientiousness(double conscientiousness, EmotionTree emotions)
		{
			//Emotions here are based on the emotions affected by conscientiousness AND the notion that
			//you are more likely to abandon standards in times of stress, and more liekly to stick to standards
			//in times of happyness
			//the /3 at the end of the following line is worked out from the number of emotions invovled divided by 2.
			//because the maximum range of the addition is +- (num of emotions /2)
			double emotionalBonus = (emotions[EmotionType.Pride] - emotions[EmotionType.Distress] + emotions[EmotionType.Joy] -  emotions[EmotionType.Shame] 
			                         + emotions[EmotionType.Admiration] - emotions[EmotionType.Reproach] - emotions[EmotionType.Anger] + emotions[EmotionType.Admiration]) /3;
			return cleanedDouble(conscientiousness + emotionalBonus);
		}
		
		/// <summary>
		/// Calculates the actual value of agreeableness after considering the current emotional state
		/// </summary>
		/// <param name="agreeableness">The raw agreeableness value of the bot</param>
		/// <param name="emotions">The emotions of the bot</param>
		/// <returns>The new value of agreeableness</returns>
		private double emotionalAffectedAgreeableness(double agreeableness, EmotionTree emotions)
		{
			//Emotions here are based on those affected by agreeableneess
			//the /5 at the end of the following line is worked out from the number of emotions invovled divided by 2.
			//because the maximum range of the addition is +- (num of emotions /2)
			double emotionalBonus = (emotions[EmotionType.Joy] - emotions[EmotionType.Distress] + emotions[EmotionType.Love] - emotions[EmotionType.Hate] +
			                         emotions[EmotionType.HappyFor] - emotions[EmotionType.Resentment] + emotions[EmotionType.Admiration] - emotions[EmotionType.Reproach] +
			                         emotions[EmotionType.Gratitude] - emotions[EmotionType.Anger]) / 5;
			return cleanedDouble(agreeableness + emotionalBonus);
		}
		
		/// <summary>
		/// Restricts a double value to be between MAX_PERSONALITY_INTENSITY and MIN_PERSONALITY_INTENSITY
		/// </summary>
		/// <param name="d">The double to be cleaned</param>
		/// <returns></returns>
		private double cleanedDouble(double d)
		{
			return Math.Max(PersonalityModel.MIN_PERSONALITY_INTENSITY, Math.Min(PersonalityModel.MAX_PERSONALITY_INTENSITY, d));
		}
	}
}

