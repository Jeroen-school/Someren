/**
 * Main document ready function - executes when the DOM is fully loaded (using jQuery)
 * Sets up various UI behaviors for the entire application
 */
$(document).ready(function () {
    // Auto-hide alerts after 60 seconds (except warning alerts)
    // Creates a disappearing notification effect for success/error messages
    setTimeout(function () {
        $(".alert:not(.alert-warning)").fadeOut("slow");
    }, 60000);

    // Adds a fade-in animation to modal dialogs when they appear
    // Makes the modals appear more elegantly with an animation
    $('.modal').on('show.bs.modal', function () {
        $(this).find('.modal-content').addClass('fade-in');
    });

    // Cleans up modal backdrop when modals are closed
    // Fixes a common Bootstrap issue where the dark overlay remains
    // after closing a modal dialog
    $('.modal').on('hidden.bs.modal', function () {
        document.body.classList.remove('modal-open');
        $('.modal-backdrop').remove();
    });

    // Initializes Bootstrap tooltips for elements with data-bs-toggle="tooltip"
    // Creates hover tooltips for buttons, icons, etc.
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Form validation styling
    // Adds visual indicators (red borders) to empty required fields on submit
    $('form').on('submit', function () {
        $(this).find('select, input').each(function () {
            if (!$(this).val()) {
                $(this).addClass('is-invalid');
            } else {
                $(this).removeClass('is-invalid');
            }
        });
    });

    // Removes validation styling when a field is filled
    // Once the user enters data, the red border disappears
    $('select, input').on('change', function () {
        if ($(this).val()) {
            $(this).removeClass('is-invalid');
        }
    });

    // Initialize drink order functionality if on a drink order page
    initDrinkOrderFunctionality();
});

/**
 * Filter functionality for bar duty table
 * Sets up event listeners for buttons that filter table content
 */
document.addEventListener('DOMContentLoaded', function () {
    // Show only bar duty rows when the bar duty button is clicked
    const showBarDutyBtn = document.getElementById('showBarDutyBtn');
    if (showBarDutyBtn) {
        showBarDutyBtn.addEventListener('click', function () {
            const rows = document.querySelectorAll('tbody tr');
            rows.forEach(row => {
                if (row.classList.contains('bar-duty-row')) {
                    row.style.display = '';  // Show bar duty rows
                } else {
                    row.style.display = 'none';  // Hide other rows
                }
            });
        });
    }

    // Show all rows when the show all button is clicked
    const showAllBtn = document.getElementById('showAllBtn');
    if (showAllBtn) {
        showAllBtn.addEventListener('click', function () {
            const rows = document.querySelectorAll('tbody tr');
            rows.forEach(row => {
                row.style.display = '';  // Show all rows
            });
        });
    }
});

/**
 * Initializes functionality specific to the Drink Order page
 * Sets up event handlers for the drink ordering process
 */
function initDrinkOrderFunctionality() {
    // When a drink row is clicked in the table, select it in the dropdown
    $('#drinks-table').on('click', '.clickable-row', function () {
        const drinkId = $(this).data('drink-id');
        selectDrink(drinkId);
    });

    // Update the price preview when drink or quantity changes
    $('#DrinkId, #Quantity').on('change keyup', function () {
        updatePricePreview();
    });

    // Initialize price preview when the page loads
    updatePricePreview();
}

/**
 * Updates the price preview display in the drink order form
 * Calculates the total based on selected drink and quantity
 */
function updatePricePreview() {
    // Get all the relevant elements
    const drinkSelect = document.getElementById('DrinkId');
    const quantityInput = document.getElementById('Quantity');
    const unitPriceElement = document.getElementById('unit-price');
    const quantityPreviewElement = document.getElementById('quantity-preview');
    const totalPriceElement = document.getElementById('total-price');

    // Exit if not on the drink order page (elements don't exist)
    if (!drinkSelect || !quantityInput || !unitPriceElement || !quantityPreviewElement || !totalPriceElement) {
        return; // Not on the drink order page
    }

    // Get the unit price from the selected drink option
    let unitPrice = 0;
    if (drinkSelect.selectedIndex > 0) {
        const selectedOption = drinkSelect.options[drinkSelect.selectedIndex];
        unitPrice = parseFloat(selectedOption.dataset.price);
    }

    // Calculate the total price based on quantity
    const quantity = parseInt(quantityInput.value) || 1;
    const totalPrice = unitPrice * quantity;

    // Update the display with formatted prices (Euro symbol and 2 decimal places)
    unitPriceElement.textContent = `€${unitPrice.toFixed(2)}`;
    quantityPreviewElement.textContent = quantity;
    totalPriceElement.textContent = `€${totalPrice.toFixed(2)}`;
}

/**
 * Scrolls smoothly to an element and focuses it
 * Used to guide the user to the next step in the form
 */
function scrollToElement(selector) {
    const element = document.querySelector(selector);
    if (element) {
        // Scroll the element into view with smooth animation
        element.scrollIntoView({ behavior: 'smooth', block: 'center' });

        // Focus the element after scrolling is complete
        setTimeout(() => {
            element.focus();
        }, 500);
    }
}

/**
 * Selects a drink in the dropdown by ID
 * Used when a drink is clicked in the table
 */
function selectDrink(drinkId) {
    // Get the dropdown element
    const drinkSelect = document.getElementById('DrinkId');
    if (!drinkSelect) return;

    // Try direct value setting first (more reliable)
    drinkSelect.value = drinkId;

    // If direct setting didn't work (older browsers), try using selectedIndex
    // This serves as a fallback method
    if (drinkSelect.value !== drinkId) {
        for (let i = 0; i < drinkSelect.options.length; i++) {
            if (drinkSelect.options[i].value === drinkId) {
                drinkSelect.selectedIndex = i;
                break;
            }
        }
    }

    // Trigger the change event manually to update any listeners
    const event = new Event('change');
    drinkSelect.dispatchEvent(event);

    // Highlight the dropdown briefly with a border color change
    // Creates a visual feedback that the selection changed
    $('#DrinkId').addClass('border-primary');
    setTimeout(function () {
        $('#DrinkId').removeClass('border-primary');
    }, 1000);

    // Update the price display with the new selection
    updatePricePreview();

    // Scroll to quantity field for easy adjustment
    scrollToElement('#Quantity');
}