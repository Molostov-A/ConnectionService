# ConnectionLogger

```sh
docker network create my_network
docker compose -p connection_logger up -d
```

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

# Методы HTTP для API сервиса отслеживания IP-адресов пользователей:

## 1. **POST /api/users/{userId}/connect**

### Описание: Добавляет новое подключение пользователя с указанным IP-адресом.
#### Тело запроса:
```
{
    "userId": 1,
    "ipAddress": "192.168.1.1"
}
```
Ответ:  
200 OK: Подключение успешно добавлено.  
400 Bad Request: Ошибка в данных запроса.

## 2. **GET /api/users/search?ipPart={ipPart}&ipVersion={ipVersion}**

### Описание: Ищет пользователей по части IP-адреса.
#### Параметры запроса:
#### ipPart: Часть IP-адреса для поиска.
#### Ответ:
200 OK: Список ID пользователей, соответствующих запросу.
```
[
    1,
    2,
    3
]
```
## 3. **GET /api/users/{userId}/ips**

### Описание: Получает все IP-адреса, связанные с конкретным пользователем.
#### Параметры запроса:
#### userId: ID пользователя.
#### Ответ:
200 OK: Список IP-адресов пользователя.
```
[
    "192.168.1.1",
    "192.168.1.2"
]
```
## 4. **GET /api/connections/search?userId={userId}&orderBy={orderBy}&direction={direction}**  

### Описание: Получает информацию о последнем подключении пользователя.
#### Параметры запроса:
#### userId: ID пользователя.
#### orderBy=dateCreated.
#### direction=asc/desc.
#### Ответ:
200 OK: Информация о последнем подключении.
```
{
    "id": 1,
    "userId": 1,
    "ipAddress": "192.168.1.1",
    "connectionTime": "2023-10-10T12:34:56Z"
}
```
