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
        public int flight_no;
        public int value;
        public bool visited = false;
        public int priority = 0;
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
            List<Node> connections_j = new List<Node>();
            Dictionary<int, List<Node>> connections_i = new Dictionary<int, List<Node>>();
            while (row != null)
            {
                connections_j = new List<Node>();
                cells = row.Split(' ');
                for (int j = 0; j < cells.Length - 1; ++j)
                {
                    if (i == 0 || j == 0)
                    {
                        connections_j.Add(new Node { flight_no = j, value = int.Parse(cells[j]) });
                    }
                    else
                    {
                        if (j == 174)
                        {
                            connections_j.Add(new Node { flight_no = j, value = int.Parse(cells[j]) * (arrivals[i]) });
                        }
                        else
                        {
                            connections_j.Add(new Node { flight_no = j, value = int.Parse(cells[j]) * (departures[j] - arrivals[i]) });
                        }
                    }
                }
                connections_j = connections_j.OrderByDescending(p => p.priority).ThenBy(v => v.value).ThenBy(f => f.flight_no).ToList();
                connections_i.Add(i, connections_j);
                ++i;
                row = matrix.ReadLine();
            }

            List<int> flights = new List<int>();
            foreach(KeyValuePair<int, List<Node>> flight in connections_i)
            {
                if (flight.Key != 0 && flight.Key != 174)
                {
                    flights.Add(flight.Key);
                }
            }

            StreamWriter sw = new StreamWriter("results.txt");
            int nodes_count = 0;
            int iteration = 0;
            List<int> priority_list = new List<int>();
            while (iteration < 1001)
            {
                ++iteration;
                nodes_count = 0;
                sw.WriteLine("Iteration " + iteration);
                List<int> visited_nodes = new List<int>();
                for (int k = 0; k < connections_i[0].Count; ++k)
                {
                    if (connections_i[0][k].visited == false && connections_i[0][k].value > 0)
                    {
                        connections_i[0][k].visited = true;
                        Stack<int> path = NextNodeSearch(connections_i, connections_i[0][k].flight_no);
                        List<int> visited_columns = new List<int>();
                        while (path.Count > 0)
                        {
                            int node = path.Pop();
                            ++nodes_count;
                            sw.Write(node + "->");
                            visited_nodes.Add(node);
                            visited_columns.Add(node);
                        }
                        sw.WriteLine();
                        for (int l = 0; l < connections_i.Count; ++l)
                        {
                            for (int m = 1; m < connections_i[l].Count; ++m)
                            {
                                if (visited_columns.Contains(connections_i[l][m].flight_no))
                                {
                                    connections_i[l][m].visited = true;
                                }
                            }
                        }
                    }
                }
                List<int> missing_flights = flights.Except(visited_nodes).ToList();
                if (missing_flights.Count > 0)
                {
                    for (int l = 0; l < connections_i.Count; ++l)
                    {
                        for (int m = 0; m < connections_i[l].Count; ++m)
                        {
                            connections_i[l][m].visited = false;
                        }
                    }
                    for (int l = 0; l < missing_flights.Count; ++l)
                    {
                        if (!priority_list.Contains(missing_flights[l]))
                        {
                            priority_list.Add(missing_flights[l]);
                        }
                        else
                        {
                            int index_up = priority_list.IndexOf(missing_flights[l]);
                            int index_down = priority_list.IndexOf(missing_flights[l]) - 1;
                            int tmp = priority_list[index_down];
                            priority_list[index_down] = priority_list[index_up];
                            priority_list[index_up] = tmp;
                        }
                    }
                    connections_i = ChangePriority(connections_i, priority_list);
                    connections_i[0] = Shuffle(connections_i[0]);
                }
                else
                {
                    break;
                }
            }
            sw.WriteLine("Count:" + nodes_count);
            sw.Close();
            Console.WriteLine("DONE");
        }

        public static Dictionary<int, List<Node>> ChangePriority(Dictionary<int, List<Node>> matrix, List<int> missing_flights)
        {
            Dictionary<int, List<Node>> tmp_matrix = matrix;
            for (int i = 0; i < tmp_matrix.Count; ++i)
            {
                foreach(Node flight in tmp_matrix[i])
                {
                    if (missing_flights.Contains(flight.flight_no))
                    {
                        flight.priority = missing_flights.Count - missing_flights.IndexOf(flight.flight_no);
                    }
                }
            }
            return tmp_matrix;
        }

        public static List<Node> Shuffle(List<Node> list)
        {
            var rnd = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                Node value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        public static Stack<int> NextNodeSearch(Dictionary<int, List<Node>> matrix, int flight)
        {
            matrix[flight] = matrix[flight].OrderByDescending(p => p.priority).ThenBy(v => v.value).ThenBy(f => f.flight_no).ToList();
            for (int j = 0; j < matrix[flight].Count; ++j)
            {
                if (matrix[flight][j].visited == false && matrix[flight][j].value > 0)
                {
                    matrix[flight][j].visited = true;
                    if (matrix[flight][j].flight_no == matrix[flight].Count - 1)
                    {
                        // Sink found
                        Stack<int> path = new Stack<int>();
                        path.Push(flight);
                        return path;
                    }
                    else
                    {
                        Stack<int> path = NextNodeSearch(matrix, matrix[flight][j].flight_no);
                        path.Push(flight);
                        return path;
                    }
                }
            }
            return new Stack<int>();
        }
    }
}
