using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Alachisoft.NCache.Runtime.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace StatelessASPNetCoreClient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NCacheController : Controller
    {
        private readonly ICache _cache;
        private readonly Tag _tag = new Tag("items");

        public NCacheController(ICache cache)
        {
            _cache = cache;
        }

        // GET: api/NCache
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var items = await Task.Run(() => _cache.SearchService.GetByTag<string>(_tag));
            return Json(items);
        }

        // GET: api/NCache/key
        [HttpGet("{id}", Name = "Get")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            try
            {
                var value = await Task.Run(() => _cache.Get<string>(id));

                if (value == null)
                {
                    return NotFound();
                }

                return Json(value);
            }
            catch (Exception e)
            {
                return BadRequest($"Something wrong. {e}");
            }
        }

        // POST: api/NCache/key
        [HttpPost("{key}")]
        public async Task<IActionResult> PostAsync(string key, [FromBody] string value)
        {
            try
            {
                if (await Task.Run(() => !_cache.Contains(key)))
                {
                    await Task.Run(() => _cache.Add(key, new CacheItem(value) 
                    { Tags = new Tag[] { _tag } }));

                    return CreatedAtAction(nameof(GetByIdAsync), key, value);
                }
                else
                {
                    return BadRequest("Item already exists in cache");
                }
            }
            catch (Exception e)
            {
                return BadRequest($"Something went wrong. Details \n{e}");
            }
        }

        // PUT: api/NCache/key
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(string id, [FromBody] string value)
        {
            try
            {
                await _cache.InsertAsync(id, new CacheItem(value) { Tags = new Tag[] { _tag } });
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest($"Something went wrong. Details \n{e}");
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            try
            {
                await _cache.RemoveAsync<string>(id);
                return NoContent();
            }
            catch (Exception e)
            {

                return BadRequest($"Something went wrong. Details \n{e}");
            }
        }
    }
}
