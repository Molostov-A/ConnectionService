# Создание таблиц для API сервиса отслеживания IP-адресов пользователей:

## Создание таблицы IpAddress
```sql
CREATE TABLE IpAddresses (
    Id BIGINT NOT NULL IDENTITY PRIMARY KEY,
    Address NVARCHAR(45) NOT NULL,
    Protocol NVARCHAR(15) NOT NULL,
    CONSTRAINT UC_IpAddress_Address UNIQUE (Address)
);
```

## Создание таблицы User
```sql
CREATE TABLE Users (
    Id BIGINT NOT NULL IDENTITY PRIMARY KEY,
    LastName NVARCHAR(45) NULL,
    FirstName NVARCHAR(45) NULL
);
```

## Создание таблицы Connection
```sql
CREATE TABLE Connections (
    UserId BIGINT NOT NULL,
    IpAddressId BIGINT NOT NULL,
    ConnectedAt DATETIME NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY (UserId, IpAddressId, ConnectedAt),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (IpAddressId) REFERENCES IpAddresses(Id)
);
```

## Создание индексов для таблицы Connection
```sql
CREATE INDEX IX_Connection_UserId ON Connections(UserId);
```
