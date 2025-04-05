using Microsoft.AspNetCore.Mvc;
using Someren.Models;
using Someren.Repositories;
using System;
using System.Linq;
using System.Transactions;
using System.Collections.Generic;

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

        // GET: DrinkOrder/Index
        public IActionResult Index(string searchName = "")
        {
            try
            {
                ViewBag.Drinks = _drinkRepository.GetAll();
                ViewBag.Lecturers = _lecturerRepository.GetAll(false);

                // If search name is provided, use filtered list, otherwise get all students
                ViewBag.Students = string.IsNullOrEmpty(searchName)
                    ? _studentsRepository.GetAll()
                    : _studentsRepository.GetFiltered(searchName);

                ViewBag.SearchName = searchName ?? "";
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading data: {ex.Message}";
                return View(new DrinkOrder());
            }
        }

        // POST: DrinkOrder/PlaceOrder
        [HttpPost]
        public IActionResult PlaceOrder(DrinkOrder order)
        {
            if (order == null)
            {
                TempData["ErrorMessage"] = "Invalid order data submitted.";
                return RedirectToAction("Index");
            }

            try
            {
                // Perform all validations in a single method
                var (isValid, student, drink, errorMessage, errorField) = ValidateOrder(order);

                if (!isValid)
                {
                    // Set appropriate error message
                    ViewBag[$"{errorField}Error"] = errorMessage;

                    // Re-populate needed data for the view
                    PopulateViewBagForOrderForm();
                    return View("Index", order);
                }

                // Store data for confirmation view
                ViewBag.Student = student;
                ViewBag.Drink = drink;
                ViewBag.TotalPrice = drink?.DrinkPrice * order.Quantity;

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
            if (order == null)
            {
                TempData["ErrorMessage"] = "Invalid order data submitted.";
                return RedirectToAction("Index");
            }

            try
            {
                // Re-validate the order before final processing
                var (isValid, student, drink, errorMessage, _) = ValidateOrder(order);

                if (!isValid)
                {
                    TempData["ErrorMessage"] = errorMessage;
                    return RedirectToAction("Index");
                }

                // Process order using transaction scope to ensure consistency
                using (var scope = new TransactionScope())
                {
                    // Add order to database
                    _orderRepository.AddOrder(order);

                    // Update stock level
                    _drinkRepository.UpdateStock(order.DrinkId, order.Quantity);

                    // Complete the transaction
                    scope.Complete();
                }

                // Format currency properly
                string formattedPrice = (drink.DrinkPrice * order.Quantity).ToString("F2");
                TempData["SuccessMessage"] = $"Order successfully placed for {student?.FirstName} {student?.LastName}. {order.Quantity} × {drink.DrinkType} for €{formattedPrice}";

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

                // Use LINQ to efficiently calculate totals in a single pass
                var (totalInventoryValue, totalAlcoholicValue, totalNonAlcoholicValue) = CalculateInventoryValues(drinks);
                var (totalConsumption, alcoholicConsumption, nonAlcoholicConsumption) = CalculateConsumptionMetrics(drinks);

                // Set ViewBag values
                ViewBag.TotalInventoryValue = totalInventoryValue;
                ViewBag.TotalAlcoholicValue = totalAlcoholicValue;
                ViewBag.TotalNonAlcoholicValue = totalNonAlcoholicValue;
                ViewBag.TotalConsumption = totalConsumption;
                ViewBag.AlcoholicConsumption = alcoholicConsumption;
                ViewBag.NonAlcoholicConsumption = nonAlcoholicConsumption;

                return View(drinks);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading inventory data: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private (bool isValid, Student? student, Drink? drink, string? errorMessage, string? errorField) ValidateOrder(DrinkOrder order)
        {
            // Validate student exists
            var student = _studentsRepository.GetByNum(order.StudentId);
            if (student == null)
            {
                return (false, null, null, "Selected student not found in the system.", "StudentId");
            }

            // Validate drink exists
            var drink = _drinkRepository.GetById(order.DrinkId);
            if (drink == null)
            {
                return (false, student, null, "Selected drink is not available.", "DrinkId");
            }

            // Quantity validation
            if (order.Quantity <= 0)
            {
                return (false, student, drink, "Quantity must be greater than zero.", "Quantity");
            }

            if (order.Quantity > 20)
            {
                return (false, student, drink, "Maximum order quantity is 20 units per transaction.", "Quantity");
            }

            // Stock validation
            if (drink.DrinkStock < order.Quantity)
            {
                return (false, student, drink, $"Not enough stock available. Current stock: {drink.DrinkStock}", "Quantity");
            }

            // All validations passed
            return (true, student, drink, null, null);
        }

        private void PopulateViewBagForOrderForm()
        {
            ViewBag.Students = _studentsRepository.GetAll();
            ViewBag.Drinks = _drinkRepository.GetAll();
        }

        private (decimal total, decimal alcoholic, decimal nonAlcoholic) CalculateInventoryValues(IEnumerable<Drink> drinks)
        {
            decimal total = 0;
            decimal alcoholic = 0;
            decimal nonAlcoholic = 0;

            foreach (var drink in drinks)
            {
                decimal value = drink.DrinkPrice * drink.DrinkStock;
                total += value;

                if (drink.VatRate == 21)
                {
                    alcoholic += value;
                }
                else
                {
                    nonAlcoholic += value;
                }
            }

            return (total, alcoholic, nonAlcoholic);
        }

        private (int total, int alcoholic, int nonAlcoholic) CalculateConsumptionMetrics(IEnumerable<Drink> drinks)
        {
            int total = 0;
            int alcoholic = 0;
            int nonAlcoholic = 0;

            foreach (var drink in drinks)
            {
                total += drink.DrinkConsumed;

                if (drink.VatRate == 21)
                {
                    alcoholic += drink.DrinkConsumed;
                }
                else
                {
                    nonAlcoholic += drink.DrinkConsumed;
                }
            }

            return (total, alcoholic, nonAlcoholic);
        }
    }
}