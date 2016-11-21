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

            Dictionary<int, Stack<int>> complete_paths = new Dictionary<int, Stack<int>>();
            for (int k = 0; k < connections_i[0].Count; ++k)
            {
                if (connections_i[0][k].visited == false && connections_i[0][k].value > 0)
                {
                    connections_i[0][k].visited = true;
                    complete_paths.Add(k, NextNodeSearch(connections_i, k));
                }
            }
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
                        return NextNodeSearch(matrix, j);
                    }
                }
            }
            return new Stack<int>();
        }
    }
}
