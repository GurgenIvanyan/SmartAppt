using Business.SmartAppt.Models;
using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services.Interfaces;
using Data.SmartAppt.SQL.Services.Implementation;

namespace Business.SmartAppt.Services.Implementation
{
    public class CustomerService : ICustomerService
    {
        protected readonly IBookingRepository BookingRepository;
        protected readonly ICustomerRepository CustomerRepository;
        protected readonly IServiceRepository ServiceRepository;
        protected readonly IBusinessRepository BusinessRepository;
        protected readonly IOpeningHoursRepository OpeningHoursRepository;
        protected readonly IHolidayRepository HolidayRepository;

        public CustomerService(IBookingRepository bookingRepository, ICustomerRepository customerRepository, IServiceRepository serviceRepository, IBusinessRepository businessRepository, IOpeningHoursRepository openingHoursRepository, IHolidayRepository holidayRepository)
        {
            BookingRepository = bookingRepository;
            CustomerRepository = customerRepository;
            ServiceRepository = serviceRepository;
            BusinessRepository = businessRepository;
            OpeningHoursRepository = openingHoursRepository;
            HolidayRepository = holidayRepository;
        }

        public virtual async Task<BaseResponse> CancelBookingAsync(int customerId, int bookingId)
        {
            try
            {
                var booking = await BookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return new BaseResponse { Status = BaseResponseStatus.NotFound};
                }

                var customer = await CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    return new BaseResponse { Status = BaseResponseStatus.NotFound };

                if (booking.CustomerId != customerId)
                    return new BaseResponse { Status = BaseResponseStatus.ValidationError };

                await BookingRepository.CancelAsync(bookingId);
                return new BaseResponse { Status = BaseResponseStatus.Success};
            }
            catch (Exception ex)
            {
                return new BaseResponse { Status = BaseResponseStatus.UnknownError };
            }

        }

        public virtual async Task<BaseResponse> CreateBookingAsync(int customerId, CreateBookingDto booking)
        {
            try
            {
               
                if (booking.StartAtUtc <= DateTime.UtcNow)
                {
                    return new BaseResponse { Status = BaseResponseStatus.ValidationError};
                }

                booking.StartAtUtc = booking.StartAtUtc.AddMilliseconds(-booking.StartAtUtc.Millisecond);


                var customer = await CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };

                var business = await BusinessRepository.GetByIdAsync(booking.BusinessId);
                if (business == null)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

                var service = await ServiceRepository.GetByIdAsync(booking.ServiceId);
                if (service == null)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidService };

                 
                if (service.BusinessId != business.BusinessId)
                    return new BaseResponse { Status = BaseResponseStatus.ValidationError };

                if (!service.IsActive)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidStatus };

                
                var endAtUtc = booking.StartAtUtc.AddMinutes(service.DurationMin);

                
                var holiday = await HolidayRepository.GetByBusinessIdAsync(booking.BusinessId, booking.StartAtUtc.Date);
                if (holiday != null)
                    return new BaseResponse { Status = BaseResponseStatus.Holiday };

                byte dow = (byte)(((int)booking.StartAtUtc.DayOfWeek + 6) % 7 + 1);  

                var hours = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(booking.BusinessId, dow);
                if (hours == null)
                    return new BaseResponse { Status = BaseResponseStatus.NoWorkingHours };

                var openAt = booking.StartAtUtc.Date + hours.OpenTime; 
                var closeAt = booking.StartAtUtc.Date + hours.CloseTime;

                
                if (booking.StartAtUtc < openAt || endAtUtc > closeAt)
                    return new BaseResponse { Status = BaseResponseStatus.ValidationError };


                if ((booking.StartAtUtc.TimeOfDay - hours.OpenTime).TotalMinutes % service.DurationMin != 0)
                    return new BaseResponse { Status = BaseResponseStatus.ValidationError};


                var clientBooking = await BookingRepository.GetAllSpecAsync
                (new BookingFilter
                {
                    BusinessId = booking.BusinessId,
                    ServiceId = booking.ServiceId,
                    CustomerId = customerId,
                    Date = booking.StartAtUtc.Date
                });

                int count = clientBooking.Count();
                if (count > 0)
                    return new BaseResponse { Status = BaseResponseStatus.AlreadyExists };


                var existing = await BookingRepository.GetAllSpecAsync
                (new BookingFilter
                {
                    BusinessId = booking.BusinessId,
                    ServiceId = booking.ServiceId,
                    Status = "Confirmed",
                    Date = booking.StartAtUtc.Date
                });

                foreach (var b in existing)
                {
                    if (b.StartAtUtc == booking.StartAtUtc)
                        return new BaseResponse { Status = BaseResponseStatus.AlreadyExists };
                }

                var entity = new BookingEntity
                {
                    BusinessId = booking.BusinessId,
                    ServiceId = booking.ServiceId,
                    CustomerId = customerId,
                    Status = booking.Status,
                    Notes = booking.Notes,
                    StartAtUtc = booking.StartAtUtc,
                    EndAtUtc = endAtUtc
                };

                int id = await BookingRepository.CreateAsync(entity);

                return new BookingDto
                {
                    BookingId = id,
                    BusinessId = booking.BusinessId,
                    ServiceId = booking.ServiceId,
                    CustomerId = customerId,
                    Notes = booking.Notes,
                    StartAtUtc = booking.StartAtUtc,
                    EndAtUtc = endAtUtc,
           
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse { Status = BaseResponseStatus.UnknownError};
            }
        }

        public virtual async Task<BaseResponse> DeleteBookingAsync(int customerId, int bookingId)
        {
            try
            {
                var booking = await BookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return new BaseResponse { Status = BaseResponseStatus.InvalidBooking };
                }

                var customer = await CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer};

                if (booking.CustomerId != customerId)
                    return new BaseResponse { Status = BaseResponseStatus.ValidationError };

                await BookingRepository.DeleteAsync(bookingId);
                return new BaseResponse { Status = BaseResponseStatus.Success };
            }
            catch (Exception ex)
            {
                return new BaseResponse { Status = BaseResponseStatus.UnknownError};
            }
        }

        public virtual async Task<BaseResponse> GetDailyFreeSlots(int businessId, int serviceId, DateOnly date)
        {
            try
            {

                var business = await BusinessRepository.GetByIdAsync(businessId);
                if (business == null)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

                var service = await ServiceRepository.GetByIdAsync(serviceId);
                if (service == null)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidService};

                if (service.BusinessId != businessId)
                    return new BaseResponse { Status = BaseResponseStatus.ValidationError};

                if (!service.IsActive)
                    return new BaseResponse { Status = BaseResponseStatus.ValidationError};

                var holiday = await HolidayRepository.GetByBusinessIdAsync(businessId, new DateTime(date.Year, date.Month, date.Day));
                if (holiday != null)
                {
                    return new DailySlotsDto
                    {
                        Date = date,
                        Status = BaseResponseStatus.Busy,
                    };
                }

                byte dow = (byte)((((int)date.DayOfWeek) + 6) % 7 + 1);

                var hours = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(businessId, dow);
                if (hours == null)
                {
                    return new DailySlotsDto
                    {
                        Date = date,
                        Status = BaseResponseStatus.Busy,
                    };
                }

                var bookings = await BookingRepository.GetAllSpecAsync(new BookingFilter
                {
                    BusinessId = businessId,
                    ServiceId = serviceId,
                    Status = "Confirmed",
                    Date = new DateTime(date.Year, date.Month, date.Day)
                });


            
                List<TimeSpan> freeSlots = new List<TimeSpan>();
                TimeSpan slotTime = hours.OpenTime;
                var duration = TimeSpan.FromMinutes(service.DurationMin);

                var bookedSlots = new HashSet<TimeSpan>(
                bookings.Select(b => b.StartAtUtc.TimeOfDay)
                );

                while (slotTime + duration <= hours.CloseTime)
                {
                    if (!bookedSlots.Contains(slotTime))
                        freeSlots.Add(slotTime);

                    slotTime += duration;
                }

                return new DailySlotsDto
                {
                    Date = date,
                    FreeSlots = freeSlots,
                    Status = BaseResponseStatus.Success
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse()
                {
                    Status = BaseResponseStatus.UnknownError,
                };
            }
        }

        public virtual async Task<BaseResponse> GetMonthlyCalendar(int businessId, int serviceId, int month, int year)
        {
            try
            {
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
                var bookings = await BookingRepository.GetBookingsCountByBusinessAsync(businessId, serviceId, startDate, endDate);

                var monthlyCalendar = new CalendarDto
                {
                    Month = month,
                    Year = year,
                    Status = BaseResponseStatus.Success,
                };

                for (int d = 1; d <= daysInMonth; d++)
                {
                    var date = new DateTime(year, month, d);
                    var dayAvailability = GetDayAvailability(date, holidayDates, weekHours, service.DurationMin, bookings);

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

        public virtual async Task<BaseResponse> GetMyBookingsAsync(int customerId, int skip = 0, int take = 10)
        {
            try
            {
                if (skip < 0 || skip >= 100 || take <= 0 || take >= 100)
                    return new BaseResponse { Status = BaseResponseStatus.ValidationError};

                var bookings = await BookingRepository.GetAllSpecAsync(new BookingFilter
                {
                    CustomerId = customerId,
                    Skip = skip,
                    Take = take
                });

                return new BookingListDto { Bookings = bookings, Status = BaseResponseStatus.Success};
            }
            catch (Exception ex)
            {
                return new BaseResponse { Status = BaseResponseStatus.UnknownError };
            }
        }

        public virtual async Task<BaseResponse> UpdateBookingAsync(int customerId, int bookingId, UpdateBookingDto booking)
        {
            try
            {
                // check the date 
                if (booking.StartAtUtc <= DateTime.UtcNow)
                {
                    return new BaseResponse {Status = BaseResponseStatus.ValidationError };
                }

              
                booking.StartAtUtc = booking.StartAtUtc.AddMilliseconds(-booking.StartAtUtc.Millisecond);

                var existing = await BookingRepository.GetByIdAsync(bookingId);
                if (existing == null)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidBooking };

                var customer = await CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };

                if (existing.CustomerId != customerId)
                    return new BaseResponse {Status = BaseResponseStatus.ValidationError };

                // new booking
                var proposedBooking = new CreateBookingDto
                {
                    BusinessId = existing.BusinessId,
                    ServiceId = existing.ServiceId,
                    StartAtUtc = booking.StartAtUtc,
                    Notes = booking.Notes ?? existing.Notes,
                };

                var service = await ServiceRepository.GetByIdAsync(existing.ServiceId);
                if (service == null)
                    return new BaseResponse {Status = BaseResponseStatus.InvalidService };

             
                var endAtUtc = proposedBooking.StartAtUtc.AddMinutes(service.DurationMin);

             
                var holiday = await HolidayRepository.GetByBusinessIdAsync(proposedBooking.BusinessId, proposedBooking.StartAtUtc.Date);
                if (holiday != null)
                    return new BaseResponse { Status = BaseResponseStatus.Holiday };

                byte dow = (byte)(((int)booking.StartAtUtc.DayOfWeek + 6) % 7 + 1);  
                var hours = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(proposedBooking.BusinessId, dow);
                if (hours == null)
                    return new BaseResponse {Status = BaseResponseStatus.NoWorkingHours };

                var openAt = proposedBooking.StartAtUtc.Date + hours.OpenTime;
                var closeAt = proposedBooking.StartAtUtc.Date + hours.CloseTime;

                if (proposedBooking.StartAtUtc < openAt || endAtUtc > closeAt)
                    return new BaseResponse {Status = BaseResponseStatus.NoWorkingHours };

                if ((proposedBooking.StartAtUtc.TimeOfDay - hours.OpenTime).TotalMinutes % service.DurationMin != 0)
                    return new BaseResponse {Status = BaseResponseStatus.ValidationError };

                var existingBookings = await BookingRepository.GetAllSpecAsync(new BookingFilter
                {
                    BusinessId = proposedBooking.BusinessId,
                    ServiceId = proposedBooking.ServiceId,
                    Status = "Confirmed",
                    Date = proposedBooking.StartAtUtc.Date
                });

                foreach (var b in existingBookings)
                {
                    if (b.BookingId == bookingId) continue; 
                    if (b.StartAtUtc == proposedBooking.StartAtUtc)
                        return new BaseResponse {Status = BaseResponseStatus.AlreadyExists };
                }

                existing.CustomerId = customerId;
                existing.StartAtUtc = proposedBooking.StartAtUtc;
                existing.EndAtUtc = endAtUtc;
                existing.Notes = proposedBooking.Notes;

                await BookingRepository.UpdateAsync(existing);

                return new BaseResponse { Status = BaseResponseStatus.Success };
            }
            catch (Exception ex)
            {
                return new BaseResponse { Status = BaseResponseStatus.UnknownError};
            }
        }

        private DayAvailability GetDayAvailability(DateTime current, HashSet<DateTime> holidays, IEnumerable<OpeningHoursEntity> weekHours, int durationMin, Dictionary<DateOnly, int> bookings)
        {
          
            if (current < DateTime.UtcNow.Date)
                return new DayAvailability { Day = current.Day, IsOpen = false, HasFreeSlots = false };

          
            if (holidays.Contains(current.Date))
                return new DayAvailability { Day = current.Day, IsOpen = false, HasFreeSlots = false };

           
            byte dow = (byte)(((int)current.DayOfWeek + 6) % 7 + 1);
            var hours = weekHours.FirstOrDefault(h => h.DayOfWeek == dow);

            if (hours == null)
                return new DayAvailability { Day = current.Day, IsOpen = false, HasFreeSlots = false };

            int openMinutes = (int)(hours.CloseTime - hours.OpenTime).TotalMinutes;
            int maxSlots = openMinutes / durationMin;

            bookings.TryGetValue(DateOnly.FromDateTime(current), out int bookedCount);

            return new DayAvailability
            {
                Day = current.Day,
                IsOpen = true,
                HasFreeSlots = bookedCount < maxSlots
            };
        }


    }
}