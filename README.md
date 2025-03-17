# Cервис для работы с пользователями и их IP-адресами (ConnectionLogger)

# Описание
**Сервис накапливает и обрабатывает информацию о подключениях пользователей с различных IP-адресов, сохраняя данные в PostgreSQL. Один пользователь может подключаться с разных IP, а несколько пользователей могут использовать один и тот же IP.**

### Общая схема работы сервиса:
**Система работает через очередь сообщений и состоит из трёх ключевых компонентов:**
- WebProduser отправляет события о подключениях в очередь и ожидает ответа.
- WebConsumer принимает сообщения, анализирует, и направляет их в нужные сервисы обработки (Handlers).
- Handlers подгатавлявают данные и отправляют к сервису взаимодействия с базой данных, после чего отправляют результат обратно к WebProducer по очереди ответов.

### Особенности:
Масштабируемость: система легко расширяется добавлением новых handlers.
Гибкость: сервисы могут изменяться и дополняться без изменения архитектуры.


# Установка и развертывание
**Чтобы запустить проект локально, необходимо выполнить следующие действия:**

- Выполните следующие команды для создания сети и запуска необходимых контейнеров в Docker:
```sh
docker network create my_network
docker compose -p connection_logger up -d
```

# Создание таблиц для API сервиса отслеживания IP-адресов пользователей:
Ниже представлены схемы таблиц, используемых для хранения данных о пользователях, их IP-адресах и подключениях.

IpAddresses – хранит уникальные IP-адреса.
Users – содержит информацию о пользователях.
Connections – фиксирует факты подключения пользователей с определённых IP-адресов, включая время подключения.

## IpAddress
```sql
CREATE TABLE IpAddresses (
    Id BIGINT NOT NULL IDENTITY PRIMARY KEY,
    Address NVARCHAR(45) NOT NULL,
    Protocol NVARCHAR(15) NOT NULL,
    CONSTRAINT UC_IpAddress_Address UNIQUE (Address)
);
```

## User
```sql
CREATE TABLE Users (
    Id BIGINT NOT NULL IDENTITY PRIMARY KEY,
    LastName NVARCHAR(45) NULL,
    FirstName NVARCHAR(45) NULL
);
```

## Connection
```sql
CREATE TABLE Connections (
    UserId BIGINT NOT NULL,
    IpAddressId BIGINT NOT NULL,
    ConnectedAt DATETIME NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY (UserId, IpAddressId, ConnectedAt),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (IpAddressId) REFERENCES IpAddresses(Id)
);
CREATE INDEX IX_Connection_UserId ON Connections(UserId);
```


# Методы HTTP для API сервиса:

## 1. **POST /api/users/{userId}/connect**
Добавляет новое подключение пользователя с указанным IP-адресом.

#### Тело запроса:
```
{
    "ip": "192.168.1.1"
}
```
Ответ:  
200 OK: Подключение успешно добавлено.  
400 Bad Request: Ошибка в данных запроса.

## 2. **GET /api/users/search?ipPart={ipPart}&ipVersion={ipVersion}**
Ищет пользователей по части IP-адреса.

#### Параметры запроса:
**ipPart: Часть IP-адреса для поиска.**

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
Получает все IP-адреса, связанные с конкретным пользователем.

#### Параметры запроса:
**userId: ID пользователя.**

#### Ответ:
200 OK: Список IP-адресов пользователя.
```
[
    "192.168.1.1",
    "192.168.1.2"
]
```
## 4. **GET /api/connections/search?userId={userId}&orderBy={orderBy}&direction={direction}**  
Получает информацию о последнем подключении пользователя.

#### Параметры запроса:
**userId: ID пользователя**
**orderBy=dateCreated**
**direction=asc/desc**

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