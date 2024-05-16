using System;
using System.Collections;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class MemoryDatabase : MonoBehaviour
{
    private IDbConnection dbConnection;
    public float updateInterval = 60f;
    public float recencyDecayFactor = 0.1f;
    public float importanceReinforcementFactor = 0.1f;
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
            "CREATE TABLE IF NOT EXISTS Observations (ObservationID INTEGER PRIMARY KEY AUTOINCREMENT, NPCName TEXT, Observation TEXT, RecencyScore DECIMAL(10,2), ImportanceScore DECIMAL(10,2), Created TEXT)";
        dbCommandCreateTable.ExecuteReader();

        return dbConnection;
    }
    public void SaveVisionObservation(String NPCName, String objectName, Region region)
    {
        IDbCommand dbCommand = dbConnection.CreateCommand();
        string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string regionName = !region ? "World" : region.name;
        string observationText = objectName + " spotted at " + Time.time + " at location " + regionName;
        dbCommand.CommandText =
            "INSERT INTO Observations (NPCName, Observation, RecencyScore, Created) VALUES ('" + NPCName + "', '" + observationText + "', " + 10f + ", '" + currentTime + "')";
        dbCommand.ExecuteReader();
    }

    public void SaveEatingObservation(String NPCName, String foodName, float nutritionValue)
    {
        IDbCommand dbCommand = dbConnection.CreateCommand();
        string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string observationText = "Ate " + foodName + " with nutrition value " + nutritionValue + " at " + Time.time;
        dbCommand.CommandText =
            "INSERT INTO Observations (NPCName, Observation, RecencyScore, Created) VALUES ('" + NPCName + "', '" + observationText + "', " + 10f + ", '" + currentTime + "')";
        dbCommand.ExecuteReader();
    }

    public void SaveBasicNeedsObservation(String NPCName, String observationType, float percentage)
    {
        IDbCommand dbCommandUpdateImportance = dbConnection.CreateCommand();
        dbCommandUpdateImportance.CommandText =
            $"UPDATE Observations SET ImportanceScore = ImportanceScore - (ImportanceScore * {importanceReinforcementFactor}) WHERE NPCName = '{NPCName}' AND Observation LIKE '{observationType} level at%'";
        dbCommandUpdateImportance.ExecuteReader();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string observationText = $"{observationType} level at {percentage}% at {Time.time}";
        float importanceScore = (float)Math.Round(10f * Math.Abs(percentage / 100f - 1f), 2);
        dbCommand.CommandText =
            $"INSERT INTO Observations (NPCName, Observation, ImportanceScore, Created) VALUES ('{NPCName}', '{observationText}', {importanceScore}, '{currentTime}')";
        dbCommand.ExecuteReader();
    }

    public void genericObsevation(String NPCName, String observationText, float importance)
    {
        IDbCommand dbCommand = dbConnection.CreateCommand();
        string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        float roundedImportance = (float)Math.Round(importance, 2);
        dbCommand.CommandText =
            $"INSERT INTO Observations (NPCName, Observation, ImportanceScore, Created) VALUES ('{NPCName}', '{observationText}', {roundedImportance}, '{currentTime}')";
        dbCommand.ExecuteReader();
    }
    
    private void OnDestroy()
    {
        dbConnection.Close();
    }
}
