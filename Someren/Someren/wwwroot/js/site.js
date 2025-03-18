// Fade out alerts after 5 seconds
$(document).ready(function () {
    setTimeout(function () {
        $(".alert:not(.alert-warning)").fadeOut("slow");
    }, 5000);

    // Add animation class to modals when shown
    $('.modal').on('show.bs.modal', function () {
        $(this).find('.modal-content').addClass('fade-in');
    });

    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});


$(document).ready(function () {
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
});

// Initialize tooltips
var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
});

// Filter functionality
document.getElementById('showBarDutyBtn').addEventListener('click', function () {
    const rows = document.querySelectorAll('tbody tr');
    rows.forEach(row => {
        if (row.classList.contains('bar-duty-row')) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
});

document.getElementById('showAllBtn').addEventListener('click', function () {
    const rows = document.querySelectorAll('tbody tr');
    rows.forEach(row => {
        row.style.display = '';
    });
});