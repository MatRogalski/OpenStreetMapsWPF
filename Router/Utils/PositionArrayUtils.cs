using GeoJSON.Net.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Router.Utils
{
    public static class PositionArrayUtils
    {
        public static Position[] GetPositionsArray(Position source, Position destination, Position[] intermediates)
        {
            Position[] positions = new Position[intermediates.Length + 2];
            positions[0] = source;
            Array.Copy(intermediates, 0, positions, 1, intermediates.Length);
            positions[positions.Length - 1] = destination;
            return positions;
        }
    }
}
