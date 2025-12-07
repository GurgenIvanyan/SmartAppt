using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.SmartAppt.Models;
using Common.Logging;
using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services.Interfaces;
namespace Business.SmartAppt.Services.Implementation
{
    public class BusinessService : IBusinessService
    {
        // ---- Logger ----
        private static readonly ILogger _log =
            AppLoggerFactory.CreateLogger<BusinessService>();

        // ---- Repositories ----
        protected readonly IBookingRepository BookingRepository;
        protected readonly ICustomerRepository CustomerRepository;
        protected readonly IServiceRepository ServiceRepository;
        protected readonly IBusinessRepository BusinessRepository;
        protected readonly IOpeningHoursRepository OpeningHoursRepository;
        protected readonly IHolidayRepository HolidayRepository;

        public BusinessService(
            IBookingRepository bookingRepository,
            ICustomerRepository customerRepository,
            IServiceRepository serviceRepository,
            IBusinessRepository businessRepository,
            IOpeningHoursRepository openingHoursRepository,
            IHolidayRepository holidayRepository)
        {
            BookingRepository = bookingRepository;
            CustomerRepository = customerRepository;
            ServiceRepository = serviceRepository;
            BusinessRepository = businessRepository;
            OpeningHoursRepository = openingHoursRepository;
            HolidayRepository = holidayRepository;

            _log.Debug("BusinessService created");
        }

        public virtual async Task<BaseResponse> CreateBusinessAsync(BusinessDto dto)
        {
            _log.Info("CreateBusinessAsync started", dto);

            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    _log.Warn("CreateBusinessAsync validation failed: Name is empty", dto);

                    return new BusinessDto
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };
                }

                var entity = new BusinessEntity
                {
                    Name = dto.Name.Trim(),
                    Email = dto.Email,
                    Phone = dto.Phone,
                    TimeZone = dto.TimeZone,
                    SettingsJson = dto.SettingsJson
                };

                var id = await BusinessRepository.CreateAsync(entity);

                _log.Info("Business created successfully", new { BusinessId = id, entity.Name });

                return new BusinessDto
                {
                    BusinessId = id,
                    Name = entity.Name,
                    Email = entity.Email,
                    Phone = entity.Phone,
                    TimeZone = entity.TimeZone,
                    SettingsJson = entity.SettingsJson,
                    CreatedAtUtc = DateTime.UtcNow,
                    Status = BaseResponseStatus.Success,
                };
            }
            catch (Exception ex)
            {
                _log.Error("CreateBusinessAsync failed", ex, dto);

                return new BusinessDto
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        public virtual async Task<BaseResponse> UpdateBusinessByIdAsync(int id, BusinessDto dto)
        {
            _log.Info("UpdateBusinessByIdAsync started", new { id, dto });

            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    _log.Warn("UpdateBusinessByIdAsync validation failed: Name is empty", new { id, dto });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };
                }

                var existing = await BusinessRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    _log.Warn("UpdateBusinessByIdAsync: business not found", new { id });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness,
                    };
                }

                existing.Name = dto.Name.Trim();
                existing.Email = dto.Email;
                existing.Phone = dto.Phone;
                existing.TimeZone = dto.TimeZone;
                existing.SettingsJson = dto.SettingsJson;

                await BusinessRepository.UpdateAsync(existing);

                _log.Info("Business updated successfully", new { id });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.Success
                };
            }
            catch (Exception ex)
            {
                _log.Error("UpdateBusinessByIdAsync failed", ex, new { id, dto });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        public virtual async Task<BaseResponse> GetBusinessByIdAsync(int id)
        {
            _log.Debug("GetBusinessByIdAsync started", new { id });

            try
            {
                var entity = await BusinessRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    _log.Warn("GetBusinessByIdAsync: business not found", new { id });

                    return new BusinessDto { Status = BaseResponseStatus.InvalidBusiness };
                }

                _log.Debug("GetBusinessByIdAsync: business loaded", new { id });

                return new BusinessDto
                {
                    BusinessId = entity.BusinessId,
                    Name = entity.Name,
                    Email = entity.Email,
                    Phone = entity.Phone,
                    TimeZone = entity.TimeZone,
                    SettingsJson = entity.SettingsJson,
                    CreatedAtUtc = entity.CreatedAtUtc,
                    Status = BaseResponseStatus.Success,
                };
            }
            catch (Exception ex)
            {
                _log.Error("GetBusinessByIdAsync failed", ex, new { id });

                return new BusinessDto
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        public virtual async Task<BaseResponse> DeleteAsync(int businessId)
        {
            _log.Info("DeleteAsync started", new { businessId });

            try
            {
                var existing = await BusinessRepository.GetByIdAsync(businessId);
                if (existing == null)
                {
                    _log.Warn("DeleteAsync: business not found", new { businessId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness,
                    };
                }

                await BusinessRepository.DeleteAsync(businessId);

                _log.Info("Business deleted successfully", new { businessId });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.Success
                };
            }
            catch (Exception ex)
            {
                _log.Error("DeleteAsync failed", ex, new { businessId });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        public virtual async Task<BaseResponse> AddServiceAsync(int businessId, ServiceDto service)
        {
            _log.Info("AddServiceAsync started", new { businessId, service });

            try
            {
                if (businessId <= 0)
                {
                    _log.Warn("AddServiceAsync: invalid businessId", new { businessId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness,
                    };
                }

                if (service == null)
                {
                    _log.Warn("AddServiceAsync: service is null", new { businessId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };
                }

                if (string.IsNullOrWhiteSpace(service.Name))
                {
                    _log.Warn("AddServiceAsync validation failed: Name empty", new { businessId, service });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };
                }

                if (service.DurationMin <= 0)
                {
                    _log.Warn("AddServiceAsync validation failed: DurationMin <= 0", new { businessId, service });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };
                }

                if (service.Price < 0)
                {
                    _log.Warn("AddServiceAsync validation failed: Price < 0", new { businessId, service });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };
                }

                var business = await BusinessRepository.GetByIdAsync(businessId);
                if (business == null)
                {
                    _log.Warn("AddServiceAsync: business not found", new { businessId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness,
                    };
                }

                var entity = new ServiceEntity
                {
                    BusinessId = businessId,
                    Name = service.Name.Trim(),
                    DurationMin = service.DurationMin,
                    Price = service.Price,
                    IsActive = true
                };

                var id = await ServiceRepository.CreateAsync(entity);

                _log.Info("Service created successfully", new { businessId, ServiceId = id });

                return new ServiceDto
                {
                    ServiceId = id,
                    BusinessId = entity.BusinessId,
                    Name = entity.Name,
                    DurationMin = entity.DurationMin,
                    Price = entity.Price,
                    IsActive = entity.IsActive,
                    Status = BaseResponseStatus.Success,
                };
            }
            catch (Exception ex)
            {
                _log.Error("AddServiceAsync failed", ex, new { businessId, service });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        public virtual async Task<BaseResponse> DeleteService(int id)
        {
            _log.Info("DeleteService started", new { id });

            try
            {
                if (id <= 0)
                {
                    _log.Warn("DeleteService: invalid id", new { id });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidService
                    };
                }

                var existing = await ServiceRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    _log.Warn("DeleteService: service not found", new { id });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidService,
                    };
                }

                await ServiceRepository.DeleteAsync(id);

                _log.Info("Service deleted successfully", new { id });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.Success
                };
            }
            catch (Exception ex)
            {
                _log.Error("DeleteService failed", ex, new { id });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        public virtual async Task<BaseResponse> GetMyServicesAsync(int businessId, int skip = 0, int take = 10)
        {
            _log.Debug("GetMyServicesAsync started", new { businessId, skip, take });

            try
            {
                if (businessId <= 0)
                {
                    _log.Warn("GetMyServicesAsync: invalid businessId", new { businessId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness
                    };
                }
                var entity = await BusinessRepository.GetByIdAsync(businessId);
                if (entity == null)
                {
                    _log.Warn("GetMyServicesAsync: business not found", new { businessId });
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness
                    };
                }



                if (skip < 0 || skip >= 100 || take <= 0 || take >= 100)
                {
                    _log.Warn("GetMyServicesAsync: invalid paging", new { skip, take });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError
                    };
                }

                var all = await ServiceRepository.GetAllAsync(0, 100000);

                var services = all
                    .Where(s => s.BusinessId == businessId)
                    .Skip(skip)
                    .Take(take)
                    .ToList();

                _log.Debug("GetMyServicesAsync: services loaded",
                    new { businessId, Count = services.Count });

                return new ServiceListDto
                {
                    Services = services,
                    Status = BaseResponseStatus.Success
                };
            }
            catch (Exception ex)
            {
                _log.Error("GetMyServicesAsync failed", ex, new { businessId, skip, take });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        public virtual async Task<BaseResponse> DeactivateServiceAsync(int businessId, int serviceId)
        {
            _log.Info("DeactivateServiceAsync started", new { businessId, serviceId });

            try
            {
                if (businessId <= 0 || serviceId <= 0)
                {
                    _log.Warn("DeactivateServiceAsync: invalid ids", new { businessId, serviceId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };
                }

                var service = await ServiceRepository.GetByIdAsync(serviceId);
                if (service == null)
                {
                    _log.Warn("DeactivateServiceAsync: service not found", new { serviceId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidService
                    };
                }

                if (service.BusinessId != businessId)
                {
                    _log.Warn("DeactivateServiceAsync: service belongs to other business",
                        new { businessId, service.BusinessId, serviceId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError
                    };
                }

                if (!service.IsActive)
                {
                    _log.Warn("DeactivateServiceAsync: service already inactive",
                        new { businessId, serviceId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError
                    };
                }

                service.IsActive = false;
                await ServiceRepository.UpdateAsync(service);

                _log.Info("Service deactivated successfully", new { businessId, serviceId });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.Success
                };
            }
            catch (Exception ex)
            {
                _log.Error("DeactivateServiceAsync failed", ex, new { businessId, serviceId });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        public virtual async Task<BaseResponse> GetMonthlyCalendarAsync(
            int businessId,
            int serviceId,
            int month,
            int year)
        {
            _log.Info("GetMonthlyCalendarAsync started",
                new { businessId, serviceId, month, year });

            try
            {
                if (businessId <= 0)
                {
                    _log.Warn("GetMonthlyCalendarAsync: invalid businessId", new { businessId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness,
                    };
                }

                if (serviceId <= 0)
                {
                    _log.Warn("GetMonthlyCalendarAsync: invalid serviceId", new { serviceId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidService,
                    };
                }

                var business = await BusinessRepository.GetByIdAsync(businessId);
                if (business == null)
                {
                    _log.Warn("GetMonthlyCalendarAsync: business not found", new { businessId });

                    return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
                }

                var service = await ServiceRepository.GetByIdAsync(serviceId);
                if (service == null)
                {
                    _log.Warn("GetMonthlyCalendarAsync: service not found", new { serviceId });

                    return new BaseResponse { Status = BaseResponseStatus.InvalidService };
                }

                if (service.BusinessId != businessId)
                {
                    _log.Warn("GetMonthlyCalendarAsync: service belongs to other business",
                        new { businessId, service.BusinessId, serviceId });

                    return new BaseResponse { Status = BaseResponseStatus.ValidationError };
                }

                if (!service.IsActive)
                {
                    _log.Warn("GetMonthlyCalendarAsync: service inactive",
                        new { businessId, serviceId });

                    return new BaseResponse { Status = BaseResponseStatus.InvalidStatus };
                }

                var monthHolidays = await HolidayRepository.GetAllByMonthAsync(businessId, year, month);
                var holidayDates = new HashSet<DateTime>(monthHolidays.Select(h => h.HolidayDate.Date));

                var weekHours = await OpeningHoursRepository.GetByBusinessIdAsync(businessId);

                int daysInMonth = DateTime.DaysInMonth(year, month);
                var startDate = new DateOnly(year, month, 1);
                var endDate = startDate.AddDays(daysInMonth);

                var bookings = await BookingRepository.GetBookingsCountByBusinessAsync(
                    businessId, serviceId, startDate, endDate);

                var monthlyCalendar = new CalendarDto
                {
                    Month = month,
                    Year = year,
                    Status = BaseResponseStatus.Success,
                };

                for (int d = 1; d <= daysInMonth; d++)
                {
                    var current = new DateTime(year, month, d);
                    var dayAvailability = GetDayAvailability(
                        current,
                        holidayDates,
                        weekHours,
                        service.DurationMin,
                        bookings);

                    monthlyCalendar.Days.Add(dayAvailability);
                }

                _log.Debug("GetMonthlyCalendarAsync: calendar built",
                    new { businessId, serviceId, month, year, Days = monthlyCalendar.Days.Count });

                return monthlyCalendar;
            }
            catch (Exception ex)
            {
                _log.Error("GetMonthlyCalendarAsync failed",
                    ex, new { businessId, serviceId, month, year });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        private DayAvailability GetDayAvailability(
            DateTime current,
            HashSet<DateTime> holidays,
            IEnumerable<OpeningHoursEntity> weekHours,
            int durationMin,
            Dictionary<DateOnly, int> bookings)
        {
            // small helper – debug-level logging only if something special
            if (current.Date < DateTime.UtcNow.Date)
            {
                return new DayAvailability { Day = current.Day, IsOpen = false, HasFreeSlots = false };
            }

            if (holidays.Contains(current.Date))
            {
                return new DayAvailability { Day = current.Day, IsOpen = false, HasFreeSlots = false };
            }

            byte dow = (byte)(((int)current.DayOfWeek + 6) % 7 + 1);
            var hours = weekHours.FirstOrDefault(h => h.DayOfWeek == dow);

            if (hours == null)
            {
                return new DayAvailability { Day = current.Day, IsOpen = false, HasFreeSlots = false };
            }

            int openMinutes = (int)(hours.CloseTime - hours.OpenTime).TotalMinutes;
            int maxSlots = durationMin > 0 ? openMinutes / durationMin : 0;

            bookings.TryGetValue(DateOnly.FromDateTime(current), out int bookedCount);

            return new DayAvailability
            {
                Day = current.Day,
                IsOpen = true,
                HasFreeSlots = bookedCount < maxSlots
            };
        }

        public virtual async Task<BaseResponse> GetDailyBookingsAsync(
            int businessId,
            int serviceId,
            DateOnly date,
            int skip = 0,
            int take = 50)
        {
            _log.Info("GetDailyBookingsAsync started",
                new { businessId, serviceId, date, skip, take });

            try
            {
                if (businessId <= 0)
                {
                    _log.Warn("GetDailyBookingsAsync: invalid businessId", new { businessId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness
                    };
                }

                if (serviceId <= 0)
                {
                    _log.Warn("GetDailyBookingsAsync: invalid serviceId", new { serviceId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidService
                    };
                }

                if (skip < 0 || take <= 0 || take > 500)
                {
                    _log.Warn("GetDailyBookingsAsync: invalid paging", new { skip, take });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError
                    };
                }

                var business = await BusinessRepository.GetByIdAsync(businessId);
                if (business == null)
                {
                    _log.Warn("GetDailyBookingsAsync: business not found", new { businessId });

                    return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
                }

                var service = await ServiceRepository.GetByIdAsync(serviceId);
                if (service == null)
                {
                    _log.Warn("GetDailyBookingsAsync: service not found", new { serviceId });

                    return new BaseResponse { Status = BaseResponseStatus.InvalidService };
                }

                if (service.BusinessId != businessId)
                {
                    _log.Warn("GetDailyBookingsAsync: service belongs to other business",
                        new { businessId, service.BusinessId, serviceId });

                    return new BaseResponse { Status = BaseResponseStatus.ValidationError };
                }

                var dayDateTime = new DateTime(date.Year, date.Month, date.Day);

                var bookings = await BookingRepository.GetAllSpecAsync(new BookingFilter
                {
                    BusinessId = businessId,
                    ServiceId = serviceId,
                    Date = dayDateTime,
                    Skip = skip,
                    Take = take
                });

                var bookingList = bookings.ToList();

                if (bookingList.Count == 0)
                {
                    _log.Debug("GetDailyBookingsAsync: no bookings for this day",
                        new { businessId, serviceId, date });

                    return new DailyBookingsDto
                    {
                        Date = date,
                        Bookings = new List<BookingWithCustomerDto>(),
                        Status = BaseResponseStatus.Success,
                    };
                }

                var customerIds = bookingList
                    .Select(b => b.CustomerId)
                    .Distinct()
                    .ToList();

                var customersDict = new Dictionary<int, CustomerEntity>();

                foreach (var cid in customerIds)
                {
                    var c = await CustomerRepository.GetByIdAsync(cid);
                    if (c != null)
                    {
                        customersDict[cid] = c;
                    }
                }

                var resultItems = new List<BookingWithCustomerDto>();

                foreach (var b in bookingList)
                {
                    customersDict.TryGetValue(b.CustomerId, out var cust);

                    var customerDto = cust != null
                        ? new CustomerShortDto
                        {
                            CustomerId = cust.CustomerId,
                            FullName = cust.FullName,
                            Email = cust.Email,
                            Phone = cust.Phone
                        }
                        : new CustomerShortDto
                        {
                            CustomerId = b.CustomerId,
                            FullName = string.Empty
                        };

                    resultItems.Add(new BookingWithCustomerDto
                    {
                        BookingId = b.BookingId,
                        BusinessId = b.BusinessId,
                        ServiceId = b.ServiceId,
                        StartAtUtc = b.StartAtUtc,
                        EndAtUtc = b.EndAtUtc,
                        Status = b.Status,
                        Notes = b.Notes,
                        Customer = customerDto
                    });
                }

                _log.Debug("GetDailyBookingsAsync: bookings loaded",
                    new { businessId, serviceId, date, Count = resultItems.Count });

                return new DailyBookingsDto
                {
                    Date = date,
                    Bookings = resultItems,
                    Status = BaseResponseStatus.Success,
                };
            }
            catch (Exception ex)
            {
                _log.Error("GetDailyBookingsAsync failed",
                    ex, new { businessId, serviceId, date, skip, take });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError
                };
            }
        }

        public virtual async Task<BaseResponse> GetBookingsAsync(
            int businessId,
            int? serviceId = null,
            string? status = null,
            DateOnly? date = null,
            int skip = 0,
            int take = 50)
        {
            _log.Info("GetBookingsAsync started",
                new { businessId, serviceId, status, date, skip, take });

            try
            {
                if (businessId <= 0)
                {
                    _log.Warn("GetBookingsAsync: invalid businessId", new { businessId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness
                    };
                }

                if (skip < 0 || skip >= 1000 || take <= 0 || take > 1000)
                {
                    _log.Warn("GetBookingsAsync: invalid paging", new { skip, take });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError
                    };
                }

                var business = await BusinessRepository.GetByIdAsync(businessId);
                if (business == null)
                {
                    _log.Warn("GetBookingsAsync: business not found", new { businessId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness,
                    };
                }

                DateTime? filterDate = date.HasValue
                    ? date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                    : (DateTime?)null;

                var filter = new BookingFilter
                {
                    BusinessId = businessId,
                    ServiceId = serviceId,
                    Status = string.IsNullOrWhiteSpace(status) ? null : status,
                    Date = filterDate,
                    Skip = skip,
                    Take = take
                };

                var bookings = await BookingRepository.GetAllSpecAsync(filter);

                _log.Debug("GetBookingsAsync: bookings loaded",
                    new
                    {
                        businessId,
                        serviceId,
                        StatusFilter = status,
                        DateFilter = filterDate,
                        Count = bookings.Count()
                    });

                return new BookingListDto
                {
                    Bookings = bookings,
                    Status = BaseResponseStatus.Success,
                };
            }
            catch (Exception ex)
            {
                _log.Error("GetBookingsAsync failed",
                    ex, new { businessId, serviceId, status, date, skip, take });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        public async Task<BaseResponse> DecideBookingAsync(int businessId, int bookingId, bool confirm)
        {
            _log.Info("DecideBookingAsync started",
                new { businessId, bookingId, confirm });

            try
            {
                if (businessId <= 0 || bookingId <= 0)
                {
                    _log.Warn("DecideBookingAsync: invalid ids",
                        new { businessId, bookingId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };
                }

                var booking = await BookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    _log.Warn("DecideBookingAsync: booking not found",
                        new { bookingId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBooking
                    };
                }

                if (booking.BusinessId != businessId)
                {
                    _log.Warn("DecideBookingAsync: booking belongs to other business",
                        new { businessId, booking.BusinessId, bookingId });

                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError
                    };
                }

                if (confirm)
                {
                    if (booking.Status == "Confirmed")
                    {
                        _log.Debug("DecideBookingAsync: already confirmed",
                            new { bookingId });

                        return new BaseResponse { Status = BaseResponseStatus.Success };
                    }

                    if (booking.Status == "Cancelled")
                    {
                        _log.Debug("DecideBookingAsync: already cancelled, cannot confirm",
                            new { bookingId });

                        return new BaseResponse
                        {
                            Status = BaseResponseStatus.AlreadyCanceled
                        };
                    }

                    await BookingRepository.ConfirmAsync(bookingId);

                    _log.Info("DecideBookingAsync: booking confirmed",
                        new { bookingId });
                }
                else
                {
                    if (booking.Status == "Cancelled")
                    {
                        _log.Debug("DecideBookingAsync: already cancelled",
                            new { bookingId });

                        return new BaseResponse { Status = BaseResponseStatus.Success };
                    }

                    await BookingRepository.CancelAsync(bookingId);

                    _log.Info("DecideBookingAsync: booking cancelled",
                        new { bookingId });
                }

                return new BaseResponse
                {
                    Status = BaseResponseStatus.Success
                };
            }
            catch (Exception ex)
            {
                _log.Error("DecideBookingAsync failed",
                    ex, new { businessId, bookingId, confirm });

                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }
    }
}
