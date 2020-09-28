-- Script Date: 04-Jul-20 3:03 PM  - ErikEJ.SqlCeScripting version 3.5.2.86
-- Database information:
-- Database: E:\Github\Inventory-Management\InventoryManagement\Database\Inventory.db
-- ServerVersion: 3.30.1
-- DatabaseSize: 8 KB
-- Created: 01-May-20 11:54 PM

-- User Table information:
-- Number of tables: 1
-- Inventory: -1 row(s)

SELECT 1;
PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE [Inventory] (
  [Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [Number] text NULL
, [Object] text NULL
, [Incoming Date] text NULL
, [Price] real NULL
, [Repack] text NULL
);
CREATE TABLE [ListViewColVisible] (
  [Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [ColName] text NULL
, [Boolean] INTEGER NULL
);
INSERT INTO [ListViewColVisible] ([ColName], [Boolean]) VALUES ('Id', 1);
INSERT INTO [ListViewColVisible] ([ColName], [Boolean]) VALUES ('Number', 1);
INSERT INTO [ListViewColVisible] ([ColName], [Boolean]) VALUES ('Object', 1);
INSERT INTO [ListViewColVisible] ([ColName], [Boolean]) VALUES ('Income Date', 1);
INSERT INTO [ListViewColVisible] ([ColName], [Boolean]) VALUES ('Price', 1);
INSERT INTO [ListViewColVisible] ([ColName], [Boolean]) VALUES ('Repack', 1);

CREATE TABLE [Language] (
  [Id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL
, [Languages] text NULL
, [Boolean] int NULL
);
COMMIT;

