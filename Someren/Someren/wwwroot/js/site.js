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