CREATE TABLE Client (
    Id INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    Name varchar (150) NOT NULL,
    Age int NOT NULL,
    Active bit NOT NULL DEFAULT 1
);