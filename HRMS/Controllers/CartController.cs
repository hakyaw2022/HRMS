using HRMS.Data;
using HRMS.Helpers;
using HRMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HRMS.Controllers
{
    [Authorize(Roles ="Admin, Receptionist")]
    [Route("cart")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Route("index")]
        public async Task<IActionResult> IndexAsync()
        {
            var rooms = await _context.Room.Where(s => s.RoomStatus == RoomStatus.Occupied).ToListAsync();
            var cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            ViewBag.cart = cart;
            if (rooms.Any())
            {
                ViewBag.rooms = rooms;
            }            
            else
            {
                ViewBag.rooms = "";
            }
            if(cart is not null)
            {
                ViewBag.total = cart.Sum(item => item.Restaurant.UnitPrice * item.OrderQuantity);
            }
            
            return View();
        }

        [Route("buy/{id}")]
        public async Task<IActionResult> BuyAsync(int id)
        {
            
            if (SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart") == null)
            {
                List<Item> cart = new List<Item>();
                Restaurant restaurant = _context.Restaurant.First(x => x.Id == id);
                cart.Add(new Item { Restaurant = restaurant, OrderQuantity = 1 });
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            }
            else
            {
                List<Item> cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
                int index = isExist(id);
                if (index != -1)
                {
                    cart[index].OrderQuantity++;
                }
                else
                {
                    Restaurant? restaurant = await _context.Restaurant.FirstOrDefaultAsync(x => x.Id == id);
                    if(!ReferenceEquals(restaurant, null))
                    {
                        cart.Add(new Item { Restaurant = restaurant, OrderQuantity = 1 });
                    }
                    
                }
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            }
            return RedirectToAction("Order" ,"Restaurants");
        }

        [Route("remove/{id}")]
        public IActionResult Remove(int id)
        {
            List<Item> cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            int index = isExist(id);
            cart.RemoveAt(index);
            SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            return RedirectToAction("Index");
        }

        [Route("charge/{id}")]
        public async Task<IActionResult> charge(int id , [Bind("TransactionType,TransactionStatus,Amount,Room,Comment")] Transaction transaction)
        {
            var cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            transaction.Room = await _context.Room.FirstAsync(s => s.Id == id);            
            transaction.Amount = cart.Sum(item => item.Restaurant.UnitPrice * item.OrderQuantity);
            transaction.TransactionStatus = TransactionStatus.Active;      
            transaction.TransactionType = TransactionType.Restaurant;
            transaction.Comment = "";
            int count = 0;
            foreach(var item in cart)
            {
                if(count == 0)
                {
                    transaction.Comment += item.Restaurant.Name + "-" + item.OrderQuantity + "-" + item.Restaurant.UnitPrice;
                }
                else
                {
                    transaction.Comment += "--" + item.Restaurant.Name + "-" + item.OrderQuantity + "-" + item.Restaurant.UnitPrice;
                }
                count++;
               
            }

            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                SessionHelper.DeleteObjectFromJson(HttpContext.Session, "cart");
                return RedirectToAction("Order", "Restaurants");
            }
            return RedirectToAction("Index", "Cart");
        }

        [Route("charge_now")]
        public async Task<IActionResult> charge_now([Bind("TransactionType,TransactionStatus,Amount,Comment")] Transaction transaction, IEnumerable<Item>? cart)
        {
                cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart"); 
                
                transaction.Amount = cart.Sum(item => item.Restaurant.UnitPrice * item.OrderQuantity);
                transaction.TransactionStatus = TransactionStatus.Inactive;
                transaction.TransactionType = TransactionType.Restaurant;
                transaction.Comment = "";
            
            int count = 0;
            foreach (var item in cart)
            {
                if (count == 0)
                {
                    transaction.Comment += item.Restaurant.Name + "-" + item.OrderQuantity + "-" + item.Restaurant.UnitPrice;
                }
                else
                {
                    transaction.Comment += "--" + item.Restaurant.Name + "-" + item.OrderQuantity + "-" + item.Restaurant.UnitPrice;
                }
                count++;

            }

            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();

                var transactionSplit = transaction.Comment.Split("--");
                string fmt = "yyyyMdHHmmss";
                DateTime createdDate = DateTime.Now;
                string receiptNumber = createdDate.ToString(fmt);
                if (transactionSplit.Count() > 1)
                {
                    foreach (var item in transactionSplit)
                    {
                        var itemSplit = item.Split("-");
                        var receipt = new Receipt();
                        receipt.Name = itemSplit[0] + "-" + itemSplit[1];
                        receipt.Amount = int.Parse(itemSplit[2]) * int.Parse(itemSplit[1]);
                        receipt.CreatedDate = createdDate;
                        receipt.ReceiptNumber = receiptNumber;
                        receipt.TransactionType = TransactionType.Restaurant;
                        _context.Add(receipt);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    var item = transactionSplit[0].Split("-");
                    var receipt = new Receipt();
                    receipt.Name = item[0] + "-" + item[1];
                    receipt.Amount = int.Parse(item[2]) * int.Parse(item[1]); ;
                    receipt.CreatedDate = createdDate;
                    receipt.ReceiptNumber = receiptNumber;
                    receipt.TransactionType = TransactionType.Restaurant;
                    _context.Add(receipt);
                    await _context.SaveChangesAsync();

                }
                SessionHelper.DeleteObjectFromJson(HttpContext.Session, "cart");
                return RedirectToAction("Order", "Restaurants");
            }
            
            return RedirectToAction("Index", "Cart");
        }
        private int isExist(int id)
        {
            List<Item> cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            for (int i = 0; i < cart.Count; i++)
            {
                if (cart[i].Restaurant.Id.Equals(id))
                {
                    return i;
                }
            }
            return -1;
        }

    }
}