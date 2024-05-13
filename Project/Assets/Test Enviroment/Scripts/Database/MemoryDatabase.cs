using System;
using System.Collections;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class MemoryDatabase : MonoBehaviour
{
    private IDbConnection dbConnection;
    public float updateInterval = 60f;
    public float recencyDecayFactor = 0.1f; // Factor by which to decrease the recency score
    public float importanceReinforcementFactor = 0.1f; // Factor by which to increase the importance score
    public float recencyThreshold = 0f;
    public float importanceThreshold = 10f;

    private void Awake()
    {
        dbConnection = CreateAndOpenDatabase();
        StartCoroutine(UpdateDatabase());
    }

    private IEnumerator UpdateDatabase()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);

            IDbCommand dbCommandUpdateRecency = dbConnection.CreateCommand();
            dbCommandUpdateRecency.CommandText =
                "UPDATE Observations SET RecencyScore = RecencyScore - (RecencyScore * " + recencyDecayFactor + ")";
            dbCommandUpdateRecency.ExecuteReader();

            IDbCommand dbCommandDeleteOld = dbConnection.CreateCommand();
            dbCommandDeleteOld.CommandText =
                "DELETE FROM Observations WHERE RecencyScore <= " + recencyThreshold + " AND ImportanceScore < " + importanceThreshold;
            dbCommandDeleteOld.ExecuteReader();
        }
    }

    private IDbConnection CreateAndOpenDatabase()
    {
        var dbUri = "URI=file:" + Application.persistentDataPath + "/Memory.db";
        dbConnection = new SqliteConnection(dbUri);
        dbConnection.Open();

        IDbCommand dbCommandCreateTable = dbConnection.CreateCommand();
        dbCommandCreateTable.CommandText =
            "CREATE TABLE IF NOT EXISTS Observations (ObservationID INTEGER PRIMARY KEY AUTOINCREMENT, NPCName TEXT, Observation TEXT, RecencyScore REAL, ImportanceScore REAL, Created TEXT)";
        dbCommandCreateTable.ExecuteReader();

        return dbConnection;
    }

    public void SaveVisionObservation(String NPCName, String objectName, Region region)
    {
        IDbCommand dbCommand = dbConnection.CreateCommand();
        string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string regionName = region == null ? "World" : region.name;
        string observationText = objectName + " spotted at " + Time.time + " at location " + regionName;
        dbCommand.CommandText =
            "INSERT INTO Observations (NPCName, Observation, RecencyScore, Created) VALUES ('" + NPCName + "', '" + observationText + "', " + 10f + ", '" + currentTime + "')";
        dbCommand.ExecuteReader();
    }

    private void OnDestroy()
    {
        dbConnection.Close();
    }
}
