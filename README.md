A C# class and enum that helps perform a variety of hex-grid functions, such as computing distance, getting directions, and drawing various shapes: lines, asterisks, arcs, rings, circles, and rectangles.

This library assumes a hex grid of the following orientation:

```
o o o o
 o o o o
o o o o
 o o o o
```

* (0, 0) is the top-left cell
* (1, 0) is the second cell in the first row
* (0, 1) is the first cell in the second row
* etc

The Direction helper enum contains the six directions of movement allowed in a grid of this orientation: NorthWest, NorthEast, East, SouthEast, SouthWest, and West.

This library does not make any assumptions or have any requirements about how you store your tiles, however the above orientation was chosen because it is easy to represent using a traditional 2D array. All the methods provided by this library use (int x, int y) tuples to represent coordinates, accepting and returning coordinates in that form.

* nuget package: https://www.nuget.org/packages/BenMakesGames.HexGrid
* GitHub repo: https://github.com/BenMakesGames/HexGrid

---

## int HexGrid.Distance((int x, int y) p1, (int x, int y) p2)

Returns the number of cells needed to travel between two points (**p1** and **p2**) on a hex grid.

Example usage:

```c#
int distance = HexGrid.Distance((0, 0), (4, 7));
```

## (int x, int y) HexGrid.Move((int x, int y) origin, Direction d, int distance = 1)

Returns the coordinates for a point **distance** tiles away from **origin**, in the given direction, **d**.

* **origin**: The starting location.
* **d**: The direction to move in, from the Direction enum.
* **distance**: The number of tiles to move; optional, defaulting to 1.

Example usage:

```c#
(int x, int y) currentLocation = (4, 4);

currentLocation = HexGrid.Move(currentLocation, Direction.NorthEast);
```

## int HexGrid.ComputeLeftRightOrientation((int x, int y) origin, (int x, int y) target)

Returns -1 if **target** is left of **origin**; 1 if **target** is right of **origin**; 0 if they are in the same visual column.

Remember, with hex grids aligned in the following orientation, this is not a trivial problem:

```
o o o o
 o o o o
o o o o
 o o o o
```

* **origin**: The tile to compute the direction FROM.
* **target**: The tile to compute the direction TOWARDS.

Example usage:

```c#
(int x, int y) origin = (1, 1);
(int x, int y) target = (1, 2);

int orientation = HexGrid.ComputeLeftRightOrientation(origin, target);

if(orientation < 1)
    Console.WriteLine("Target is to the left of Origin.");
else if(orientation > 1)
    Console.WriteLine("Target is to the right of Origin.");
else
    Console.WriteLine("Target and Origin are directly above/below one another.");
```

## Direction? HexGrid.ComputeDirection((int x, int y) origin, (int x, int y) destination)

Returns a the Direction you would need to travel, from **origin**, in a straight line, to reach **destination**. If it is not possible to go in a straight line, returns null, instead.

* **origin**: The starting tile.
* **destination**: The theoretical ending cell.

Example usage:

```c#
(int x, int y) origin = (1, 1);
(int x, int y) target = (1, 2);

Direction d = HexGrid.ComputeDirection(origin, target);

if(d == null)
    Console.WriteLine("Origin and Target are not in a straight line.");
else
    Console.WriteLine("Origin can move " + d.ToString() + " in a straight line to reach Target.")
```

## Direction HexGrid.Rotate(Direction direction, int turns)

In a hex grid, a straight line can be rotated 60 degrees left or right.

This method returns a new Direction which represents the given **direction** rotated 60 degrees clockwise **turns** times. (A negative value for **turns** represents counter-clockwise rotation.)

```c#
Direction newDirection = HexGrid.Rotate(Direction.NorthWest, 2);

Console.WriteLine(newDirection.ToString()); // outputs "East"
```

## List<(int x, int y)> HexGrid.ComputeLine((int x, int y) origin, Direction direction, int minDistance, int maxDistance)

Returns a list of coordinates that represent a line, starting at the **origin** and extending **maxDistance** cells away in the given **direction**. If a **minDistance** is given, then that many cells from the start of the line are skipped.

Example usage:

```c#
var line = HexGrid.ComputeLine((0, 0), Direction.SouthEast, 0, 4);
```

## List<(int x, int y)> HexGrid.ComputeAsterisk((int x, int y) origin, int minDistance, int maxDistance)

Returns a list of coordinates that represents an asterisk (6 straight lines) emanating from the **origin**. Each line will start **minDistance** cells away from the origin, and end **maxDistance** cells away from it. (A **minDistance** of 0 makes a complete asterisk.)

Example usage:

```c#
var asterisk = HexGrid.ComputeAsterisk((2, 2), 1, 2);
```

The above code would return a list of coordinates representing the following asterisk (the upper-left tile is (0, 0)). Note that the center tile is skipped, because a **minDistance** of 1 was used:

```
o X o X o
 o X X o o
X X o X X
 o X X o o
o X o X o
```

## List<(int x, int y)> HexGrid.ComputeArc((int x, int y) origin, Direction direction, int sideLength)

An "arc" is formed by connecting two lines at the **origin**, such that the two lines form an arrowhead pointing in the given **direction**. The length of each side of the arrow head will be **sideLength**.

Example usage:

```
var arc = HexGrid.ComputeArc((4, 3), Direction.East, 3);
```

The above code would return a list of coordinates representing the following arc (the upper-left tile is (0, 0)):

```
o o o o o o
 o o o X o o
o o o o X o
 o o o o X o
o o o o X o
 o o o X o o
```

## List<(int x, int y)> HexGrid.ComputeRing((int x, int y) origin, int distance)

Returns a list of coordinates that represent a ring (hexagon) centered on the **origin**, with a radius of **distance**.

## List<(int x, int y)> HexGrid.ComputeRing((int x, int y) origin, int minDistance, int maxDistance)

Returns a list of coordinates that represent a solid donut or circle (hexagon) centered on the **origin**, with an inner radius of **minDistance** and an outer radius of **maxDistance**. A **minDistance** of 0 results in a solid circle with no hole.

## List<(int x, int y)> HexGrid.ComputeRectangle((int x, int y) origin, int w, int h)

Returns a list of coordinates that represent a solid rectangle whose upper-left coordinate is **origin**, having width **w** and height **h**.