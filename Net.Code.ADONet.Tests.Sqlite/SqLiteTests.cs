﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Net.Code.ADONet.Tests.Sqlite
{
    public static class Data
    {
        public static MyObject Create()
        {
            return new MyObject
                   {
                       Id = 1,
                       StringNotNull = "TestString1",
                       StringNull = "TestString2",
                       NullableUniqueId = Guid.NewGuid(),
                       NonNullableUniqueId = Guid.NewGuid(),
                       NullableInt = 2,
                       NonNullableInt = 3
                   };
        }

    }

    [TestClass]
    public class SqLiteTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            var connectionStringName = "sqlite";

            using (var db = Db.FromConfig(connectionStringName))
            {
                db.Execute("DROP TABLE IF EXISTS MyTable");
                db.Execute("CREATE TABLE MyTable (" +
                           "Id int not null, " +
                           "StringNotNull nvarchar(25) not null," +
                           "StringNull nvarchar(25) null," +
                           "NullableUniqueId uniqueidentifier null," +
                           "NonNullableUniqueId uniqueidentifier not null," +
                           "NullableInt int null," +
                           "NonNullableInt int not null" +
                           ")"
                    );
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var connectionStringName = "sqlite";
            using (var db = Db.FromConfig(connectionStringName))
            {
                db.Execute("DROP TABLE MyTable");
            }

        }

        private static IEnumerable<MyObject> Select(string connectionStringName)
        {
            using (var db = Db.FromConfig(connectionStringName))
            {
                return db.Sql("SELECT Id, " +
                              "StringNotNull, " +
                              "StringNull, " +
                              "NullableUniqueId, " +
                              "NonNullableUniqueId," +
                              "NullableInt," +
                              "NonNullableInt " +
                              "FROM MyTable")
                    .AsEnumerable()
                    .Select(d => new MyObject
                                 {
                                     Id = d.Id,
                                     StringNotNull = d.StringNotNull,
                                     StringNull = d.StringNull,
                                     NullableUniqueId = d.NullableUniqueId,
                                     NonNullableUniqueId = d.NonNullableUniqueId,
                                     NullableInt = d.NullableInt,
                                     NonNullableInt = d.NonNullableInt
                                 })
                    .ToList();
            }
        }

        private static void Insert(string connectionStringName, MyObject myObject)
        {
            using (var db = Db.FromConfig(connectionStringName))
            {
                db.Sql("INSERT INTO MyTable(" +
                       "Id, " +
                       "StringNotNull, " +
                       "StringNull, " +
                       "NullableUniqueId, " +
                       "NonNullableUniqueId," +
                       "NullableInt," +
                       "NonNullableInt" +
                       ") VALUES (" +
                       "@Id, " +
                       "@StringNotNull, " +
                       "@StringNull, " +
                       "@NullableUniqueId," +
                       "@NonNullableUniqueId," +
                       "@NullableInt," +
                       "@NonNullableInt" +
                       ")")
                    .WithParameters(
                        myObject
                    ).AsNonQuery();
            }
        }

        [TestMethod]
        public void An_inserted_object_can_be_selected()
        {
            var myObject = new MyObject
            {
                Id = 1,
                StringNotNull = "StringNotNull",
                StringNull = "StringNull",
                NonNullableUniqueId = Guid.NewGuid(),
                NullableUniqueId = Guid.NewGuid(),
                NullableInt = 2,
                NonNullableInt = 3
            };

            Insert("sqlite", myObject);

            var item = Select("sqlite").First();

            Assert.AreEqual(myObject, item);
        }
        [TestMethod]
        public void An_parameter_can_be_updated()
        {
            var myObject = new MyObject
            {
                Id = 1,
                StringNotNull = "StringNotNull",
                StringNull = "StringNull",
                NonNullableUniqueId = Guid.NewGuid(),
                NullableUniqueId = Guid.NewGuid(),
                NullableInt = 2,
                NonNullableInt = 3
            };

            Insert("sqlite", myObject);

            using (var db = Db.FromConfig("sqlite"))
            {
                var commandBuilder = db.Sql("UPDATE MyTable SET StringNull = @stringValue WHERE Id = @id")
                    .WithParameter("id", 1)
                    .WithParameter("stringValue", "StringNull updated");
                commandBuilder.AsNonQuery();
                commandBuilder.WithParameter("stringValue", "StringNull updated again");
                commandBuilder.AsNonQuery();
                var result = db.Sql("SELECT StringNull FROM MyTable where Id = 1").AsScalar<string>();
                Assert.AreEqual("StringNull updated again", result);
            }
        }
    }
}