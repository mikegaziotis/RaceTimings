/*===================================
---------------TABLES----------------
====================================*/
 
CREATE TABLE Race (
    Id UUID PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Distance INTEGER NOT NULL,
    GenderDivision SMALLINT NOT NULL,
    Location VARCHAR(255) NOT NULL,
    StartDateTime TIMESTAMPTZ NULL,
    Status SMALLINT NOT NULL,
    LaneCount SMALLINT NOT NULL,
    IsArchived BOOLEAN NOT NULL DEFAULT false,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    LastUpdatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

