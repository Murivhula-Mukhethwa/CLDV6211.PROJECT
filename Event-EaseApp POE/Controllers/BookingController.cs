using System.Runtime.InteropServices;
using Event_EaseApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Event_EaseApp.Controllers
{
    public class BookingController : Controller
    {

        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var booking = _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                booking = booking.Where(b =>
                b.Event.EventName.Contains(searchString) || 
                b.Venue.VenueName.Contains(searchString));
            }



                return View(await booking.ToListAsync());
        }


        public ActionResult Create()
        {
            // Populate the dropdown lists for Venue and Event
            ViewBag.VenueList = new SelectList(_context.Venue, "VenueID", "VenueName");
            ViewBag.EventList = new SelectList(_context.Event, "EventID", "EventName");

            ViewData["Events"] = _context.Event.ToList();
            ViewData["Venues"] = _context.Venue.ToList();   
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            //Re-populate dropdowns for return in case of error 
            ViewData["EventID"] = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);

            //Check if selected event exists 
            var selectedEvent = await _context.Event.FirstOrDefaultAsync(e => e.EventID == booking.EventID);
            if (selectedEvent == null)
            {
                ModelState.AddModelError("EventID", "Selected event does not exist.");
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                // Check for double booking on same date and venue 
                var conflict = await _context.Booking
                    .AnyAsync(b => b.VenueID == booking.VenueID &&
                              b.BookingDate.Date == booking.BookingDate.Date);

                if (conflict)
                {
                    ModelState.AddModelError("BookingDate", "This venue is already booked for the selected date.");
                    return View(booking);
                }

                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(booking);

        }

            public async Task<IActionResult> Details(int? id)
        {
      
            var booking = await _context.Booking.FirstOrDefaultAsync(m => m.BookingID == id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }



        public async Task<IActionResult> Delete(int? id)
        {
            var booking = await _context.Booking.FirstOrDefaultAsync(m => m.BookingID == id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

        [HttpPost]

        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Booking.FindAsync(id);


            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingID == id);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }
        [HttpPost]

        public async Task<IActionResult> Edit(int id, [Bind("BookingID,EventID,VenueID,BookingDate")] Booking booking)
        {
            if (id != booking.BookingID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }
    }

}
