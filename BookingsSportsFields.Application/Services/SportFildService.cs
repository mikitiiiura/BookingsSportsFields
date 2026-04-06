using BookingsSportsFields.Application.InterfaceServices;
using BookingsSportsFields.Core;
using BookingsSportsFields.DataAccess.Abstruction;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingsSportsFields.Application.Contracts.Request;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace BookingsSportsFields.Application.Services
{
    public class SportFildService : ISportFildService
    {
        private readonly IConfiguration _configuration;
        private readonly ISportsFieldsRepository _sportsFieldsRepository;
        private readonly ILogger<SportFildService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SportFildService(
            ISportsFieldsRepository sportsFieldsRepository,
            ILogger<SportFildService> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _sportsFieldsRepository = sportsFieldsRepository;
            _logger = logger;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<SportsFieldsEntity>> GetAll()
        {
            return await _sportsFieldsRepository.GetAll();
        }

        public async Task<List<SportsFieldsEntity>> GetAllByOwnerID(Guid ownerId)
        {
            return await _sportsFieldsRepository.GetAllByOwnerID(ownerId);
        }

        public async Task<List<SportsFieldsEntity>> GetFilteredFild(int? type, string? searchTitleOrAddres,
            DateTime? date, string? startTime, string? duration, string? city)
        {
            var day = date.HasValue ? UtcDateTimeHelper.UtcStartOfCalendarDay(date.Value) : (DateTime?)null;
            return await _sportsFieldsRepository.GetFilteredFild(type, searchTitleOrAddres, day, startTime, duration, city);
        }

        public async Task<SportsFieldsEntity> AddSportsFields(SportsFieldsEntity sportsFields)
        {
            return await _sportsFieldsRepository.CreateSportsField(sportsFields);
        }

        public async Task UpdateAsync(UpdateSportsFieldDto dto)
        {
            _logger.LogInformation("=== СЕРВІС: Початок оновлення ID: {Id} ===", dto.Id);

            var existing = await _sportsFieldsRepository.GetByIdWithTrackingAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException($"SportsField {dto.Id} not found");

            _logger.LogInformation("Завантажено сутність. Кількість типів: {Count}",
                existing.TypesWithDetails?.Count ?? 0);

            // 1. Базові поля
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                _logger.LogInformation("Змінюємо назву: {Old} → {New}", existing.Name, dto.Name);
                existing.Name = dto.Name;
            }

            if (dto.Description != null)
                existing.Description = dto.Description;

            // 2. Типи спорту
            if (dto.Types != null && dto.Types.Any())
            {
                _logger.LogInformation("Оновлюємо {Count} типів спорту", dto.Types.Count);

                foreach (var typeDto in dto.Types)
                {
                    var existingType = existing.TypesWithDetails.FirstOrDefault(t => t.Type == typeDto.Type);

                    if (existingType == null)
                    {
                        _logger.LogInformation("Додаємо НОВИЙ тип: {Type}", typeDto.Type);
                        var newType = new SportsFieldSportTypeEntity
                        {
                            Id = Guid.Empty, // <-- Важливо: Guid.Empty для нового запису
                            SportsFieldId = existing.Id,
                            Type = typeDto.Type,
                            PricePerHour = typeDto.PricePerHour,
                            WarningInformation = typeDto.WarningInformation ?? "",
                            WeeklySchedules = typeDto.WeeklySchedules.Select(ws => new SportsFieldSchedule
                            {
                                Id = Guid.Empty, // <-- Важливо: Guid.Empty для нового розкладу
                                DayOfWeek = ws.DayOfWeek,
                                AvailableFrom = ws.AvailableFrom,
                                AvailableTo = ws.AvailableTo
                            }).ToList()
                        };

                        newType.Instances = CreateInstancesForType(newType, typeDto);
                        existing.TypesWithDetails.Add(newType);
                    }
                    else
                    {
                        _logger.LogInformation("Оновлюємо існуючий тип: {Type}", typeDto.Type);
                        existingType.PricePerHour = typeDto.PricePerHour;
                        existingType.WarningInformation = typeDto.WarningInformation ?? "";

                        // --- ОНОВЛЕННЯ РОЗКЛАДУ ---
                        var incomingSchedules = typeDto.WeeklySchedules.ToList();
                        var currentSchedules = existingType.WeeklySchedules.ToList();

                        // 1. Оновлюємо існуючі записи розкладу
                        for (int i = 0; i < currentSchedules.Count; i++)
                        {
                            if (i < incomingSchedules.Count)
                            {
                                currentSchedules[i].DayOfWeek = incomingSchedules[i].DayOfWeek;
                                currentSchedules[i].AvailableFrom = incomingSchedules[i].AvailableFrom;
                                currentSchedules[i].AvailableTo = incomingSchedules[i].AvailableTo;
                            }
                        }

                        // 2. Видаляємо зайві записи розкладу, якщо їх стало менше
                        if (currentSchedules.Count > incomingSchedules.Count)
                        {
                            for (int i = incomingSchedules.Count; i < currentSchedules.Count; i++)
                            {
                                existingType.WeeklySchedules.Remove(currentSchedules[i]);
                            }
                        }

                        // 3. Додаємо нові записи розкладу, якщо їх стало більше
                        for (int i = currentSchedules.Count; i < incomingSchedules.Count; i++)
                        {
                            existingType.WeeklySchedules.Add(new SportsFieldSchedule
                            {
                                Id = Guid.Empty, // <-- Важливо: Guid.Empty для нового розкладу
                                DayOfWeek = incomingSchedules[i].DayOfWeek,
                                AvailableFrom = incomingSchedules[i].AvailableFrom,
                                AvailableTo = incomingSchedules[i].AvailableTo
                            });
                        }

                        // Оновлюємо Instances
                        UpdateInstances(existingType, typeDto);
                    }
                }
            }

            _logger.LogInformation("Перед збереженням. Змінені типи: {Count}", existing.TypesWithDetails.Count);

            // Зберігаємо усі зміни
            await _sportsFieldsRepository.UpdateAsync(existing);

            _logger.LogInformation("=== СЕРВІС: Оновлення завершено успішно ===");
        }

        public async Task<SportsFieldsEntity?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _sportsFieldsRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<string> UpdateSportsFieldImageAsync(Guid id, IFormFile imageFile)
        {
            if (!imageFile.ContentType.StartsWith("image/"))
                throw new ArgumentException("Тільки зображення дозволені (image/*)");

            if (imageFile.Length > 5 * 1024 * 1024)
                throw new ArgumentException("Файл завеликий, максимум 5 МБ");

            // Тут Guid.NewGuid() залишається, оскільки він генерує унікальне ім'я для ФАЙЛУ
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var sportsDir = Path.Combine(webRoot, "images", "sportsfields");
            Directory.CreateDirectory(sportsDir);
            var filePath = Path.Combine(sportsDir, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            var baseUrl = GetPublicBaseUrl().TrimEnd('/');
            var newUrl = $"{baseUrl}/images/sportsfields/{fileName}";

            _logger.LogInformation("Збережено зображення: {NewUrl}", newUrl);

            await _sportsFieldsRepository.UpdateImageUrlAsync(id, newUrl);

            return newUrl;
        }

        public async Task<bool> DeleteAsync(Guid sportFieldId)
        {
            return await _sportsFieldsRepository.Delete(sportFieldId);
        }

        // ====================== Допоміжні методи ======================

        /// <summary>Публічний базовий URL API: з поточного запиту (правильний порт/схема) або з AppSettings.</summary>
        private string GetPublicBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
                return $"{request.Scheme}://{request.Host}";
            return _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5035";
        }

        private List<SportsFieldInstanceEntity> CreateInstancesForType(SportsFieldSportTypeEntity typeEntity, UpdateSportTypeDetailDto dto)
        {
            if (dto.Instances != null && dto.Instances.Any())
            {
                return dto.Instances.Select(i => new SportsFieldInstanceEntity
                {
                    Id = Guid.Empty, // EF зробить INSERT
                    DisplayName = string.IsNullOrWhiteSpace(i.DisplayName) ? "Без назви" : i.DisplayName.Trim(),
                    SportTypeId = typeEntity.Id,
                    SportsFieldId = typeEntity.SportsFieldId,
                    IsActive = true
                }).ToList();
            }

            int qty = dto.Quantity ?? 1;
            return Enumerable.Range(1, qty)
                .Select(i => new SportsFieldInstanceEntity
                {
                    Id = Guid.Empty, // EF зробить INSERT
                    DisplayName = $"{typeEntity.Type} №{i}",
                    SportTypeId = typeEntity.Id,
                    SportsFieldId = typeEntity.SportsFieldId,
                    IsActive = true
                }).ToList();
        }

        private void UpdateInstances(SportsFieldSportTypeEntity existingType, UpdateSportTypeDetailDto dto)
        {
            var incomingInstances = dto.Instances ?? new List<UpdateInstanceDto>();
            var currentActiveInstances = existingType.Instances.Where(i => i.IsActive).ToList();

            // 1. Деактивуємо всі поточні в пам'яті
            foreach (var inst in existingType.Instances)
            {
                inst.IsActive = false;
            }

            // 2. Опрацьовуємо список, який прийшов з фронтенду
            if (incomingInstances.Any())
            {
                foreach (var inc in incomingInstances)
                {
                    // Перевірка: чи має присланий об'єкт існуючий Id?
                    var isNew = inc.Id == null || inc.Id == Guid.Empty;
                    var existingInst = isNew ? null : existingType.Instances.FirstOrDefault(i => i.Id == inc.Id);

                    if (existingInst != null)
                    {
                        // Оновлюємо існуючий (EF знає цей Id, тому зробить UPDATE)
                        existingInst.DisplayName = string.IsNullOrWhiteSpace(inc.DisplayName) ? "Без назви" : inc.DisplayName.Trim();
                        existingInst.IsActive = true;
                    }
                    else
                    {
                        // Створюємо НОВИЙ запис. Id залишаємо Guid.Empty для INSERT
                        existingType.Instances.Add(new SportsFieldInstanceEntity
                        {
                            Id = Guid.Empty,
                            DisplayName = string.IsNullOrWhiteSpace(inc.DisplayName) ? "Без назви" : inc.DisplayName.Trim(),
                            SportTypeId = existingType.Id,
                            SportsFieldId = existingType.SportsFieldId, 
                            IsActive = true
                        });
                    }
                }
            }
            // 3. Запасний варіант: змінили лише Quantity без списку Instances
            else if (dto.Quantity.HasValue && dto.Quantity.Value > 0)
            {
                int desiredQty = dto.Quantity.Value;
                int currentQty = currentActiveInstances.Count;

                for (int i = 0; i < Math.Min(desiredQty, currentQty); i++)
                {
                    currentActiveInstances[i].IsActive = true;
                }

                if (desiredQty > currentQty)
                {
                    for (int i = currentQty + 1; i <= desiredQty; i++)
                    {
                        existingType.Instances.Add(new SportsFieldInstanceEntity
                        {
                            Id = Guid.Empty, // Guid.Empty для INSERT
                            DisplayName = $"{existingType.Type} №{i}",
                            SportTypeId = existingType.Id,
                            SportsFieldId = existingType.SportsFieldId,
                            IsActive = true
                        });
                    }
                }
            }
        }
    }
}



// using BookingsSportsFields.Application.InterfaceServices;
// using BookingsSportsFields.DataAccess.Abstruction;
// using BookingsSportsFields.DataAccess.ModelEntity;
// using Microsoft.Extensions.Logging;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using BookingsSportsFields.Application.Contracts.Request;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Configuration;
//
// namespace BookingsSportsFields.Application.Services
// {
//     public class SportFildService : ISportFildService
//     {
//         private readonly IConfiguration _configuration;
//         private readonly ISportsFieldsRepository _sportsFieldsRepository;
//         private readonly ILogger _logger;
//         
//
//         public SportFildService(ISportsFieldsRepository sportsFieldsRepository, ILogger<SportFildService> logger, IConfiguration configuration)
//         {
//             _configuration = configuration;
//             _sportsFieldsRepository = sportsFieldsRepository; 
//             _logger = logger;
//         }
//         // Отримати всі завдання
//         public async Task<List<SportsFieldsEntity>> GetAll()
//         {
//             return await _sportsFieldsRepository.GetAll();
//         }
//
//         public async Task<List<SportsFieldsEntity>> GetAllByOwnerID(Guid ownerId)
//         {
//             return await _sportsFieldsRepository.GetAllByOwnerID(ownerId);
//         }
//
//         public async Task<List<SportsFieldsEntity>> GetFilteredFild(int? type, string? searchTitleOrAddres,
//             DateTime? date, string? startTime, string? duration, string? city)
//         {
//             return await _sportsFieldsRepository.GetFilteredFild(type, searchTitleOrAddres, date, startTime, duration,
//                 city);
//         }
//
//         public async Task<SportsFieldsEntity> AddSportsFields(SportsFieldsEntity sportsFields)
//         {
//             return await _sportsFieldsRepository.CreateSportsField(sportsFields);
//         }
//
//         
//         public async Task UpdateAsync(UpdateSportsFieldDto dto)
// {
//     _logger.LogInformation("=== СЕРВІС: Початок оновлення ID: {Id} ===", dto.Id);
//
//     var existing = await _sportsFieldsRepository.GetByIdWithTrackingAsync(dto.Id);
//     if (existing == null)
//         throw new KeyNotFoundException($"SportsField {dto.Id} not found");
//
//     _logger.LogInformation("Завантажено сутність. Кількість типів: {Count}", existing.TypesWithDetails?.Count ?? 0);
//
//     // 1. Базові поля
//     if (!string.IsNullOrWhiteSpace(dto.Name))
//     {
//         _logger.LogInformation("Змінюємо назву: {Old} → {New}", existing.Name, dto.Name);
//         existing.Name = dto.Name;
//     }
//     if (dto.Description != null)
//         existing.Description = dto.Description;
//
//     // 2. Типи спорту
//     if (dto.Types != null && dto.Types.Any())
//     {
//         _logger.LogInformation("Оновлюємо {Count} типів спорту", dto.Types.Count);
//
//         foreach (var typeDto in dto.Types)
//         {
//             var existingType = existing.TypesWithDetails
//                 .FirstOrDefault(t => t.Type == typeDto.Type);
//
//             if (existingType == null)
//             {
//                 _logger.LogInformation("Додаємо НОВИЙ тип: {Type}", typeDto.Type);
//                 var newType = new SportsFieldSportTypeEntity
//                 {
//                     Id = Guid.NewGuid(),
//                     SportsFieldId = existing.Id,
//                     Type = typeDto.Type,
//                     PricePerHour = typeDto.PricePerHour,
//                     WarningInformation = typeDto.WarningInformation ?? "",
//                     WeeklySchedules = typeDto.WeeklySchedules.Select(ws => new SportsFieldSchedule
//                     {
//                         Id = Guid.NewGuid(),
//                         DayOfWeek = ws.DayOfWeek,
//                         AvailableFrom = ws.AvailableFrom,
//                         AvailableTo = ws.AvailableTo
//                     }).ToList()
//                 };
//
//                 newType.Instances = CreateInstancesForType(newType, typeDto);
//                 existing.TypesWithDetails.Add(newType);
//             }
//             else
//             {
//                 _logger.LogInformation("Оновлюємо існуючий тип: {Type}", typeDto.Type);
//                 existingType.PricePerHour = typeDto.PricePerHour;
//                 existingType.WarningInformation = typeDto.WarningInformation ?? "";
//
//                 // Розклад
//                 existingType.WeeklySchedules.Clear();
//                 foreach (var ws in typeDto.WeeklySchedules)
//                 {
//                     existingType.WeeklySchedules.Add(new SportsFieldSchedule
//                     {
//                         Id = Guid.NewGuid(),
//                         DayOfWeek = ws.DayOfWeek,
//                         AvailableFrom = ws.AvailableFrom,
//                         AvailableTo = ws.AvailableTo
//                     });
//                 }
//
//                 UpdateInstances(existingType, typeDto);
//             }
//         }
//     }
//
//     _logger.LogInformation("Перед збереженням. Змінені типи: {Count}", existing.TypesWithDetails.Count);
//     await _sportsFieldsRepository.UpdateAsync(existing);
//     
//
//     _logger.LogInformation("=== СЕРВІС: Оновлення завершено успішно ===");
// }
//
// // ====================== Допоміжні методи ======================
//
// private List<SportsFieldInstanceEntity> CreateInstancesForType(
//     SportsFieldSportTypeEntity typeEntity, UpdateSportTypeDetailDto dto)
// {
//     if (dto.Instances != null && dto.Instances.Any())
//     {
//         return dto.Instances.Select(i => new SportsFieldInstanceEntity
//         {
//             Id = i.Id ?? Guid.NewGuid(),
//             DisplayName = i.DisplayName.Trim(),
//             SportTypeId = typeEntity.Id,
//             IsActive = true
//         }).ToList();
//     }
//
//     int qty = dto.Quantity ?? 1;
//     return Enumerable.Range(1, qty)
//         .Select(i => new SportsFieldInstanceEntity
//         {
//             Id = Guid.NewGuid(),
//             DisplayName = $"{typeEntity.Type} №{i}",
//             SportTypeId = typeEntity.Id,
//             IsActive = true
//         }).ToList();
// }
//
// private void UpdateInstances(SportsFieldSportTypeEntity existingType, UpdateSportTypeDetailDto dto)
// {
//     var incomingInstances = dto.Instances ?? new List<UpdateInstanceDto>();
//
//     // Якщо користувач надіслав конкретні інстанси — оновлюємо назви
//     if (incomingInstances.Any())
//     {
//         foreach (var inc in incomingInstances)
//         {
//             var existingInst = existingType.Instances.FirstOrDefault(i => i.Id == inc.Id);
//             if (existingInst != null)
//             {
//                 existingInst.DisplayName = inc.DisplayName.Trim();
//                 // IsActive залишаємо true
//             }
//             else
//             {
//                 // Додаємо новий інстанс
//                 existingType.Instances.Add(new SportsFieldInstanceEntity
//                 {
//                     Id = Guid.NewGuid(),
//                     DisplayName = inc.DisplayName.Trim(),
//                     SportTypeId = existingType.Id,
//                     IsActive = true
//                 });
//             }
//         }
//     }
//     // Якщо тільки quantity — коригуємо кількість (але не видаляємо ті, на які є бронювання)
//     else if (dto.Quantity.HasValue)
//     {
//         int desiredQty = dto.Quantity.Value;
//         int currentActive = existingType.Instances.Count(i => i.IsActive);
//
//         if (desiredQty > currentActive)
//         {
//             // Додаємо нові
//             for (int i = currentActive + 1; i <= desiredQty; i++)
//             {
//                 existingType.Instances.Add(new SportsFieldInstanceEntity
//                 {
//                     Id = Guid.NewGuid(),
//                     DisplayName = $"{existingType.Type} №{i}",
//                     SportTypeId = existingType.Id,
//                     IsActive = true
//                 });
//             }
//         }
//         // Якщо менше — просто деактивуємо зайві (НЕ видаляємо!)
//         else if (desiredQty < currentActive)
//         {
//             var activeOnes = existingType.Instances.Where(i => i.IsActive).ToList();
//             for (int i = desiredQty; i < activeOnes.Count; i++)
//             {
//                 activeOnes[i].IsActive = false;   // ← Важливо!
//             }
//         }
//     }
// }
// //         public async Task UpdateAsync(UpdateSportsFieldDto dto)
// // {
// //     _logger.LogInformation("Оновлення майданчика ID: {Id}", dto.Id);
// //
// //     var existing = await _sportsFieldsRepository.GetByIdWithDetailsAsync(dto.Id);
// //     if (existing == null) throw new KeyNotFoundException($"SportsField {dto.Id} not found");
// //
// //     // Оновлюємо тільки те, що прийшло (без .Update сутності!)
// //     bool needsSave = false;
// //
// //     if (dto.Name != null && dto.Name != existing.Name)
// //     {
// //         existing.Name = dto.Name;
// //         needsSave = true;
// //     }
// //     if (dto.Description != null && dto.Description != existing.Description)
// //     {
// //         existing.Description = dto.Description;
// //         needsSave = true;
// //     }
// //     if (dto.ImageUrl != null && dto.ImageUrl != existing.ImageUrl)
// //     {
// //         existing.ImageUrl = dto.ImageUrl;
// //         needsSave = true;
// //     }
// //
// //     if (dto.Types != null)
// //     {
// //         var newTypeEntities = dto.Types.Select(t =>
// //         {
// //             var typeEntity = new SportsFieldSportTypeEntity
// //             {
// //                 Id = Guid.NewGuid(),
// //                 Type = t.Type,
// //                 PricePerHour = t.PricePerHour,
// //                 WarningInformation = t.WarningInformation ?? "",
// //                 WeeklySchedules = t.WeeklySchedules.Select(ws => new SportsFieldSchedule
// //                 {
// //                     Id = Guid.NewGuid(),
// //                     DayOfWeek = ws.DayOfWeek,
// //                     AvailableFrom = ws.AvailableFrom,
// //                     AvailableTo = ws.AvailableTo
// //                 }).ToList(),
// //                 Instances = t.Instances.Select(i => new SportsFieldInstanceEntity
// //                 {
// //                     Id = i.Id ?? Guid.NewGuid(),
// //                     DisplayName = i.DisplayName,
// //                     IsActive = true
// //                 }).ToList()
// //             };
// //             return typeEntity;
// //         }).ToList();
// //
// //         // Логіка quantity
// //         for (int idx = 0; idx < newTypeEntities.Count; idx++)
// //         {
// //             var typeDto = dto.Types[idx];
// //             var typeEnt = newTypeEntities[idx];
// //             if (typeDto.Quantity.HasValue && !typeDto.Instances?.Any() == true)
// //             {
// //                 typeEnt.Instances.Clear();
// //                 for (int i = 1; i <= typeDto.Quantity.Value; i++)
// //                 {
// //                     typeEnt.Instances.Add(new SportsFieldInstanceEntity
// //                     {
// //                         Id = Guid.NewGuid(),
// //                         DisplayName = $"{typeDto.Type} №{i}",
// //                         IsActive = true
// //                     });
// //                 }
// //             }
// //         }
// //
// //         await _sportsFieldsRepository.ReplaceTypesAndSchedulesAndInstancesAsync(existing.Id, newTypeEntities);
// //         needsSave = true; // бо ми змінили пов'язані дані
// //     }
// //
// //     // Зберігаємо ТІЛЬКИ якщо щось реально змінилося
// //     if (needsSave)
// //     {
// //         _logger.LogInformation("Перед збереженням змін: Name={Name}, Desc={Desc}, TypesCount={Count}",
// //             existing.Name, existing.Description, existing.TypesWithDetails?.Count ?? 0);
// //
// //         // Оновлюємо базові поля через репозиторій (він сам збереже)
// //         await _sportsFieldsRepository.UpdateBasicFieldsAsync(
// //             existing.Id,
// //             existing.Name,
// //             existing.Description,
// //             existing.ImageUrl
// //         );
// //     }
// //     else
// //     {
// //         _logger.LogInformation("Немає змін для збереження");
// //     }
// // }
//         
//         public async Task<SportsFieldsEntity?> GetByIdWithDetailsAsync(Guid id)
//         {
//             return await _sportsFieldsRepository.GetByIdWithDetailsAsync(id);
//         }
//         
//         public async Task<string> UpdateSportsFieldImageAsync(Guid id, IFormFile imageFile)
//         {
//             // Перевірки
//             if (!imageFile.ContentType.StartsWith("image/"))
//                 throw new ArgumentException("Тільки зображення дозволені (image/*)");
//
//             if (imageFile.Length > 5 * 1024 * 1024)
//                 throw new ArgumentException("Файл завеликий, максимум 5 МБ");
//
//             // Збереження файлу
//             var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
//             var filePath = Path.Combine("wwwroot", "images", "sportsfields", fileName);
//
//             Directory.CreateDirectory(Path.GetDirectoryName(filePath));
//
//             using (var stream = new FileStream(filePath, FileMode.Create))
//             {
//                 await imageFile.CopyToAsync(stream);
//             }
//
//             var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:44313";
//             var newUrl = $"{baseUrl}/images/sportsfields/{fileName}";
//
//             _logger.LogInformation("Збережено зображення: {NewUrl}", newUrl);
//
//             // Оновлюємо ТІЛЬКИ ImageUrl — без виклику повного UpdateAsync
//             await _sportsFieldsRepository.UpdateImageUrlAsync(id, newUrl);
//
//             return newUrl;
//         }
//
//         public async Task<bool> DeleteAsync(Guid sportFieldId)
//         {
//             // Повертаємо результат операції вище
//             return await _sportsFieldsRepository.Delete(sportFieldId);
//         }
//     }
// }
//
