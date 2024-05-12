using System;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class MemoryDatabase : MonoBehaviour
{
    private IDbConnection dbConnection;

    private void Awake()
    {
        dbConnection = CreateAndOpenDatabase();
    }

    private IDbConnection CreateAndOpenDatabase()
    {
        var dbUri = "URI=file:" + Application.persistentDataPath + "/Memory.db";
        dbConnection = new SqliteConnection(dbUri);
        dbConnection.Open();

        IDbCommand dbCommandCreateTable = dbConnection.CreateCommand();
        dbCommandCreateTable.CommandText =
            "CREATE TABLE IF NOT EXISTS Observations (ObservationID INTEGER PRIMARY KEY AUTOINCREMENT, NPCName TEXT, Observation TEXT, RecencyScore INTEGER, ImportanceScore INTEGER, Created TEXT)";
        dbCommandCreateTable.ExecuteReader();

        return dbConnection;
    }

    public void SaveVisionObservation(String NPCName, String objectName)
    {
        IDbCommand dbCommand = dbConnection.CreateCommand();
        string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        dbCommand.CommandText =
            "INSERT INTO Observations (NPCName, Observation, RecencyScore, Created) VALUES ('" + NPCName + "', '" + objectName + "', " + 10 + ", '" + currentTime + "')";
        dbCommand.ExecuteReader();
    }

    private void OnDestroy()
    {
        dbConnection.Close();
    }
}
