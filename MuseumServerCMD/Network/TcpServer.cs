using MuseumServer.Data;
using MuseumServer.Models;
using MuseumServer.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace MuseumServer.Network
{
    public class TcpServer
    {
        private readonly int port;
        private readonly SessionService sessionService;

        // 🔒 Ограничение клиентов
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(100);

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

                await _semaphore.WaitAsync();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await HandleClientAsync(client);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CRITICAL] {ex}");
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                });
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            string endpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";

            try
            {
                client.ReceiveTimeout = 10000;
                client.SendTimeout = 10000;

                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                string requestJson = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(requestJson))
                {
                    Console.WriteLine($"[DEBUG] Пустой запрос от {endpoint}");
                    return;
                }

                Console.WriteLine($"[DEBUG] {endpoint} -> {requestJson}");

                // 🔒 Ограничение размера (пример: 10KB)
                if (requestJson.Length > 10_000)
                {
                    await SendError(writer, "REQUEST_TOO_LARGE");
                    return;
                }

                Console.WriteLine($"[DEBUG] JSON: {requestJson}");

                Request request;
                try
                {
                    request = JsonSerializer.Deserialize<Request>(requestJson);
                }
                catch
                {
                    await SendError(writer, "INVALID_JSON");
                    return;
                }

                if (request == null || string.IsNullOrWhiteSpace(request.Action))
                {
                    await SendError(writer, "INVALID_REQUEST");
                    return;
                }

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
                        await SendError(writer, "UNKNOWN_ACTION");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {endpoint}: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"[DEBUG] Закрыт клиент: {endpoint}");
                client.Close();
            }
        }
        // ===== HANDLERS =====

        private async Task HandleRegister(Request request, StreamWriter writer)
        {
            string userType = request.Data?.GetProperty("userType").GetString() ?? "guest";
            string password = request.Data?.GetProperty("password").GetString() ?? "";

            if (userType == "admin" && !sessionService.ValidateAdminPassword(password))
            {
                await SendError(writer, "ADMIN_AUTH_FAIL");
                return;
            }

            string token = sessionService.CreateSession(userType);

            await SendOk(writer, new { token });
        }

        private async Task HandleDepartments(Request request, StreamWriter writer)
        {
            if (!Validate(request.Token))
            {
                await SendError(writer, "INVALID_SESSION");
                return;
            }

            using var db = new MuseumContext();

            var departments = db.Departments
                .Select(d => new { d.DepartmentId, d.Name, d.Description })
                .ToList();

            await SendOk(writer, departments);
        }

        private async Task HandleExhibits(Request request, StreamWriter writer)
        {
            if (!Validate(request.Token))
            {
                await SendError(writer, "INVALID_SESSION");
                return;
            }

            if (!request.Data.HasValue ||
                !request.Data.Value.TryGetProperty("departmentId", out var deptProp) ||
                !deptProp.TryGetInt32(out int deptId))
            {
                await SendError(writer, "INVALID_DATA");
                return;
            }

            using var db = new MuseumContext();

            var exhibits = db.Exhibits
                .Where(e => e.DepartmentId == deptId)
                .Select(e => new { e.ExhibitId, e.Name, e.Description, e.ImagePath })
                .ToList();

            await SendOk(writer, exhibits);
        }

        private async Task HandleExhibit(Request request, StreamWriter writer)
        {
            if (!Validate(request.Token))
            {
                await SendError(writer, "INVALID_SESSION");
                return;
            }

            if (!request.Data.HasValue ||
                !request.Data.Value.TryGetProperty("exhibitId", out var exProp) ||
                !exProp.TryGetInt32(out int exId))
            {
                await SendError(writer, "INVALID_DATA");
                return;
            }

            using var db = new MuseumContext();

            var exhibit = db.Exhibits
                .Where(e => e.ExhibitId == exId)
                .Select(e => new { e.ExhibitId, e.Name, e.Description, e.ImagePath })
                .FirstOrDefault();

            await SendOk(writer, exhibit);
        }

        // ===== HELPERS =====

        private bool Validate(string token)
        {
            return !string.IsNullOrWhiteSpace(token) &&
                   sessionService.ValidateSession(token);
        }

        private async Task SendOk(StreamWriter writer, object data)
        {
            var response = new Response { Status = "ok", Data = data };
            string json = JsonSerializer.Serialize(response);
            await writer.WriteLineAsync(json);
        }

        private async Task SendError(StreamWriter writer, string message)
        {
            var response = new Response { Status = "error", Message = message };
            string json = JsonSerializer.Serialize(response);
            await writer.WriteLineAsync(json);
        }
    }
}