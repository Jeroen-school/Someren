using Microsoft.AspNetCore.Mvc;
using Someren.Models;
using Someren.Repositories;
using System;

namespace Someren.Controllers
{
    public class DrinkOrderController : Controller
    {
        private readonly IStudentsRepository _studentsRepository;
        private readonly IDrinkRepository _drinkRepository;
        private readonly IDrinkOrderRepository _orderRepository;
        private readonly ILecturersRepository _lecturerRepository;

        public DrinkOrderController(
            IStudentsRepository studentsRepository,
            IDrinkRepository drinkRepository,
            IDrinkOrderRepository orderRepository,
            ILecturersRepository lecturersRepository)
        {
            _studentsRepository = studentsRepository;
            _drinkRepository = drinkRepository;
            _orderRepository = orderRepository;
            _lecturerRepository = lecturersRepository;
        }

        /// GET: DrinkOrder/Index
        public IActionResult Index(string searchName = "")
        {
            ViewBag.Drinks = _drinkRepository.GetAll();
            ViewBag.Lecturers = _lecturerRepository.GetAll(false);

            // If search name is provided, use filtered list, otherwise get all students
            if (!string.IsNullOrEmpty(searchName))
            {
                ViewBag.Students = _studentsRepository.GetFiltered(searchName);
                ViewBag.SearchName = searchName; // Store search term for the form
            }
            else
            {
                ViewBag.Students = _studentsRepository.GetAll();
                ViewBag.SearchName = "";
            }

            return View();
        }

        // POST: DrinkOrder/PlaceOrder
        [HttpPost]
        public IActionResult PlaceOrder(DrinkOrder order)
        {
            try
            {
                // Input validation
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Invalid order data submitted.";
                    return RedirectToAction("Index");
                }

                // Validate student exists
                var student = _studentsRepository.GetByNum(order.StudentId);
                if (student == null)
                {
                    ViewBag.StudentIdError = "Selected student not found in the system.";
                    ViewBag.Students = _studentsRepository.GetAll();
                    ViewBag.Drinks = _drinkRepository.GetAll();
                    return View("Index", order);
                }

                // Validate drink exists
                var drink = _drinkRepository.GetById(order.DrinkId);
                if (drink == null)
                {
                    ViewBag.DrinkIdError = "Selected drink is not available.";
                    ViewBag.Students = _studentsRepository.GetAll();
                    ViewBag.Drinks = _drinkRepository.GetAll();
                    return View("Index", order);
                }

                // Quantity validation
                if (order.Quantity <= 0)
                {
                    ViewBag.QuantityError = "Quantity must be greater than zero.";
                    ViewBag.Students = _studentsRepository.GetAll();
                    ViewBag.Drinks = _drinkRepository.GetAll();
                    return View("Index", order);
                }

                if (order.Quantity > 20)
                {
                    ViewBag.QuantityError = "Maximum order quantity is 20 units per transaction.";
                    ViewBag.Students = _studentsRepository.GetAll();
                    ViewBag.Drinks = _drinkRepository.GetAll();
                    return View("Index", order);
                }

                // Stock validation
                if (drink.DrinkStock < order.Quantity)
                {
                    ViewBag.QuantityError = $"Not enough stock available. Current stock: {drink.DrinkStock}";
                    ViewBag.Students = _studentsRepository.GetAll();
                    ViewBag.Drinks = _drinkRepository.GetAll();
                    return View("Index", order);
                }

                // Store data for confirmation view
                ViewBag.Student = student;
                ViewBag.Drink = drink;
                ViewBag.TotalPrice = drink.DrinkPrice * order.Quantity;

                return View("Confirm", order);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error processing order: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: DrinkOrder/Confirm
        [HttpPost]
        public IActionResult Confirm(DrinkOrder order)
        {
            try
            {
                // Input validation
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Invalid order data submitted.";
                    return RedirectToAction("Index");
                }

                // Final validation checks
                var drink = _drinkRepository.GetById(order.DrinkId);
                if (drink == null)
                {
                    TempData["ErrorMessage"] = "The selected drink is no longer available.";
                    return RedirectToAction("Index");
                }

                // Check if student exists
                var student = _studentsRepository.GetByNum(order.StudentId);
                if (student == null)
                {
                    TempData["ErrorMessage"] = "The selected student no longer exists.";
                    return RedirectToAction("Index");
                }

                // Verify stock again (in case it changed between PlaceOrder and Confirm)
                if (drink.DrinkStock < order.Quantity)
                {
                    TempData["ErrorMessage"] = $"Stock level has changed. Only {drink.DrinkStock} units of {drink.DrinkType} remain.";
                    return RedirectToAction("Index");
                }

                // Process order
                try
                {
                    _orderRepository.AddOrder(order);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Failed to register the order: {ex.Message}";
                    return RedirectToAction("Index");
                }

                // Update drink stock
                try
                {
                    _drinkRepository.UpdateStock(order.DrinkId, order.Quantity);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Order was placed but failed to update inventory: {ex.Message}";
                    return RedirectToAction("Index");
                }

                TempData["SuccessMessage"] = $"Order successfully placed for {student.FirstName} {student.LastName}. {order.Quantity} × {drink.DrinkType} for €{(drink.DrinkPrice * order.Quantity).ToString("F2")}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error confirming order: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: DrinkOrder/Stock
        public IActionResult Stock()
        {
            try
            {
                var drinks = _drinkRepository.GetAll();

                // Calculate total inventory value
                decimal totalInventoryValue = 0;
                decimal totalAlcoholicValue = 0;
                decimal totalNonAlcoholicValue = 0;

                foreach (var drink in drinks)
                {
                    decimal inventoryValue = drink.DrinkPrice * drink.DrinkStock;
                    totalInventoryValue += inventoryValue;

                    if (drink.VatRate == 21)
                    {
                        totalAlcoholicValue += inventoryValue;
                    }
                    else
                    {
                        totalNonAlcoholicValue += inventoryValue;
                    }
                }

                ViewBag.TotalInventoryValue = totalInventoryValue;
                ViewBag.TotalAlcoholicValue = totalAlcoholicValue;
                ViewBag.TotalNonAlcoholicValue = totalNonAlcoholicValue;

                // Calculate consumption metrics
                int totalConsumption = drinks.Sum(d => d.DrinkConsumed);
                ViewBag.TotalConsumption = totalConsumption;
                ViewBag.AlcoholicConsumption = drinks.Where(d => d.VatRate == 21).Sum(d => d.DrinkConsumed);
                ViewBag.NonAlcoholicConsumption = drinks.Where(d => d.VatRate == 9).Sum(d => d.DrinkConsumed);

                return View(drinks);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading inventory data: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}