using System.Collections;
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
    public static bool is_coord_in_grid(int x) { return x >= 0 && x < Length(); }  // check whether x is the in grid-range

    // A 2-dim coordinate
    public class Coord2 {
        public int x, y;
        public Coord2(int x_ = 0, int y_ = 0) {
            x = x_;
            y = y_;
        }
        public virtual bool is_in_grid() {
            return is_coord_in_grid(x) && is_coord_in_grid(y);
        }
        public static Coord2 operator+(Coord2 lhs, Coord2 rhs) => new Coord2(lhs.x + rhs.x, lhs.y + rhs.y);
        public virtual int getIdx() { return coordToIdx(x, y); }
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
        public override bool is_in_grid() {
            return is_coord_in_grid(x) && is_coord_in_grid(y) && is_coord_in_grid(z);
        }
        public static Coord3 operator +(Coord3 lhs, Coord3 rhs) => new Coord3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        public override int getIdx() { return coordToIdx(x, y, z); }
        public bool sameSquareCoord(Coord3 other) { return getSquareCoord() == other.getSquareCoord() && z == other.z; }
    }

    public static int getx(int idx) { return idx % Length(); }  // extracts the x coordinate from an index
    public static int gety(int idx) { return (idx / Length()) % Length(); }  // extracts the y coordinate from an index
    public static int getz(int idx) { return idx / (Length() * Length()); }  // extracts the z coordinate from an index
    public static Coord2 MakeCoord2(int idx) { return new Coord2(getx(idx), gety(idx)); }  // extracts the Coord2 from an index in [0,Length()^2)
    public static Coord2 MakeCoord3(int idx) { return new Coord3(getx(idx), gety(idx), getz(idx)); }  // extracts the Coord3 from an index in [0,Length()^3)
    public static int coordToIdx(int x, int y, int z = 0) { return x + Length() * (y + Length() * z); }  // convert a set of coordinates into an index
    public static int coordToIdx(Coord2 coord) { return coordToIdx(coord.x, coord.y); }  // convert a set of coordinates into an index
    public static int coordToIdx(Coord3 coord) { return coordToIdx(coord.x, coord.y, coord.z); }  // convert a set of coordinates into an index
}
