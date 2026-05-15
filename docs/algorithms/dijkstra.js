/**
 * Dijkstra's Algorithm implementation for AlgoSphere
 * Finds the shortest path from start node to all other nodes.
 */
function dijkstra(graph, start) {
    const distances = {};
    const visited = new Set();
    const nodes = Object.keys(graph);

    for (let node of nodes) {
        distances[node] = Infinity;
    }
    distances[start] = 0;

    while (visited.size < nodes.length) {
        let currentNode = null;
        let minDistance = Infinity;

        for (let node of nodes) {
            if (!visited.has(node) && distances[node] < minDistance) {
                currentNode = node;
                minDistance = distances[node];
            }
        }

        if (currentNode === null) break;

        visited.add(currentNode);

        for (let neighbor in graph[currentNode]) {
            let distance = graph[currentNode][neighbor];
            let newDistance = distances[currentNode] + distance;

            if (newDistance < distances[neighbor]) {
                distances[neighbor] = newDistance;
            }
        }
    }

    return distances;
}

// Example Usage:
const graph = {
    'A': { 'B': 4, 'C': 2 },
    'B': { 'A': 4, 'C': 5, 'D': 10 },
    'C': { 'A': 2, 'B': 5, 'D': 3 },
    'D': { 'B': 10, 'C': 3 }
};
console.log(dijkstra(graph, 'A'));
