using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Authorization;
using IronPdf;
using HRMS.Helpers;

namespace HRMS.Controllers
{
    [Authorize(Roles = "Admin,Manager,Receptionist")]
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rooms
        public IActionResult Index()
        {

            var rooms = _context.Room.Include("RoomType").OrderBy(r => r.RoomType).ToList();
            return View(rooms);
        }
        public async Task<IActionResult> IndexAll()
        {
            var rooms = _context.Room.Include("RoomType")
                            .OrderBy(r => r.RoomType)
                            .ThenBy(r => r.RoomNumber).ToList();
            return View(rooms);

            /*
            List<String?> RoomTypeNames = _context.RoomType.Select(x => x.Name).ToList();
            int count = 0;
            foreach(var roomType in RoomTypeNames)
            {                
                ViewData[count.ToString()] = roomType;
                count++;
            }
            ViewData["Counter"] = count;
            return View(await _context.Room.Where(s => s.RoomStatus == RoomStatus.Available).Include(s => s.RoomType).ToListAsync());
            */
        }

        public async Task<IActionResult> IndexAvailable()
        {
            List<String?> RoomTypeNames = _context.RoomType.Select(x => x.Name).ToList();
            int count = 0;
            foreach (var roomType in RoomTypeNames)
            {
                ViewData[count.ToString()] = roomType;
                count++;
            }
            ViewData["Counter"] = count;
            return View(await _context.Room.Where(s => s.RoomStatus == RoomStatus.Available).Include(s => s.RoomType).ToListAsync());
        }

        public async Task<IActionResult> IndexReserved()
        {

            List<Booking> bookings = await _context.Booking
                                        .Where(b => b.BookingStatus == BookingStatus.Booked
                                                    && b.From.Date.Equals(Utility.GetMMTimeNow().Date))
                                        .ToListAsync();

            List<Room> RoomsReservedForToday = new List<Room>();

            foreach (Booking booking in bookings)
            {
                //DateTime Now = DateTime.Now;
                //if (booking.From.AddMinutes(30) >= Now)
                //{

                Room room = await _context.Room
                                .Where(r => r.Id == booking.RoomId)
                                .Include(r => r.RoomType)
                                .Include(r => r.Guest)
                                .FirstAsync();

                if (booking.RoomStatusUpdate == false)
                {

                    room.RoomStatus = RoomStatus.Reserved;
                    room.GuestId = booking.GuestId;
                    _context.Room.Update(room);

                    booking.RoomStatusUpdate = true;
                    _context.Booking.Update(booking);

                    await _context.SaveChangesAsync();
                }

                room.CurrentBookedFrom = booking.From.Date;
                room.CurrentBookedTo = booking.To.Date;

                RoomsReservedForToday.Add(room);
                //}
            }

            //List<String?> RoomTypeNames = _context.RoomType.Select(x => x.Name).ToList();
            //int count = 0;
            //foreach (var roomType in RoomTypeNames)
            //{
            //    ViewData[count.ToString()] = roomType;
            //    count++;
            //}
            //ViewData["Counter"] = count;
            //return View(await _context.Room.Where(s => s.RoomStatus == RoomStatus.Reserved || s.RoomStatus == RoomStatus.ReservedHourly).Include(t => t.RoomType).Include(t => t.Guest).ToListAsync());

            return View(RoomsReservedForToday);
        }

        public async Task<IActionResult> IndexCheckedIn()
        {
            //List<String?> RoomTypeNames = _context.RoomType.Select(x => x.Name).ToList();
            //int count = 0;
            //foreach (var roomType in RoomTypeNames)
            //{
            //    ViewData[count.ToString()] = roomType;
            //    count++;
            //}
            //ViewData["Counter"] = count;
            var rooms = await _context.Room
                        .Where(r => r.RoomStatus == RoomStatus.Occupied)
                        .Include(r => r.RoomType)
                        .Include(r => r.Guest)
                        .ToListAsync();

            foreach (var room in rooms)
            {
                // get booked dates
                var booking = await _context.Booking
                                .Where(b => b.RoomId == room.Id
                                        && b.BookingStatus == BookingStatus.CheckedIn
                                ).FirstOrDefaultAsync();
                room.CurrentBookedFrom = booking!.From;
                room.CurrentBookedTo = booking!.To;

                // get checkin customer
                var checkedInCustomer = await _context.CheckedInCustomer
                                        .Where(c => c.RoomId == room.Id)
                                        .Include(c => c.Guest)
                                        .FirstOrDefaultAsync();

                room.CheckedInCustomer = checkedInCustomer;
            }

            return View(rooms);
        }

        public ApplicationDbContext Get_context()
        {
            return _context;
        }

        public async Task<IActionResult> IndexAvailableRoomType(string id)
        {
            List<String?> RoomTypeNames = _context.RoomType.Select(x => x.Name).ToList();
            int count = 0;
            foreach (var roomType in RoomTypeNames)
            {
                ViewData[count.ToString()] = roomType;
                count++;
            }
            ViewData["Counter"] = count;
            int roomTypeId = (_context.RoomType.First(x => x.Name == id)).Id;
            return View(await _context.Room.Where(y => y.RoomTypeId == roomTypeId).Where(s => s.RoomStatus == RoomStatus.Available).ToListAsync());
        }

        public async Task<IActionResult> IndexReservedRoomType(string id)
        {
            List<String?> RoomTypeNames = await _context.RoomType.Select(x => x.Name).ToListAsync();
            int count = 0;
            foreach (var roomType in RoomTypeNames)
            {
                ViewData[count.ToString()] = roomType;
                count++;
            }
            ViewData["Counter"] = count;
            int roomTypeId = (_context.RoomType.First(x => x.Name == id)).Id;
            return View(await _context.Room.Where(y => y.RoomTypeId == roomTypeId).Where(s => s.RoomStatus == RoomStatus.Reserved || s.RoomStatus == RoomStatus.ReservedHourly).ToListAsync());
        }

        public async Task<IActionResult> IndexCheckedInRoomType(string id)
        {
            List<String?> RoomTypeNames = _context.RoomType.Select(x => x.Name).ToList();
            int count = 0;
            foreach (var roomType in RoomTypeNames)
            {
                ViewData[count.ToString()] = roomType;
                count++;
            }
            ViewData["Counter"] = count;
            int roomTypeId = (_context.RoomType.First(x => x.Name == id)).Id;
            return View(await _context.Room.Where(y => y.RoomTypeId == roomTypeId).Where(s => s.RoomStatus == RoomStatus.Occupied).ToListAsync());
        }
        public async Task<IActionResult> WalkIn()
        {
            var roomTypes = await _context.RoomType.ToListAsync();
            ViewData["RoomTypeId"] = new SelectList(roomTypes, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> WalkInFindRooms(IFormCollection form, List<Guest> guests, List<CheckedInCustomer> guestsCheckedIn)
        {
            guestsCheckedIn = _context.CheckedInCustomer.Include(s => s.Guest).ToList();
            guests = _context.Guest.ToList();
            if (guestsCheckedIn.Any())
            {
                for (int i = 0; i < guestsCheckedIn.Count; i++)
                {
                    guests.Remove(guestsCheckedIn[i].Guest);
                }
            }
            ViewData["GuestId"] = new SelectList(guests, "Id", "Name");


            var roomType = _context.RoomType.Where(s => s.Id == int.Parse(form["roomTypeId"])).First();

            ViewData["Price"] = roomType.Price;

            ViewData["RoomTypeId"] = roomType.Id;

            string toDate = form["CheckedOutDate"];

            string toTime = form["CheckedOutHour"] + ":" + form["CheckedOutMinute"];

            DateTime fromDateTime = DateTime.Now;

            var toSplit = toDate.Split('-');
            int toYear = int.Parse(toSplit[0]);
            int toMonth = int.Parse(toSplit[1]);
            int toDay = int.Parse(toSplit[2]);

            DateTime toDateTime = new(toYear, toMonth, toDay, int.Parse(form["CheckedOutHour"]), int.Parse(form["CheckedOutMinute"]), 0);

            var to = toDate + ":" + toTime;
            var roomsBooked = new List<Booking>();
            if (fromDateTime.ToLongDateString() != toDateTime.ToLongDateString())
            {
                roomsBooked = await _context.Booking.Where(s => s.Room.RoomTypeId == roomType.Id).
                     Where(s => s.BookingStatus != BookingStatus.Cancelled).
                     Where(s => s.BookingStatus != BookingStatus.Finished).
                     Where(s => s.BookingStatus != BookingStatus.BookedService).
                     Where(s => fromDateTime <= s.To || toDateTime <= s.From).ToListAsync();
            }
            else
            {
                roomsBooked = await _context.Booking.Where(s => s.Room.RoomTypeId == roomType.Id).
                    Where(s => s.BookingStatus != BookingStatus.Cancelled).
                    Where(s => s.BookingStatus != BookingStatus.Finished).
                    Where(s => s.BookingStatus != BookingStatus.BookedService).
                    Where(s => fromDateTime >= s.From && fromDateTime < s.To).ToListAsync();
                if (roomsBooked.Any())
                {
                    foreach (var roomBooked in roomsBooked)
                    {
                        if (fromDateTime <= roomBooked.From)
                        {
                            roomsBooked.Remove(roomBooked);
                        }
                    }
                }
            }
            List<Room> rooms = await _context.Room.Where(s => s.RoomType.Id == roomType.Id).Include(t => t.RoomType).ToListAsync();
            if (roomsBooked.Any())
            {
                foreach (var roomBooked in roomsBooked)
                {
                    rooms.Remove(rooms.First(s => s.Id == roomBooked.RoomId));
                }
            }
            if (!rooms.Any())
            {
                TempData["error"] = "Sorry No Room available";
                return RedirectToAction(nameof(WalkIn));
            }
            ViewData["FromDateTime"] = fromDateTime;
            ViewData["ToDateTime"] = toDateTime;
            return View(rooms);
        }

        //public async Task<IActionResult> CheckIn(string id, List<Guest> guests, List<CheckedInCustomer> guestsCheckedIn)
        public async Task<IActionResult> CheckIn(string id)
        {
            var guestsCheckedIn = _context.CheckedInCustomer.Include(s => s.Guest).ToList();
            var guests = _context.Guest.ToList();
            if (guestsCheckedIn.Any())
            {
                for (int i = 0; i < guestsCheckedIn.Count; i++)
                {
                    guests.Remove(guestsCheckedIn[i].Guest);
                }
            }



            ViewData["GuestId"] = new SelectList(_context.Guest.Select(
                                                    g => new
                                                    {
                                                        Id = g.Id,
                                                        Name = g.Name + " - " + g.NRC
                                                    }).OrderBy(gg => gg.Name),
                                                    "Id", "Name");
            ViewData["RoomId"] = id;
            var room = await _context.Room.Where(s => s.Id == int.Parse(id)).Include(s => s.RoomType).FirstAsync();
            int roomPrice = 0;
            if (room.RoomStatus == RoomStatus.Reserved)
            {
                roomPrice = room.RoomType.Price;
            }
            else
            {
                roomPrice = room.RoomType.HourlyPrice;
            }

            ViewData["Price"] = roomPrice;
            ViewBag.RoomName = $"{room.RoomType.Name} - {room.RoomNumber}";
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(string id, IFormCollection form)
        {
            int roomId = int.Parse(form["RoomId"]);
            //DateTime datetime = DateTime.Now.AddMinutes(30);
            DateTime datetime = DateTime.Now;
            DateTime mmDateTime = Utility.GetMMTimeNow();

            // to save checkin customer
            List<int> guestIdList = new();
            for (int i = 1; i <= 1; i++)
            {
                string guestId = "Guest" + i.ToString();
                string guestIdForm = form[guestId];

                if (guestIdForm != "")
                {
                    guestIdList.Add(int.Parse(guestIdForm));
                    var check = new CheckedInCustomer();
                    check.RoomId = roomId;
                    check.GuestId = int.Parse(guestIdForm);
                    check.checkedInTimeStamp = mmDateTime;
                    _context.Add(check);
                    //await _context.SaveChangesAsync();
                }
            }

            int deposit = int.Parse(form["Deposit"]);

            //var bookings = await _context.Booking.Where(s => s.BookingStatus != BookingStatus.Finished).
            //    Where(s => s.BookingStatus != BookingStatus.Cancelled).
            //    Where(s => s.BookingStatus != BookingStatus.CheckedIn).
            //    Where(s => s.RoomStatusUpdate == true).ToListAsync();
            //Booking booking = bookings.Where(s => s.RoomId == roomId).First();

            var booking = await _context.Booking.Where(b => b.BookingStatus == BookingStatus.Booked
                                                        && b.RoomId == roomId)
                                                .Include(b => b.Agent)
                                                .Include(b => b.Guest)
                                                .FirstOrDefaultAsync();
            var room = await _context.Room.Include(r => r.RoomType)
                                .Where(r => r.Id == roomId).FirstOrDefaultAsync();

            //string oldComment = "";

            TimeSpan t = booking!.To.Subtract(booking.From);
            int totalDay = Math.Max(1, (int)Math.Ceiling(t.TotalDays));
            //string totalDayS = totalDay.ToString();
            int totalHour = (int)Math.Ceiling(t.TotalHours);
            //string totalHourS = totalHour.ToString();

            // remove hourly code from transaction
            /*
            if (deposit > 0 && booking.BookingStatus != BookingStatus.BookedHourly)
            {
                Transaction transaction = new();
                transaction.TransactionType = TransactionType.Room;
                transaction.RoomId = roomId;
                transaction.TransactionStatus = TransactionStatus.Paid;
                transaction.Amount = deposit;
                transaction.CreatedDate = DateTime.Now;
                var roomTypeId = _context.Room.Where(s => s.Id == roomId).First().RoomTypeId;
                var roomType = _context.RoomType.Where(s => s.Id == roomTypeId).First();
                int roomPrice = booking.RoomPrice;
                int total = roomPrice * totalDay;
                if (oldComment == "")
                {
                    transaction.Comment = roomType.Name + "-" + totalDayS + "x" + roomPrice.ToString() + "-" + total.ToString() + "-" + booking.BookingStatus;
                }
                else
                {
                    transaction.Comment = roomType.Name + "-" + totalDayS + "x" + roomPrice.ToString() + "-" + total.ToString() + "-" + booking.BookingStatus + "-" + oldComment;
                }

                _context.Transaction.Add(transaction);
                await _context.SaveChangesAsync();
            }
            else if (deposit > 0 && booking.BookingStatus == BookingStatus.BookedHourly)
            {
                Transaction transaction = new();
                transaction.TransactionType = TransactionType.Room;
                transaction.RoomId = roomId;
                transaction.TransactionStatus = TransactionStatus.Paid;
                transaction.Amount = deposit;
                transaction.CreatedDate = DateTime.Now;
                var roomTypeId = _context.Room.Where(s => s.Id == roomId).First().RoomTypeId;
                var roomType = _context.RoomType.Where(s => s.Id == roomTypeId).First();
                int total = roomType.HourlyPrice * totalHour;
                if (oldComment == "")
                {
                    transaction.Comment = roomType.Name + "-" + totalHourS + "x" + roomType.HourlyPrice.ToString() + "-" + total.ToString() + "-" + booking.BookingStatus;
                }
                else
                {
                    transaction.Comment = roomType.Name + "-" + totalHourS + "x" + roomType.HourlyPrice.ToString() + "-" + total.ToString() + "-" + booking.BookingStatus + "-" + oldComment;
                }

                _context.Transaction.Add(transaction);
                await _context.SaveChangesAsync();

            }
            */

            // to save transactions
            Transaction t_room = new Transaction
            {
                TransactionType = TransactionType.Room,
                TransactionStatus = TransactionStatus.Active,
                RoomId = roomId,
                Guest = booking.Guest,
                Agent = booking.Agent,
                CreatedDate = mmDateTime,
                Amount = booking.RoomPrice,
                Quantity = totalDay,
                SubTotal = totalDay * booking.RoomPrice,

            };
            _context.Transaction.Add(t_room);

            if (deposit > 0)
            {
                Transaction t_deposit = new Transaction
                {
                    TransactionType = TransactionType.RoomDeposit,
                    TransactionStatus = TransactionStatus.Active,
                    RoomId = roomId,
                    Guest = booking.Guest,
                    Agent = booking.Agent,
                    CreatedDate = mmDateTime,
                    Amount = deposit,
                    Quantity = 1,
                    SubTotal = deposit,
                    Description = $"Deposit {deposit} paid."
                };
                _context.Transaction.Add(t_deposit);
            }

            // to save room and booking status
            room.RoomStatus = RoomStatus.Occupied;
            booking.BookingStatus = BookingStatus.CheckedIn;

            // save all the changes
            await _context.SaveChangesAsync();

            //return RedirectToAction(nameof(Index));
            return RedirectToAction(nameof(IndexReserved));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WalkInCheckIn(string id, IFormCollection form)
        {
            int totalRoom = int.Parse(form["total"]);
            List<int> roomSelected = new();
            int count = 0;
            for (int i = 0; i <= totalRoom; i++)
            {
                string roomSelect = form[i.ToString()];
                if (roomSelect != null)
                {

                    count++;
                    roomSelected.Add(int.Parse(roomSelect));
                }
            }
            if (count > 1)
            {
                TempData["error"] = "Sorry! Only One Room should be selected for Walk In Check In";
                return RedirectToAction(nameof(WalkIn));
            }
            else if (count < 1)
            {
                TempData["error"] = "Sorry! One Room should be selected for Walk In Check In";
                return RedirectToAction(nameof(WalkIn));
            }
            int roomId = roomSelected[0];
            List<int> guestIdList = new();
            int guestCounter = 0;
            for (int i = 1; i <= 5; i++)
            {
                string guestId = "Guest" + i.ToString();
                string guestIdForm = form[guestId];

                if (guestIdForm != "")
                {
                    guestIdList.Add(int.Parse(guestIdForm));
                    var check = new CheckedInCustomer();
                    check.RoomId = roomId;
                    check.GuestId = int.Parse(guestIdForm);
                    check.checkedInTimeStamp = DateTime.Now;
                    _context.Add(check);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    guestCounter++;
                }
            }
            if (guestCounter == 5)
            {
                TempData["error"] = "Sorry! At least ONE Guest should be selected for Walk In Check In";
                return RedirectToAction(nameof(WalkIn));
            }

            _context.Room.Where(s => s.Id == roomId).First().RoomStatus = RoomStatus.Occupied;
            await _context.SaveChangesAsync();

            int deposit = int.Parse(form["Deposit"]);

            DateTime fromDateTime = DateTime.Parse(form["fromDateTime"]);
            DateTime toDateTime = DateTime.Parse(form["toDateTime"]);

            TimeSpan t = toDateTime.Subtract(fromDateTime);
            int totalDay = Math.Max(1, (int)t.TotalDays);
            string totalDayS = totalDay.ToString();
            int totalHour = (int)t.TotalHours;
            string totalHourS = totalHour.ToString();

            var roomType = _context.RoomType.Where(s => s.Id == int.Parse(form["roomTypeId"])).First();

            if (deposit > 0 && fromDateTime.Date != toDateTime.Date)
            {
                Transaction transaction = new();
                transaction.TransactionType = TransactionType.Room;
                transaction.RoomId = roomId;
                transaction.TransactionStatus = TransactionStatus.Paid;
                transaction.Amount = deposit;
                transaction.CreatedDate = DateTime.Now;

                int roomPrice = roomType.Price;
                int total = roomPrice * totalDay;

                var checkedInOutTime = _context.CheckedInOutTime.Where(s => s.RoomType == roomType).First();

                var checkedInTimeSplit = checkedInOutTime.CheckedInTime.Split(":");
                int checkedInHour = int.Parse(checkedInTimeSplit[0]);
                int checkedInMinute = int.Parse(checkedInTimeSplit[1]);
                int checkedInTimeCalculated = checkedInHour * 60 + checkedInMinute;

                var checkedOutTimeSplit = checkedInOutTime.CheckedOutTime.Split(":");
                int checkedOutHour = int.Parse(checkedOutTimeSplit[0]);
                int checkedOutMinute = int.Parse(checkedOutTimeSplit[1]);
                int checkedOutTimeCalculated = checkedOutHour * 60 + checkedOutMinute;

                int fromTimeHour = fromDateTime.Hour;
                int fromTimeMinute = fromDateTime.Minute;
                int fromTimeCalculated = fromTimeHour * 60 + fromTimeMinute;

                int toTimeHour = toDateTime.Hour;
                int toTimeMinute = toDateTime.Minute;
                int toTimeCalculated = toTimeHour * 60 + toTimeMinute;

                string commentString = "";

                if (fromTimeCalculated < checkedInTimeCalculated && toTimeCalculated <= checkedOutTimeCalculated)
                {
                    commentString = "WalkInEarlyCheckIn";
                }
                else if (fromTimeCalculated < checkedInTimeCalculated && toTimeCalculated > checkedOutTimeCalculated)
                {
                    commentString = "WalkInEarlyCheckInAndLateCheckOut";
                }
                else if (fromTimeCalculated >= checkedInTimeCalculated && toTimeCalculated <= checkedOutTimeCalculated)
                {
                    commentString = "WalkIn";
                }
                else if (fromTimeCalculated >= checkedInTimeCalculated && toTimeCalculated > checkedOutTimeCalculated)
                {
                    commentString = "WalkInLateCheckOut";
                }
                transaction.Comment = roomType.Name + "-" + totalDayS + "x" + roomPrice.ToString() + "-" + total.ToString() + "-" + commentString;



                _context.Transaction.Add(transaction);
                await _context.SaveChangesAsync();
            }
            else if (deposit > 0 && fromDateTime.Date == toDateTime.Date)
            {
                Transaction transaction = new();
                transaction.TransactionType = TransactionType.Room;
                transaction.RoomId = roomId;
                transaction.TransactionStatus = TransactionStatus.Paid;
                transaction.Amount = deposit;
                transaction.CreatedDate = DateTime.Now;
                int total = roomType.HourlyPrice * totalHour;
                transaction.Comment = roomType.Name + "-" + totalHourS + "x" + roomType.HourlyPrice.ToString() + "-" + total.ToString() + "-WalkInHourly";

                _context.Transaction.Add(transaction);
                await _context.SaveChangesAsync();

            }

            var booking = new Booking();
            booking.CreatedDate = DateTime.Now;
            booking.RoomId = roomId;
            booking.GuestId = guestIdList[0];
            booking.From = fromDateTime;
            booking.To = toDateTime;
            booking.BookingStatus = BookingStatus.CheckedIn;
            booking.RoomStatusUpdate = true;
            booking.RoomPrice = _context.RoomType.Where(s => s.Id == int.Parse(form["roomTypeId"])).First().Price;
            booking.Comment = form["comment"];
            booking.AgentId = _context.Agent.Where(s => s.Name == "NoAgent").First().Id;
            _context.Booking.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CheckOut(string id)
        {
            int roomId = int.Parse(id);
            var today = Utility.GetMMTimeNow();
            ViewBag.RoomId = roomId;
            ViewBag.Today = today;

            var room = await _context.Room
                        .Where(r => r.Id == roomId)
                        .Include(r => r.Guest)
                        .Include(r => r.RoomType)
                        .FirstOrDefaultAsync();

            var booking = await _context.Booking
                            .Where(b => b.RoomId == roomId
                                    && b.BookingStatus == BookingStatus.CheckedIn
                            ).FirstOrDefaultAsync();
            room!.CurrentBookedFrom = booking!.From;
            room!.CurrentBookedTo = booking!.To;

            // get checkin customer
            var checkedInCustomer = await _context.CheckedInCustomer
                                    .Where(c => c.RoomId == room.Id)
                                    .Include(c => c.Guest)
                                    .FirstOrDefaultAsync();

            room.CheckedInCustomer = checkedInCustomer;

            ViewBag.Room = room;
            var actualStayDay = (int)Math.Ceiling(today.Subtract(booking!.From).TotalDays);
            ViewBag.ActualStayDay = actualStayDay;

            var trans = await _context.Transaction
                         .Where(t => t.TransactionStatus == TransactionStatus.Active
                                && t.RoomId == roomId
                                && t.Guest == room!.Guest
                                && t.CreatedDate.Date >= booking.From.Date
                         )
                         .Include(t => t.Agent)
                         .Include(t => t.Service)
                         .Include(t => t.Restaurant)
                         .OrderBy(t => t.CreatedDate)
                         .ToListAsync();

            // to update actual room fee based on checkout date
            var roomFeeTrans = trans.Where(t => t.TransactionType == TransactionType.Room).ToList();

            // update room fee transactions
            foreach (var tran in roomFeeTrans)
            {
                tran.Quantity = actualStayDay;
                tran.SubTotal = actualStayDay * tran.Amount;
                _context.Update(tran);
            }
            await _context.SaveChangesAsync();

            // get total amount
            var deposit = trans.Where(t => t.TransactionType == TransactionType.RoomDeposit).Sum(t => t.SubTotal);
            var total = trans.Where(t => t.TransactionType != TransactionType.RoomDeposit).Sum(t => t.SubTotal);
            var grandTotal = total - deposit;

            ViewBag.GrandTotal = grandTotal;

            return View(trans);
            /*
            List<Transaction> transactions = await _context.Transaction.
                Where(s => s.TransactionStatus != TransactionStatus.Inactive).
                Where(s => s.RoomId == roomId).
                ToListAsync();
            ViewBag.Transactions = transactions;
            Transaction roomTranx = transactions.Where(s => s.TransactionType == TransactionType.Room).First();
            int deposit = (int)roomTranx.Amount;
            int fullroomCharge = int.Parse(roomTranx.Comment.Split("-")[2]);
            ViewBag.Total = transactions.Sum(s => s.Amount) - deposit + fullroomCharge;

            Room room = _context.Room.Where(s => s.Id == roomId).First();
            room.RoomStatus = RoomStatus.Available;
            _context.Update(room);
            await _context.SaveChangesAsync();

            foreach (var transaction in transactions)
            {
                if (transaction.TransactionType == TransactionType.Room)
                {
                    string comment = transaction.Comment;
                    var commentSplit = comment.Split("-");
                    int amount = int.Parse(commentSplit[2]);
                    ViewBag.Deposit = transaction.Amount;
                    ViewBag.ToSettle = ViewBag.Total - ViewBag.Deposit;
                    transaction.Amount = amount;
                }
                transaction.TransactionStatus = TransactionStatus.Inactive;
                //_context.Update(transaction);
                //await _context.SaveChangesAsync();
            }
            List<CheckedInCustomer> checkedInList = await _context.CheckedInCustomer.Where(s => s.RoomId == roomId).ToListAsync();

            foreach (var each in checkedInList)
            {
                _context.Remove(each);
                //await _context.SaveChangesAsync();
            }

            Booking booking = _context.Booking.Where(s => s.BookingStatus == BookingStatus.CheckedIn).
                Where(s => s.RoomId == roomId).Include(s => s.Guest).First();
            booking.BookingStatus = BookingStatus.Finished;
            ViewBag.GuestName = booking.Guest.Name;
            ViewBag.GuestId = booking.GuestId;
            ViewBag.AgentId = booking.AgentId;
            //_context.Update(booking);
            //await _context.SaveChangesAsync();

            return View(transactions);
            */
        }

        public async Task<IActionResult> CheckOutConfirmed(string id)
        {

            int roomId = int.Parse(id);
            var today = Utility.GetMMTimeNow();
            ViewBag.RoomId = roomId;
            ViewBag.Today = today;

            var room = await _context.Room
                        .Where(r => r.Id == roomId)
                        .Include(r => r.Guest)
                        .Include(r => r.RoomType)
                        .FirstOrDefaultAsync();

            var booking = await _context.Booking
                            .Where(b => b.RoomId == roomId
                                    && b.BookingStatus == BookingStatus.CheckedIn
                            ).FirstOrDefaultAsync();
            room!.CurrentBookedFrom = booking!.From;
            room!.CurrentBookedTo = booking!.To;

            // get checkin customer
            var checkedInCustomer = await _context.CheckedInCustomer
                                    .Where(c => c.RoomId == room.Id)
                                    .Include(c => c.Guest)
                                    .FirstOrDefaultAsync();

            room.CheckedInCustomer = checkedInCustomer;

            ViewBag.Room = room;
            var actualStayDay = (int)Math.Ceiling(today.Subtract(booking!.From).TotalDays);
            ViewBag.ActualStayDay = actualStayDay;

            var trans = await _context.Transaction
                         .Where(t => t.TransactionStatus == TransactionStatus.Active
                                && t.RoomId == roomId
                                && t.Guest == room!.Guest
                                && t.CreatedDate.Date >= booking.From.Date
                         )
                         .Include(t => t.Agent)
                         .Include(t => t.Service)
                         .Include(t => t.Restaurant)
                         .OrderBy(t => t.CreatedDate)
                         .ToListAsync();

            // to update actual room fee based on checkout date
            var roomFeeTrans = trans.Where(t => t.TransactionType == TransactionType.Room).ToList();

            // update room fee transactions
            foreach (var tran in roomFeeTrans)
            {
                tran.Quantity = actualStayDay;
                tran.SubTotal = actualStayDay * tran.Amount;
                _context.Update(tran);
            }
            await _context.SaveChangesAsync();

            // get total amount
            var deposit = trans.Where(t => t.TransactionType == TransactionType.RoomDeposit).Sum(t => t.SubTotal);
            var total = trans.Where(t => t.TransactionType != TransactionType.RoomDeposit).Sum(t => t.SubTotal);
            var grandTotal = total - deposit;

            ViewBag.GrandTotal = grandTotal;

            // update transactions
            foreach (var tran in trans)
            {
                tran.TransactionStatus = TransactionStatus.Paid;
                _context.Update(tran);
            }

            // update room status
            room.RoomStatus = RoomStatus.Available;
            room.Guest = null;
            _context.Update(room);

            // remove checked in customer
            _context.Remove(checkedInCustomer);

            // update booking
            booking.BookingStatus = BookingStatus.Finished;
            _context.Update(booking);

            _context.SaveChangesAsync();

            return RedirectToAction(nameof(IndexCheckedIn));
        }

        public async Task<IActionResult> SaveReceipts(IFormCollection form)
        {
            int counter = int.Parse(form["Counter"]);
            string fmt = "yyyyMdHHmmss";
            DateTime createdDate = DateTime.Now;
            string receiptNumber = createdDate.ToString(fmt);
            for (int i = 0; i < counter; i++)
            {
                string comment = "Comment" + i.ToString();
                string amount = "Amount" + i.ToString();
                string type = "Type" + i.ToString();
                if (form[amount] != "")
                {
                    Receipt receipt = new();
                    receipt.AgentId = int.Parse(form["AgentId"]);
                    receipt.RoomId = int.Parse(form["RoomId"]);
                    receipt.GuestId = int.Parse(form["GuestId"]);
                    if (form[type] == "Room")
                    {
                        receipt.TransactionType = TransactionType.Room;
                    }
                    else if (form[type] == "Service")
                    {
                        receipt.TransactionType = TransactionType.Service;
                    }
                    else if (form[type] == "Restaurant")
                    {
                        receipt.TransactionType = TransactionType.Restaurant;
                    }
                    receipt.CreatedDate = createdDate;
                    receipt.ReceiptNumber = receiptNumber;
                    try
                    {
                        receipt.Amount = int.Parse(form[amount]);
                    }
                    catch
                    {
                        receipt.Amount = 0;
                    }

                    receipt.Name = form[comment];
                    _context.Add(receipt);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Receipt", "Rooms");

        }
        public async Task<IActionResult> Receipt()
        {
            var receipt = _context.Receipt.OrderBy(s => s.CreatedDate).Include(s => s.Guest).Include(s => s.Room).Include(s => s.Agent).Last();
            var receiptNumber = receipt.ReceiptNumber;
            ViewData["Guest"] = receipt.Guest.Name;
            ViewData["Room"] = receipt.Room.RoomNumber;
            ViewData["Agent"] = receipt.Agent.Name;

            var receipts = await _context.Receipt.Where(s => s.ReceiptNumber == receiptNumber).ToListAsync();
            ViewData["Total"] = receipts.Sum(s => s.Amount);
            return View(receipts);
        }

        public ActionResult DownloadPdf(IFormCollection form)
        {
            var render = new ChromePdfRenderer();
            using var doc = render.RenderHtmlAsPdf(form["GridHtml"]);
            doc.SaveAs("HtmlString.pdf");
            return RedirectToAction("Receipt", "Rooms");
        }


        // GET: Rooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Room == null)
            {
                RedirectToAction("Error", "Home");
            }

            var room = await _context.Room
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(room);
        }

        // GET: Rooms/Create
        public IActionResult Create()
        {
            ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name");
            return View();
        }

        // POST: Rooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id, RoomTypeId,RoomNumber,RoomStatus,Level")] Room room, IFormCollection form)
        {

            int from = int.Parse(form["From"]);
            int to = int.Parse(form["To"]);

            var existingRoomNumbers = _context.Room.Where(r => r.RoomNumber >= from && r.RoomNumber <= to)
                                          .Select(r => r.RoomNumber.ToString()).ToList();

            var existingRoomNumberStr = string.Join(",", existingRoomNumbers);

            if (!string.IsNullOrEmpty(existingRoomNumberStr))
            {
                ModelState.AddModelError(string.Empty, $"Room number inputs are invalid. Existing room numbers: {existingRoomNumberStr}");
                return View(room);
            }

            if (to >= from)
            {
                for (int i = from; i <= to; i++)
                {

                    var roomExists = _context.Room.Any(r => r.RoomTypeId == room.RoomTypeId && r.RoomNumber == i);

                    if (!roomExists)
                    {

                        var addRoom = new Room();
                        addRoom.RoomStatus = RoomStatus.Available; // room.RoomStatus;
                        addRoom.RoomNumber = i;
                        addRoom.RoomTypeId = room.RoomTypeId;
                        if (ModelState.IsValid)
                        {
                            _context.Add(addRoom);
                            await _context.SaveChangesAsync();

                        }
                    }
                    //try
                    //{
                    //    var roomExists = _context.Room.Where(s => s.RoomNumber == i).First();
                    //}
                    //catch
                    //{
                    //}
                }
                return RedirectToAction(nameof(IndexAll));
            }

            ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name", room.RoomTypeId);
            return RedirectToAction(nameof(IndexAll));
        }

        // GET: Rooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Room == null)
            {
                RedirectToAction("Error", "Home");
            }

            var room = await _context.Room.FindAsync(id);
            if (room == null)
            {
                RedirectToAction("Error", "Home");
            }
            ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Name", room.RoomTypeId);
            return View(room);
        }

        // POST: Rooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomTypeId,RoomNumber,RoomStatus,Level")] Room room)
        {
            if (id != room.Id)
            {
                RedirectToAction("Error", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    try
                    {
                        _context.Room.Where(s => s.RoomNumber == room.RoomNumber).First();
                        TempData["ErrorMessage"] = "Room Number already exit";
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        _context.Update(room);
                        await _context.SaveChangesAsync();
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                    {
                        RedirectToAction("Error", "Home");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomTypeId"] = new SelectList(_context.RoomType, "Id", "Id", room.RoomTypeId);
            return View(room);
        }

        // GET: Rooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Room == null)
            {
                RedirectToAction("Error", "Home");
            }

            var room = await _context.Room
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                RedirectToAction("Error", "Home");
            }

            return View(room);
        }

        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Room == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Room'  is null.");
            }
            var room = await _context.Room.FindAsync(id);
            if (room != null)
            {
                _context.Room.Remove(room);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
            return _context.Room.Any(e => e.Id == id);
        }
    }
}
