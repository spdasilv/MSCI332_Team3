using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace MSCI_332_Heuristic
{
    class Node
    {
        public int value;
        public bool visited = false;
    }
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader arr_read = new StreamReader("flight_arrival_time.txt");
            Dictionary<int, int> arrivals = new Dictionary<int, int>();
            string arr = arr_read.ReadLine().Trim();
            int count = 0;
            while (arr != null)
            {
                arrivals.Add(count, int.Parse(arr));
                arr = arr_read.ReadLine();
                ++count;
            }
            arr_read.Close();

            StreamReader dep_read = new StreamReader("flight_departure_time.txt");
            Dictionary<int, int> departures = new Dictionary<int, int>();
            string dep = dep_read.ReadLine().Trim();
            count = 0;
            while (dep != null)
            {
                departures.Add(count, int.Parse(dep));
                dep = dep_read.ReadLine();
                ++count;
            }
            dep_read.Close();

            StreamReader matrix = new StreamReader("adjecency_matrix.txt");
            string[] cells = null;
            string row = matrix.ReadLine();
            int i = 0;
            Dictionary<int, Node> connections_j = new Dictionary<int, Node>();
            Dictionary<int, Dictionary<int, Node>> connections_i = new Dictionary<int, Dictionary<int, Node>>();
            while (row != null)
            {
                connections_j = new Dictionary<int, Node>();
                cells = row.Split(' ');
                for (int j = 0; j < 175; ++j)
                {
                    if (i == 0 || j == 0)
                    {
                        connections_j.Add(j, new Node { value = int.Parse(cells[j]) });
                    }
                    else
                    {
                        if (j == 174)
                        {
                            connections_j.Add(j, new Node { value = int.Parse(cells[j]) * (arrivals[i]) });
                        }
                        else
                        {
                            connections_j.Add(j, new Node { value = int.Parse(cells[j]) * (departures[j] - arrivals[i]) });
                        }
                    }
                }
                connections_i.Add(i, connections_j);
                ++i;
                row = matrix.ReadLine();
            }

            StreamWriter sw = new StreamWriter("results.txt");
            int nodes_count = 0;
            Dictionary<int, Stack<int>> complete_paths = new Dictionary<int, Stack<int>>();
            for (int k = 0; k < connections_i[0].Count; ++k)
            {
                if (connections_i[0][k].visited == false && connections_i[0][k].value > 0)
                {
                    connections_i[0][k].visited = true;
                    Stack<int> path = NextNodeSearch(connections_i, k);
                    complete_paths.Add(k, path);
                    Stack<int> tmp_path = complete_paths[k];
                    List<int> visited_columns = new List<int>();
                    while (tmp_path.Count > 0)
                    {
                        int node = tmp_path.Pop();
                        ++nodes_count;
                        Console.Write(node + " ");
                        sw.Write(node + " ");
                        visited_columns.Add(node);
                    }
                    Console.WriteLine();
                    sw.WriteLine();
                    for (int l = 0; l < connections_i.Count; ++l)
                    {
                        for (int m = 1; m < connections_i[l].Count; ++m)
                        {
                            if (visited_columns.Contains(m))
                            {
                                connections_i[l][m].visited = true;
                            }
                        }
                    }
                }
            }
            Console.WriteLine(nodes_count);
            sw.WriteLine("Count:" + nodes_count);
            sw.Close();
            Console.WriteLine("DONE");
        }

        public static Stack<int> NextNodeSearch(Dictionary<int, Dictionary<int, Node>> matrix, int flight)
        {
            for (int j = flight; j < matrix[flight].Count; ++j)
            {
                if (matrix[flight][j].visited == false && matrix[flight][j].value > 0)
                {
                    matrix[flight][j].visited = true;
                    if (j == matrix[flight].Count - 1)
                    {
                        // Sink found
                        Stack<int> path = new Stack<int>();
                        path.Push(flight);
                        return path;
                    }
                    else
                    {
                        Stack<int> path = NextNodeSearch(matrix, j);
                        path.Push(flight);
                        return path;
                    }
                }
            }
            return new Stack<int>();
        }
    }
}
