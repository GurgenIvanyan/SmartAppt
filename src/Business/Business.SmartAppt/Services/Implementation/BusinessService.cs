using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.SmartAppt.Models;
using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services.Implementation;
using Data.SmartAppt.SQL.Services.Interfaces;

namespace Business.SmartAppt.Services.Implementation
{
    public class BusinessService : IBusinessService
    {
        protected readonly IBookingRepository BookingRepository;
        protected readonly ICustomerRepository CustomerRepository;
        protected readonly IServiceRepository ServiceRepository;
        protected readonly IBusinessRepository BusinessRepository;
        protected readonly IOpeningHoursRepository OpeningHoursRepository;
        protected readonly IHolidayRepository HolidayRepository;

        public BusinessService(IBookingRepository bookingRepository, ICustomerRepository customerRepository, IServiceRepository serviceRepository, IBusinessRepository businessRepository, IOpeningHoursRepository openingHoursRepository, IHolidayRepository holidayRepository)
        {
            BookingRepository = bookingRepository;
            CustomerRepository = customerRepository;
            ServiceRepository = serviceRepository;
            BusinessRepository = businessRepository;
            OpeningHoursRepository = openingHoursRepository;
            HolidayRepository = holidayRepository;
        }
        public virtual async Task<BaseResponse> CreateBusinessAsync(BusinessDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
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
                return new BusinessDto
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        public virtual async Task<BaseResponse> UpdateBusinessByIdAsync(int id, BusinessDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };
                }

                var existing = await BusinessRepository.GetByIdAsync(id);
                if (existing == null)
                {
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

                return new BaseResponse
                {
                    Status = BaseResponseStatus.Success
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }
        public virtual async Task<BaseResponse> GetBusinessByIdAsync(int id)
        {
            try
            {
                var entity = await BusinessRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return new BusinessDto { Status = BaseResponseStatus.InvalidBusiness };
                }
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
                return new BusinessDto
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }
        public virtual async Task<BaseResponse> DeleteAsync(int businessId)
        {
            try
            {
                var existing = await BusinessRepository.GetByIdAsync(businessId);
                if (existing == null)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness,
                    };
                }

                await BusinessRepository.DeleteAsync(businessId);

                return new BaseResponse
                {
                    Status = BaseResponseStatus.Success
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }

        }
        public virtual async Task<BaseResponse> AddServiceAsync(int businessId, ServiceDto service)
        {
            try
            {
                if (businessId <= 0)
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness,
                    };

                if (service == null)
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };

                if (string.IsNullOrWhiteSpace(service.Name))
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };

                if (service.DurationMin <= 0)
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };

                if (service.Price < 0)
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };


                var business = await BusinessRepository.GetByIdAsync(businessId);
                if (business == null)
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness,
                    };

                var entity = new ServiceEntity
                {
                    BusinessId = businessId,
                    Name = service.Name.Trim(),
                    DurationMin = service.DurationMin,
                    Price = service.Price,
                    IsActive = true
                };

                var id = await ServiceRepository.CreateAsync(entity);

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
                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        public virtual async Task<BaseResponse> DeleteService(int id)
        {
            try
            {
                if (id <= 0)
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidService
                    };

                var existing = await ServiceRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidService,
                    };
                }

                await ServiceRepository.DeleteAsync(id);

                return new BaseResponse
                {
                    Status = BaseResponseStatus.Success
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        public virtual async Task<BaseResponse> GetMyServicesAsync(int businessId, int skip = 0, int take = 10)
        {
            try
            {
                if (businessId <= 0)
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness
                    };

                if (skip < 0 || skip >= 100 || take <= 0 || take >= 100)
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError
                    };


                var all = await ServiceRepository.GetAllAsync(0, 100000);

                var services = all
                    .Where(s => s.BusinessId == businessId)
                    .Skip(skip)
                    .Take(take)
                    .ToList();

                return new ServiceListDto
                {
                    Services = services,
                    Status = BaseResponseStatus.Success
                };
            }
            catch (Exception ex)
            {
            
                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };

            }
        }

        public virtual async Task<BaseResponse> DeactivateServiceAsync(int businessId, int serviceId)
        {
            try
            {
                if (businessId <= 0 || serviceId <= 0)
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };

                var service = await ServiceRepository.GetByIdAsync(serviceId);
                if (service == null)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidService
                    };
                }

                if (service.BusinessId != businessId)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError
                    };
                }

                if (!service.IsActive)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError
                    };
                }

                service.IsActive = false;
                await ServiceRepository.UpdateAsync(service);

                return new BaseResponse
                {
                    Status = BaseResponseStatus.Success
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }
        public virtual async Task<BaseResponse> GetMonthlyCalendarAsync(int businessId, int serviceId, int month, int year)
        {
            try
            {

                if (businessId <= 0)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness,

                    };
                }

                if (serviceId <= 0)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidService,

                    };
                }


                var business = await BusinessRepository.GetByIdAsync(businessId);
                if (business == null)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };


                var service = await ServiceRepository.GetByIdAsync(serviceId);
                if (service == null)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidService };

                if (service.BusinessId != businessId)
                    return new BaseResponse { Status = BaseResponseStatus.ValidationError };

                if (!service.IsActive)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidStatus };

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
                    var dayAvailability = GetDayAvailability(current, holidayDates, weekHours, service.DurationMin, bookings);
                    monthlyCalendar.Days.Add(dayAvailability);
                }

                return monthlyCalendar;
            }
            catch (Exception ex)
            {
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

            if (current.Date < DateTime.UtcNow.Date)
                return new DayAvailability { Day = current.Day, IsOpen = false, HasFreeSlots = false };


            if (holidays.Contains(current.Date))
                return new DayAvailability { Day = current.Day, IsOpen = false, HasFreeSlots = false };


            byte dow = (byte)(((int)current.DayOfWeek + 6) % 7 + 1);
            var hours = weekHours.FirstOrDefault(h => h.DayOfWeek == dow);

            if (hours == null)
                return new DayAvailability { Day = current.Day, IsOpen = false, HasFreeSlots = false };

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

        public virtual async Task<BaseResponse> GetDailyBookingsAsync(int businessId, int serviceId, DateOnly date, int skip = 0, int take = 50)
        {
            try
            {
                if (businessId <= 0)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness
                    };
                }
                if (serviceId <= 0)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidService
                    };
                }

                if (skip < 0 || take <= 0 || take > 500)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError
                    };
                }

                var business = await BusinessRepository.GetByIdAsync(businessId);
                if (business == null)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

                var service = await ServiceRepository.GetByIdAsync(serviceId);
                if (service == null)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidService };

                if (service.BusinessId != businessId)
                    return new BaseResponse { Status = BaseResponseStatus.ValidationError };

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

                return new DailyBookingsDto
                {
                    Date = date,
                    Bookings = resultItems,
                    Status = BaseResponseStatus.Success,
                };
            }
            catch (Exception ex)
            {
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
            try
            {
                if (businessId <= 0)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBusiness
                    };
                }

                if (skip < 0 || skip >= 1000 || take <= 0 || take > 1000)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError
                    };
                }


                var business = await BusinessRepository.GetByIdAsync(businessId);
                if (business == null)
                {
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

                return new BookingListDto
                {
                    Bookings = bookings,
                    Status = BaseResponseStatus.Success,
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }
        public async Task<BaseResponse> DecideBookingAsync(int businessId, int bookingId, bool confirm)
        {
            try
            {
                if (businessId <= 0 || bookingId <= 0)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError,
                    };
                }

                var booking = await BookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.InvalidBooking
                    };
                }

                if (booking.BusinessId != businessId)
                {
                    return new BaseResponse
                    {
                        Status = BaseResponseStatus.ValidationError
                    };
                }


                if (confirm)
                {
                    if (booking.Status == "Confirmed")
                    {

                        return new BaseResponse { Status = BaseResponseStatus.Success };
                    }

                    if (booking.Status == "Cancelled")
                    {
                        return new BaseResponse
                        {
                            Status = BaseResponseStatus.AlreadyCanceled
                        };
                    }

                    await BookingRepository.ConfirmAsync(bookingId);
                }
                else
                {
                    if (booking.Status == "Cancelled")
                    {
                        return new BaseResponse { Status = BaseResponseStatus.Success };
                    }


                    await BookingRepository.CancelAsync(bookingId);
                }

                return new BaseResponse
                {
                    Status = BaseResponseStatus.Success
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }
    }
}

