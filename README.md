SQL Bulk Loader
===============
A convention based wrapper to allow for bulk loading data into a SQL RDMS. 

The purpose of this utility is to reduce the amount of code necessary to preform bulk inserts into database tables. This utility allows for bulk inserting with only a collection of DTOs and a call to the BulkLoader class. 

The class is configurable and handles remapping fields between DTO and target table, ignoring DTO properties, and controlling batch size. As well, the bulk loader avoids using a DataTable to reduce the memory overhead of preforming the SQLBulkCopy (SQL Server). The PostgreSQL version uses the binary COPY feature built into Postgre.  

Nuget: 
* SQL Server: https://www.nuget.org/packages/ivaldez.Sql.SqlBulkLoader/
* PostgreSQL: https://www.nuget.org/packages/ivaldez.Sql.SqlBulkLoader.PostgreSql/

## Targets

* .NETFramework 4.6.1
* .NETStandard 2.0

# Basic usage

With this sample table:

```sql
CREATE TABLE dbo.Sample(
    Pk INT IDENTITY(1,1) PRIMARY KEY,
    TextValue nvarchar(200) NULL,
    IntValue int NULL,
    DecimalValue decimal(18,8) NULL
)
```

## Simple bulk load with insert of identity values.

```csharp
var dtos = new[]
{
    new SampleSurrogateKey
    {
        Pk = 100,
        TextValue = "JJ",
        IntValue = 100,
        DecimalValue = 100.99m
    }
};

new BulkLoader()
    .InsertWithOptions("Sample", conn, true, dtos)
    .Execute();
```

<hr />

## Bulk load with DTO to target table remapping.

```csharp
var dtos = new[]
{
    new SampleSurrogateKeyDifferentNamesDto
    {
        Pk = 100,
        TextValueExtra = "JJ",
        IntValueExtra = 100,
        DecimalValueExtra = 100.99m
    }
};

new BulkLoader()
    .InsertWithOptions("Sample", conn, true, dtos)
    .With(c => c.TextValueExtra, "TextValue")
    .With(c => c.IntValueExtra, "IntValue")
    .With(c => c.DecimalValueExtra, "DecimalValue")
    .Execute();
```
<hr />

## Bulk load with ignore of property values.

```csharp
var dtos = new[]
{
    new SampleSurrogateKey
    {
        Pk = 100,
        TextValue = "JJ",
        IntValue = 100,
        DecimalValue = 100.99m
    }
};

 new BulkLoader()
    .InsertWithOptions("Sample", conn, true, dtos)
    .Without(c => c.Pk)
    .Execute();
```

# Dependencies

The SQL Server (SqlBulkCopy) version of this utility uses fast-member to create an efficient IDataReader that can feed directly into the SqlBulkCopy WriteToServer method. Normally, a DataTable is created for each batched write to the server.
fast-member: https://github.com/mgravell/fast-member

The PostgreSQL (COPY) verison of this utility uses the Npgsql data provider to interact with a PostgreSQL instance. 
Npgsql: https://github.com/npgsql/npgsql