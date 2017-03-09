using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ViAgents.Personality
{
    #region "Enumerations"
    public enum EmotionType
    {
        Joy,
        Distress,
        Pride,
        Shame,
        Admiration,
        Reproach,
        Love,
        Hate,
        Interest,
        Disgust,
        Gratification,
        Remorse,
        Gratitude,
        Anger,
        Hope,
        Fear,
        HappyFor,
        Resentment,
        Gloating,
        Pity,
        None
    }

    public enum EventType
    {
        Consequence,
        Action,
        Object,
        ActionConsequence
    }

    public enum ActionType
    {
        Positive,
        PositiveTarget,
        Negative,
        NegativeTarget,
        Goal
    }

    public enum EventAlignment
    {
        Positive,
        Neutral,
        Negative
    }

    public enum EmotionTrigger
    {
        Insult,
        Praise,
        Hit,
        Heal,
        Ease,
        Unease
    }
    #endregion

    [System.Serializable]
    public class PersonalityModel
    {
        public const string UniqueId = "Personality";

        #region "Private Members"

        //The following are all the emotions a bot can feel

        private EmotionProcessor _emotionProcessor = new EmotionProcessor();
        private EmotionNode _joyDistressNode;
        private EmotionNode _prideShameNode;
        private EmotionNode _admirationReproachNode;
        private EmotionNode _loveHateNode;
        private EmotionNode _interestDisgustNode;
        private EmotionNode _gratificationRemorseNode;
        private EmotionNode _gratitudeAngerNode;
        private EmotionNode _happyforResentmentNode;
        private EmotionNode _gloatingPityNode;
        private EmotionNode _hopeFearNode;

        //The following are the five personality factors
        [System.NonSerialized]
        public EmotionTree Emotions = new EmotionTree();
        public double Neuroticism;
        public double Introversion;
        public double Openness;
        public double Agreeableness;
        public double Conscientiousness;

        //Constants
        public const double MAX_PERSONALITY_INTENSITY = 1;
        public const double NEUTRAL_PERSONALITY_INTENSITY = 0.5;
        public const double MIN_PERSONALITY_INTENSITY = 0;

        #endregion

        #region "Constructors"

        static PersonalityModel()
        {
            Actions = new List<PersonalAction>();
        }

        public PersonalityModel() : this(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
        {
        }

        public PersonalityModel(double joy, double distress, double pride, double shame, double admiration, double reproach, double love,
                                    double hate, double interest, double disgust, double gratification, double remorse, double gratitude,
                                    double anger, double happyfor, double resentment, double gloating, double pity, double hope, double fear,
                                    double neuroticism, double introversion, double openness, double agreeableness, double conscientiousness)
        {
            _joyDistressNode = new EmotionNode(EmotionType.Joy, joy, EmotionType.Distress, distress);
            _prideShameNode = new EmotionNode(EmotionType.Pride, pride, EmotionType.Shame, shame);
            _admirationReproachNode = new EmotionNode(EmotionType.Admiration, admiration, EmotionType.Reproach, reproach);
            _loveHateNode = new EmotionNode(EmotionType.Love, love, EmotionType.Hate, hate);
            _interestDisgustNode = new EmotionNode(EmotionType.Interest, interest, EmotionType.Disgust, disgust);
            _gratificationRemorseNode = new EmotionNode(EmotionType.Gratification, gratification, EmotionType.Remorse, remorse);
            _happyforResentmentNode = new EmotionNode(EmotionType.HappyFor, happyfor, EmotionType.Resentment, resentment);
            _gloatingPityNode = new EmotionNode(EmotionType.Gloating, gloating, EmotionType.Pity, pity);
            _hopeFearNode = new EmotionNode(EmotionType.Hope, hope, EmotionType.Fear, fear);
            _gratitudeAngerNode = new EmotionNode(EmotionType.Gratitude, gratitude, EmotionType.Anger, anger);

            SetUpEmotionTree();

            Neuroticism = neuroticism;
            Introversion = introversion;
            Openness = openness;
            Agreeableness = agreeableness;
            Conscientiousness = conscientiousness;
        }

        /// <summary>
        /// This sets up the emotional tree hierarchy based on the inheritance-based hierarchy of emotions defined
        /// by Steunebrink, Dastani, Meyer (The OCC Model Revisited) based on the original OCC model.
        /// </summary>
        private void SetUpEmotionTree()
        {
            // first we add the highest level nodes (Joy/Distress, Hope/Fear, Pride/Shame, Admiration/Reproach, 
            // Love/Hate, Interest/Disgust)
            Emotions.Add(_joyDistressNode);
            Emotions.Add(_hopeFearNode);
            Emotions.Add(_prideShameNode);
            Emotions.Add(_admirationReproachNode);
            Emotions.Add(_loveHateNode);
            Emotions.Add(_interestDisgustNode);

            // Now we add to the _joyDistressNode all the child emotional nodes (gratification/remorse, gratitude/anger,
            // happyfor/resentment, gloating/pity
            _joyDistressNode.Add(_gratificationRemorseNode);
            _joyDistressNode.Add(_gratitudeAngerNode);
            _joyDistressNode.Add(_happyforResentmentNode);
            _joyDistressNode.Add(_gloatingPityNode);

            // Now we add to the _prideShameNode all the child emotional nodes (gratification/remorse)
            _prideShameNode.Add(_gratificationRemorseNode);

            // Now we add to the _admirationReproach all the child emotion nodes (gratitude/anger)
            _admirationReproachNode.Add(_gratitudeAngerNode);

            DisplayAllEmotions(); //purely for debugging
        }

        #endregion

        #region "Events"
        public event Action<PersonalityModel> EmotionsChanged;
        public event Action<PersonalityModel, EmotionType, double, string> EmotionExpressed;
        #endregion

        #region "Properties"

        //Lists of possible reactions to emotional effects
        public static List<PersonalAction> Actions { get; set; }

        public static IEnumerable<PersonalAction> NegativeActions { get { return Actions.Where(w => w.ActionType == ActionType.Negative); } }

        public static IEnumerable<PersonalAction> NegativeActionsTowardsTarget { get { return Actions.Where(w => w.ActionType == ActionType.NegativeTarget); } }

        public static IEnumerable<PersonalAction> PositiveActions { get { return Actions.Where(w => w.ActionType == ActionType.Positive); } }

        public static IEnumerable<PersonalAction> PositiveActionsTowardsTarget { get { return Actions.Where(w => w.ActionType == ActionType.PositiveTarget); } }

        public static IEnumerable<PersonalAction> GoalActions { get { return Actions.Where(w => w.ActionType == ActionType.Goal); } }

        #endregion "Properties"

        //		#region IChromosomeProvider implementation
        //			
        //		public Chromosome GetChromosome ()
        //		{
        //			var chromosome = new Chromosome ();
        //			int id = 0;
        //			chromosome.Genes.Add (GetGene (id++, "Openness", Openness));
        //			chromosome.Genes.Add (GetGene (id++, "Conscientiousness", Conscientiousness));
        //			chromosome.Genes.Add (GetGene (id++, "Introversion", Introversion));
        //			chromosome.Genes.Add (GetGene (id++, "Agreeableness", Agreeableness));
        //			chromosome.Genes.Add (GetGene (id++, "Neuroticism", Neuroticism));
        //			return chromosome;
        //		}

        //		public void SetChromosome (Chromosome chromosome)
        //		{
        //			Openness = chromosome.Genes.Find (w => w.Name == "Openness").Value;
        //			Conscientiousness = chromosome.Genes.Find (w => w.Name == "Conscientiousness").Value;
        //			Introversion = chromosome.Genes.Find (w => w.Name == "Introversion").Value;
        //			Agreeableness = chromosome.Genes.Find (w => w.Name == "Agreeableness").Value;
        //			Neuroticism = chromosome.Genes.Find (w => w.Name == "Neuroticism").Value;
        //		}
        //			
        //		private Gene GetGene (int id, string name, double value)
        //		{
        //			return new Gene { Id = id, Name = name, Value = (float) value, Descriptor = gp };
        //		}
        //			
        //		#endregion


        #region "Personality-based Emotion Manipulation Methods"

        /// <summary>
        /// The net intensity of an emotion after applying the effect of all the personality attributes
        /// </summary>
        /// <param name="et">The emotion to be evaluated</param>
        /// <param name="value">The intensity of the emotion</param>
        /// <returns>The net intensity of an emotion after applying all the personality attributes</returns>
        private double NetEmotion(EmotionType et, double emotionalValue)
        {
            ////Console code to validate the effect of personality on emotional effect
            Debug.Log(String.Format("Event was evaluated to have a raw net emotional value of {0}({1}).", et.ToString(), emotionalValue.ToString()));
            Debug.Log(String.Format("Bonuses from personality: Neuroticism ({0}), Introversion ({1}), Openness ({2}), Agreeableness ({3}), Conscientiousness ({4}).",
                                                NeuroticimFactor(et, emotionalValue), IntroversionFactor(et, emotionalValue), OpennessFactor(et, emotionalValue),
                                                AgreeablenessFactor(et, emotionalValue), ConscientiousnessFactor(et, emotionalValue)));
            Debug.Log(String.Format("Net emotion to be applied was {0} ({1}).", et.ToString(), Math.Max(0, emotionalValue + NeuroticimFactor(et, emotionalValue) + IntroversionFactor(et, emotionalValue) +
                OpennessFactor(et, emotionalValue) + AgreeablenessFactor(et, emotionalValue) +
                ConscientiousnessFactor(et, emotionalValue)).ToString()));

            //Apply all the personality factors to work out net emotional value, making sure we never go below 0
            //since a raw increase in emotion should never result in a net decrease
            return Math.Max(0, emotionalValue + NeuroticimFactor(et, emotionalValue) + IntroversionFactor(et, emotionalValue) +
                OpennessFactor(et, emotionalValue) + AgreeablenessFactor(et, emotionalValue) +
                ConscientiousnessFactor(et, emotionalValue));
        }

        /// <summary>
        /// Calculates the amount to which neuroticism affects a specified emotion
        /// </summary>
        /// <param name="et">The emotion to be evaluated</param>
        /// <param name="value">The intensity of the emotion</param>
        /// <returns>The amount to which neuroticism affects a specified emotion</returns>
        private double NeuroticimFactor(EmotionType et, double value)
        {
            double trueNeuroticism = Neuroticism - NEUTRAL_PERSONALITY_INTENSITY;

            switch (et)
            {
                // neurotic indviduals feel less joy and gratification, but non-neurotic
                // individuals gain no bonus for neuroticism (Doce et al. - Creating Individual Agents
                // Through Personality Traits)
                case EmotionType.Joy:
                case EmotionType.Gratification:
                    if (trueNeuroticism > 0)
                        return -1 * (Neuroticism - NEUTRAL_PERSONALITY_INTENSITY) * value;
                    else
                        return 0;
                // neurotic individuals feel more distress and remorse, non-neurotic individuals
                // gain no bonus (Doce et al.)
                case EmotionType.Distress:
                case EmotionType.Remorse:
                    if (trueNeuroticism > 0)
                        return trueNeuroticism * value;
                    else
                        return 0;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Calculates the amount to which neuroticism affects a specified emotion
        /// </summary>
        /// <param name="et">The emotion to be evaluated</param>
        /// <param name="value">The intensity of the emotion</param>
        /// <returns>The amount to which neuroticism affects a specified emotion</returns>
        private double IntroversionFactor(EmotionType et, double value)
        {
            double trueIntroversion = Introversion - NEUTRAL_PERSONALITY_INTENSITY;

            // Introverts tend to feel emotions more intensly, while extroverts feel emotions
            // less intensly but overall this effect is smaller than the other personalities (Doce et al.)
            return value * trueIntroversion * 0.1; // The 0.1 has been chosen to ensure that the effect is small
        }

        /// <summary>
        /// Calculates the amount to which neuroticism affects a specified emotion
        /// </summary>
        /// <param name="et">The emotion to be evaluated</param>
        /// <param name="value">The intensity of the emotion</param>
        /// <returns>The amount to which neuroticism affects a specified emotion</returns>
        private double OpennessFactor(EmotionType et, double value)
        {
            switch (et)
            {
                // Open minded agents will feel Pride, Shame, Admiration, Reproach with
                // less intensity. Closed minded agents are the opposite(Doce et al)
                case EmotionType.Pride:
                case EmotionType.Shame:
                case EmotionType.Admiration:
                case EmotionType.Reproach:
                    return (NEUTRAL_PERSONALITY_INTENSITY - Openness) * value;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Calculates the amount to which neuroticism affects a specified emotion
        /// </summary>
        /// <param name="et">The emotion to be evaluated</param>
        /// <param name="value">The intensity of the emotion</param>
        /// <returns>The amount to which neuroticism affects a specified emotion</returns>
        private double AgreeablenessFactor(EmotionType et, double value)
        {
            switch (et)
            {
                // High agreeableness = more emotions towards positive emotions towards others
                // Low agreeableness = opposite
                // (Doce et al)
                case EmotionType.Love:
                case EmotionType.HappyFor:
                case EmotionType.Pity:
                case EmotionType.Admiration:
                case EmotionType.Gratitude:
                    return (Agreeableness - NEUTRAL_PERSONALITY_INTENSITY) * value;

                // High agreeableness = less emotions towards positive emotions towards others
                // Low agreeableness = opposite
                // (Doce et al)
                case EmotionType.Hate:
                case EmotionType.Resentment:
                case EmotionType.Gloating:
                case EmotionType.Reproach:
                case EmotionType.Anger:
                    return (NEUTRAL_PERSONALITY_INTENSITY - Agreeableness) * value;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Calculates the amount to which neuroticism affects a specified emotion
        /// </summary>
        /// <param name="et">The emotion to be evaluated</param>
        /// <param name="value">The intensity of the emotion</param>
        /// <returns>The amount to which neuroticism affects a specified emotion</returns>
        private double ConscientiousnessFactor(EmotionType et, double value)
        {
            switch (et)
            {
                // Conscientiousness is highly linked to Pride (Doce et al)
                case EmotionType.Pride:
                    return (Conscientiousness - NEUTRAL_PERSONALITY_INTENSITY) * value;
                // High score of conscientiousness delays gratification
                // A low conscientiousness score = higher gratification (Doce et al)
                case EmotionType.Gratification:
                    return (NEUTRAL_PERSONALITY_INTENSITY - Conscientiousness) * value;
                default:
                    return 0;
            }
        }

        #endregion

        /// <summary>
        /// Based on an eventMessage string, determines and applies the net emotional effect of an event to the agent
        /// and the reaction. For the expression of emotion, the bot will choose the dominant emotion it currently feels.
        /// We consider that this dominant emotion supresses other emotions the bot feels (though we still consider all
        /// feelings in the determination of a reaction to a bot's actions). 
        /// This method takes into considering the individual personality attributes of an agent when determining the
        /// true net emotional effect of an event.
        /// </summary>
        /// <param name="eventMessage"></param>
        public void EvaluateEvent(string eventMessage, PriorityPlanningAgent target)
        {
            // parse input text and generate series of emotional actions and consequences
            List<Reaction> events = _emotionProcessor.ProcessText(eventMessage);

            // clasify generated actions and generate general emotions
            Dictionary<EmotionType, double> resultingEmotions = ClassifyEvent(events);

            // calculate real emotions based on personality
            Dictionary<EmotionType, double> resultingPersonalityAffectedEmotions = CalculateNetEmotions(resultingEmotions);

            // modify emotions in the emotion tree
            ModifyEmotions(resultingPersonalityAffectedEmotions);

            // evaluate which emotion is the strongest
            EmotionType dominantEmotion = Emotions.findDominantEmotion(resultingPersonalityAffectedEmotions);

            // notify which emotion is the strongest
            OnEmotionRaised(dominantEmotion);

            // perform action with the highest relevance to current emotional status and a goal
            DetermineReactionBasedOnPersonality(DominantEmotionalEffect(resultingPersonalityAffectedEmotions), target);
        }

        public PersonalAction DecideGoalReaction(string goal)
        {
            if (GoalActions == null)
                return null;
            var actions = GoalActions.Where(w => w.Goal == goal);

            if (!actions.Any())
            {
                return null;
            }

            // now react
            return React(actions);
        }


        #region Emotion Classification + Quantification

        /// <summary>
        /// Using the parsed event objects, this method determines what emotional categories are effected
        /// and to what extent. This corresponds to the Classification and Quantification stages outlined
        /// by Bartneck (Integrating the OCC Model of Emotions in Embodied Characters)
        /// </summary>
        /// <param name="events">Events generated by the generateEvents() method</param>
        private Dictionary<EmotionType, double> ClassifyEvent(List<Reaction> events)
        {
            Dictionary<EmotionType, double> emotions = new Dictionary<EmotionType, double>();

            foreach (Reaction evt in events)
            {
                switch (evt.EventType)
                {
                    case EventType.Action:
                        ReactionToAction((ActionEvent)evt, emotions);
                        break;
                    case EventType.ActionConsequence:
                        ReactionToActionConsequence((ActionConsequenceEvent)evt, emotions);
                        break;
                    case EventType.Consequence:
                        ReactionToConsequence((ConsequenceEvent)evt, emotions);
                        break;
                    case EventType.Object:
                        // to do
                        break;
                }
            }

            return emotions;
        }

        /// <summary>
        /// Determine the emotions related to reactions to actions by an agent
        /// and add to the emotions dictionary. The emotional intensity has been
        /// calculated arbritrarily as stub code. Ideally, this emotional model
        /// would be integrated with a system capable of more accurate
        /// appraisals.
        /// </summary>
        /// <param name="evt">The action by an agent to processed</param>
        /// <param name="emotions">The emotions dictionary to which to add the resulting emotion</param>
        private void ReactionToAction(ActionEvent evt, Dictionary<EmotionType, double> emotions)
        {
            if (evt.FocusedOn == "me")
            {
                if (evt.IsPositiveEvent)
                    emotions.Add(EmotionType.Pride, evt.PraiseWorthiness / 5);
                else
                    emotions.Add(EmotionType.Shame, evt.PraiseWorthiness / 5);
            }
            else {
                if (evt.IsPositiveEvent)
                    emotions.Add(EmotionType.Admiration, evt.PraiseWorthiness / 5);
                else
                    emotions.Add(EmotionType.Reproach, evt.PraiseWorthiness / 5);
            }
        }

        /// <summary>
        /// Determine the emotions related to consequences of an event or action
        /// and add to the emotions dictionary. The emotional intensity has been
        /// calculated arbritrarily as stub code. Ideally, this emotional model
        /// would be integrated with a system capable of more accurate
        /// appraisals.
        /// </summary>
        /// <param name="evt">The event to be processed</param>
        /// <param name="emotions">The emotions dictionary to which to add the resulting emotion</param>
        private void ReactionToConsequence(ConsequenceEvent evt, Dictionary<EmotionType, double> emotions)
        {
            if (evt.getFocusedOn == "me")
            {
                if (evt.ProspectRelevant)
                {
                    if (evt.IsPositiveEvent)
                        emotions.Add(EmotionType.Hope, evt.Desirability / 5);
                    else
                        emotions.Add(EmotionType.Fear, evt.Desirability / 5);
                }
                else {
                    if (evt.IsPositiveEvent)
                        emotions.Add(EmotionType.Joy, evt.Desirability / 5);
                    else
                        emotions.Add(EmotionType.Distress, evt.Desirability / 5);
                }
            }
            else {
                if (evt.DesirableForOther)
                {
                    if (evt.IsPositiveEvent)
                        emotions.Add(EmotionType.HappyFor, evt.Desirability / 5);
                    else
                        emotions.Add(EmotionType.Resentment, evt.Desirability / 5);
                }
                else {
                    if (evt.IsPositiveEvent)
                        emotions.Add(EmotionType.Gloating, evt.Desirability / 5);
                    else
                        emotions.Add(EmotionType.Pity, evt.Desirability / 5);
                }
            }
        }

        /// <summary>
        /// Determine the compound emotions related to both the consequences of an event and
        /// to reactions to actions by an agent and add to the emotions dictionary.
        /// </summary>
        /// <param name="evt">The event to be processed</param>
        /// <param name="emotions">The emotions dictionary to which to add the resulting emotion</param>
        private void ReactionToActionConsequence(ActionConsequenceEvent evt, Dictionary<EmotionType, double> emotions)
        {
            if (evt.Action.FocusedOn == "me")
            {
                if (evt.Consequence.IsPositiveEvent)
                    emotions.Add(EmotionType.Gratification, CompoundEmotionValue(evt.Action.PraiseWorthiness, evt.Consequence.Desirability));
                else
                    emotions.Add(EmotionType.Remorse, CompoundEmotionValue(evt.Action.PraiseWorthiness, evt.Consequence.Desirability));
            }
            else {
                if (evt.Consequence.IsPositiveEvent)
                    emotions.Add(EmotionType.Gratitude, CompoundEmotionValue(evt.Action.PraiseWorthiness, evt.Consequence.Desirability));
                else
                    emotions.Add(EmotionType.Anger, CompoundEmotionValue(evt.Action.PraiseWorthiness, evt.Consequence.Desirability));
            }
        }


        /// <summary>
        /// The emotional intensity has been calculated arbritrarily as stub code. Ideally, this emotional model
        /// would be integrated with a system capable of more accurate
        /// appraisals.
        /// </summary>
        /// <param name="actionPart">The emotional value of the emotion resulting from a reaction to an act by an agent</param>
        /// <param name="consequencePart">The emotional value of the emotion resulting from a reaction to a consequence
        /// of an event</param>
        /// <returns></returns>
        private double CompoundEmotionValue(double actionPart, double consequencePart)
        {
            return ((actionPart / 5) + (consequencePart / 5));
        }

        #endregion

        #region "Emotional Modification"

        /// <summary>
        /// Generate a dictionary of emotions that have been processed by the personality of the bot
        /// </summary>
        /// <param name="emotions">The raw emotions resulting from an event, uninfluenced by personality</param>
        /// <returns>A dictionary of emotions and their intensities after being processed by the bot's
        /// personality</returns>
        private Dictionary<EmotionType, double> CalculateNetEmotions(Dictionary<EmotionType, double> emotions)
        {
            Dictionary<EmotionType, double> netEmotions = new Dictionary<EmotionType, double>();

            foreach (KeyValuePair<EmotionType, double> pair in emotions)
            {
                netEmotions.Add(pair.Key, NetEmotion(pair.Key, pair.Value));
            }

            return netEmotions;
        }

        /// <summary>
        /// Using the resultant emotions determined from classifyEvent() we proceed to modify the current
        /// emotions felt by the agent. This corresponds to the Interaction phase outlined by Bartneck. 
        /// </summary>
        private void ModifyEmotions(Dictionary<EmotionType, double> emotionalEffects)
        {
            Emotions.backupTree(); //so we can see the previous values in debug

            foreach (var pair in emotionalEffects)
            {
                Emotions.addToEmotion(pair.Key, pair.Value);
                System.Console.WriteLine(String.Format("{0}:{1}", pair.Key.ToString(), Emotions[pair.Key].ToString()));
            }

            // notify
            if (EmotionsChanged != null)
            {
                EmotionsChanged(this);
            }
        }

        #endregion

        #region "Emotion Reporting"

        /// <summary>
        /// Provides a categorical measure of emotional intensity from a 
        /// numerical value.
        /// </summary>
        /// <param name="intensity">The intensity of an emotion between 0 and 1.</param>
        /// <returns>The categorical measure of emotional intensity</returns>
        private string EmotionIntensityString(double intensity)
        {
            if (intensity > 0.85)
                return "EXTREMELEY ";
            if (intensity > 0.6)
                return "VERY ";
            if (intensity > 0.3)
                return "";

            return "A LITTLE ";
        }

        /// <summary>
        /// Provides an emotional state string from an emotion.
        /// </summary>
        /// <param name="emotion">The emotion for which a state will be returned</param>
        /// <returns>A string representing the emotional state of the bot</returns>
        private string StateFromEmotion(EmotionType emotion)
        {
            switch (emotion)
            {
                case EmotionType.Joy:
                    return "Joyful";
                case EmotionType.Pride:
                    return "Prideful";
                case EmotionType.Admiration:
                    return "filled with Admiration";
                case EmotionType.Love:
                    return "filled with love";
                case EmotionType.Interest:
                    return "Interested";
                case EmotionType.Gratification:
                    return "Gratified";
                case EmotionType.Gratitude:
                    return "Grateful";
                case EmotionType.Hope:
                    return "Hopeful";
                case EmotionType.HappyFor:
                    return "Happy for you";
                case EmotionType.Gloating:
                    return "Gloatful";
                case EmotionType.Distress:
                    return "Distressed";
                case EmotionType.Shame:
                    return "Shamed";
                case EmotionType.Reproach:
                    return "Reproached";
                case EmotionType.Hate:
                    return "filled with hate";
                case EmotionType.Disgust:
                    return "Disgusted";
                case EmotionType.Remorse:
                    return "Remorseful";
                case EmotionType.Anger:
                    return "Angry";
                case EmotionType.Fear:
                    return "Fearful";
                case EmotionType.Resentment:
                    return "Resentful";
                case EmotionType.Pity:
                    return "filled with Pity";
            }

            return "unsure of what I'm feeling now";
        }

        /// <summary>
        /// Plays an animation representing the supplied emotion.
        /// </summary>
        /// <param name="dominantEmotion">The emotion to be displayed</param>
        protected void OnEmotionRaised(EmotionType dominantEmotion)
        {
            if (EmotionExpressed != null)
            {
                // notify
                EmotionExpressed(
                        this,
                        dominantEmotion,
                        Emotions[dominantEmotion],
                        String.Format("I'm {0}{1}", EmotionIntensityString(Emotions[dominantEmotion]), StateFromEmotion(dominantEmotion)));
            }
        }

        /// <summary>
        /// This method is purely for debugging - it displays the values of all the current
        /// emotions as well as their previous value so you can see how they have changed.
        /// </summary>
        private void DisplayAllEmotions()
        {
            Console.WriteLine("+++++++++++++++++++++++++++++++ All Emotions ++++++++++++++++++++++++++++++++++++");

            foreach (EmotionType et in Enum.GetValues(typeof(EmotionType)))
                Console.WriteLine(String.Format("{0}: {1} --> {2}", et.ToString().PadLeft(15), Emotions[et, 1].ToString().PadLeft(7), Emotions[et, 0].ToString().PadLeft(7)));

            Console.WriteLine("++++++++++++++++++++++++++++++++++  End ++++++++++++++++++++++++++++++++++++++");
        }

        #endregion "Emotion Reporting"

        #region "Reaction Determination"

        /// <summary>
        /// Determine the reaction to an event based on the supplied emotional effect.
        /// </summary>
        /// <param name="dominantEmotionalEffect">The dominant emotional effect from an evaluated event</param>
        /// <param name="target">The agent from which an event was caused</param>
        private void DetermineReactionBasedOnPersonality(KeyValuePair<EmotionType, double> dominantEmotionalEffect, PriorityPlanningAgent target)
        {
            if (IsNegativeEmotion(dominantEmotionalEffect.Key))
            {
                if (IsTargetBasedEmotion(dominantEmotionalEffect.Key))
                    React(NegativeActionsTowardsTarget, target);
                else {
                    //normally, at this point we would work out an action for the bot for an emotion
                    //based on reacting to the bot itself. (ie. how it may react to its dominant emotion)
                    //We could then transfer control back  to a reaction to the target (if applicable).
                    //For the demonstrator, since we know all testing actions are the result of another bot
                    //and would thus justify a response to that bot, we will simply the code and just
                    //react to that bot. We have also done this because we have yet to work out
                    //how different personalities would express different intensities of emotion...for now, 
                    //in a higher level of the code, the bot just performs an animation dependent upon its
                    //dominant emotion...then control will fall to here.
                    React(NegativeActions);
                }
            }
            else {
                if (IsTargetBasedEmotion(dominantEmotionalEffect.Key))
                    React(PositiveActionsTowardsTarget, target);
                else {
                    //Refer to the comment in the above section...
                    React(PositiveActions);
                }
            }
        }

        /// <summary>
        /// Searches a dictionary of EmotionType and intensities for the EmotionType with the biggest
        /// value.
        /// </summary>
        /// <param name="emotionalEffects">A dictionary of EmotionType/Intensity</param>
        /// <returns>The key value pair with the greatest emotional value</returns>
        private KeyValuePair<EmotionType, double> DominantEmotionalEffect(Dictionary<EmotionType, double> emotionalEffects)
        {
            double biggestEmotionValue = Double.MinValue;
            EmotionType dominantEmotion = EmotionType.None;

            foreach (var pair in emotionalEffects)
            {
                if (pair.Value > biggestEmotionValue)
                {
                    biggestEmotionValue = pair.Value;
                    dominantEmotion = pair.Key;
                }
            }

            return new KeyValuePair<EmotionType, double>(dominantEmotion, biggestEmotionValue);
        }

        /// <summary>
        /// Determines if an emotion is negative.
        /// </summary>
        /// <param name="emotion">The emotion to be checked</param>
        /// <returns>True if negative, false otherwise</returns>
        private bool IsNegativeEmotion(EmotionType emotion)
        {
            switch (emotion)
            {
                case EmotionType.Joy:
                case EmotionType.Pride:
                case EmotionType.Admiration:
                case EmotionType.Love:
                case EmotionType.Interest:
                case EmotionType.Gratification:
                case EmotionType.Gratitude:
                case EmotionType.Hope:
                case EmotionType.HappyFor:
                case EmotionType.Gloating:
                    return false;
                case EmotionType.Distress:
                case EmotionType.Shame:
                case EmotionType.Reproach:
                case EmotionType.Hate:
                case EmotionType.Disgust:
                case EmotionType.Remorse:
                case EmotionType.Anger:
                case EmotionType.Fear:
                case EmotionType.Resentment:
                case EmotionType.Pity:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if an emotion is an emotion which requires an agent other than the current agent
        /// </summary>
        /// <param name="emotion">The emotion to be tested</param>
        /// <returns>True if target based, otherwise false</returns>
        private bool IsTargetBasedEmotion(EmotionType emotion)
        {
            if (emotion == EmotionType.Anger || emotion == EmotionType.Gratitude || emotion == EmotionType.HappyFor ||
                emotion == EmotionType.Resentment || emotion == EmotionType.Admiration || emotion == EmotionType.Reproach ||
                emotion == EmotionType.Gloating || emotion == EmotionType.Pity)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Determine which action towards a target from a provided list of actions has the highest utility and perform it.
        /// </summary>
        /// <param name="target">The avatar that caused an event</param>
        /// <param name="possibleActions">The list of possible actions to choose from</param>
        private PersonalAction React(IEnumerable<PersonalAction> possibleActions, PriorityPlanningAgent target = null)
        {
            if (possibleActions == null)
            {
                //Log.Warn ("There are no actions!");
                return null;
            }

            PersonalAction highestUtilityAction = null;
            double highestUtility = Double.MinValue;

            foreach (PersonalAction action in possibleActions)
            {
                double testUtility = action.Utility(this); //save as its quite costly to calculate
                                                           //Log.Info ("Performing: " + action.ToString () + ": " + testUtility.ToString ()); //line for debugging

                if (testUtility > highestUtility)
                {
                    highestUtility = testUtility;
                    highestUtilityAction = action;
                }
            }

            if (highestUtilityAction != null)
            {
                throw new NotImplementedException("MAKE THIS WORK!");
                //highestUtilityAction.Perform (Agent, target);
            }

            return highestUtilityAction;
        }
        #endregion "Reaction Determination"
    }
}

