using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ViAgents.Personality
{
    /// <summary>
    /// A tree structure to aid with the processing of emotions
    /// </summary>
    public class EmotionTree
    {
        private List<EmotionNode> _root; //store top level nodes in a list

        public EmotionTree()
        {
            _root = new List<EmotionNode>();
        }

        public EmotionTree(List<EmotionNode> list)
        {
            _root = list;
        }

        public void Add(EmotionNode node)
        {
            _root.Add(node);
        }

        public double this[EmotionType et]
        {
            get
            {
                return this[et, 0];
            }
        }

        public double this[EmotionType et, int index]
        {
            get
            {
                EmotionNode node = getNode(et);

                if (node != null)
                    return node.EmotionValue(et, index);
                else
                    return -99999;
            }
        }

        /// <summary>
        /// Perform a top recursive search of all the nodes in the tree for
        /// the specified emotion.
        /// </summary>
        /// <param name="et">The emotion to search for</param>
        /// <returns>The emotion node containing the emotion if found, otherwise null</returns>
        public EmotionNode getNode(EmotionType et)
        {
            foreach (EmotionNode child in _root)
            {
                EmotionNode result = recursiveSearch(child, et);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// The recursive component of getNode()
        /// </summary>
        /// <param name="node">The node to search</param>
        /// <param name="et">The emotion to search for</param>
        /// <returns>The emotion node containing the emotion if found, otherwise null</returns>
        private EmotionNode recursiveSearch(EmotionNode node, EmotionType et)
        {
            if (node.Represents(et))
                return node;
            else
            {
                foreach (EmotionNode child in node.Children)
                {
                    EmotionNode result = recursiveSearch(child, et);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Add to the intensity of a specific emotion in the tree
        /// </summary>
        /// <param name="et">The emotion type to add to</param>
        /// <param name="value">The intensity to add</param>
        public void addToEmotion(EmotionType et, double value)
        {
            EmotionNode node = getNode(et);

            if (node != null)
                node.addToEmotion(et, value);
        }

        /// <summary>
        /// Find the most intense emotion in the tree. Is recursive.
        /// </summary>
        /// <param name="recentEmotionChanges">A dictionary of the most recent processed emotions to be used for tie breakers</param>
        /// <returns>The most intense emotion in the tree found</returns>
        public EmotionType findDominantEmotion(Dictionary<EmotionType, double> recentEmotionChanges)
        {
            List<EmotionType> dominantEmotions = new List<EmotionType>();

            double largestValue = Double.MinValue;

            foreach (EmotionNode child in _root)
            {
                findDominantEmotion(child, dominantEmotions, ref largestValue);
            }

            // if there are two emotions with the same intensity
            if (dominantEmotions.Count > 1)
            {
                // check the most recent emotional changes, and choose the first one found
                foreach (KeyValuePair<EmotionType, double> pair in recentEmotionChanges.OrderByDescending(key => key.Value))
                {
                    if (recentEmotionChanges.Keys.Contains<EmotionType>(pair.Key))
                        return pair.Key;
                }

                // if not a recent change, then just randomly select one
                Random random = new Random();
                return dominantEmotions[random.Next(0, dominantEmotions.Count)];
            }
            else //there is only one dominant emotion
                return dominantEmotions[0];
        }

        /// <summary>
        /// A recursive search for the dominant emotion in a node and its children
        /// </summary>
        /// <param name="node">The node to be searched</param>
        /// <param name="dominantEmotions">The dominant emotions found</param>
        /// <param name="largestValue">The largest emotional intensity found so far</param>
        private void findDominantEmotion(EmotionNode node, List<EmotionType> dominantEmotions, ref double largestValue)
        {           
            if (node.PositiveEmotionValue > largestValue)
            {
                dominantEmotions.Clear();
                dominantEmotions.Add(node.PositiveEmotion);
                largestValue = node.PositiveEmotionValue;
            }

            if (node.NegativeEmotionValue > largestValue)
            {
                dominantEmotions.Clear();
                dominantEmotions.Add(node.NegativeEmotion);
                largestValue = node.NegativeEmotionValue;
            }

            if (node.PositiveEmotionValue == largestValue && !dominantEmotions.Contains(node.PositiveEmotion))
            {
                dominantEmotions.Add(node.PositiveEmotion);
            }

            if (node.NegativeEmotionValue == largestValue && !dominantEmotions.Contains(node.NegativeEmotion))
            {
                dominantEmotions.Add(node.NegativeEmotion);
            }

            foreach (EmotionNode child in node.Children)
            {
                findDominantEmotion(child, dominantEmotions, ref largestValue);
            }
        }

        /// <summary>
        /// Save the pervious values in a tree. Used for debugging.
        /// </summary>
        public void backupTree()
        {
            foreach (EmotionNode node in _root)
            {
                backupTreeRecursive(node);
            }
        }

        /// <summary>
        /// The recursive save function used only by backupTree()
        /// </summary>
        /// <param name="node">The node to be gone through</param>
        private void backupTreeRecursive(EmotionNode node)
        {
            node.backupState();

            foreach (EmotionNode child in node.Children)
            {
                child.backupState();
            }
        }
       
    }
}
