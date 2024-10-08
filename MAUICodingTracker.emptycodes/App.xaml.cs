﻿using Microsoft.Data.Sqlite;
using Dapper;
using System.Diagnostics;

namespace CodingTracker;
public partial class App : Application
{
    public static string? ConnectionString { get; private set; }
    public static string? DateFormat { get; private set; }
    public static string? DatabasePath { get; private set; } 
    public App()
    {
        InitializeComponent();
        CheckAndRequestPermission();
        CreateDatabase();
        MainPage = new AppShell();
    }

    async Task CheckAndRequestPermission()
    {
        var writeStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        if (writeStatus != PermissionStatus.Granted)
        {
            writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
        }

        var readStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (readStatus != PermissionStatus.Granted)
        {
            readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
        }

        if (writeStatus != PermissionStatus.Granted || readStatus != PermissionStatus.Granted)
        {
            Debug.WriteLine("Storage permissions denied.");
            throw new UnauthorizedAccessException("Storage permissions are required to save goals.");
        }
        else
        {
            Debug.WriteLine("Storage read and write permissions granted.");
        }
    }

    private void CreateDatabase()
    {
        DateFormat = "yyyy-MM-dd HH:mm";
        DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "coding_tracker.db");
        ConnectionString = $"Data Source={DatabasePath}";

        if (File.Exists(DatabasePath))
        {
            Debug.WriteLine($"Database file {DatabasePath} already exists.");
        }
        else
        {
            using var conn = new SqliteConnection(ConnectionString);

            var createTableQuery = @"
                CREATE TABLE IF NOT EXISTS CodingSessions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StartTime TEXT NOT NULL,
                    EndTime TEXT NOT NULL,
                    Duration TEXT NOT NULL
                );";

            var createGoalsTableQuery = @"
            CREATE TABLE IF NOT EXISTS Goals (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                GoalHours INTEGER NOT NULL,
                GoalDeadline TEXT NOT NULL,
                CurrentHours REAL NOT NULL,
                DailyTarget REAL NOT NULL,
                GoalStatus TEXT NOT NULL
            );";
            try
            {
                conn.Open();
                conn.Execute(createTableQuery);
                conn.Execute(createGoalsTableQuery);
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DatabasePath);
                Debug.WriteLine($"Database file {DatabasePath} successfully created at {dbPath}. The database is ready to use.");
            }
            catch (SqliteException e)
            {
                Debug.WriteLine($"Error occurred while trying to create the database Table\n - Details: {e.Message}");
            }
        }
    }
}