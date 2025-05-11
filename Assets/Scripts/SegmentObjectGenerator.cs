using System.Collections.Generic;
using UnityEngine;

public class SegmentObjectGenerator : MonoBehaviour
{
    [Header("Lane Settings")]
    [SerializeField] private int laneCount = 3;
    [SerializeField] private float segmentLength = 115f;

    // Fixed lane positions
    private float[] lanePositions = new float[] { -20f, 0f, 20f };

    [Header("Coin Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinsPerLane = 9;
    [SerializeField] private int coinsPerCluster = 3;
    [SerializeField] private float coinSpacing = 2f; // Space between coins in a cluster

    [Header("Obstacle Settings")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private int obstaclesPerType = 2; // Fixed to 2 as requested

    [Header("Spacing Settings")]
    [SerializeField] private float minDistanceBetweenObstacles = 25f;
    [SerializeField] private float minDistanceBetweenClusters = 15f;
    [SerializeField] private float minDistanceBetweenObstacleAndCluster = 10f;
    [SerializeField] private float edgeBuffer = 5f; // Buffer from segment edges

    // Lists to keep track of occupied positions
    private List<OccupiedSpace> occupiedSpaces = new List<OccupiedSpace>();

    private class OccupiedSpace
    {
        public float startZ;
        public float endZ;
        public int lane;

        public OccupiedSpace(float start, float end, int lane)
        {
            this.startZ = start;
            this.endZ = end;
            this.lane = lane;
        }
    }

    private void Start()
    {
        GenerateSegmentObjects();
    }

    public void GenerateSegmentObjects()
    {
        // Clear any existing objects if regenerating
        ClearExistingObjects();

        // Set up containers for organization
        Transform coinsContainer = CreateContainer("Coins");
        Transform obstaclesContainer = CreateContainer("Obstacles");

        // Place obstacles first (they have higher priority)
        PlaceObstacles(obstaclesContainer);

        // Then place coins in the remaining spaces
        PlaceCoinClusters(coinsContainer);
    }

    private Transform CreateContainer(string name)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(transform);
        container.transform.localPosition = Vector3.zero;
        return container.transform;
    }

    private void ClearExistingObjects()
    {
        // Clear existing objects if we're regenerating
        Transform coinsContainer = transform.Find("Coins");
        Transform obstaclesContainer = transform.Find("Obstacles");

        if (coinsContainer != null)
            DestroyImmediate(coinsContainer.gameObject);

        if (obstaclesContainer != null)
            DestroyImmediate(obstaclesContainer.gameObject);

        // Clear the occupied spaces list
        occupiedSpaces.Clear();
    }

    private void PlaceObstacles(Transform container)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("No obstacle prefabs assigned!");
            return;
        }

        // For each type of obstacle
        for (int obstacleType = 0; obstacleType < obstaclePrefabs.Length; obstacleType++)
        {
            // Place exactly 2 of each type as requested
            for (int i = 0; i < obstaclesPerType; i++)
            {
                // Try to find a valid position
                bool positionFound = false;
                int maxAttempts = 50;
                int attempts = 0;

                while (!positionFound && attempts < maxAttempts)
                {
                    attempts++;

                    // Choose a random lane (0, 1, or 2)
                    int lane = Random.Range(0, laneCount);

                    // Get the exact X position for this lane from our array
                    float laneX = lanePositions[lane] +3f;

                    // Random Z position within segment length
                    float z = Random.Range(edgeBuffer, segmentLength - edgeBuffer);

                    // Determine obstacle size (approximated)
                    float obstacleSize = 5f; // Default size approximation

                    // Check if position is valid
                    if (IsPositionValid(z - obstacleSize / 2, z + obstacleSize / 2, lane, minDistanceBetweenObstacles))
                    {
                        // Position is valid, place obstacle
                        GameObject obstacle = Instantiate(obstaclePrefabs[obstacleType], container);
                        obstacle.transform.localPosition = new Vector3(laneX, 0, z);

                        // Mark this space as occupied
                        occupiedSpaces.Add(new OccupiedSpace(z - obstacleSize / 2, z + obstacleSize / 2, lane));

                        positionFound = true;
                    }
                }

                if (attempts >= maxAttempts)
                {
                    Debug.LogWarning("Could not find valid position for obstacle after " + maxAttempts + " attempts");
                }
            }
        }
    }

    private void PlaceCoinClusters(Transform container)
    {
        // Calculate clusters per lane
        int clustersPerLane = coinsPerLane / coinsPerCluster;
        if (coinsPerLane % coinsPerCluster != 0)
        {
            clustersPerLane++;
            Debug.LogWarning("Coins per lane is not divisible by coins per cluster. Rounding up clusters.");
        }

        // For each lane
        for (int lane = 0; lane < laneCount; lane++)
        {
            // Get the exact X position for this lane
            float laneX = lanePositions[lane] +3f;

            // Place clusters in this lane
            for (int cluster = 0; cluster < clustersPerLane; cluster++)
            {
                // Try to find valid position for cluster
                bool positionFound = false;
                int maxAttempts = 50;
                int attempts = 0;

                while (!positionFound && attempts < maxAttempts)
                {
                    attempts++;

                    // Calculate total length of cluster
                    float clusterLength = (coinsPerCluster - 1) * coinSpacing;

                    // Find random start Z position for cluster
                    float startZ = Random.Range(edgeBuffer, segmentLength - clusterLength - edgeBuffer);
                    float endZ = startZ + clusterLength;

                    // Check if position is valid for entire cluster
                    if (IsPositionValid(startZ, endZ, lane, minDistanceBetweenClusters))
                    {
                        // Position is valid, place coin cluster
                        GameObject clusterObj = new GameObject("Cluster_" + lane + "_" + cluster);
                        clusterObj.transform.SetParent(container);
                        clusterObj.transform.localPosition = Vector3.zero;

                        // Place individual coins in cluster
                        for (int coin = 0; coin < coinsPerCluster; coin++)
                        {
                            float coinZ = startZ + coin * coinSpacing;
                            GameObject coinObj = Instantiate(coinPrefab, clusterObj.transform);
                            coinObj.transform.localPosition = new Vector3(laneX, 1f, coinZ); // Slight y-offset for visibility
                        }

                        // Mark this space as occupied
                        occupiedSpaces.Add(new OccupiedSpace(startZ, endZ, lane));

                        positionFound = true;
                    }
                }

                if (attempts >= maxAttempts)
                {
                    Debug.LogWarning("Could not find valid position for coin cluster after " + maxAttempts + " attempts");
                }
            }
        }
    }

    private bool IsPositionValid(float startZ, float endZ, int lane, float minDistance)
    {
        // Check against all occupied spaces
        foreach (OccupiedSpace space in occupiedSpaces)
        {
            // If in the same lane, check for z-axis collision with proper spacing
            if (space.lane == lane)
            {
                // Check if new object overlaps with occupied space + minimum distance buffer
                bool overlapsInZ = !(endZ + minDistance < space.startZ || startZ - minDistance > space.endZ);

                if (overlapsInZ)
                    return false;
            }
            else
            {
                // If different lanes, we still need to check for cross-lane obstacles
                // with a reduced distance requirement (can be closer in different lanes)
                float crossLaneMinDistance = minDistanceBetweenObstacleAndCluster;

                // Check if new object overlaps with occupied space + cross-lane minimum distance
                bool overlapsInZ = !(endZ + crossLaneMinDistance < space.startZ || startZ - crossLaneMinDistance > space.endZ);

                if (overlapsInZ)
                    return false;
            }
        }

        // Position is valid if we reach here
        return true;
    }

    // Public method to manually regenerate from the editor
    public void RegenerateFromEditor()
    {
        GenerateSegmentObjects();
    }

    // Optional: Draw gizmos to visualize the lanes in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        // Draw lane marks using the exact lane positions
        for (int lane = 0; lane < laneCount; lane++)
        {
            float laneX = lanePositions[lane] + 3f;

            // Draw lane center line
            Gizmos.DrawLine(
                new Vector3(laneX, 0.1f, 0),
                new Vector3(laneX, 0.1f, segmentLength)
            );
        }

        // Draw segment bounds
        Gizmos.color = Color.green;

        // Calculate the width based on the lane positions
        float minX = lanePositions[0] - 10f;
        float maxX = lanePositions[laneCount - 1] + 10f;
        float width = maxX - minX;
        float centerX = (minX + maxX) / 2f;

        Gizmos.DrawWireCube(
            new Vector3(centerX, 0, segmentLength / 2),
            new Vector3(width, 1, segmentLength)
        );
    }
}