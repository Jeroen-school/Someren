// Fade out alerts after 5 seconds
$(document).ready(function () {
    setTimeout(function () {
        $(".alert:not(.alert-warning)").fadeOut("slow");
    }, 60000);

    // Add animation class to modals when shown
    $('.modal').on('show.bs.modal', function () {
        $(this).find('.modal-content').addClass('fade-in');
    });

    // Modal cleanup when hidden - fixes backdrop issue
    $('.modal').on('hidden.bs.modal', function () {
        document.body.classList.remove('modal-open');
        $('.modal-backdrop').remove();
    });

    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Add validation styling
    $('form').on('submit', function () {
        $(this).find('select, input').each(function () {
            if (!$(this).val()) {
                $(this).addClass('is-invalid');
            } else {
                $(this).removeClass('is-invalid');
            }
        });
    });

    // Remove validation styling on change
    $('select, input').on('change', function () {
        if ($(this).val()) {
            $(this).removeClass('is-invalid');
        }
    });

    // Drink Order specific functionality
    initDrinkOrderFunctionality();
});

// Filter functionality
document.addEventListener('DOMContentLoaded', function () {
    const showBarDutyBtn = document.getElementById('showBarDutyBtn');
    if (showBarDutyBtn) {
        showBarDutyBtn.addEventListener('click', function () {
            const rows = document.querySelectorAll('tbody tr');
            rows.forEach(row => {
                if (row.classList.contains('bar-duty-row')) {
                    row.style.display = '';
                } else {
                    row.style.display = 'none';
                }
            });
        });
    }

    const showAllBtn = document.getElementById('showAllBtn');
    if (showAllBtn) {
        showAllBtn.addEventListener('click', function () {
            const rows = document.querySelectorAll('tbody tr');
            rows.forEach(row => {
                row.style.display = '';
            });
        });
    }
});

// Drink Order functions
function initDrinkOrderFunctionality() {
    // Handle drink table row clicks
    $('#drinks-table').on('click', '.clickable-row', function () {
        const drinkId = $(this).data('drink-id');
        selectDrink(drinkId);
    });

    // Update price preview when inputs change
    $('#DrinkId, #Quantity').on('change keyup', function () {
        updatePricePreview();
    });

    // Initialize price preview on page load
    updatePricePreview();
}

function updatePricePreview() {
    const drinkSelect = document.getElementById('DrinkId');
    const quantityInput = document.getElementById('Quantity');
    const unitPriceElement = document.getElementById('unit-price');
    const quantityPreviewElement = document.getElementById('quantity-preview');
    const totalPriceElement = document.getElementById('total-price');

    if (!drinkSelect || !quantityInput || !unitPriceElement || !quantityPreviewElement || !totalPriceElement) {
        return; // Not on the drink order page
    }

    let unitPrice = 0;
    if (drinkSelect.selectedIndex > 0) {
        const selectedOption = drinkSelect.options[drinkSelect.selectedIndex];
        unitPrice = parseFloat(selectedOption.dataset.price);
    }

    const quantity = parseInt(quantityInput.value) || 1;
    const totalPrice = unitPrice * quantity;

    unitPriceElement.textContent = `€${unitPrice.toFixed(2)}`;
    quantityPreviewElement.textContent = quantity;
    totalPriceElement.textContent = `€${totalPrice.toFixed(2)}`;
}

function selectDrink(drinkId) {
    const drinkSelect = document.getElementById('DrinkId');
    if (!drinkSelect) return;

    for (let i = 0; i < drinkSelect.options.length; i++) {
        if (drinkSelect.options[i].value === drinkId) {
            drinkSelect.selectedIndex = i;
            break;
        }
    }
    updatePricePreview();
    scrollToElement('#Quantity');
}

function scrollToElement(selector) {
    const element = document.querySelector(selector);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'center' });
        setTimeout(() => {
            element.focus();
        }, 500);
    }
}

// Drink Order functions
function initDrinkOrderFunctionality() {
    // Handle drink table row clicks
    $('#drinks-table').on('click', '.clickable-row', function () {
        const drinkId = $(this).data('drink-id');
        selectDrink(drinkId);
    });

    // Update price preview when inputs change
    $('#DrinkId, #Quantity').on('change keyup', function () {
        updatePricePreview();
    });

    // Initialize price preview on page load
    updatePricePreview();
}

function selectDrink(drinkId) {
    // Get the dropdown element
    const drinkSelect = document.getElementById('DrinkId');
    if (!drinkSelect) return;

    // Set the value directly
    drinkSelect.value = drinkId;

    // Trigger the change event manually to update any listeners
    const event = new Event('change');
    drinkSelect.dispatchEvent(event);

    // Highlight the dropdown to show it changed
    $('#DrinkId').addClass('border-primary');
    setTimeout(function () {
        $('#DrinkId').removeClass('border-primary');
    }, 1000);
}