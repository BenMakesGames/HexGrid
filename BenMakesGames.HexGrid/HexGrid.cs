﻿using System;
using System.Collections.Generic;

namespace BenMakesGames.HexGrid;

public static class HexGrid
{
    // from https://stackoverflow.com/questions/14491444/calculating-distance-on-a-hexagon-grid
    // (with simplifications, "abusing" integer division)
    public static int Distance((int x, int y) p1, (int x, int y) p2) => Math.Max(
        Math.Abs(p2.y - p1.y),
        Math.Max(
            Math.Abs(-(p2.y / 2) + p2.x + (p1.y / 2) - p1.x),
            Math.Abs(-p2.y + (p2.y / 2) - p2.x + p1.y - (p1.y / 2) + p1.x)
        )
    );

    /// <summary>
    /// Returns the coordinates of the location `distance` spaces away from the `origin` in the given `direction`.
    /// </summary>
    public static (int x, int y) Move((int x, int y) origin, Direction d, int distance = 1)
    {
        if (d == Direction.East)
            return (origin.x + distance, origin.y);

        if (d == Direction.West)
            return (origin.x - distance, origin.y);

        // you'll just have to trust me on this (or draw it on paper)
        // o o o o
        //  o o o o
        // o o o o
        //  o o o o
        var finalY = origin.y + (d == Direction.NorthEast || d == Direction.NorthWest ? -distance : distance);

        var xOffset = 0;

        if (Math.Abs(finalY % 2) > Math.Abs(origin.y % 2))
            xOffset = -1;
        else if (Math.Abs(finalY % 2) < Math.Abs(origin.y % 2))
            xOffset = 1;

        var finalX = (origin.x * 2 + xOffset + (d == Direction.NorthEast || d == Direction.SouthEast ? distance : -distance)) / 2;

        return (finalX, finalY);
    }

    public static List<(int x, int y)> ComputeRectangle((int x, int y) origin, int w, int h)
    {
        var tiles = new List<(int, int)>();

        for (var xx = origin.x; xx < origin.x + w; xx++)
        {
            for (var yOffset = 0; yOffset < h; yOffset++)
            {
                if (yOffset % 2 == 0)
                    tiles.Add((xx, origin.y + yOffset));
                else
                {
                    var tileToAdd = Move((xx, origin.y + yOffset - 1), Direction.SouthWest);

                    tiles.Add(tileToAdd);

                    if (xx == origin.x + w - 1)
                    {
                        tiles.Add((tileToAdd.x + 1, tileToAdd.y));
                    }
                }
            }
        }

        return tiles;
    }

    /// <summary>
    /// Returns -1 if `target` is to the left of `origin`
    /// Returns 1 if `target` is to the right of `origin`
    /// Returns 0 otherwise
    /// </summary>
    public static int ComputeLeftRightOrientation((int x, int y) origin, (int x, int y) target)
    {
        // tile and origin are either BOTH ODD, or BOTH EVEN
        if (Math.Abs(target.y % 2) == Math.Abs(origin.y % 2))
        {
            if (target.x < origin.x)
                return -1;

            if (target.x > origin.x)
                return 1;

            return 0;
        }

        // tile is on ODD row; origin is on EVEN row:
        if (Math.Abs(target.y % 2) == 1 && origin.y % 2 == 0)
        {
            return target.x >= origin.x ? 1 : -1;
        }

        // tile is on EVEN row; origin is on ODD row:
        return target.x <= origin.x ? -1 : 1;
    }

    /// <summary>
    /// Given an `origin` and `destination`, returns a `Direction` that one would have to move, IN A STRAIGHT LINE from
    /// the `origin` to reach the `destination`. If it is not possible to move in a straight line, this function
    /// returns `null`.</summary>
    public static Direction? ComputeDirection((int x, int y) origin, (int x, int y) destination)
    {
        // micro-optimization: if Y coordinates are equal, then this is real easy to find out:
        if (destination.y == origin.y)
        {
            if (destination.x < origin.x)
                return Direction.West;
            else if (destination.x > origin.x)
                return Direction.East;
            else
                return null;
        }

        var distance = Distance(origin, destination);

        var directions = new[]
        {
            Direction.NorthEast,
            Direction.NorthWest,
            Direction.SouthEast,
            Direction.SouthWest
        };

        foreach (var dir in directions)
        {
            var end = Move(origin, dir, distance);

            if (end.x == destination.x && end.y == destination.y)
                return dir;
        }

        return null;
    }

    /// <summary>
    /// An "arc" is formed by connecting two lines at a cell in such a way that the "arc" points in the given
    /// direction. For example, an arc facing `Direction.East` would look like a greater-than symbol: >
    ///
    /// The `sideLength` parameter determines how long each half of the arc is.
    /// </summary>
    public static List<(int x, int y)> ComputeArc((int x, int y) origin, Direction direction, int sideLength)
    {
        var arc = new List<(int, int)>() { origin };

        arc.AddRange(ComputeLine(origin, Rotate(direction, -2), 1, sideLength));
        arc.AddRange(ComputeLine(origin, Rotate(direction, 2), 1, sideLength));

        return arc;
    }

    /// <summary>
    /// Given an initial `direction`, and a number of clockwise `turns` to make, returns the direction after making
    /// that many `turns`. (A Negative number of `turns` indicates COUNTER-clockwise rotation.)
    /// </summary>
    public static Direction Rotate(Direction direction, int turns)
    {
        if (turns == 0)
            return direction;

        var directions = new List<Direction>()
        {
            Direction.NorthWest,
            Direction.NorthEast,
            Direction.East,
            Direction.SouthEast,
            Direction.SouthWest,
            Direction.West
        };

        var angle = directions.IndexOf(direction) + turns;

        if (angle < 0)
            angle += directions.Count * (-turns / directions.Count + 1);

        return directions[angle % directions.Count];
    }

    /// <summary>
    /// Returns a list of coordinates to form an asterisk shape. A `minDistance` greater than 0 will cause the center
    /// of the asterisk to be missing, as if a circle of radius `minDistance` was cut out of the center.
    /// </summary>
    public static List<(int x, int y)> ComputeAsterisk((int x, int y) origin, int minDistance, int maxDistance)
    {
        var asterisk = new List<(int x, int y)>();

        asterisk.AddRange(ComputeLine(origin, Direction.NorthWest, minDistance, maxDistance));
        asterisk.AddRange(ComputeLine(origin, Direction.NorthEast, minDistance, maxDistance));
        asterisk.AddRange(ComputeLine(origin, Direction.East, minDistance, maxDistance));
        asterisk.AddRange(ComputeLine(origin, Direction.SouthEast, minDistance, maxDistance));
        asterisk.AddRange(ComputeLine(origin, Direction.SouthWest, minDistance, maxDistance));
        asterisk.AddRange(ComputeLine(origin, Direction.West, minDistance, maxDistance));

        return asterisk;
    }

    /// <summary>
    /// Returns a list of coordinates starting at the `origin`, and traveling `maxDistance` tiles in the given
    /// direction. A `minDistance` greater than 0 will cause that many cells to be skipped from the beginning of the
    /// line.
    /// </summary>
    public static List<(int x, int y)> ComputeLine((int x, int y) origin, Direction direction, int minDistance, int maxDistance)
    {
        var line = new List<(int x, int y)>();

        var current = Move(origin, direction, minDistance - 1);

        var length = maxDistance - minDistance + 1;

        for (var i = 0; i < length; i++)
        {
            current = Move(current, direction);

            line.Add(current);
        }

        return line;
    }

    /// <summary>
    /// Returns a list of coordinates centered on the `origin` that represents a filled circle or ring. A `minDistance`
    /// of 0 will create a circle.
    /// </summary>
    public static List<(int x, int y)> ComputeRing((int x, int y) origin, int minDistance, int maxDistance)
    {
        var ring = new List<(int x, int y)>();

        for (var d = minDistance; d <= maxDistance; d++)
            ring.AddRange(ComputeRing(origin, d));

        return ring;
    }

    /// <summary>
    /// Returns a list of coordinates centered on the `origin` that represents a unfilled circle/ring.
    /// </summary>
    public static List<(int x, int y)> ComputeRing((int x, int y) origin, int distance)
    {
        if (distance == 0)
            return new List<(int x, int y)>() { origin };

        var ring = new List<(int x, int y)>();

        var current = Move(origin, Direction.NorthWest, distance);

        for (var side = 0; side < 6; side++)
        {
            var d = side switch
            {
                0 => Direction.East,
                1 => Direction.SouthEast,
                2 => Direction.SouthWest,
                3 => Direction.West,
                4 => Direction.NorthWest,
                _ => Direction.NorthEast
            };

            for (var l = 0; l < distance; l++)
            {
                current = Move(current, d);

                ring.Add(current);
            }
        }

        return ring;
    }
}