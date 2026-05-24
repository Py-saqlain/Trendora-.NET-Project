document.addEventListener('DOMContentLoaded', function () {
    // Update cart count to 0
    const cartCount = document.getElementById('cartCount');
    if (cartCount) {
        cartCount.textContent = '0';
        cartCount.style.display = 'none';
    }

    // Print-friendly functionality
    const printButton = document.createElement('button');
    printButton.className = 'btn btn-outline-secondary position-fixed d-none d-md-block';
    printButton.style.cssText = 'bottom: 20px; right: 20px; z-index: 1000;';
    printButton.innerHTML = '<i class="fas fa-print me-2"></i> Print Receipt';
    printButton.onclick = () => window.print();
    document.body.appendChild(printButton);

    // Add mobile-friendly touch enhancements
    if ('ontouchstart' in window) {
        document.querySelectorAll('.btn').forEach(btn => {
            btn.classList.add('touch-target');
        });
    }
});



document.addEventListener('DOMContentLoaded', function () {
    // Form validation
    const form = document.getElementById('checkoutForm');
    const termsCheck = document.getElementById('termsCheck');
    const submitBtn = form.querySelector('button[type="submit"]');

    form.addEventListener('submit', function (e) {
        if (!termsCheck.checked) {
            e.preventDefault();
            alert('Please agree to the Terms & Conditions');
            return false;
        }

        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i> Processing...';
        submitBtn.disabled = true;
    });
});