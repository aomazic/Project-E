using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class MemoryDatabase : MonoBehaviour
{
    private IDbConnection dbConnection;
    [SerializeField] float updateInterval = 60f;
    [SerializeField] float recencyDecayFactor = 2f;
    [SerializeField] float importanceReinforcementFactor = 0.1f;
    [SerializeField] TimeManager timeManager;

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
                "UPDATE Observations SET RecencyScore = RecencyScore - (@recencyDecayFactor / (ImportanceScore + 1))";
            IDbDataParameter param = dbCommandUpdateRecency.CreateParameter();
            param.ParameterName = "recencyDecayFactor";
            param.Value = recencyDecayFactor;
            dbCommandUpdateRecency.Parameters.Add(param);
            dbCommandUpdateRecency.ExecuteReader();

            IDbCommand dbCommandDeleteOld = dbConnection.CreateCommand();
            dbCommandDeleteOld.CommandText =
                "DELETE FROM Observations WHERE RecencyScore <= 0";
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
        string currentTime = timeManager.GetGameDateTime();
        string regionName = !region ? "Somewhere" : region.name;
        string observationText = objectName + " spotted at " + currentTime + " at location " + regionName;

        dbCommand.CommandText =
            "INSERT INTO Observations (NPCName, Observation, RecencyScore, Created) VALUES (@NPCName, @Observation, @RecencyScore, @Created)";

        IDbDataParameter paramNPCName = dbCommand.CreateParameter();
        paramNPCName.ParameterName = "@NPCName";
        paramNPCName.Value = NPCName;
        dbCommand.Parameters.Add(paramNPCName);

        IDbDataParameter paramObservation = dbCommand.CreateParameter();
        paramObservation.ParameterName = "@Observation";
        paramObservation.Value = observationText;
        dbCommand.Parameters.Add(paramObservation);

        IDbDataParameter paramRecencyScore = dbCommand.CreateParameter();
        paramRecencyScore.ParameterName = "@RecencyScore";
        paramRecencyScore.Value = 10f;
        dbCommand.Parameters.Add(paramRecencyScore);

        IDbDataParameter paramCreated = dbCommand.CreateParameter();
        paramCreated.ParameterName = "@Created";
        paramCreated.Value = currentTime;
        dbCommand.Parameters.Add(paramCreated);

        dbCommand.ExecuteReader();
    }

    public void SaveEatingObservation(String NPCName, String foodName, float nutritionValue)
    {
        IDbCommand dbCommand = dbConnection.CreateCommand();
        string currentTime = timeManager.GetGameDateTime();
        string observationText = "Ate " + foodName + " with nutrition value " + nutritionValue + " at " + currentTime;
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
        string currentTime = timeManager.GetGameDateTime();
        string observationText = $"{observationType} level at {percentage}% at {currentTime}";
        float importanceScore = (float)Math.Round(10f * Math.Abs(percentage / 100f - 1f), 2);
        dbCommand.CommandText =
            $"INSERT INTO Observations (NPCName, Observation, ImportanceScore, Created) VALUES ('{NPCName}', '{observationText}', {importanceScore}, '{currentTime}')";
        dbCommand.ExecuteReader();
    }

    public void genericObsevation(String NPCName, String observationText, float importance)
    {
        IDbCommand dbCommand = dbConnection.CreateCommand();
        string currentTime = timeManager.GetGameDateTime();
        float roundedImportance = (float)Math.Round(importance, 2);
        dbCommand.CommandText =
            $"INSERT INTO Observations (NPCName, Observation, ImportanceScore, Created) VALUES ('{NPCName}', '{observationText}', {roundedImportance}, '{currentTime}')";
        dbCommand.ExecuteReader();
    }
    public List<string> FetchImportantRecords(int recordNum, string npcName)
    {
        List<string> importantRecords = new List<string>();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"SELECT *, (ImportanceScore + RecencyScore) as TotalScore FROM Observations WHERE NPCName = '{npcName}' ORDER BY TotalScore DESC, Created DESC LIMIT {recordNum}";

        IDataReader reader = dbCommand.ExecuteReader();
        while (reader.Read())
        {
            string record = $"NPCName: {reader["NPCName"]}, Observation: {reader["Observation"]}, ImportanceScore: {reader["ImportanceScore"]}, RecencyScore: {reader["RecencyScore"]}, TotalScore: {reader["TotalScore"]}, Created: {reader["Created"]}";
            importantRecords.Add(record);
        }

        return importantRecords;
    }
    
    private void OnDestroy()
    {
        dbConnection.Close();
    }
}
