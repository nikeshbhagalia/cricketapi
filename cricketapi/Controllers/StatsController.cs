using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cricketapi.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using cricketapi.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Auth;

namespace cricketapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly cricketapiContext _context;
        private IConfiguration _configuration;

        public StatsController(cricketapiContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Stats
        [HttpGet]
        public IEnumerable<Player> GetPlayer()
        {
            return _context.Player;
        }

        // GET: api/Stats/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayer([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var player = await _context.Player.FindAsync(id);

            if (player == null)
            {
                return NotFound();
            }

            return Ok(player);
        }

        // PUT: api/Stats/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlayer([FromRoute] int id, [FromBody] Player player)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != player.Id)
            {
                return BadRequest();
            }

            _context.Entry(player).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Stats
        [HttpPost]
        public async Task<IActionResult> PostPlayer([FromBody] Player player)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Player.Add(player);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlayer", new { id = player.Id }, player);
        }

        // DELETE: api/Stats/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayer([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var player = await _context.Player.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }

            _context.Player.Remove(player);
            await _context.SaveChangesAsync();

            return Ok(player);
        }

        private bool PlayerExists(int id)
        {
            return _context.Player.Any(e => e.Id == id);
        }

        // GET: api/Stats/Names
        [Route("name")]
        [HttpGet]
        public async Task<List<string>> GetName()
        {
            var name = (from m in _context.Player
                         select m.Name).Distinct();
            var returned = await name.ToListAsync();

            return returned;
        }

        [HttpPost, Route("upload")]
        public async Task<IActionResult> UploadFile([FromForm]PlayerImageItem cricketer)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }
            try
            {
                using (var stream = cricketer.Image.OpenReadStream())
                {
                    var cloudBlock = await UploadToBlob(cricketer.Image.FileName, null, stream);
                    //// Retrieve the filename of the file you have uploaded
                    //var filename = provider.FileData.FirstOrDefault()?.LocalFileName;
                    if (string.IsNullOrEmpty(cloudBlock.StorageUri.ToString()))
                    {
                        return BadRequest("An error has occured while uploading your file. Please try again.");
                    }

                    Player player = new Player();
                    player.Name = cricketer.Name;
                    player.Country = cricketer.Country;
                    player.Runs = cricketer.Runs;
                    player.Wickets = cricketer.Wickets;
                    player.Catches = cricketer.Catches;

                    System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                    player.Url = cloudBlock.SnapshotQualifiedUri.AbsoluteUri;

                    _context.Player.Add(player);
                    await _context.SaveChangesAsync();

                    return Ok($"File: {cricketer.Name} has successfully uploaded");
                }
            }
            catch (Exception ex)
            {
                if(ex.Message == "Object reference not set to an instance of an object.")
                {
                    return (BadRequest("hi"));
                }
                return BadRequest($"An error has occured. Details:{ex.Message}");
            }


        }

        private async Task<CloudBlockBlob> UploadToBlob(string filename, byte[] imageBuffer = null, System.IO.Stream stream = null)
        {

            var accountName = _configuration["AzureBlob:name"];
            var accountKey = _configuration["AzureBlob:key"]; ;
            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer imagesContainer = blobClient.GetContainerReference("images");

            string storageConnectionString = _configuration["AzureBlob:connectionString"];

            // Check whether the connection string can be parsed.
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                try
                {
                    // Generate a new filename for every new blob
                    var fileName = Guid.NewGuid().ToString();
                    fileName += GetFileExtention(filename);

                    // Get a reference to the blob address, then upload the file to the blob.
                    CloudBlockBlob cloudBlockBlob = imagesContainer.GetBlockBlobReference(fileName);

                    if (stream != null)
                    {
                        await cloudBlockBlob.UploadFromStreamAsync(stream);
                    }
                    else
                    {
                        return new CloudBlockBlob(new Uri(""));
                    }

                    return cloudBlockBlob;
                }
                catch (StorageException ex)
                {
                    return new CloudBlockBlob(new Uri(""));
                }
            }
            else
            {
                return new CloudBlockBlob(new Uri(""));
            }

        }

        private string GetFileExtention(string fileName)
        {
            if (!fileName.Contains("."))
                return ""; //no extension
            else
            {
                var extentionList = fileName.Split('.');
                return "." + extentionList.Last(); //assumes last item is the extension 
            }
        }
    }
}