from munkres import Munkres, print_matrix


arrivals = open('flight_arrival_time.txt', 'r')
flight_arrivals = {}
j = 0
for line in arrivals:
    arr = line.split()
    flight_arrivals[j] = int(float(arr[0]))
    j += 1
arrivals.close()
j = 0
departures = open('flight_departure_time.txt', 'r')
flight_departures = {}
i = 0
for line in departures:
    dept = line.split()
    flight_departures[i] = int(float(dept[0]))
    i += 1
departures.close()
i = 0

read_matrix = open('connections.txt', 'r')
connection_graph = []
base = []
for line in read_matrix:
    line = line.replace("\n", "")
    connections = list(map(int, line.split(',')))
    vertices = []
    if i == 0:
            base = connections
    for j in range(0, len(connections)):
        if float(int(connections[j])) == 0 or i == 0:
            vertices.append(999999)
        else:
            vertices.append(abs(flight_departures[j] - flight_arrivals[i]))
    connection_graph.append(vertices)
    i += 1

m = Munkres()
paths = []
indexes = m.compute(connection_graph)
print_matrix(connection_graph, msg='Lowest cost through this matrix:')
total = 0
for row, column in indexes:
    value = connection_graph[row][column]
    total += value
    print('(%d, %d) -> %d' % (row, column, value))
    new_path = True
    for path in paths:
        if row in path:
            path.append(column)
            new_path = False
            continue
    if new_path:
        path = [row, column]
        paths.append(path)

connected_flights = 0
for path in paths:
    if path[-2] != 174:
        path[-1] = 174
    else:
        del path[-1]
    if path[0] != 0:
        print(path)
        connected_flights += len(path) - 1

print(connected_flights)
print('total cost: %d' % total)

minimun_len = 3
for route, path in enumerate(paths):
        n = 0
        for index, item in enumerate(path):
            if item == 174:
                break
            if n >= minimun_len and base[item] == 0 and base[path[index + 1]] == 1:
                sub_path = path[n+1:]
                if len(sub_path) >= minimun_len:
                    copy_path = path[:n+1]
                    copy_path.append(174)
                    paths[route] = copy_path
                    paths.append(sub_path)
                break
            else:
                n += 1


for path in paths:
    source = 0
    path.insert(0, 0)
    print(path)
print('Done')
