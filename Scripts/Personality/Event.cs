using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViAgents.Personality
{
    /// <summary>
    /// A base class to present events for emotional processing
    /// </summary>
    abstract class Reaction
    {
        protected EventType _eventType;
        protected bool _isPositiveEvent;

        public Reaction(EventType eventType, bool positiveEvent)
        {
            _eventType = eventType;
            _isPositiveEvent = positiveEvent;
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool IsPositiveEvent
        {
            get { return _isPositiveEvent; }
        }
    }

    /// <summary>
    /// Represents an event that causes a consequence
    /// </summary>
    class ConsequenceEvent : Reaction
    {
        private string _focusedOn;
        private double _disirability;
        private bool _prospectRelevant;
        private bool _desirableForOther;

        public ConsequenceEvent(EventType eventType, bool positiveEvent, string focusedOn, 
                                double disirability, bool prospectRelevant, bool desirableForOther) : base(eventType, positiveEvent)
        {
            _focusedOn = focusedOn;
            _disirability = disirability;
            _prospectRelevant = prospectRelevant;
            _desirableForOther = desirableForOther;
        }

        public string getFocusedOn
        {
            get { return _focusedOn; }
        }

        public double Desirability
        {
            get { return _disirability; }
        }

        public bool ProspectRelevant
        {
            get { return _prospectRelevant; }
        }

        public bool DesirableForOther
        {
            get { return _desirableForOther; }
        }
    }

    /// <summary>
    /// Represents an event that was brought about by the actions of an agent
    /// </summary>
    class ActionEvent : Reaction
    {
        private string _focusedOn;
        private double _praiseworthiness;

        public ActionEvent(EventType eventType, bool positiveEvent, string focusedOn, double praiseworthiness)
            : base(eventType, positiveEvent)
        {
            _focusedOn = focusedOn;
            _praiseworthiness = praiseworthiness;
        }

        public string FocusedOn
        {
            get { return _focusedOn; }
        }

        public double PraiseWorthiness
        {
            get { return _praiseworthiness; }
        }
    }

    /// <summary>
    /// Represents an event that has both a consequence to be evaluated and an action by an agent to be evaluated
    /// </summary>
    class ActionConsequenceEvent : Reaction
    {
        private ConsequenceEvent _consequence;
        private ActionEvent _action;

        public ActionConsequenceEvent(EventType eventType, bool positiveEvent, ConsequenceEvent consequence, ActionEvent action)
            : base(eventType, positiveEvent)
        {
            _consequence = consequence;
            _action = action;
        }

        public ConsequenceEvent Consequence
        {
            get { return _consequence; }
        }

        public ActionEvent Action
        {
            get { return _action; }
        }
    }

    /// <summary>
    /// Represents an event to do with liking/disliking a certain object
    /// </summary>
    class ObjectEvent : Reaction
    {
        private double _appeal;

        public ObjectEvent(EventType eventType, bool positiveEvent, double appeal)
            : base(eventType, positiveEvent)
        {
            _appeal = appeal;
        }
    }
}
