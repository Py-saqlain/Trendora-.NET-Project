// Cart functionality - Only runs on cart page
(function () {
    'use strict';

    // Check if we're on the cart page
    const isCartPage = window.location.pathname.toLowerCase().includes('/cart');

    if (!isCartPage) {
        console.log('Not on cart page, skipping Cart.js initialization');
        return;
    }

    console.log('Initializing Cart.js on cart page');

    // Safe DOM access functions
    function $(selector) {
        try {
            return document.querySelector(selector);
        } catch (e) {
            console.warn('Query selector failed:', selector);
            return null;
        }
    }

    function $$(selector) {
        try {
            return document.querySelectorAll(selector);
        } catch (e) {
            console.warn('Query selector all failed:', selector);
            return [];
        }
    }

    // Initialize when DOM is loaded
    document.addEventListener('DOMContentLoaded', function () {
        console.log('Cart.js initialized on cart page');
        bindCartButtons();
        updateCartCount();
    });

    function bindCartButtons() {
        console.log('Binding cart buttons...');

        // Use event delegation for better performance
        document.addEventListener('click', function (e) {
            // Handle plus button
            if (e.target.closest('.plus') || (e.target.classList && e.target.classList.contains('plus'))) {
                e.preventDefault();
                const btn = e.target.closest('.plus') || e.target;
                console.log('Plus clicked:', btn.dataset);
                changeQty(btn, 1);
            }

            // Handle minus button
            if (e.target.closest('.minus') || (e.target.classList && e.target.classList.contains('minus'))) {
                e.preventDefault();
                const btn = e.target.closest('.minus') || e.target;
                console.log('Minus clicked:', btn.dataset);
                changeQty(btn, -1);
            }

            // Handle remove button
            if (e.target.closest('.remove-btn') || (e.target.classList && e.target.classList.contains('remove-btn'))) {
                e.preventDefault();
                const btn = e.target.closest('.remove-btn') || e.target;
                console.log('Remove clicked:', btn.dataset);
                removeItem(btn);
            }
        });

        // Clear cart button
        const clearBtn = $('#clearCartBtn');
        if (clearBtn) {
            clearBtn.addEventListener('click', function (e) {
                e.preventDefault();
                clearCart();
            });
        }

        // Log button count
        const plusButtons = $$('.plus');
        const minusButtons = $$('.minus');
        console.log(`Found ${plusButtons.length} plus buttons and ${minusButtons.length} minus buttons`);
    }

    function changeQty(btn, delta) {
        const id = btn.dataset.productId;
        const size = btn.dataset.size || '';
        const color = btn.dataset.color || '';

        console.log('Changing quantity:', { id, size, color, delta });

        // Find input
        const selector = `.quantity-input[data-product-id="${id}"][data-size="${size}"][data-color="${color}"]`;
        const input = $(selector);

        if (!input) {
            console.error('Input not found with selector:', selector);
            showNotification('Could not find item', 'error');
            return;
        }

        let qty = parseInt(input.value) + delta;

        if (qty < 1) {
            removeItem(btn);
            return;
        }

        // Update the input value immediately for better UX
        input.value = qty;

        // Call API
        updateQuantity(id, qty, size, color);
    }

    function updateQuantity(id, qty, size, color) {
        showLoading();

        const token = $('input[name="__RequestVerificationToken"]')?.value;

        if (!token) {
            console.error('Anti-forgery token not found');
            showNotification('Security token missing', 'error');
            hideLoading();
            return;
        }

        console.log('Updating quantity:', { id, qty, size, color });

        fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                productId: parseInt(id),
                quantity: qty,
                size: size || null,
                color: color || null
            })
        })
            .then(handleResponse)
            .then(function (data) {
                console.log('Update response:', data);

                if (data.success) {
                    // Update item total
                    const selector = `.item-total[data-product-id="${id}"][data-size="${size}"][data-color="${color}"]`;
                    const itemTotal = $(selector);
                    if (itemTotal) {
                        itemTotal.textContent = '$' + parseFloat(data.itemTotal).toFixed(2);
                    }

                    // Update summary
                    updateSummary(data.cartTotal, data.cartCount, data.subtotal, data.tax, data.grandTotal);
                    showNotification(data.message || 'Quantity updated', 'success');
                } else {
                    showNotification(data.message || 'Failed to update quantity', 'error');
                    // Revert input value
                    const inputSelector = `.quantity-input[data-product-id="${id}"][data-size="${size}"][data-color="${color}"]`;
                    const input = $(inputSelector);
                    if (input) {
                        // Reload page to get correct values
                        setTimeout(function () { location.reload(); }, 1000);
                    }
                }
            })
            .catch(function (error) {
                console.error('Error:', error);
                showNotification('An error occurred. Please try again.', 'error');
                setTimeout(function () { location.reload(); }, 1500);
            })
            .finally(function () {
                hideLoading();
                updateCartCount();
            });
    }

    function removeItem(btn) {
        if (!confirm('Are you sure you want to remove this item from your cart?')) {
            return;
        }

        const id = btn.dataset.productId;
        const size = btn.dataset.size || '';
        const color = btn.dataset.color || '';

        showLoading();

        const token = $('input[name="__RequestVerificationToken"]')?.value;

        if (!token) {
            console.error('Anti-forgery token not found');
            showNotification('Security token missing', 'error');
            hideLoading();
            return;
        }

        fetch('/Cart/RemoveItem', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                productId: parseInt(id),
                size: size || null,
                color: color || null
            })
        })
            .then(handleResponse)
            .then(function (data) {
                console.log('Remove response:', data);

                if (data.success) {
                    // Remove item from UI
                    const safeSize = (size || 'nosize').replace(/\s+/g, '_');
                    const safeColor = (color || 'nocolor').replace(/\s+/g, '_');
                    const itemId = 'cartItem_' + id + '_' + safeSize + '_' + safeColor;
                    const itemElement = $('#' + itemId);

                    if (itemElement) {
                        itemElement.style.opacity = '0.5';
                        setTimeout(function () {
                            if (itemElement.parentNode) {
                                itemElement.remove();
                            }
                        }, 300);
                    }

                    updateSummary(data.cartTotal, data.cartCount, data.subtotal, data.tax, data.grandTotal);
                    showNotification(data.message || 'Item removed', 'success');

                    // If cart is empty, reload
                    if (data.cartCount === 0) {
                        setTimeout(function () { location.reload(); }, 1000);
                    }
                } else {
                    showNotification(data.message || 'Failed to remove item', 'error');
                }
            })
            .catch(function (error) {
                console.error('Error:', error);
                showNotification('An error occurred. Please try again.', 'error');
            })
            .finally(function () {
                hideLoading();
                updateCartCount();
            });
    }

    function clearCart() {
        if (!confirm('Are you sure you want to clear your entire cart? This cannot be undone.')) {
            return;
        }

        showLoading();

        const token = $('input[name="__RequestVerificationToken"]')?.value;

        if (!token) {
            console.error('Anti-forgery token not found');
            showNotification('Security token missing', 'error');
            hideLoading();
            return;
        }

        fetch('/Cart/Clear', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token,
                'Content-Type': 'application/json'
            }
        })
            .then(handleResponse)
            .then(function (data) {
                console.log('Clear response:', data);

                if (data.success) {
                    showNotification(data.message || 'Cart cleared', 'success');
                    setTimeout(function () { location.reload(); }, 1000);
                } else {
                    showNotification(data.message || 'Failed to clear cart', 'error');
                }
            })
            .catch(function (error) {
                console.error('Error:', error);
                showNotification('An error occurred. Please try again.', 'error');
            })
            .finally(function () {
                hideLoading();
                updateCartCount();
            });
    }

    function handleResponse(response) {
        if (!response.ok) {
            throw new Error('HTTP error ' + response.status);
        }
        return response.json();
    }

    function updateSummary(cartTotal, cartCount, subtotal, tax, grandTotal) {
        try {
            const cartTotalItems = $('#cartTotalItems');
            const cartSubtotal = $('#cartSubtotal');
            const taxAmount = $('#taxAmount');
            const cartGrandTotal = $('#cartGrandTotal');

            if (cartTotalItems) {
                cartTotalItems.textContent = cartCount || 0;
            }

            if (cartSubtotal) {
                cartSubtotal.textContent = (subtotal || cartTotal || 0).toFixed(2);
            }

            if (taxAmount) {
                taxAmount.textContent = (tax || (cartTotal * 0.08) || 0).toFixed(2);
            }

            if (cartGrandTotal) {
                const total = grandTotal ||
                    ((parseFloat(cartSubtotal?.textContent || 0) +
                        parseFloat(taxAmount?.textContent || 0) + 5)).toFixed(2);
                cartGrandTotal.textContent = total;
            }
        } catch (error) {
            console.warn('Error updating summary:', error);
        }
    }

    function updateCartCount() {
        fetch('/Cart/GetCartCount')
            .then(handleResponse)
            .then(function (data) {
                if (data.success) {
                    const el = $('#cartCount');
                    if (el) {
                        el.textContent = data.count;
                        el.style.display = data.count > 0 ? 'inline-block' : 'none';
                    }
                }
            })
            .catch(function (error) {
                console.error('Error fetching cart count:', error);
            });
    }

    function showNotification(message, type) {
        // Remove existing notifications
        $$('.alert.position-fixed').forEach(function (alert) {
            alert.remove();
        });

        // Create notification
        const notification = document.createElement('div');
        notification.className = 'alert alert-' + (type === 'success' ? 'success' : 'danger') +
            ' alert-dismissible fade show position-fixed';
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';

        const icon = type === 'success' ? 'check-circle' : 'exclamation-circle';
        notification.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="fas fa-${icon} me-2"></i>
                <div>${message}</div>
            </div>
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(notification);

        // Auto-remove
        setTimeout(function () {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 3000);
    }

    function showLoading() {
        const overlay = $('#loadingOverlay');
        if (overlay) {
            overlay.style.display = 'flex';
        }
    }

    function hideLoading() {
        const overlay = $('#loadingOverlay');
        if (overlay) {
            overlay.style.display = 'none';
        }
    }

    // Make functions available globally (for debugging)
    window.Cart = {
        bindCartButtons: bindCartButtons,
        updateCartCount: updateCartCount,
        changeQty: changeQty,
        removeItem: removeItem,
        clearCart: clearCart
    };
})();