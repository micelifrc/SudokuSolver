using System.Collections;
using UnityEngine;

// this class contains some utility classes and functions
public class UTL {
    public static int InvalidInt = -1;  // will be used to denote an invalid integer, or invalid index (for example, and integer that has not been initialized yet)

    // a class containing some of the basic math functions
    public class Math {
        static public int Min(int n1, int n2) { return n1 < n2 ? n1 : n2; }
        static public int Max(int n1, int n2) { return n1 < n2 ? n2 : n1; }
    };

    public static int Length() { return GameManager.Instance.GridLength(); }  // keep track of the size of the Sudoku-Puzzle

    // A 2-dim coordinate
    public class Coord2 {
        public int x, y;
        public Coord2(int x_ = 0, int y_ = 0) {
            x = x_;
            y = y_;
        }
    };

    // A 3-dim coordinate
    public class Coord3 : Coord2 {
        public int z;
        public Coord3(int x_ = 0, int y_ = 0, int z_ = 0) {
            x = x_;
            y = y_;
            z = z_;
        }
    };

    public static int getx(int idx) { return idx % Length(); }  // extracts the x coordinate from an index
    public static int gety(int idx) { return (idx / Length()) % Length(); }  // extracts the y coordinate from an index
    public static int getz(int idx) { return idx / (Length() * Length()); }  // extracts the z coordinate from an index
    public static Coord2 MakeCoord2(int idx) { return new Coord2(getx(idx), gety(idx)); }  // extracts the Coord2 from an index in [0,Length()^2)
    public static Coord2 MakeCoord3(int idx) { return new Coord3(getx(idx), gety(idx), getz(idx)); }  // extracts the Coord3 from an index in [0,Length()^3)
};
