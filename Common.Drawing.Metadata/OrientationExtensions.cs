using System.Collections.Generic;

namespace Scar.Common.Drawing.Metadata
{
    public static class OrientationExtensions
    {
        private static readonly LinkedList<Orientation> Orientations = new LinkedList<Orientation>(
            new[]
            {
                Orientation.Straight,
                Orientation.Left,
                Orientation.Reverse,
                Orientation.Right
            });

        public static Orientation GetNextOrientation(this Orientation orientation, RotationType rotationType)
        {
            if (orientation == Orientation.NotSpecified)
            {
                orientation = Orientation.Straight;
            }

            var currentNode = Orientations.First;
            while (currentNode.Next != null && currentNode.Value != orientation)
            {
                currentNode = currentNode.Next;
            }

            LinkedListNode<Orientation> nextNode;
            if (rotationType == RotationType.Clockwise)
            {
                nextNode = currentNode.Next ?? Orientations.First;
            }
            else
            {
                nextNode = currentNode.Previous ?? Orientations.Last;
            }

            return nextNode.Value;
        }
    }
}