using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class contains some utility classes and functions
public class UTL {
    public static int InvalidInt = -1;  // will be used to denote an invalid integer, or invalid index (for example, and integer that has not been initialized yet)

    // a class containing some of the basic math functions
    public class Math {
        static public int Min(int n1, int n2) { return n1 < n2 ? n1 : n2; }
        static public int Max(int n1, int n2) { return n1 < n2 ? n2 : n1; }
    }

    public static int RootLength() { return GameManager.Instance.GridRootLength(); }  // keep track of the square root of the size of the Sudoku-Puzzle
    public static int Length() { return GameManager.Instance.GridLength(); }  // keep track of the size of the Sudoku-Puzzle
    public static bool IsCoordInGrid(int x) { return x >= 0 && x < Length(); }  // check whether x is the in grid-range

    // A 2-dim coordinate
    public class Coord2 {
        public int x, y;
        public Coord2(int x_ = 0, int y_ = 0) {
            x = x_;
            y = y_;
        }
        public virtual bool IsInGrid() {
            return IsCoordInGrid(x) && IsCoordInGrid(y);
        }
        public static Coord2 operator+(Coord2 lhs, Coord2 rhs) => new Coord2(lhs.x + rhs.x, lhs.y + rhs.y);
        public virtual int GetIdx() { return CoordToIdx(x, y); }
        public Coord2 getSquareCoord() { return new Coord2(x / RootLength(), y / RootLength()); }
        public bool sameSquareCoord(Coord2 other) { return getSquareCoord() == other.getSquareCoord(); }
    }

    // A 3-dim coordinate
    public class Coord3 : Coord2 {
        public int z;
        public Coord3(int x_ = 0, int y_ = 0, int z_ = 0) {
            x = x_;
            y = y_;
            z = z_;
        }
        public override bool IsInGrid() {
            return IsCoordInGrid(x) && IsCoordInGrid(y) && IsCoordInGrid(z);
        }
        public static Coord3 operator +(Coord3 lhs, Coord3 rhs) => new Coord3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        public override int GetIdx() { return CoordToIdx(x, y, z); }
        public bool SameSquareCoord(Coord3 other) { return getSquareCoord() == other.getSquareCoord() && z == other.z; }
    }

    public static int GetX(int idx) { return idx % Length(); }  // extracts the x coordinate from an index
    public static int GetY(int idx) { return (idx / Length()) % Length(); }  // extracts the y coordinate from an index
    public static int GetZ(int idx) { return idx / (Length() * Length()); }  // extracts the z coordinate from an index
    public static Coord2 MakeCoord2(int idx) { return new Coord2(GetX(idx), GetY(idx)); }  // extracts the Coord2 from an index in [0,Length()^2)
    public static Coord2 MakeCoord3(int idx) { return new Coord3(GetX(idx), GetY(idx), GetZ(idx)); }  // extracts the Coord3 from an index in [0,Length()^3)
    public static int CoordToIdx(int x, int y, int z = 0) { return x + Length() * (y + Length() * z); }  // convert a set of coordinates into an index
    public static int CoordToIdx(Coord2 coord) { return CoordToIdx(coord.x, coord.y); }  // convert a set of coordinates into an index
    public static int CoordToIdx(Coord3 coord) { return CoordToIdx(coord.x, coord.y, coord.z); }  // convert a set of coordinates into an index

    public static int ExtractBack(ref List<int> list) {
        int last = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        return last;
    }
}
