namespace ViAgents.Physiology
{
    public class PhysiologyModel
    {
        public float speedMultiplier = 1;
        public float hungerModifier = 1;
        public float thirstModifier = 1;
        public float energyModifier = 1;

        private float hunger;
        private float thirst;
        private float energy;

        float sleepMultiplier = 0.0f;

        PriorityPlanningAgent agent;

        // decay rates
        // we get hungry twice a day

        static float hungerDecay;
        static float thirstDecay;
        static float energyDecay;

        static float updateRateInSeconds = 1;

        // properties

        public bool IsThirsty { get { return this.thirst >= 50; } }

        public bool IsHungry { get { return this.hunger >= 50; } }

        public bool IsTired { get { return this.energy <= 50; } }

        public float Hunger
        {
            get { return this.hunger; }
            set { this.hunger = value; }
        }

        public float Thirst
        {
            get { return this.thirst; }
            set { this.thirst = value; }
        }

        public float Energy
        {
            get { return this.energy; }
            set { this.energy = value; }
        }

        public PhysiologyModel(float dayInSeconds, PriorityPlanningAgent agent)
        {
            // initialise multithreader
            //			var mt = Loom.Current;

            this.hunger = 0;
            this.thirst = 0;
            this.energy = 100;
            this.agent = agent;

            // find the time control component


            // decay is calculated so that in some time interval in part of the day
            // 50 is the treshold value
            // increment = (50 / timeToFillInSeconds) * updateRateInSeconds;

            // agents get hungry two times
            hungerDecay = (50 / (dayInSeconds / 2)) * updateRateInSeconds;
            // agents get thirsty four times
            thirstDecay = (50 / (dayInSeconds / 4)) * updateRateInSeconds;
            // agents get tired three times
            energyDecay = (50 / (dayInSeconds / 3)) * updateRateInSeconds;
        }


        public void Update(float elapsedSeconds)
        {
            // we have different metabolism at night than during the day
            if (this.agent.IsSleeping)
            {
                this.hunger += hungerDecay * elapsedSeconds * speedMultiplier * hungerModifier * sleepMultiplier;
                this.thirst += thirstDecay * elapsedSeconds * speedMultiplier * thirstModifier * sleepMultiplier;
                this.energy += energyDecay * elapsedSeconds * speedMultiplier * energyModifier * 4;

            }
            else {
                this.hunger += hungerDecay * elapsedSeconds * speedMultiplier * hungerModifier;
                this.thirst += thirstDecay * elapsedSeconds * speedMultiplier * thirstModifier;
                this.energy -= energyDecay * elapsedSeconds * speedMultiplier * energyModifier;
            }


            // clamp values     //aram --  from 0 to 100
            this.hunger = Clamp(this.hunger, 0, 100);
            this.thirst = Clamp(this.thirst, 0, 100);
            this.energy = Clamp(this.energy, 0, 100);
        }

        float Clamp(float val, float min = 0, float max = 100)
        {
            if (val < min)
            {
                return min;
            }
            if (val > max)
            {
                return max;
            }
            return val;
        }
    }
}

