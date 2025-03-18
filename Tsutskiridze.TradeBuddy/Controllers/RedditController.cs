using Microsoft.AspNetCore.Mvc;
using Tsutskiridze.Bloom.Core.Common.Base;
using Tsutskiridze.TradeBuddy.Services.News;

namespace Tsutskiridze.TradeBuddy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedditController : ApiControllerBase
    {

        private readonly RedditService _reddit;

        public RedditController(RedditService reddit)
        {
            _reddit = reddit;
        }


        [HttpPost("stocks/{symbol}")]
        public async Task<IActionResult> GetRedditPosts(string symbol)
        {
            try
            {

                var posts = await _reddit.GetTopPostsAsync(symbol, "new", 5);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
