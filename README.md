# MuseumClientServerApp

## Museum Server (C# TCP/UDP Backend)

Серверная часть клиент-серверного приложения для музея.
Реализована на **C# (.NET)** с использованием:

* TCP — основная логика
* UDP — автообнаружение сервера
* Entity Framework Core — работа с базой данных
* JSON — формат обмена данными

---

# 📌 Архитектура

```
Клиент
   │
   ├── UDP (поиск сервера)
   │
   └── TCP (запросы/ответы JSON)
           │
           ├── SessionService
           ├── Handlers (логика)
           └── Database (EF Core)
```

---

# 🚀 Запуск сервера

## 1. Требования

* .NET 6+ / .NET 7+
* SQL Server (локальный или удалённый)

## 2. Настройка БД

Строка подключения находится в:

```
Data/MuseumContext.cs
```

```csharp
optionsBuilder.UseSqlServer(
    @"Data Source=GERMAN-PC;Initial Catalog=MuseumDB;Integrated Security=True;Trust Server Certificate=True"
);
```

👉 Замени `GERMAN-PC` на свой сервер при необходимости.

---

## 3. Запуск

```bash
dotnet run
```

После запуска:

* UDP сервер: `9000`
* TCP сервер: `9001`

---

# 📡 Поиск сервера (UDP)

Клиент отправляет broadcast:

```
DISCOVER_SERVER
```

Сервер отвечает:

```
SERVER_HERE
```

---

## 📌 Пример клиента (UDP)

```csharp
UdpClient client = new UdpClient();
client.EnableBroadcast = true;

byte[] data = Encoding.UTF8.GetBytes("DISCOVER_SERVER");

client.Send(data, data.Length,
    new IPEndPoint(IPAddress.Broadcast, 9000));

var ep = new IPEndPoint(IPAddress.Any, 0);
var response = client.Receive(ref ep);

Console.WriteLine(Encoding.UTF8.GetString(response)); // SERVER_HERE
Console.WriteLine(ep.Address); // IP сервера
```

---

# 🔌 Работа с TCP

После получения IP:

```
TCP порт: 9001
```

---

## 📦 Формат протокола (JSON)

### 📥 Запрос

```json
{
  "action": "GET_DEPARTMENTS",
  "token": "string",
  "data": { }
}
```

---

### 📤 Ответ (успех)

```json
{
  "status": "ok",
  "data": [...]
}
```

---

### 📤 Ответ (ошибка)

```json
{
  "status": "error",
  "message": "ERROR_CODE"
}
```

---

# 🔑 Аутентификация и сессии

Сервер использует токены.

## Регистрация сессии

### Гость:

```json
{
  "action": "REGISTER_SESSION",
  "data": {
    "userType": "guest"
  }
}
```

---

### Админ:

```json
{
  "action": "REGISTER_SESSION",
  "data": {
    "userType": "admin",
    "password": "qwerty"
  }
}
```

⚠️ Пароль захардкожен в коде (по дизайну проекта)

---

### Ответ:

```json
{
  "status": "ok",
  "data": {
    "token": "GUID"
  }
}
```

---

# 📚 API Методы

---

## 1. Получить отделы

### Запрос:

```json
{
  "action": "GET_DEPARTMENTS",
  "token": "your_token"
}
```

---

### Ответ:

```json
[
  {
    "departmentId": 1,
    "name": "История",
    "description": "..."
  }
]
```

---

## 2. Получить экспонаты

### Запрос:

```json
{
  "action": "GET_EXHIBITS",
  "token": "your_token",
  "data": {
    "departmentId": 1
  }
}
```

---

---

## 3. Получить один экспонат

### Запрос:

```json
{
  "action": "GET_EXHIBIT",
  "token": "your_token",
  "data": {
    "exhibitId": 5
  }
}
```

---

---

# 🧠 Как работает код

---

## 📁 Program.cs

Точка входа:

* запускает UDP сервер
* запускает TCP сервер
* чистит старые сессии каждые 2 часа

---

## 📁 Network/UdpServer.cs

Отвечает за:

* приём broadcast
* отправку ответа клиенту

---

## 📁 Network/TcpServer.cs

Главный сервер:

* принимает TCP клиентов
* читает JSON
* обрабатывает команды
* возвращает ответы

---

## 📁 Services/SessionService.cs

Работа с сессиями:

* создание токена
* проверка токена
* очистка старых сессий

---

## 📁 Data/MuseumContext.cs

Entity Framework:

* подключение к БД
* таблицы:

  * Sessions
  * Departments
  * Exhibits

---

## 📁 Data/Session.cs

Модель сессии:

* Token
* UserType
* CreatedAt
* LastAccess

---

## 📁 Data/Department.cs

Модель:

* Department
* Exhibit (связь один-ко-многим)

---

# ⚠️ Ограничения

* ❌ Нет шифрования (TCP без SSL)
* ❌ Пароль хранится в коде
* ❌ Нет защиты от DDoS
* ❌ Нет логирования (кроме Console)

---

# 💡 Рекомендации по улучшению

* Добавить HTTPS / TLS
* Вынести конфигурацию в appsettings.json
* Добавить логирование (Serilog)
* Добавить retry на клиенте
* Добавить heartbeat (PING)

---

# 🔌 Минимальный TCP клиент

```csharp
TcpClient client = new TcpClient("127.0.0.1", 9001);

using var stream = client.GetStream();
using var writer = new StreamWriter(stream) { AutoFlush = true };
using var reader = new StreamReader(stream);

var request = new
{
    action = "GET_DEPARTMENTS",
    token = "your_token"
};

string json = JsonSerializer.Serialize(request);

writer.WriteLine(json);

string response = reader.ReadLine();

Console.WriteLine(response);
```

---

# 🏁 Итог

Сервер реализует:

* автообнаружение через UDP
* TCP API
* JSON протокол
* систему сессий
* работу с БД
