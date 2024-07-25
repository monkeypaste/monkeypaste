namespace AutoComplete {
    public class BKTree {
        // maximum number of words in dict[]
        static int MAX_WORD_COUNT = 100;

        // defines the tolerance value
        const int MATCH_TOLERANCE = 2;

        // defines maximum length of a word
        public static int MAX_WORD_LEN = 10;


        // stores the root Node
        public static Node RootNode { get; set; }

        // stores every Node of the tree
        static Node[] Tree { get; set; } = new Node[MAX_WORD_COUNT];

        // index for the current Node of the tree
        public static int ptr;

        public static void SetLimits(int maxWords, int maxWordLength) {
            MAX_WORD_COUNT = maxWords;
            MAX_WORD_LEN = maxWordLength;
            Tree = new Node[MAX_WORD_COUNT];
        }
        static int Min(int a, int b, int c) {
            return Math.Min(a, Math.Min(b, c));
        }

        // Edit Distance
        // Dynamic-Approach O(m*n)
        public static int EditDistance(string a, string b) {
            if (a == null || b == null) {
                return -1; // or handle the case where either string is null
            }

            int m = a.Length;
            int n = b.Length;
            int[,] dp = new int[m + 1, n + 1];

            // filling base cases
            for (int i = 0; i <= m; i++) {
                dp[i, 0] = i;
            }
            for (int j = 0; j <= n; j++) {
                dp[0, j] = j;
            }
            // populating matrix using dp-approach
            for (int i = 1; i <= m; i++) {
                for (int j = 1; j <= n; j++) {
                    if (a[i - 1] != b[j - 1]) {
                        int del = 1 + dp[i - 1, j];
                        int ins = 1 + dp[i, j - 1];
                        int rep = 1 + dp[i - 1, j - 1];
                        dp[i, j] = Math.Min(del, Math.Min(ins, rep));
                    } else {
                        dp[i, j] = dp[i - 1, j - 1];
                    }
                        
                }
            }
            return dp[m, n];
        }

        // adds curr Node to the tree
        public static void Add(Node root, Node curr) {
            if (root == null) {
                RootNode = curr;
                return;
            }

            if (string.IsNullOrEmpty(root.Word)) {
                // if it is the first Node then make it the root Node
                root.Word = curr.Word;
                return;
            }

            // get its editDistance from the Root Node
            int dist = EditDistance(curr.Word, root.Word);

            if (root.Next == null) {
                root.Next = new int[2 * MAX_WORD_LEN];
                for (int i = 0; i < 2 * MAX_WORD_LEN; i++)
                    root.Next[i] = 0;
            }

            if (Tree[root.Next[dist]] == null) {
                Tree[root.Next[dist]] = new Node();
            }

            if (string.IsNullOrEmpty(Tree[root.Next[dist]].Word)) {
                /* if no Node exists at this dist from root
                * make it a child of root Node*/

                // incrementing the pointer for curr Node
                ptr++;

                // adding curr Node to the tree
                Tree[ptr] = curr;

                // curr as a child of root Node
                root.Next[dist] = ptr;
            } else {
                // recursively find the parent for curr Node
                Add(Tree[root.Next[dist]], curr);
            }
        }

        public static List<(string,int)> GetSimilarWords(Node root, string s) {
            var ret = new List<(string,int)>();
            if (root == null || root.Word == null || root.Word == "") {
                return ret;
            }
                

            // calculating edit distance of s from root
            int dist = EditDistance(root.Word, s);

            // if dist is less than the tolerance value
            // add it to similar words
            if (dist <= MATCH_TOLERANCE) {
                ret.Add((root.Word, dist));
            }

            // iterate over the string having tolerance
            // in range (dist-TOL , dist+TOL)
            int start = dist - MATCH_TOLERANCE;
            if (start < 0) {
                start = 1;
            }
                

            while (start <= dist + MATCH_TOLERANCE) {
                var tmp = GetSimilarWords(Tree[root.Next[start]], s);
                ret.AddRange(tmp);
                start++;
            }
            return ret;
        }

    }
}




