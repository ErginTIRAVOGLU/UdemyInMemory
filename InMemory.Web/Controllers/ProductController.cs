using InMemory.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace InMemory.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IMemoryCache _memoryCache;

        public ProductController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public IActionResult Index()
        {
            //1.yol
            if (string.IsNullOrEmpty(_memoryCache.Get<string>("zaman")))
            {
                _memoryCache.Set<string>("zaman", DateTime.Now.ToString());
            }

            //2.yol
            if (_memoryCache.TryGetValue("zaman", out string zamanCache))
            {
                //_memoryCache.Set<string>("zaman", DateTime.Now.ToString());
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();

                //options.SlidingExpiration = TimeSpan.FromSeconds(10);
                options.AbsoluteExpiration = DateTime.Now.AddSeconds(30);
                options.Priority = CacheItemPriority.High;//önemli data memory dolarsa enson sil
                options.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    _memoryCache.Set("callback", $"{key}->{value} => sebep :{reason}");
                });
                _memoryCache.Set<string>("zaman", DateTime.Now.ToString(), options);

                Product p = new Product { Id = 1, Name = "Kalem", Price = 200 };
                _memoryCache.Set<Product>("product:1", p);
            }

            return View();
        }

        public IActionResult Show()
        {
            //Key'i silmek için
            _memoryCache.Remove("zaman");
            _memoryCache.TryGetValue("callback", out string callback);


            // Varsa al yoksa yarat
            var zaman = _memoryCache.GetOrCreate<string>("zaman", entry =>
            {
                return DateTime.Now.ToString();
            });

            ViewBag.zaman = _memoryCache.Get<string>("zaman");
            ViewBag.callback = callback;

            ViewBag.product = _memoryCache.Get<Product>("product:1");
            return View();
        }
    }
}
