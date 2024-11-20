using System;
using System.Collections.Generic;

namespace Pacman
{
    public class SimplePriorityQueue<T>
    {
        private List<Tuple<T, double>> elements = new List<Tuple<T, double>>();

        public int Count => elements.Count;

        public void Enqueue(T item, double priority)
        {
            elements.Add(Tuple.Create(item, priority));
        }

        public T Dequeue()
        {
            if (elements.Count == 0)
                throw new InvalidOperationException("The priority queue is empty.");

            int bestIndex = 0;
            double bestPriority = elements[0].Item2;

            for (int i = 1; i < elements.Count; i++)
            {
                if (elements[i].Item2 < bestPriority)
                {
                    bestPriority = elements[i].Item2;
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].Item1;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }

        public bool IsEmpty()
        {
            return elements.Count == 0;
        }
    }
}
