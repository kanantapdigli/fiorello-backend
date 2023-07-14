using Fiorello.DAL;
using Fiorello.Models;
using Fiorello.ViewModels.Basket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Fiorello.Controllers
{
	[Authorize]
	public class BasketController : Controller
	{
		private readonly AppDbContext _context;
		private readonly UserManager<User> _userManager;

		public BasketController(AppDbContext context, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);
			if(user is null) return Unauthorized();

			var basket = await _context.Baskets
										.Include(b => b.BasketProducts)
										.ThenInclude(bp => bp.Product)
										.ThenInclude(p => p.ProductPhotos)
										.FirstOrDefaultAsync(b => b.UserId == user.Id);

			var model = new List<BasketProductVM>();
			if(basket == null)	return View(model);			

            foreach (var basketProduct in basket.BasketProducts)
            {
				var basketProductItem = new BasketProductVM
				{
					Id = basketProduct.Id,
					Count = basketProduct.Count,
					PhotoName = basketProduct.Product.ProductPhotos.FirstOrDefault(p => p.IsMain).Name,
					StockQuantity = basketProduct.Product.Stock,
					Price = basketProduct.Product.Price,
					Title = basketProduct.Product.Title,
				};
				model.Add(basketProductItem);
            }
			return View(model);
        }
		public async Task<IActionResult> AddAsync(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Unauthorized();

			var basket = await _context.Baskets.FirstOrDefaultAsync(b => b.UserId == user.Id);
			if (basket is null)
			{
				basket = new Basket
				{
					UserId = user.Id,
				};
				await _context.Baskets.AddAsync(basket);
			}
			var product = await _context.Products.FindAsync(id);
			if (product is null)
				return NotFound("Product not found");

			var basketProduct = await _context.BasketProducts.FirstOrDefaultAsync(bp => bp.ProductId == id &&
																						bp.Basket.UserId == user.Id);

			if (basketProduct is null)
			{
				basketProduct = new BasketProduct
				{
					Basket = basket,
					ProductId = product.Id,
					Count = 1,
				};
				await _context.BasketProducts.AddAsync(basketProduct);
			}
			else
			{
				basketProduct.Count++;
				_context.BasketProducts.Update(basketProduct);
			}

			await _context.SaveChangesAsync();

			return Ok("Product added to basket");
		}

		[HttpGet]
		public async Task<IActionResult> IncreaseCount(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Unauthorized();

			var basket = await _context.Baskets.FirstOrDefaultAsync(b => b.UserId == user.Id);
			if(basket is null)
				return NotFound("Basket not Found");

			var basketProduct = await _context.BasketProducts.FirstOrDefaultAsync(bp => bp.Id == id && bp.BasketId == basket.Id);
			if( basketProduct is null) 
				return NotFound("No such product is found in Basket");

			var product = await _context.Products.FindAsync(basketProduct.ProductId);
			if (product is null)
				return NotFound("Product is not found");

			if (product.Stock == basketProduct.Count)
				return NotFound("No more left in stock");

			basketProduct.Count++;
			_context.BasketProducts.Update(basketProduct);
			await _context.SaveChangesAsync();

			return Ok();
		}

		[HttpGet]
		public async Task<IActionResult> DecreaseCount(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Unauthorized();

			var basket = await _context.Baskets.FirstOrDefaultAsync(b => b.UserId == user.Id);
			if (basket is null)
				return NotFound("Basket not Found");

			var basketProduct = await _context.BasketProducts.FirstOrDefaultAsync(bp => bp.Id == id && bp.BasketId == basket.Id);
			if (basketProduct is null)
				return NotFound("No such product is found in Basket");

			var product = await _context.Products.FindAsync(basketProduct.ProductId);
			if (product is null)
				return NotFound("Product is not found");

			if (basketProduct.Count == 0)
				basket.BasketProducts.Remove(basketProduct);

			basketProduct.Count--;
			_context.BasketProducts.Update(basketProduct);
			await _context.SaveChangesAsync();

			return Ok();
		}

		[HttpGet]
		public async Task<IActionResult> RemoveProduct(int id)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Unauthorized();

			var basket = await _context.Baskets.FirstOrDefaultAsync(b => b.UserId == user.Id);
			if (basket is null)
				return NotFound("Basket not Found");

			var basketProduct = await _context.BasketProducts.FirstOrDefaultAsync(bp => bp.Id == id && bp.BasketId == basket.Id);
			if (basketProduct is null)
				return NotFound("No such product is found in Basket");

			var product = await _context.Products.FindAsync(basketProduct.ProductId);
			if (product is null)
				return NotFound("Product is not found");

			_context.BasketProducts.Remove(basketProduct);
			await _context.SaveChangesAsync();

			return Ok();
		}

		//[HttpGet]
		//public async Task<IActionResult> Index()
		//{
		//          List<BasketModel> basket;

		//          var basketCookie = Request.Cookies["basket"];
		//	if (basketCookie is null)
		//		basket = new List<BasketModel>();
		//	else
		//		basket = JsonConvert.DeserializeObject<List<BasketModel>>(basketCookie);

		//	foreach(var basketProduct in basket)
		//	{
		//		var product = await _context.Products.Include(p => p.ProductPhotos).FirstOrDefaultAsync(p => p.Id == basketProduct.Id);
		//		if (product != null)
		//		{
		//			basketProduct.Title = product.Title;
		//			basketProduct.Price = product.Price;
		//			basketProduct.StockQuantity = product.Stock;
		//			basketProduct.PhotoName = product.ProductPhotos.FirstOrDefault(p => p.IsMain).Name;
		//		}
		//	}

		//	return View(basket);
		//}

		//[HttpGet]
		//public async Task<IActionResult> AddAsync(int id)
		//{
		//	List<BasketModel> basket;

		//	var basketCookie = Request.Cookies["basket"];
		//	if (basketCookie == null)
		//		basket = new List<BasketModel>();
		//	else
		//		basket = JsonConvert.DeserializeObject<List<BasketModel>>(basketCookie);

		//	var product = await _context.Products.FindAsync(id);
		//	if (product is null)
		//		return NotFound("Product is not found");

		//	var basketProduct = basket.Find(bp => bp.Id == product.Id);
		//	if (basketProduct is not null)
		//	{
		//		if (product.Stock == basketProduct.Count)
		//			return NotFound("No more left at stock");

		//		basketProduct.Count++;
		//	}
		//	else
		//	{
		//		basket.Add(new BasketModel
		//		{
		//			Id = product.Id,
		//			Count = 1
		//		});
		//	}

		//	Response.Cookies.Append("basket", JsonConvert.SerializeObject(basket));
		//	return Ok("Product successfully added to basket");
		//}

		//[HttpGet]
		//public async Task<IActionResult> IncreaseCount(int id)
		//{
		//	List<BasketModel> basket = JsonConvert.DeserializeObject<List<BasketModel>>(Request.Cookies["basket"]);

		//	var product = await _context.Products.FindAsync(id);
		//	if (product is null)
		//		return NotFound("Product is not found");

		//	var basketProduct = basket.Find(bp => bp.Id == product.Id);
		//	if (basketProduct is not null)
		//	{
		//		if (product.Stock == basketProduct.Count)
		//			return NotFound("No more left at stock");

		//		basketProduct.Count++;
		//	}

		//	Response.Cookies.Append("basket", JsonConvert.SerializeObject(basket));
		//	return RedirectToAction(nameof(Index));
		//}

		//[HttpGet]
		//public async Task<IActionResult> DecreaseCount(int id)
		//{
		//	List<BasketModel> basket = JsonConvert.DeserializeObject<List<BasketModel>>(Request.Cookies["basket"]);

		//	var product = await _context.Products.FindAsync(id);
		//	if (product is null)
		//		return NotFound("Product is not found");

		//	var basketProduct = basket.Find(bp => bp.Id == product.Id);
		//	if (basketProduct is not null)
		//	{
		//		if (basketProduct.Count == 1)
		//			basket.Remove(basketProduct);

		//		basketProduct.Count--;
		//	}

		//	Response.Cookies.Append("basket", JsonConvert.SerializeObject(basket));
		//	return RedirectToAction(nameof(Index));
		//}
	}
}
