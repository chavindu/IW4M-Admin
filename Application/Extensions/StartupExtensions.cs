﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using SharedLibraryCore;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database;
using SharedLibraryCore.Database.MigrationContext;

namespace IW4MAdmin.Application.Extensions
{
    public static class StartupExtensions
    {
        private static ILogger _defaultLogger = null;

        public static IServiceCollection AddBaseLogger(this IServiceCollection services,
            ApplicationConfiguration appConfig)
        {
            if (_defaultLogger == null)
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile(Path.Join(Utilities.OperatingDirectory, "Configuration", "LoggingConfiguration.json"))
                    .Build();

                var loggerConfig = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning);


                if (Utilities.IsDevelopment)
                {
                    loggerConfig = loggerConfig.WriteTo.Console(
                            outputTemplate:
                            "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Server} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Debug();
                }

                _defaultLogger = loggerConfig.CreateLogger();
            }

            services.AddLogging(builder => builder.AddSerilog(_defaultLogger, dispose: true));
            return services;
        }

        public static IServiceCollection AddDatabaseContextOptions(this IServiceCollection services,
            ApplicationConfiguration appConfig)
        {
            var activeProvider = appConfig.DatabaseProvider?.ToLower();

            if (string.IsNullOrEmpty(appConfig.ConnectionString) || activeProvider == "sqlite")
            {
                var currentPath = Utilities.OperatingDirectory;
                currentPath = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? $"{Path.DirectorySeparatorChar}{currentPath}"
                    : currentPath;

                var connectionStringBuilder = new SqliteConnectionStringBuilder
                    {DataSource = Path.Join(currentPath, "Database", "Database.db")};
                var connectionString = connectionStringBuilder.ToString();

                var builder = new DbContextOptionsBuilder<SqliteDatabaseContext>()
                    .UseSqlite(connectionString);

                services.AddSingleton((DbContextOptions) builder.Options);
                return services;
            }

            switch (activeProvider)
            {
                case "mysql":
                    var appendTimeout = !appConfig.ConnectionString.Contains("default command timeout",
                        StringComparison.InvariantCultureIgnoreCase);
                    var mysqlBuilder = new DbContextOptionsBuilder<MySqlDatabaseContext>()
                        .UseMySql(appConfig.ConnectionString + (appendTimeout ? ";default command timeout=0" : ""),
                            mysqlOptions => mysqlOptions.EnableRetryOnFailure());
                    services.AddSingleton((DbContextOptions) mysqlBuilder.Options);
                    return services;
                case "postgresql":
                    appendTimeout = !appConfig.ConnectionString.Contains("Command Timeout",
                        StringComparison.InvariantCultureIgnoreCase);
                    var postgresqlBuilder = new DbContextOptionsBuilder<PostgresqlDatabaseContext>()
                        .UseNpgsql(appConfig.ConnectionString + (appendTimeout ? ";Command Timeout=0" : ""),
                            postgresqlOptions => postgresqlOptions.EnableRetryOnFailure());
                    services.AddSingleton((DbContextOptions) postgresqlBuilder.Options);
                    return services;
                default:
                    throw new ArgumentException($"No context available for {appConfig.DatabaseProvider}");
            }
        }
    }
}