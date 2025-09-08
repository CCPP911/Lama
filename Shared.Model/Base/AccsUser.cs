using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Shared.Model.Base
{
    /// <summary>
    /// Модель пользователя системы управления доступом (Accounts User)
    /// </summary>
    public class AccsUser
    {
        /// <summary> Уникальный идентификатор пользователя </summary>
        public string? Id { get; set; }

        /// <summary> Список альтернативных идентификаторов (соцсети, старые ID и т.п.) </summary>
        public List<string> Ids { get; set; } = new List<string>();

        /// <summary> Есть ли у пользователя установленный пароль </summary>
        public bool IsPasswd { get; set; }

        /// <summary> Дата истечения действия аккаунта или токена </summary>
        public DateTime Expires { get; set; }

        /// <summary> Идентификатор группы/роли (1 - админ, 2 - пользователь и т.д.) </summary>
        public int Group { get; set; }

        /// <summary> Заблокирован ли пользователь </summary>
        public bool Ban { get; set; }

        /// <summary> Сообщение о причине блокировки </summary>
        public string? BanMsg { get; set; }

        /// <summary> Комментарий администратора </summary>
        public string? Comment { get; set; }

        /// <summary> Динамические параметры пользователя </summary>
        public Dictionary<string, object>? Params { get; set; }

        // ────────────────────────────────────────────────────────────────
        // 🔐 Устройства и сессии пользователя
        // ────────────────────────────────────────────────────────────────

        /// <summary> Список активных и исторических сессий пользователя по устройствам </summary>
        public List<DeviceSession> Devices { get; set; } = new List<DeviceSession>();

        /// <summary> Количество активных устройств (только IsActive == true) </summary>
        [JsonIgnore]
        public int ActiveDeviceCount => Devices.Count(d => d.IsActive);

        /// <summary> Общее количество устройств (всех, включая неактивные) </summary>
        [JsonIgnore]
        public int TotalDeviceCount => Devices.Count;

        // ────────────────────────────────────────────────────────────────
        // 🧰 Вспомогательные методы для управления устройствами
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Добавляет новую сессию устройства
        /// </summary>
        public void AddDevice(
            string? macAddress = null,
            string? userAgent = null,
            string? ipAddress = null,
            string? deviceName = null)
        {
            Devices.Add(new DeviceSession
            {
                SessionId = Guid.NewGuid().ToString(),
                MacAddress = macAddress,
                UserAgent = userAgent,
                IpAddress = ipAddress,
                LoginTime = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                DeviceName = deviceName,
                IsActive = true
            });
        }

        /// <summary>
        /// Завершает сессию по SessionId
        /// </summary>
        public bool RemoveDevice(string sessionId)
        {
            var device = Devices.FirstOrDefault(d => d.SessionId == sessionId);
            if (device != null)
            {
                device.IsActive = false;
                device.LastActivity = DateTime.UtcNow;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Обновляет время последней активности для сессии
        /// </summary>
        public bool UpdateLastActivity(string sessionId)
        {
            var device = Devices.FirstOrDefault(d => d.SessionId == sessionId && d.IsActive);
            if (device != null)
            {
                device.LastActivity = DateTime.UtcNow;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Проверяет, превышено ли максимальное количество активных устройств
        /// </summary>
        public bool IsDeviceLimitExceeded(int maxDevices)
        {
            return ActiveDeviceCount >= maxDevices;
        }

        /// <summary>
        /// Автоматически удаляет самую старую активную сессию (для освобождения места под новое устройство)
        /// </summary>
        public bool RemoveOldestDevice()
        {
            var oldestActive = Devices
                .Where(d => d.IsActive)
                .OrderBy(d => d.LoginTime)
                .FirstOrDefault();

            if (oldestActive != null)
            {
                oldestActive.IsActive = false;
                oldestActive.LastActivity = DateTime.UtcNow;
                return true;
            }
            return false;
        }
    }
}
