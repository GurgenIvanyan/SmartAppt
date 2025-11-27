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

        public virtual async Task<ResponseBase> CancelBookingAsync(int customerId, int bookingId)
        {
            try
            {
                var booking = await BookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return new ResponseBase { StatusCode = 1, ErrorMessage = $"Booking with {bookingId} id not found" };
                }

                var customer = await CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    return new ResponseBase { StatusCode = 1, ErrorMessage = $"Customer {customerId} not found" };

                if (booking.CustomerId != customerId)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "You don't have permissions for this booking" };

                await BookingRepository.CancelAsync(bookingId);
                return new ResponseBase { StatusCode = 0 };
            }
            catch (Exception ex)
            {
                return new ResponseBase { StatusCode = 5, ErrorMessage = ex.Message };
            }

        }

        public virtual async Task<ResponseBase> CreateBookingAsync(int customerId, CreateBookingDto booking)
        {
            try
            {
               
                if (booking.StartAtUtc <= DateTime.UtcNow)
                {
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Cant book for previous date" };
                }

                booking.StartAtUtc = booking.StartAtUtc.AddMilliseconds(-booking.StartAtUtc.Millisecond);


                var customer = await CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    return new ResponseBase { StatusCode = 1, ErrorMessage = $"Customer with ID {customerId} not found" };

                var business = await BusinessRepository.GetByIdAsync(booking.BusinessId);
                if (business == null)
                    return new ResponseBase { StatusCode = 1, ErrorMessage = $"Business with ID {booking.BusinessId} not found" };

                var service = await ServiceRepository.GetByIdAsync(booking.ServiceId);
                if (service == null)
                    return new ResponseBase { StatusCode = 1, ErrorMessage = $"Service with ID {booking.ServiceId} not found" };

                 
                if (service.BusinessId != business.BusinessId)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Business doesn/t have that service" };

                if (!service.IsActive)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Service is not active" };

                
                var endAtUtc = booking.StartAtUtc.AddMinutes(service.DurationMin);

                
                var holiday = await HolidayRepository.GetByBusinessIdAsync(booking.BusinessId, booking.StartAtUtc.Date);
                if (holiday != null)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = $"It's a holiday {holiday.Reason}" };

                byte dow = (byte)(((int)booking.StartAtUtc.DayOfWeek + 6) % 7 + 1);  

                var hours = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(booking.BusinessId, dow);
                if (hours == null)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "This business has no opening hours for this day." };

                var openAt = booking.StartAtUtc.Date + hours.OpenTime; 
                var closeAt = booking.StartAtUtc.Date + hours.CloseTime;

                
                if (booking.StartAtUtc < openAt || endAtUtc > closeAt)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Business is not open at this time." };


                if ((booking.StartAtUtc.TimeOfDay - hours.OpenTime).TotalMinutes % service.DurationMin != 0)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Booking must start on service-aligned slot" };


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
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "You have already booking for that day" };


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
                        return new ResponseBase { StatusCode = 2, ErrorMessage = "Time overlaps with an existing booking." };
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
                    Status = booking.Status,
                    StatusCode = 0
                };
            }
            catch (Exception ex)
            {
                return new ResponseBase { StatusCode = 5, ErrorMessage = ex.Message };
            }
        }

        public virtual async Task<ResponseBase> DeleteBookingAsync(int customerId, int bookingId)
        {
            try
            {
                var booking = await BookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return new ResponseBase { StatusCode = 1, ErrorMessage = $"Booking with {bookingId} id not found" };
                }

                var customer = await CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    return new ResponseBase { StatusCode = 1, ErrorMessage = $"Customer {customerId} not found" };

                if (booking.CustomerId != customerId)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "You don't have permissions for this booking" };

                await BookingRepository.DeleteAsync(bookingId);
                return new ResponseBase { StatusCode = 0 };
            }
            catch (Exception ex)
            {
                return new ResponseBase { StatusCode = 5, ErrorMessage = ex.Message };
            }
        }

        public virtual async Task<ResponseBase> GetDailyFreeSlots(int businessId, int serviceId, DateOnly date)
        {
            try
            {

                var business = await BusinessRepository.GetByIdAsync(businessId);
                if (business == null)
                    return new ResponseBase { StatusCode = 1, ErrorMessage = $"Business with ID {businessId} not found" };

                var service = await ServiceRepository.GetByIdAsync(serviceId);
                if (service == null)
                    return new ResponseBase { StatusCode = 1, ErrorMessage = $"Service with ID {serviceId} not found" };

                if (service.BusinessId != businessId)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Business doesn't have that service" };

                if (!service.IsActive)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Service is not active" };

                var holiday = await HolidayRepository.GetByBusinessIdAsync(businessId, new DateTime(date.Year, date.Month, date.Day));
                if (holiday != null)
                {
                    return new DailySlotsDto
                    {
                        Date = date,
                        StatusCode = 2
                    };
                }

                byte dow = (byte)((((int)date.DayOfWeek) + 6) % 7 + 1);

                var hours = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(businessId, dow);
                if (hours == null)
                {
                    return new DailySlotsDto
                    {
                        Date = date,
                        StatusCode = 2
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
                    StatusCode = 0
                };
            }
            catch (Exception ex)
            {
                return new ResponseBase()
                {
                    StatusCode = 5,
                    ErrorMessage = ex.Message
                };
            }
        }

        public virtual async Task<ResponseBase> GetMonthlyCalendar(int businessId, int serviceId, int month, int year)
        {
            try
            {
                var business = await BusinessRepository.GetByIdAsync(businessId);
                if (business == null)
                    return new ResponseBase { StatusCode = 1, ErrorMessage = $"Business with ID {businessId} not found" };

                var service = await ServiceRepository.GetByIdAsync(serviceId);
                if (service == null)
                    return new ResponseBase { StatusCode = 1, ErrorMessage = $"Service with ID {serviceId} not found" };

                if (service.BusinessId != businessId)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Business doesn't have that service" };

                if (!service.IsActive)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Service is not active" };

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
                    StatusCode = 0
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
                return new ResponseBase
                {
                    StatusCode = 5,
                    ErrorMessage = ex.Message
                };
            }
        }

        public virtual async Task<ResponseBase> GetMyBookingsAsync(int customerId, int skip = 0, int take = 10)
        {
            try
            {
                if (skip < 0 || skip >= 100 || take <= 0 || take >= 100)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Invalid Pagination" };

                var bookings = await BookingRepository.GetAllSpecAsync(new BookingFilter
                {
                    CustomerId = customerId,
                    Skip = skip,
                    Take = take
                });

                return new BookingListDto { Bookings = bookings, StatusCode = 0 };
            }
            catch (Exception ex)
            {
                return new ResponseBase { StatusCode = 5, ErrorMessage = ex.Message };
            }
        }

        public virtual async Task<ResponseBase> UpdateBookingAsync(int customerId, int bookingId, UpdateBookingDto booking)
        {
            try
            {
                // check the date 
                if (booking.StartAtUtc <= DateTime.UtcNow)
                {
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Cant book for previous date" };
                }

              
                booking.StartAtUtc = booking.StartAtUtc.AddMilliseconds(-booking.StartAtUtc.Millisecond);

                var existing = await BookingRepository.GetByIdAsync(bookingId);
                if (existing == null)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = $"Booking with ID {bookingId} not found" };

                var customer = await CustomerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    return new ResponseBase { StatusCode = 1, ErrorMessage = $"Customer {customerId} not found" };

                if (existing.CustomerId != customerId)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "You don't have permissions for this booking" };

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
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Service not found for this booking" };

             
                var endAtUtc = proposedBooking.StartAtUtc.AddMinutes(service.DurationMin);

             
                var holiday = await HolidayRepository.GetByBusinessIdAsync(proposedBooking.BusinessId, proposedBooking.StartAtUtc.Date);
                if (holiday != null)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = $"It's a holiday: {holiday.Reason}" };

                byte dow = (byte)(((int)booking.StartAtUtc.DayOfWeek + 6) % 7 + 1);  
                var hours = await OpeningHoursRepository.GetByBusinessIdAndDowAsync(proposedBooking.BusinessId, dow);
                if (hours == null)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "No opening hours for this day." };

                var openAt = proposedBooking.StartAtUtc.Date + hours.OpenTime;
                var closeAt = proposedBooking.StartAtUtc.Date + hours.CloseTime;

                if (proposedBooking.StartAtUtc < openAt || endAtUtc > closeAt)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Business is not open at this time." };

                if ((proposedBooking.StartAtUtc.TimeOfDay - hours.OpenTime).TotalMinutes % service.DurationMin != 0)
                    return new ResponseBase { StatusCode = 2, ErrorMessage = "Booking must start on service-aligned slot" };

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
                        return new ResponseBase { StatusCode = 2, ErrorMessage = "Time overlaps with an existing booking." };
                }

                existing.CustomerId = customerId;
                existing.StartAtUtc = proposedBooking.StartAtUtc;
                existing.EndAtUtc = endAtUtc;
                existing.Notes = proposedBooking.Notes;

                await BookingRepository.UpdateAsync(existing);

                return new ResponseBase { StatusCode = 0 };
            }
            catch (Exception ex)
            {
                return new ResponseBase { StatusCode = 5, ErrorMessage = ex.Message };
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