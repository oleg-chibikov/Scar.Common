using System.Collections.Generic;

namespace Scar.Common.ImageProcessing.Metadata;

public static class OrientationExtensions
{
    static readonly LinkedList<Orientation> Orientations = new(
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

        var first = Orientations.First!;
        var currentNode = first;
        while (currentNode.Next != null && currentNode.Value != orientation)
        {
            currentNode = currentNode.Next;
        }

        var nextNode = rotationType == RotationType.Clockwise ? currentNode.Next ?? first : currentNode.Previous ?? Orientations.Last!;

        return nextNode.Value;
    }
}
