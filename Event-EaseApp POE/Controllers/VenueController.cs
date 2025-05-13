
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Event_EaseApp.Models;
using System.Net;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using Azure.Storage.Blobs;

namespace Event_EaseApp.Controllers
{
    public class VenueController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VenueController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Venue
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venue.ToListAsync());
        }

        // GET: Venue/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // GET: Venue/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venue/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue)
        {
            if (ModelState.IsValid)
            {

                //Handle image upload to Azure Blob Storage if an image file was provided 
                //This is Step 4C: Modify Controller to receive  ImageFile from View (user upload)
                //This is Step 5: Upload selected image to Azure Blob Storage   
                if (venue.ImageFile != null)
                {
                    //Upload the image to Azure Blob Storage
                    
                    var blobUrl = await UploadImageToBlobAsync(venue.ImageFile); //Main part of Step 5 B(upload

                    //Step 6: Save the Blob URl intoImageUrl property (the database)
                    venue.ImageUrl = blobUrl; 
                }   
                _context.Add(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venue created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venue/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Venue/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue)
        {
            if (id != venue.VenueID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (venue.ImageFile != null)
                    {
                        //Upload new image if provided 
                        var blobUrl = await UploadImageToBlobAsync(venue.ImageFile); //Main part of Step 5 B(upload
                        
                        
                        //Step 6: Save the Blob URl intoImageUrl property (the database)
                        venue.ImageUrl = blobUrl;
                    }
                    else
                    {
                        //Keep the existing ImageUrl(Optional depending on your UI design)
                    }
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venue/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue.FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Venue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
           
            var hasBookings = await _context.Booking.AnyAsync(b => b.VenueID == id);
            
            if (hasBookings)
            {
                TempData["ErrorMessage"]= "Cannot delete venue because it has existing bookings.";
                return RedirectToAction(nameof(Index));
            } 
            
            try
            {

            
            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Venue deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting venue. Please try again.";
                
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueID == id);
        }

        //This is Step 5 (C) : Upload image to Azure Blob Storage   
        //It completes the entire uploading process inside Step 5 - from connecting to Azure to returning the Blob
        //This will upload the Image to Blob Storage Account 
        //Uploads an image to Azure Blob Storage and return the Blob URL

        private async Task<string> UploadImageToBlobAsync(IFormFile imageFile)
        {

            // Your Azure Blob Storage connection string
            var connectionString = "mmmmm";
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(connectionString);
            // Get a reference to the container
            var containerName =("cldv6211project");
            
            var containerClient= blobServiceClient.GetBlobContainerClient(containerName);   
            
            
            var blobClient = containerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(imageFile.FileName));
            
            var blobHttpHeaders =new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = imageFile.ContentType
            };  

            using  (var stream = imageFile.OpenReadStream())
            {
               
            await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobUploadOptions 
                {
                HttpHeaders = blobHttpHeaders
            }); 
            }
            return blobClient.Uri.ToString(); //Return the URL of the uploaded image
        }

        //This is a small helper method 
        //This small world will check if a Venue exists in the database 
        //Checks if a venue exists in the database  


    }
}
