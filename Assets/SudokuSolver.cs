using UnityEngine;
using UnityEngine.UI;
using System.Collections;
// using System.Collections.Generic;

public class Tile {
    private const int InvaliIdx = -1;
    bool fixed_from_input;
    public int idx;
    public int[] neighbour_ids;
    public int value;
    public int value_time;
    public int num_open;
    public int[] closed_time;

    public Tile(int RootSize = 3, int idx_ = 0, int value_ = InvaliIdx) {
            int Size = RootSize * RootSize;
            fixed_from_input = value_ != InvaliIdx;
            idx = idx_;
            value = value_;
            value_time = fixed_from_input ? 0 : InvaliIdx;
            num_open = fixed_from_input ? 1 : Size;
            closed_time = new int[Size];
            for (int idx = 0; idx < Size; ++idx) {
                closed_time[idx] = value_time;
            }

            int thisX = idx % Size, thisY = idx / Size;
            neighbour_ids = new int[3 * Size - 2 * RootSize - 1];
            int neighbour_idx = 0;
            for (int same_row = Size * thisY; same_row < Size * (thisY + 1); ++same_row) {
                if (same_row == idx) continue;
                neighbour_ids[neighbour_idx] = same_row;
                ++neighbour_idx;
            }
            for (int same_col = thisX; same_col < Size * Size; same_col += Size) {
                if (same_col == idx) continue;
                neighbour_ids[neighbour_idx] = same_col;
                ++neighbour_idx;
            }
            int this_square_x = thisX - thisX % RootSize, this_square_y = thisY - thisY % RootSize;
            for (int square_idx = 0; square_idx != Size; ++square_idx) {
                int square_x = square_idx % RootSize + this_square_x, square_y = square_idx / RootSize + this_square_y;
                if (square_x == thisX || square_y == thisY) continue;
                neighbour_ids[neighbour_idx] = square_y * Size + square_x;
                ++neighbour_idx;
            }
            if (neighbour_idx != neighbour_ids.Length) {
                Debug.Log("Wrong number of neighbours!");
                return;  // TODO should probably throw instead
            }
        }
}

public class SudokuSolver {
    public const int _RootSize = 3;
    private const int _Size = _RootSize * _RootSize;  // the size of the grid. 9 by default
    private const int _SizeSquared = _Size * _Size;  // the number of tiles in the grid. 81 by default
    private const int _NumNeighbours = 3 * _Size - 2 * _RootSize - 1;  // the number of neighbours of each tile. 20 by default.
    public Tile[] tiles;
    public SudokuSolver(int[] initialValues) {
            if (initialValues.Length != _SizeSquared) {
                Debug.Log("Wrong number of inputs in SudokuSolver constructor");
                return;  // TODO should probably throw instead
            }
            tiles = new Tile[_SizeSquared];
            for (int idx = 0; idx < _SizeSquared; ++idx) {
                tiles[idx] = new Tile(_RootSize, idx, initialValues[idx]);
            }
        }
    
    public bool Solve() {
        // TODO! need implementation
        return true;
    }
}
