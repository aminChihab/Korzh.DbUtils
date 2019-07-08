﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Korzh.DbUtils.EntityFrameworkCore
{

    public class DbContextBridgeException : Exception
    {
        public DbContextBridgeException(string message) : base(message) { }
    }


    public class DbContextBridge : IDbWriter
    {

        protected DbContext DbContext;

        protected readonly Dictionary<string, IEntityType> TableEntityTypes
            = new Dictionary<string, IEntityType>();

        public DbContextBridge(DbContext dbContext)
        {
            DbContext = dbContext;

            var entityTypes = DbContext.Model.GetEntityTypes();
            foreach (var entityType in entityTypes) {
                var mapping = entityType.Relational();
                TableEntityTypes[mapping.TableName] = entityType;
            }
        }

        public IDbConnection GetConnection()
        {
            return DbContext.Database.GetDbConnection();
        }

        public IReadOnlyCollection<DatasetInfo> GetDatasets()
        {
            var entityTypes = DbContext.Model.GetEntityTypes();
            var tables = new List<DatasetInfo>(entityTypes.Count());

            foreach (var entityType in entityTypes) {
                var tableName = entityType.Relational().TableName;
                if (tables.FirstOrDefault(t => t.Name == tableName) == null)
                    DetermineTableOrder(null, entityType, ref tables);
            }

            tables.Reverse();

            return tables;
        }

        public void WriteRecord(string tableName, IDataRecord record)
        {
            var entityType = TableEntityTypes[tableName];
            if (entityType == null) {
                throw new DbContextBridgeException("The table doesn't exist: " + tableName);
            }

            var item = Activator.CreateInstance(entityType.ClrType);
            foreach (var property in entityType.GetProperties()) {
                if (record.TryGetProperty(property.Relational().ColumnName, property.ClrType, out var propValue)) {
                    property.PropertyInfo.SetValue(item, propValue);
                }
            }

            var tableSchema = entityType.Relational().Schema;
            var fullTableName = string.IsNullOrEmpty(tableSchema) 
                ? tableName
                : tableSchema + "." + tableName;

            DbContext.Add(item);

            DbContext.Database.OpenConnection();
            var sql = "SET IDENTITY_INSERT " + fullTableName + " ON";
            var strategy = DbContext.Database.ExecuteSqlCommand(sql);// ExecuteSqlCommand(sql, fullTableName);
            DbContext.SaveChanges();
            DbContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT " + fullTableName + " OFF");
        }


        private void DetermineTableOrder(IEntityType startEntityType, IEntityType curEntityType, ref List<DatasetInfo> tables)
        {
            if (startEntityType == curEntityType) {
                throw new DbContextBridgeException($"Loop is detected between tables. Unable to find the right order for tables.");
            }

            var curTableName = curEntityType.Relational().TableName;
            var refereneces = curEntityType.GetReferencingForeignKeys();
            if (refereneces.Any()) {
                foreach (var reference in refereneces) {
                    var refTableName = reference.DeclaringEntityType.Relational().TableName;

                    if (tables.FirstOrDefault(t => t.Name == refTableName) == null
                        && refTableName != curTableName) 
                    {
                        DetermineTableOrder(startEntityType, reference.DeclaringEntityType, ref tables);
                    }
                }
            }

            if (tables.FirstOrDefault(t => t.Name == curTableName) == null)
                tables.Add(new DatasetInfo(curEntityType.Relational().TableName));
        }
    }

    public class DbContextBridge<TDbContext> : DbContextBridge where TDbContext : DbContext
    {
        public DbContextBridge(TDbContext dbContext) : base(dbContext) { }
    }
}
