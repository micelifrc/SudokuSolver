using System.Collections;
using System.Collections.Generic;

public class Solver {
    public static int InvalidInt = UTL.InvalidInt;
    public static int RootLength, Length, Area, Volume, NumNeighbours;
    private Node[] _nodes;  // there are Volume Nodes, each of which represent a possible value for a tile
    private Cluster[] _clusters;  // there are 4*Area clusters, to represent the constraints. Each cluster will have to have exactly 1 node satisfying is_true
    private List<int> _nodes_to_propagate;  // the nodes that need to be propagated
    private List<int> _guess_list;  // all the currently active guesses. If _guess_list[t] = n, it means that the (t+1)^th guess was that node _nodes[n] was true
    private int _num_fixed_nodes_propagated;  // the number of nodes for which is_fixed is true that have already been propagated

    // initialize the measure values
    public void setMeasures(int root_length_) {
        RootLength = root_length_;
        Length = RootLength * RootLength;
        Area = Length * Length;
        Volume = Area * Length;
        NumNeighbours = Length * 4 - RootLength * 2 - 2;
    }

    // initialize _nodes and _clusters
    public void initializeGraph() {
        _nodes = new Node[Volume];
        _clusters = new Cluster[Area * 4];
        for (int node_idx = 0; node_idx < Volume; ++node_idx) {
            _nodes[node_idx] = new Node(UTL.getx(node_idx), UTL.gety(node_idx), UTL.getz(node_idx), GameManager.Instance.GetTileButtons()[node_idx % Area].GetComponent<TileButton>());
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
            setNodeBool(tile_idx + Area * input_values[tile_idx], true);
        }
    }

    // The main routine
    public bool Solve() {
        if (_nodes_to_propagate.Count == 0) return false;  // TODO delete this line after implementing the guess function. It only serves to prevent Unity to crash if there are no input numbers
        _guess_list = new List<int>();
        _num_fixed_nodes_propagated = 0;
        while (_num_fixed_nodes_propagated < Volume) {
            if (_nodes_to_propagate.Count > 0) {
                if(!propagate()) {
                    if (_guess_list.Count > 0) removeLastGuess();
                    else return false;  // we have a conflict without guesses, which means that the input has no solution
                }
            } guess();
        }
        return true;
    }

    // propagate the last node in _nodes_to_propagate. This means applying all the constraints in its clusters. Returns false if we get a conflict from this propagation
    private bool propagate() {
        int node_idx_to_propagate = UTL.extractBack(ref _nodes_to_propagate);
        bool is_legal_propagation = propagate_node(ref _nodes[node_idx_to_propagate]);
        ++_num_fixed_nodes_propagated;
        return is_legal_propagation;
    }

    // propagate a node that is set as true. Return false if this leads to conflicts
    private bool propagate_node(ref Node node_to_propagate) {
        if (node_to_propagate.is_true) {
            foreach (int neighbour_idx in node_to_propagate.neighbour_ids) {
                if (!give_value_to_node(neighbour_idx, false)) return false;
            }
        }
        foreach (int cluster_idx in node_to_propagate.cluster_ids) {
            if (!give_node_value_to_cluster(ref _clusters[cluster_idx], node_to_propagate.is_true)) return false;
        }
        return true;
    }

    // set a value to a node, and set it to the propagation list. Returns false if this leads to a conflict
    private bool give_value_to_node(int node_idx, bool value) {
        if (_nodes[node_idx].is_fixed) return _nodes[node_idx].is_true == value;
        _nodes[node_idx].setBool(value, NumGuesses());
        _nodes_to_propagate.Add(node_idx);
        return true;
    }

    // set a value to a node for a cluster. Returns false if there is a conflict. If the cluster forces a node to get a true value, this method will set such a value
    private bool give_node_value_to_cluster(ref Cluster cluster, bool node_is_true) {
        if (!cluster.SetValuetoNode(node_is_true)) return false;
        if (cluster.has_true_point || cluster.num_open > 1) return true;
        foreach (int node_candidate in cluster.nodes_ids) {
            if (!_nodes[node_candidate].is_fixed)
            {
                give_value_to_node(node_candidate, true);
                return true;
            }
            else if (_nodes[node_candidate].is_true) return true;  // this can happen if the true node is waiting to be propagated
        }
        return false;  // this should never happen
    }

    // removes the last guess, since it proved to be wrong
    private void removeLastGuess() {
        // TODO implement the removeLastGuess function
    }

    // guess a value for a tile which has no value still
    private void guess() {
        // TODO impement the guess function
    }

    // the currently open number of guesses
    private int NumGuesses() { return _guess_list.Count; }

    // the node with index node_idx is set to the required boolean value, and inserted in the _nodes_to_propagate list
    private void setNodeBool(int node_idx, bool value_to_give, int current_time = 0) {
        _nodes[node_idx].setBool(value_to_give, current_time);
        _nodes_to_propagate.Add(node_idx);
    }

    public class Node {
        public bool is_fixed, is_true;  // is_fixed tell whether I have decided if the node is true or not. If it is true, is_true is also set true. If !is_fixed, then is_true has no meaning
        public int time;  // the time at which the Node became fixed. InvalidInt if it is not fixed
        public UTL.Coord3 coord;  // the coordinate of the node
        public int[] neighbour_ids, cluster_ids;  // the indices of the neighbour nodes, and of the clusters this node belongs to
        private TileButton _tile_button;  // the button that need to be modified

        public Node(int x, int y, int z, TileButton tile_button_) {
            _tile_button = tile_button_;
            is_fixed = false;
            is_true = false;
            time = InvalidInt;
            coord = new UTL.Coord3(x, y, z);
            neighbour_ids = new int[NumNeighbours];
            cluster_ids = new int[4];
            int idx = 0;
            for (int xi = 0; xi < Length; ++xi) {
                if (xi == x) continue;
                neighbour_ids[idx] = UTL.coordToIdx(xi, y, z);
                ++idx;
            }
            for (int yi = 0; yi < Length; ++yi) {
                if (yi == y) continue;
                neighbour_ids[idx] = UTL.coordToIdx(x, yi, z);
                ++idx;
            }
            for (int zi = 0; zi < Length; ++zi) {
                if (zi == z) continue;
                neighbour_ids[idx] = UTL.coordToIdx(x, y, zi);
                ++idx;
            }
            for (int yi = (y / RootLength) * RootLength; yi < (y / RootLength + 1) * RootLength; ++yi) {
                if (yi == y) continue;
                for (int xi = (x / RootLength) * RootLength; xi < (x / RootLength + 1) * RootLength; ++xi) {
                    if (xi == x) continue;
                    neighbour_ids[idx] = UTL.coordToIdx(xi, yi, z);
                    ++idx;
                }
            }
            cluster_ids[0] = UTL.coordToIdx(y, z);
            cluster_ids[1] = UTL.coordToIdx(x, z) + Area;
            cluster_ids[2] = UTL.coordToIdx(x, y) + Area * 2;
            cluster_ids[3] = UTL.coordToIdx((x / RootLength) + RootLength * (y / RootLength), z) + Area * 3;
        }

        public void setBool(bool value, int required_time) {
            change_button_value(value);
            is_fixed = true;
            is_true = value;
            time = required_time;
        }

        private void change_button_value(bool value) {
            if (value) _tile_button.changeButtonText(coord.z + 1);
            else if (is_true) _tile_button.clearText();
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
                nodes_ids[x] = UTL.coordToIdx(x, y, z);
            }
        }
        public void InitializeYCluster(int x, int z) {
            for (int y = 0; y < Length; ++y) {
                nodes_ids[y] = UTL.coordToIdx(x, y, z);
            }
        }
        public void InitializeZCluster(int x, int y) {
            for (int z = 0; z < Length; ++z) {
                nodes_ids[z] = UTL.coordToIdx(x, y, z);
            }
        }
        public void InitializeSquareCluster(int x, int y, int z) {
            for (int s = 0; s < Length; ++s) {
                nodes_ids[s] = UTL.coordToIdx(x * RootLength + s % RootLength, y * RootLength + s / RootLength, z);
            }
        }
        // a Node has just been propagated with the required value for is_true
        public bool SetValuetoNode(bool node_is_true) {
            --num_open;
            if(node_is_true) {
                if (has_true_point) return false;
                has_true_point = true;
                return true;
            }
            return num_open > 0 || has_true_point;
        }
    }
}
