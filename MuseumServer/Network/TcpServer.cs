using MuseumServer.Data;
using MuseumServer.Models;
using MuseumServer.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace MuseumServer.Network
{
    public class TcpServer
    {
        private readonly int port;
        private readonly SessionService sessionService;

        public TcpServer(int port, SessionService sessionService)
        {
            this.port = port;
            this.sessionService = sessionService;
        }

        public async Task Start()
        {
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"TCP сервер запущен на порту {port}");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                Console.WriteLine($"[DEBUG] Подключился клиент: {client.Client.RemoteEndPoint}");
                _ = HandleClientAsync(client); // fire & forget
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                string requestJson = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(requestJson))
                {
                    Console.WriteLine("[DEBUG] Пустой запрос от клиента");
                    return;
                }

                Console.WriteLine($"[DEBUG] Получен JSON запрос: {requestJson}");

                Request request;
                try
                {
                    request = JsonSerializer.Deserialize<Request>(requestJson);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Ошибка десериализации JSON: {ex.Message}");
                    await SendError(writer, "INVALID_JSON");
                    return;
                }

                if (request == null || string.IsNullOrEmpty(request.Action))
                {
                    Console.WriteLine("[DEBUG] Пустой action в запросе");
                    await SendError(writer, "INVALID_REQUEST");
                    return;
                }

                Console.WriteLine($"[DEBUG] Action: {request.Action}, Token: {request.Token}");

                switch (request.Action)
                {
                    case "REGISTER_SESSION":
                        await HandleRegister(request, writer);
                        break;
                    case "GET_DEPARTMENTS":
                        await HandleDepartments(request, writer);
                        break;
                    case "GET_EXHIBITS":
                        await HandleExhibits(request, writer);
                        break;
                    case "GET_EXHIBIT":
                        await HandleExhibit(request, writer);
                        break;
                    default:
                        Console.WriteLine("[DEBUG] Неизвестный action");
                        await SendError(writer, "UNKNOWN_ACTION");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка обработки клиента: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"[DEBUG] Закрываем соединение с клиентом: {client.Client.RemoteEndPoint}");
                client.Close();
            }
        }

        // ===== HANDLERS =====

        private async Task HandleRegister(Request request, StreamWriter writer)
        {
            Console.WriteLine("[DEBUG] Обработка REGISTER_SESSION");

            string userType = request.Data?.GetProperty("userType").GetString() ?? "guest";
            string password = request.Data?.GetProperty("password").GetString() ?? "";

            Console.WriteLine($"[DEBUG] userType={userType}, password='{password}'");

            if (userType == "admin" && password != "qwerty")
            {
                Console.WriteLine("[DEBUG] Admin authentication failed");
                await SendError(writer, "ADMIN_AUTH_FAIL");
                return;
            }

            string token = sessionService.CreateSession(userType);
            Console.WriteLine($"[DEBUG] Создан токен: {token} для {userType}");

            await SendOk(writer, new { token });
            Console.WriteLine("[DEBUG] Ответ клиенту отправлен");
        }

        private async Task HandleDepartments(Request request, StreamWriter writer)
        {
            Console.WriteLine("[DEBUG] Обработка GET_DEPARTMENTS");

            if (!Validate(request.Token))
            {
                Console.WriteLine($"[DEBUG] Неверный токен: {request.Token}");
                await SendError(writer, "INVALID_SESSION");
                return;
            }

            using var db = new MuseumContext();
            var departments = db.Departments
                .Select(d => new { d.DepartmentId, d.Name, d.Description })
                .ToList();

            Console.WriteLine($"[DEBUG] Отправка {departments.Count} отделов клиенту");
            await SendOk(writer, departments);
        }

        private async Task HandleExhibits(Request request, StreamWriter writer)
        {
            Console.WriteLine("[DEBUG] Обработка GET_EXHIBITS");

            if (!Validate(request.Token))
            {
                Console.WriteLine($"[DEBUG] Неверный токен: {request.Token}");
                await SendError(writer, "INVALID_SESSION");
                return;
            }

            if (!request.Data.HasValue ||
                !request.Data.Value.TryGetProperty("departmentId", out var deptProp) ||
                !deptProp.TryGetInt32(out int deptId))
            {
                Console.WriteLine("[DEBUG] Некорректные данные запроса GET_EXHIBITS");
                await SendError(writer, "INVALID_DATA");
                return;
            }

            using var db = new MuseumContext();
            var exhibits = db.Exhibits
                .Where(e => e.DepartmentId == deptId)
                .Select(e => new { e.ExhibitId, e.Name, e.Description, e.ImagePath })
                .ToList();

            Console.WriteLine($"[DEBUG] Отправка {exhibits.Count} экспонатов клиенту");
            await SendOk(writer, exhibits);
        }

        private async Task HandleExhibit(Request request, StreamWriter writer)
        {
            Console.WriteLine("[DEBUG] Обработка GET_EXHIBIT");

            if (!Validate(request.Token))
            {
                Console.WriteLine($"[DEBUG] Неверный токен: {request.Token}");
                await SendError(writer, "INVALID_SESSION");
                return;
            }

            if (!request.Data.HasValue ||
                !request.Data.Value.TryGetProperty("exhibitId", out var exProp) ||
                !exProp.TryGetInt32(out int exId))
            {
                Console.WriteLine("[DEBUG] Некорректные данные запроса GET_EXHIBIT");
                await SendError(writer, "INVALID_DATA");
                return;
            }

            using var db = new MuseumContext();
            var exhibit = db.Exhibits
                .Where(e => e.ExhibitId == exId)
                .Select(e => new { e.ExhibitId, e.Name, e.Description, e.ImagePath })
                .FirstOrDefault();

            Console.WriteLine($"[DEBUG] Отправка данных экспоната {exId} клиенту");
            await SendOk(writer, exhibit);
        }

        // ===== HELPERS =====

        private bool Validate(string token)
        {
            bool valid = !string.IsNullOrEmpty(token) && sessionService.ValidateSession(token);
            Console.WriteLine($"[DEBUG] Проверка токена '{token}': {valid}");
            return valid;
        }

        private async Task SendOk(StreamWriter writer, object data)
        {
            var response = new Response { Status = "ok", Data = data };
            string json = JsonSerializer.Serialize(response);
            await writer.WriteLineAsync(json);
            Console.WriteLine("[DEBUG] Отправлен ответ OK");
        }

        private async Task SendError(StreamWriter writer, string message)
        {
            var response = new Response { Status = "error", Message = message };
            string json = JsonSerializer.Serialize(response);
            await writer.WriteLineAsync(json);
            Console.WriteLine($"[DEBUG] Отправлен ответ ERROR: {message}");
        }
    }
}