using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace MSCI_332_Heuristic
{
    class Vertex
    {
        public int flight_no;
        public int value;
        public bool visited = false;
        public bool checkpoint = false;
    }
    class MetaHeuristic
    {
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
                List<Vertex> connections_j = new List<Vertex>();
                Dictionary<int, List<Vertex>> connections_i = new Dictionary<int, List<Vertex>>();
                while (row != null)
                {
                    connections_j = new List<Vertex>();
                    cells = row.Split(' ');
                    for (int j = 0; j < cells.Length - 1; ++j)
                    {
                        if (i == 0 || j == 0)
                        {
                            connections_j.Add(new Vertex { flight_no = j, value = int.Parse(cells[j]) });
                        }
                        else
                        {
                            if (j == 174)
                            {
                                connections_j.Add(new Vertex { flight_no = j, value = int.Parse(cells[j]) * (arrivals[i]) });
                            }
                            else
                            {
                                connections_j.Add(new Vertex { flight_no = j, value = int.Parse(cells[j]) * (departures[j] - arrivals[i]) });
                            }
                        }
                    }
                    connections_j = connections_j.OrderBy(v => v.value).ThenBy(f => f.flight_no).ToList();
                    connections_i.Add(i, connections_j);
                    ++i;
                    row = matrix.ReadLine();
                }

                List<int> flights = new List<int>();
                foreach (KeyValuePair<int, List<Vertex>> flight in connections_i)
                {
                    if (flight.Key != 0 && flight.Key != 174)
                    {
                        flights.Add(flight.Key);
                    }
                }

                StreamWriter sw = new StreamWriter("results.txt");
                int Vertexs_count = 0;
                int iteration = 0;

                List<int> priority_list = new List<int>();
                while (iteration < 1001)
                {
                    ++iteration;
                    Vertexs_count = 0;
                    sw.WriteLine("Iteration " + iteration);
                    sw.Write("Priority List: ");
                    foreach (int item in priority_list)
                    {
                        sw.Write(item + ", ");
                    }
                    sw.WriteLine();
                    List<int> visited_Vertexs = new List<int>();
                    for (int k = 0; k < connections_i[0].Count; ++k)
                    {
                        if (connections_i[0][k].visited == false && connections_i[0][k].value > 0)
                        {
                            connections_i[0][k].visited = true;
                            Stack<int> path = null;
                            List<int> tmp_priority_list = new List<int>(priority_list);
                            for (int q = 0; q < tmp_priority_list.Count; ++q)
                            {
                                if (visited_Vertexs.Contains(tmp_priority_list[q]))
                                {
                                    tmp_priority_list.RemoveAt(q);
                                    --q;
                                }
                            }
                            for (int p = 0; p <= priority_list.Count(); ++p)
                            {
                                if (p == priority_list.Count())
                                {
                                    path = NextVertexSearch(connections_i, connections_i[0][k].flight_no, new List<int>());
                                }
                                else
                                {
                                    path = NextVertexSearch(connections_i, connections_i[0][k].flight_no, tmp_priority_list);
                                    if (path == null && tmp_priority_list.Count > 0)
                                    {
                                        tmp_priority_list.RemoveAt(0);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            
                            List<int> visited_columns = new List<int>();
                            if (path != null)
                            {
                                while (path.Count > 0)
                                {
                                    int Vertex = path.Pop();
                                    ++Vertexs_count;
                                    sw.Write(Vertex + "->");
                                    visited_Vertexs.Add(Vertex);
                                    visited_columns.Add(Vertex);
                                }
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
                    sw.WriteLine("Count:" + Vertexs_count);
                    List<int> missing_flights = flights.Except(visited_Vertexs).ToList();
                    sw.Write("Missing flight: ");
                    foreach(int item in missing_flights)
                    {
                        sw.Write(item+", ");
                    }
                    sw.WriteLine();
                    if (missing_flights.Count > 0)
                    {
                        for (int l = 0; l < connections_i.Count; ++l)
                        {
                            for (int m = 0; m < connections_i[l].Count; ++m)
                            {
                                connections_i[l][m].visited = false;
                            }
                        }
                        foreach (int item in missing_flights)
                        {
                            if (!priority_list.Contains(item))
                            {
                                priority_list.Add(item);
                            }
                        }
                        priority_list.Sort();
                    }
                    else
                    {
                        break;
                    }
                }
                sw.Close();
                Console.WriteLine("DONE");
            }

            public static List<Vertex> Shuffle(List<Vertex> list)
            {
                var rnd = new Random();
                int n = list.Count;
                while (n > 1)
                {
                    n--;
                    int k = rnd.Next(n + 1);
                    Vertex value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
                return list;
            }

            public static Stack<int> NextVertexSearch(Dictionary<int, List<Vertex>> matrix, int flight, List<int> priority_list)
            {
                if (priority_list.Count > 0 && flight > priority_list[0] && matrix[flight].First(f => f.flight_no == priority_list[0]).visited == false)
                {
                    return null;
                }
                if (priority_list.Count > 0 && matrix[flight].First(f => f.flight_no == priority_list[0]).value > 0 && matrix[flight].First(f => f.flight_no == priority_list[0]).visited == false)
                {
                    matrix[flight].First(f => f.flight_no == priority_list[0]).visited = true;
                    int checkpoint;
                    checkpoint = priority_list[0];
                    priority_list.RemoveAt(0);
                    Stack<int> path;
                    while (true)
                    {
                        path = NextVertexSearch(matrix, checkpoint, priority_list);
                        if (path == null || path.Count <= 1)
                        {
                            if (priority_list.Count == 0)
                            {
                                break;
                            }
                            priority_list.RemoveAt(0);
                            continue;
                        }
                        else
                        {
                            path.Push(flight);
                            return path;
                        }
                    }
                    return NextVertexSearch(matrix, checkpoint, priority_list);

                }
                matrix[flight] = matrix[flight].OrderBy(v => v.value).ThenBy(f => f.flight_no).ToList();
                for (int j = 0; j < matrix[flight].Count; ++j)
                {
                    if (flight == 2)
                    {
                        //Check
                    }
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
                            Stack<int> path = NextVertexSearch(matrix, matrix[flight][j].flight_no, priority_list);
                            if (path == null)
                            {
                                matrix[flight][j].visited = false;
                                continue;
                            }
                            else if (priority_list.Count > 0)
                            {
                                if (!path.Contains(priority_list[0]))
                                {
                                    path = null;
                                }
                            }
                            else
                            {
                                path.Push(flight);
                                return path;
                            }
                        }
                    }
                }
                return null;
            }
        }
    }
}
