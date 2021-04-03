using System.Collections;
using System.Collections.Generic;

public class Solver {
    public static int InvalidInt = UTL.InvalidInt;
    public static int RootLength, Length, Area, Volume, NumNeighbours;
    private Node[] _nodes;  // there are Volume Nodes, each of which represent a possible value for a tile
    private Cluster[] _clusters;  // there are 4*Area clusters, to represent the constraints. Each cluster will have to have exactly 1 node satisfying is_true
    private List<int> _nodes_to_propagate;  // the nodes that need to be propagated
    private List<int> _guess_list;  // all the currently active guesses. If _guess_list[t] = n, it means that the (t+1)^th Guess was that node _nodes[n] was true
    private int _num_fixed_nodes_propagated;  // the number of nodes for which is_fixed is true that have already been propagated

    // initialize the measure values
    public void SetMeasures(int root_length_) {
        RootLength = root_length_;
        Length = RootLength * RootLength;
        Area = Length * Length;
        Volume = Area * Length;
        NumNeighbours = Length * 4 - RootLength * 2 - 2;
    }

    // initialize _nodes and _clusters
    public void InitializeGraph() {
        _nodes = new Node[Volume];
        _clusters = new Cluster[Area * 4];
        for (int node_idx = 0; node_idx < Volume; ++node_idx) {
            _nodes[node_idx] = new Node(UTL.GetX(node_idx), UTL.GetY(node_idx), UTL.GetZ(node_idx), GameManager.Instance.GetTileButtons()[node_idx % Area].GetComponent<TileButton>());
        }
        for (int cl_idx = 0; cl_idx < Area * 4; ++cl_idx) {
            _clusters[cl_idx] = new Cluster();
        }
        for (int cl_idx = 0; cl_idx < Area; ++cl_idx) {
            _clusters[cl_idx].InitializeXCluster(cl_idx % Length, cl_idx / Length);
            _clusters[cl_idx + Area].InitializeYCluster(cl_idx % Length, cl_idx / Length);
            _clusters[cl_idx + Area * 2].InitializeZCluster(cl_idx % Length, cl_idx / Length);
            _clusters[cl_idx + Area * 3].InitializeSquareCluster(cl_idx % RootLength, (cl_idx % Length) / RootLength, cl_idx / Length);
        }
    }

    // read the input from the user
    public void ReadInputValues(int[] input_values) {
        _nodes_to_propagate = new List<int>();
        for (int tile_idx = 0; tile_idx < Area; ++tile_idx) {
            if (input_values[tile_idx] == InvalidInt) continue;
            SetNodeBool(tile_idx + Area * input_values[tile_idx], true);
        }
    }

    // The main routine
    public bool Solve() {
        _guess_list = new List<int>();
        _num_fixed_nodes_propagated = 0;
        while (_num_fixed_nodes_propagated < Volume) {
            if (_nodes_to_propagate.Count > 0) {
                if(!Propagate()) {
                    if (_guess_list.Count > 0) RemoveLastGuess();
                    else return false;  // we have a conflict without guesses, which means that the input has no solution
                }
            } else Guess();
        }
        return true;
    }

    // Propagate the last node in _nodes_to_propagate. This means applying all the constraints in its clusters. Returns false if we get a conflict from this propagation
    private bool Propagate() {
        int node_idx_to_propagate = UTL.ExtractBack(ref _nodes_to_propagate);
        if (_nodes[node_idx_to_propagate].is_propagated) return true;  // the node has already been propagated. Shouldn't happen
        bool legal_propagation = true;
        if (_nodes[node_idx_to_propagate].is_true) {
            foreach (int neighbour_idx in _nodes[node_idx_to_propagate].neighbour_ids) {
                if (!GiveValueToNode(neighbour_idx, false)) legal_propagation = false;
            }
        }
        foreach (int cluster_idx in _nodes[node_idx_to_propagate].cluster_ids) {
            if (!GiveNodeValueToCluster(ref _clusters[cluster_idx], _nodes[node_idx_to_propagate].is_true)) legal_propagation = false;
        }
        _nodes[node_idx_to_propagate].is_propagated = true;
        ++_num_fixed_nodes_propagated;
        return legal_propagation;
    }

    // Guess a value for a tile which has no value still
    private void Guess() {
        for (int node_idx = _guess_list.Count > 0 ? _guess_list[_guess_list.Count - 1] + 1 : 0; node_idx < Volume; ++node_idx) {
            if (_nodes[node_idx].is_fixed) continue;
            _guess_list.Add(node_idx);
            GiveValueToNode(node_idx, true);
            _nodes_to_propagate = new List<int>{ node_idx };
            return;
        }
    }

    // removes the last Guess, since it proved to be wrong
    private void RemoveLastGuess() {
        if (_guess_list.Count == 0) return;  // this should never happen
        for (int node_idx = 0; node_idx < Volume; ++node_idx) {
            if (_nodes[node_idx].time == NumGuesses()) FreeNode(node_idx); ;
        }
        int last_guessed_node = UTL.ExtractBack(ref _guess_list);
        GiveValueToNode(last_guessed_node, false);
        _nodes_to_propagate = new List<int> { last_guessed_node };
    }

    // set a value to a node, and set it to the propagation list. Returns false if this leads to a conflict
    private bool GiveValueToNode(int node_idx, bool value) {
        if (_nodes[node_idx].is_fixed) return _nodes[node_idx].is_true == value;
        _nodes[node_idx].SetBool(value, NumGuesses());
        _nodes_to_propagate.Add(node_idx);
        return true;
    }

    // remove the set value given to a node
    private void FreeNode(int node_idx) {
        if (_nodes[node_idx].is_propagated) {
            --_num_fixed_nodes_propagated;
            foreach (int cluster_idx in _nodes[node_idx].cluster_ids) {
                _clusters[cluster_idx].FreeNode(_nodes[node_idx].is_true);
            }
        }
        _nodes[node_idx].FreeNode();
    }

    // set a value to a node for a cluster. Returns false if there is a conflict. If the cluster forces a node to get a true value, this method will set such a value
    private bool GiveNodeValueToCluster(ref Cluster cluster, bool node_is_true) {
        if (!cluster.SetValueToNode(node_is_true)) return false;
        if (cluster.has_true_point || cluster.num_open > 1) return true;
        foreach (int node_candidate in cluster.nodes_ids) {
            if (!_nodes[node_candidate].is_fixed) {
                GiveValueToNode(node_candidate, true);
                return true;
            }
            else if (_nodes[node_candidate].is_true) return true;  // this can happen if the true node is waiting to be propagated
        }
        return false;  // this should never happen
    }

    // the currently open number of guesses
    private int NumGuesses() { return _guess_list.Count; }

    // the node with index node_idx is set to the required boolean value, and inserted in the _nodes_to_propagate list
    private void SetNodeBool(int node_idx, bool value_to_give, int current_time = 0) {
        _nodes[node_idx].SetBool(value_to_give, current_time);
        _nodes_to_propagate.Add(node_idx);
    }

    public class Node {
        public bool is_fixed, is_true, is_propagated;  // is_fixed tell whether I have decided if the node is true or not. If it is true, is_true is also set true. is_propagated tells whether the node has been propagated yet
        public int time;  // the time at which the Node became fixed. InvalidInt if it is not fixed
        public UTL.Coord3 coord;  // the coordinate of the node
        public int[] neighbour_ids, cluster_ids;  // the indices of the neighbour nodes, and of the clusters this node belongs to
        private TileButton _tile_button;  // the button that need to be modified

        public Node(int x, int y, int z, TileButton tile_button_) {
            _tile_button = tile_button_;
            is_fixed = false;
            is_true = false;
            is_propagated = false;
            time = InvalidInt;
            coord = new UTL.Coord3(x, y, z);
            neighbour_ids = new int[NumNeighbours];
            cluster_ids = new int[4];
            int idx = 0;
            for (int xi = 0; xi < Length; ++xi) {
                if (xi == x) continue;
                neighbour_ids[idx] = UTL.CoordToIdx(xi, y, z);
                ++idx;
            }
            for (int yi = 0; yi < Length; ++yi) {
                if (yi == y) continue;
                neighbour_ids[idx] = UTL.CoordToIdx(x, yi, z);
                ++idx;
            }
            for (int zi = 0; zi < Length; ++zi) {
                if (zi == z) continue;
                neighbour_ids[idx] = UTL.CoordToIdx(x, y, zi);
                ++idx;
            }
            for (int yi = (y / RootLength) * RootLength; yi < (y / RootLength + 1) * RootLength; ++yi) {
                if (yi == y) continue;
                for (int xi = (x / RootLength) * RootLength; xi < (x / RootLength + 1) * RootLength; ++xi) {
                    if (xi == x) continue;
                    neighbour_ids[idx] = UTL.CoordToIdx(xi, yi, z);
                    ++idx;
                }
            }
            cluster_ids[0] = UTL.CoordToIdx(y, z);
            cluster_ids[1] = UTL.CoordToIdx(x, z) + Area;
            cluster_ids[2] = UTL.CoordToIdx(x, y) + Area * 2;
            cluster_ids[3] = UTL.CoordToIdx((x / RootLength) + RootLength * (y / RootLength), z) + Area * 3;
        }

        public void FreeNode() {
            ChangeButtonValue(false);
            is_fixed = false;
            is_true = false;
            is_propagated = false;
            time = InvalidInt;
        }

        public void SetBool(bool value, int required_time) {
            ChangeButtonValue(value);
            is_fixed = true;
            is_true = value;
            time = required_time;
        }

        private void ChangeButtonValue(bool value) {
            if (value) _tile_button.ChangeButtonText(coord.z + 1);
            else if (is_true) _tile_button.ClearText();
        }
    }

    public class Cluster {
        public int[] nodes_ids;  // the indices of the nodes in the cluster
        public int num_open;  // the number of nodes in the cluster that are not fixed yet
        public bool has_true_point;  // true if there is a node in the cluster for which is_true
        public Cluster() {
            nodes_ids = new int[Length];
            num_open = Length;
            has_true_point = false;
        }
        public void InitializeXCluster(int y, int z) {
            for (int x = 0; x < Length; ++x) {
                nodes_ids[x] = UTL.CoordToIdx(x, y, z);
            }
        }
        public void InitializeYCluster(int x, int z) {
            for (int y = 0; y < Length; ++y) {
                nodes_ids[y] = UTL.CoordToIdx(x, y, z);
            }
        }
        public void InitializeZCluster(int x, int y) {
            for (int z = 0; z < Length; ++z) {
                nodes_ids[z] = UTL.CoordToIdx(x, y, z);
            }
        }
        public void InitializeSquareCluster(int x, int y, int z) {
            for (int s = 0; s < Length; ++s) {
                nodes_ids[s] = UTL.CoordToIdx(x * RootLength + s % RootLength, y * RootLength + s / RootLength, z);
            }
        }
        // a Node has just been propagated with the required value for is_true
        public bool SetValueToNode(bool node_is_true) {
            --num_open;
            if(node_is_true) {
                if (has_true_point) return false;
                has_true_point = true;
                return true;
            }
            return num_open > 0 || has_true_point;
        }
        // one of the nodes of the cluster, with required value for is_true is free
        public void FreeNode(bool is_true) {
            ++num_open;
            if (is_true) has_true_point = false;
        }
    }
}
