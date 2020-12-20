using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SudokuSolver {
    public const int InvalidIdx = -1;
    public const int RootLength = GameManager.RootLength;
    public const int Length = RootLength * RootLength;
    public const int Area = Length * Length;
    public const int Volume = Area * Length;

    private Node[] nodes;
    private Cluster[] clusters;
    private List<int> nodes_to_propagate;
    private List<int> guess_list;
    private int num_written;
    private readonly bool is_initialized;

    public SudokuSolver(int[] initial_values, Button[] buttons) {
        is_initialized = false;
        if (initial_values.Length != Area) {
            Debug.Log("Wrong number of inputs in SudokuSolver constructor");
            return;  // should probably throw instead
        }
        nodes = new Node[Volume];
        for (int idx = 0; idx < Volume; idx++) {
            nodes[idx] = new Node(idx, buttons[idx % Area]);
        }
        clusters = new Cluster[Area * 4];
        for (int cluster_idx = 0; cluster_idx < Area * 4; cluster_idx++) {
            clusters[cluster_idx] = new Cluster(cluster_idx);
        }
        nodes_to_propagate = new List<int>();
        nodes_to_propagate.Capacity = Volume;  // reserve some space for nodes_to_propagate
        guess_list = new List<int>();
        num_written = 0;
        for (int idx = 0; idx < Volume; idx++) {
            if (initial_values[idx % Area] != idx / Area) continue;  // if the Node was not written from input
            write(idx);
            nodes_to_propagate.Add(idx);
        }
        is_initialized = true;
    }

    // Solve the recorded Sudoku Puzzle. Returns false if the puzzle has no solution.
    public bool Solve() {
        if (!is_initialized) return false;
        while (!hasFinished()) {
            if (nodes_to_propagate.Count > 0) {
                if (!propagateFromNodeList()) {
                    if (numGuesses() == 0) return false;  // the puzzle is impossible!
                    updateLastGuess();
                }
            } else guess();
        }
        return true;
    }

    // all the effects of the insertion of nodes[node_idx_to_propagate] on the clusters and neighbours happen here
    public bool propagateFromNodeList() {
        if (nodes_to_propagate.Count == 0) return true;  // nothing left to propagate
        int node_idx_to_propagate = nodes_to_propagate[nodes_to_propagate.Count - 1];
        nodes_to_propagate.RemoveAt(nodes_to_propagate.Count - 1);
        if (nodes[node_idx_to_propagate].is_written) {
            foreach (int cluster_idx in nodes[node_idx_to_propagate].cluster_ids) {
                if (!clusters[cluster_idx].take()) return false;
            }
            foreach (int neighbour_idx in nodes[node_idx_to_propagate].neighbour_ids) {
                if (nodes[neighbour_idx].isOpen()) {
                    nodes[neighbour_idx].close(numGuesses());
                    nodes_to_propagate.Add(neighbour_idx);
                } else if (nodes[neighbour_idx].is_written) return false;  // this is a conflict
            }
        } else {
            foreach (int cluster_idx in nodes[node_idx_to_propagate].cluster_ids) {
                if (clusters[cluster_idx].closeIdx()) {  // if there is only one possible value for the cluster
                    foreach (int neighbour_idx in clusters[cluster_idx].node_ids) {
                        if (nodes[neighbour_idx].isOpen()) {
                            write(neighbour_idx);
                            nodes_to_propagate.Add(neighbour_idx);
                            break;
                        }
                    }
                }
            }
        }
        return true;
    }

    public void updateLastGuess() {
        for (int node_idx = 0; node_idx < Volume; node_idx++) {
            if (nodes[node_idx].closed_time < numGuesses()) continue;
            bool open_written_term = nodes[node_idx].reopen();
            if (open_written_term) num_written--;
            foreach (int cluster_idx in nodes[node_idx].cluster_ids) {
                clusters[cluster_idx].reopen(open_written_term);
            }
        }
        int idx_to_close = guess_list[guess_list.Count - 1];
        guess_list.RemoveAt(guess_list.Count - 1);
        nodes[idx_to_close].close(numGuesses());
        nodes_to_propagate = new List<int>(){idx_to_close};
    }

    public void guess() {
        int new_guess = guess_list.Count > 0 ? guess_list[guess_list.Count - 1] + 1 : 0;
        for (; new_guess < Volume; new_guess++) {
            if (nodes[new_guess].isOpen()) break;
        }
        if (new_guess == Volume) {
            Debug.Log("guess has reached the index " + Volume);
            return;  // this should never happen
        }
        guess_list.Add(new_guess);
        write(new_guess);
    }

    private bool write(int node_idx) {
        if (!nodes[node_idx].write(numGuesses())) return false;
        num_written++;
        return true;
    }
    private bool hasFinished() { return num_written == Area; }
    private int numGuesses() { return guess_list.Count; }
}


// a Node denotes a tile (both X and Y coordinates) and a color C
public class Node {
    private const int InvalidIdx = SudokuSolver.InvalidIdx;
    private const int RootLength = SudokuSolver.RootLength;
    private const int Length = SudokuSolver.Length;
    private const int Area = SudokuSolver.Area;
    private const int Volume = SudokuSolver.Volume;
    const int NumNeighbours = 4 * Length - 2 * RootLength - 2;

    private readonly int idx;  // the index of the Node
    public readonly int[] neighbour_ids;
    public readonly int[] cluster_ids;
    public bool is_written;  // true only if we wrote C()+1 in the tile getCoord()
    public int closed_time;  // the time when C()+1 was either written or forbidden in getCoord()
    private Button button;

    public Node(int idx_, Button button_) {
        if (idx_ < 0 && idx_ >= Volume) {
            Debug.Log("Invalid idx " + idx_ + " for the Node");
        }
        idx = idx_;
        neighbour_ids = new int[NumNeighbours];
        int neighbour_idx = 0;
        for (int x = 0; x < Length; x++) {
            if (x == X()) continue;
            neighbour_ids[neighbour_idx] = getIdx(x, Y(), C());
            neighbour_idx++;
        }
        for (int y = 0; y < Length; y++) {
            if (y == Y()) continue;
            neighbour_ids[neighbour_idx] = getIdx(X(), y, C());
            neighbour_idx++;
        }
        for (int x = RootLength * (X() / RootLength); x < RootLength * (X() / RootLength + 1); x++) {
            if (x == X()) continue;
            for (int y = RootLength * (Y() / RootLength); y < RootLength * (Y() / RootLength + 1); y++) {
                if (y == Y()) continue;
                neighbour_ids[neighbour_idx] = getIdx(x, y, C());
                neighbour_idx++;
            }
        }
        for (int c = 0; c < Length; c++) {
            if (c == C()) continue;
            neighbour_ids[neighbour_idx] = getIdx(X(), Y(), c);
            neighbour_idx++;
        }
        if (neighbour_idx != NumNeighbours) Debug.Log("Wrong number of neighbours");  // this should never happen
        cluster_ids = new int[]{ Length*Y() + C(), Area + Length*X() + C(), 2*Area + Length*(RootLength*(Y()/RootLength)+(X()/RootLength)) + C(), 3*Area + Length*Y() + X() };
        is_written = false;
        closed_time = InvalidIdx;
        button = button_;
    }

    public static int getIdx(int x, int y, int col) { return Area * col + Length * y + x; }
    public int X() { return idx % Length; }
    public int Y() { return (idx / Length) % Length; }
    public int C() { return idx / Area; }
    public string printCoord() { return "[" + (X()+1).ToString() + "," + (Y()+1).ToString() + "]"; }
    public string printCoordVal() { return printCoord()+(C()+1).ToString(); }
    public bool isOpen() { return closed_time == InvalidIdx; }
    public bool write(int time) {
        if (!isOpen()) return is_written;
        closed_time = time;
        is_written = true;
        button.GetComponent<TileButton>().changeText(C() + 1);
        return true;
    }
    public bool cancel() {
        if (is_written) button.GetComponent<TileButton>().clearText();
        is_written = false;
        if (closed_time == InvalidIdx) return false;
        closed_time = InvalidIdx;
        return true;
    }
    // close the node at the required time, if possible. Else, return false
    public bool close(int time) {
        if (is_written) return false;
        if (closed_time == InvalidIdx || closed_time > time) closed_time = time;
        return true;
    }
    // returns true if is_written was true
    public bool reopen() {
        bool used_to_be_written = is_written;
        is_written = false;
        closed_time = InvalidIdx;
        return used_to_be_written;
    }
}


// a complete graph of Length vertices
public class Cluster {
    public const int InvalidIdx = SudokuSolver.InvalidIdx;
    public const int RootLength = SudokuSolver.RootLength;
    public const int Length = SudokuSolver.Length;
    public const int Area = SudokuSolver.Area;
    public const int Volume = SudokuSolver.Volume;

    // X direction (row), Y direction (column), S type (square), C type (color, i.e. each tile has exactly 1 color)
    public enum ClusterType { X = 0, Y = 1, S = 2, C = 3 }

    public readonly int idx;
    public readonly int idx_x, idx_y, idx_c;
    public readonly int[] node_ids;  // the node indices this cluster refers to
    public int num_open;  // the number of available nodes for this value
    public bool is_taken;  // true if we have took the value already

    public Cluster(int idx_) {
        if (idx_ < 0 || idx >= Area * 4) {  // should never happen
            Debug.Log("Invalid index = " + idx_ + " in Cluster constructor");
            return;
        }
        idx = idx_;
        node_ids = new int[Length];
        num_open = Length;
        is_taken = false;
        int subtype_idx = idx % Area;
        switch (getClusterType()) {
            case ClusterType.X:
                idx_y = subtype_idx / Length;
                idx_c = subtype_idx % Length;
                for (idx_x = 0; idx_x < Length; idx_x++) {
                    node_ids[idx_x] = Node.getIdx(idx_x, idx_y, idx_c);
                }
                idx_x = InvalidIdx;
                break;
            case ClusterType.Y:
                idx_x = subtype_idx / Length;
                idx_c = subtype_idx % Length;
                for (idx_y = 0; idx_y < Length; idx_y++) {
                    node_ids[idx_y] = Node.getIdx(idx_x, idx_y, idx_c);
                }
                idx_y = InvalidIdx;
                break;
            case ClusterType.S:
                idx_x = ((subtype_idx / Length) % RootLength) * RootLength;
                idx_y = ((subtype_idx / Length) / RootLength) * RootLength;
                idx_c = subtype_idx % Length;
                for (int node_idx = 0; node_idx < Length; node_idx++) {
                    node_ids[node_idx] = Node.getIdx(idx_x + node_idx % RootLength, idx_y + node_idx / RootLength, idx_c);
                }
                break;
            case ClusterType.C:
                idx_y = subtype_idx / Length;
                idx_x = subtype_idx % Length;
                for (idx_c = 0; idx_c < Length; idx_c++) {
                    node_ids[idx_c] = Node.getIdx(idx_x, idx_y, idx_c);
                }
                idx_c = InvalidIdx;
                break;
            default:
                Debug.Log("Invalid index = " + idx_ + " in Cluster constructor");  // this will never happen
                break;
        }
    }

    public bool closeIdx() {
        num_open--;
        return num_open == 1 && !is_taken;
    }

    public bool take() {
        if (num_open == 0 || is_taken) return false;
        is_taken = true;
        num_open--;
        return true;
    }

    public void reopen(bool reopening_written_term) {
        if (reopening_written_term) is_taken = false;
        num_open++;
    }

    public int getClusterTypeAsInt() { return idx / Area; }
    public ClusterType getClusterType() { return (ClusterType)(getClusterTypeAsInt()); }
}