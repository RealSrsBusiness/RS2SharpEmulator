using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerEmulator.Core.Game
{
    public static class Movement
    {
        public enum Direction : int
        {
            NONE = -1,
            NORTH_WEST = 0,
            NORTH = 1,
            NORTH_EAST = 2,
            EAST = 4,
            SOUTH_EAST = 7,
            SOUTH = 6,
            SOUTH_WEST = 5,
            WEST = 3,
        }

        public readonly static Coordinate[] Directions = new Coordinate[] {
            new Coordinate() { x = -1, y = 1 },
            new Coordinate() { x = 0, y = 1 },
            new Coordinate() { x = 1, y = 1 },
            new Coordinate() { x = -1, y = 0 },
            new Coordinate() { x = 1, y = 0 },
            new Coordinate() { x = -1, y = -1 },
            new Coordinate() { x = 0, y = -1 },
            new Coordinate() { x = 1, y = -1 }
        };

        public static Direction GetDirectionType(int deltaX, int deltaY)
        {
            if (deltaX < 0)
            {
                if (deltaY < 0)
                    return Direction.SOUTH_WEST;
                else if (deltaY > 0)
                    return Direction.NORTH_WEST;
                else
                    return Direction.WEST;
            }
            else if (deltaX > 0)
            {
                if (deltaY < 0)
                    return Direction.SOUTH_EAST;
                else if (deltaY > 0)
                    return Direction.NORTH_EAST;
                else
                    return Direction.EAST;
            }
            else
            {
                if (deltaY < 0)
                    return Direction.SOUTH;
                else if (deltaY > 0)
                    return Direction.NORTH;
                else
                    return Direction.NONE;
            }
        }

        public static Direction[] InterpolateWaypoints(Coordinate[] waypointCoords, int lastX, int lastY)
        {
            List<Direction> path = new List<Direction>();

            for (int i = 0; i < waypointCoords.Length; i++)
            {
                var coord = waypointCoords[i];

                //tiles difference between waypoints
                int difX = coord.x - lastX;
                int difY = coord.y - lastY;

                var direction = GetDirectionType(difX, difY);

                int steps = 0;
                //determine how many steps to make
                if (difX < 0)
                    steps = difX * (-1);
                else if (difX > 0)
                    steps = difX;
                else if (difY < 0)
                    steps = difY * (-1);
                else if (difY > 0)
                    steps = difY;

                for (int j = 0; j < steps; j++)
                    path.Add(direction);

                //this coord is compared to the next coord in the next iteration
                lastX = coord.x;
                lastY = coord.y;
            }

            return path.ToArray();
        }
    }

    public struct Coordinate3
    {
        public int x, y, z;
        public static Coordinate3 operator +(Coordinate3 c1, Coordinate3 c2) =>
            new Coordinate3() { x = c1.x + c2.x, y = c1.y + c2.y, z = c1.z + c2.z };
    }

    public struct Coordinate
    {
        public int x, y;

        public static Coordinate operator +(Coordinate c1, Coordinate c2) =>
            new Coordinate() { x = c1.x + c2.x, y = c1.y + c2.y };

        public static bool operator ==(Coordinate c1, Coordinate c2) => c1.x == c2.x && c1.y == c2.y;
        public static bool operator !=(Coordinate c1, Coordinate c2) => c1.x != c2.x || c1.y != c2.y;

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();

        public readonly static Coordinate NONE = new Coordinate { x = int.MaxValue, y = int.MaxValue };
    }
}
