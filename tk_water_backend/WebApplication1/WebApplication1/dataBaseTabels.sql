-- Script Date: 05/07/2024 15:14  - ErikEJ.SqlCeScripting version 3.5.2.95
CREATE TABLE [unitData] 
(
  [userID] INTEGER NOT NULL
, [moduleID] INTEGER DEFAULT (1) NOT NULL
, [unitID] TEXT NOT NULL UNIQUE
, [unitName] TEXT DEFAULT 'unitName' NOT NULL
, [moistureLevel] INTEGER DEFAULT (0) NOT NULL
, [moistureThreshold] INTEGER DEFAULT (0) NOT NULL
, CONSTRAINT [PK_unitData] PRIMARY KEY ([userID])
);

CREATE TABLE [unitData] 
(
  [userID] bigint NOT NULL
, [unitID] text NOT NULL
, [unitName] text DEFAULT ('unitName') NOT NULL
, [moistureLevel] bigint DEFAULT (0) NOT NULL
, [moistureThreshold] bigint DEFAULT (0) NOT NULL
, CONSTRAINT [sqlite_master_PK_unitData] PRIMARY KEY ([unitID])
, FOREIGN KEY ([userID]) REFERENCES userData([userID])
);
