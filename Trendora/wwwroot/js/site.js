// Explore page cart functionality
(function () {
    'use strict';

    console.log('Explore Cart.js loaded');

    function getRequestVerificationToken() {
        const inputToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (inputToken) return inputToken;
        const metaToken = document.querySelector('meta[name="request-verification-token"]')?.getAttribute('content');
        return metaToken || null;
    }

    // Safe DOM access functions
    function $(selector) {
        try {
            return document.querySelector(selector);
        } catch (e) {
            console.warn('Query selector failed:', selector);
            return null;
        }
    }

    // Initialize when DOM is loaded
    document.addEventListener('DOMContentLoaded', function () {
        console.log('Explore Cart initialized');
        updateCartCount();

        // Fix for index.js error
        try {
            const newsletterForm = $('#newsletterForm');
            if (!newsletterForm) {
                console.log('Newsletter form not found on this page, suppressing error');
            }
        } catch (e) {
            console.warn('Suppressed newsletter form error:', e);
        }
    });

    // Global function to add to cart from Explore page
    window.addToCart = function (productId, productName, quantity = 1, size = null, color = null, evt = null) {
        console.log('Adding to cart:', { productId, productName, quantity, size, color });

        // Get the button that was clicked
        const clickEvent = evt || (typeof event !== 'undefined' ? event : null);
        let button = clickEvent?.target;

        // If clicked on icon inside button, get the parent button
        if (button && !button.classList.contains('btn-add-to-cart') && !button.classList.contains('add-to-cart-from-modal')) {
            button = button.closest('.btn-add-to-cart') || button.closest('.add-to-cart-from-modal');
        }

        const originalText = button?.innerHTML;

        if (button) {
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Adding...';
            button.disabled = true;
        }

        const requestData = {
            productId: parseInt(productId),
            quantity: quantity,
            size: size,
            color: color
        };

        const token = getRequestVerificationToken();

        if (!token) {
            console.error('Anti-forgery token not found');
            showNotification('Security token missing', 'error');
            if (button) {
                button.innerHTML = originalText;
                button.disabled = false;
            }
            return;
        }

        fetch('/Cart/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(requestData)
        })
            .then(response => {
                console.log('Add to cart response status:', response.status);
                if (!response.ok) {
                    if (response.status === 404) {
                        throw new Error('Cart endpoint not found. Please check the server.');
                    }
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('Add to cart response:', data);
                if (data.success) {
                    showNotification(data.message || `${productName} added to cart!`, 'success');
                    updateCartCountUI(data.cartCount || data.count || 0);
                } else {
                    showNotification(data.message || 'Failed to add item to cart', 'error');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showNotification(error.message || 'An error occurred while adding to cart. Please try again.', 'error');
            })
            .finally(() => {
                if (button) {
                    setTimeout(() => {
                        button.innerHTML = originalText;
                        button.disabled = false;
                    }, 500);
                }
            });
    };

    function updateCartCount() {
        fetch('/Cart/GetCartCount')
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(data => {
                updateCartCountUI(data.count || 0);
            })
            .catch(error => console.error('Error fetching cart count:', error));
    }

    function updateCartCountUI(count) {
        const cartCount = $('#cartCount');
        if (cartCount) {
            cartCount.textContent = count;
            cartCount.style.display = count > 0 ? 'inline-block' : 'none';
        }
    }

    function showNotification(message, type = 'success') {
        // Remove existing notifications
        document.querySelectorAll('.alert.position-fixed').forEach(alert => alert.remove());

        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px; max-width: 400px;';
        notification.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>
                <div>${message}</div>
            </div>
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(notification);

        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 3000);
    }
})();



// WhatsApp Chat Functionality for Trendora
(function () {
    'use strict';

    // Configuration
    const WHATSAPP_NUMBER = '92329415975';
    const COMPANY_NAME = 'Trendora';
    const DEFAULT_MESSAGE = 'Hello%20Trendora%20Support,%20I%20would%20like%20to%20inquire%20about%20your%20products%20and%20services.';

    // Professional messages templates
    const MESSAGE_TEMPLATES = {
        general: 'Hello%20Trendora%20Support,%20I%20would%20like%20to%20inquire%20about%20your%20products%20and%20services.',
        order: 'Hello%20Trendora%20Support,%20I%20need%20assistance%20with%20my%20order.',
        product: 'Hello%20Trendora%20Support,%20I%20have%20a%20question%20about%20a%20product.',
        support: 'Hello%20Trendora%20Support,%20I%20need%20technical%20support.',
        shipping: 'Hello%20Trendora%20Support,%20I%20have%20a%20question%20about%20shipping%20and%20delivery.',
        return: 'Hello%20Trendora%20Support,%20I%20need%20help%20with%20a%20return%20or%20exchange.',
        custom: 'Hello%20Trendora%20Support,%20'
    };

    // WhatsApp widget state
    let widgetVisible = false;
    let chatStarted = false;

    // Initialize when DOM is loaded
    document.addEventListener('DOMContentLoaded', function () {
        console.log('WhatsApp functionality initialized');
        initWhatsApp();
        updateCartCount(); // Initialize cart count
    });

    function initWhatsApp() {
        // Get WhatsApp elements
        const whatsappFloat = document.getElementById('whatsappFloat');
        const whatsappWidget = document.getElementById('whatsappWidget');
        const closeWidgetBtn = document.getElementById('closeWidget');
        const startChatBtn = document.getElementById('startChatBtn');

        if (!whatsappFloat) {
            console.error('WhatsApp float button not found');
            return;
        }

        // WhatsApp float button click - open WhatsApp directly
        whatsappFloat.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            // Get the WhatsApp number from data attribute or use default
            const phoneNumber = this.getAttribute('data-whatsapp-number') || WHATSAPP_NUMBER;

            // Create professional message based on current page
            const message = getProfessionalMessage();

            // Open WhatsApp
            openWhatsAppChat(phoneNumber, message);

            // Track the click
            trackWhatsAppClick('float_button');
        });

        // WhatsApp widget functionality
        if (whatsappWidget) {
            // Toggle widget on float click (alternative)
            whatsappFloat.addEventListener('click', function (e) {
                if (!widgetVisible) {
                    toggleWidget();
                }
            });

            // Close widget button
            if (closeWidgetBtn) {
                closeWidgetBtn.addEventListener('click', function (e) {
                    e.stopPropagation();
                    toggleWidget();
                });
            }

            // Start chat button in widget
            if (startChatBtn) {
                startChatBtn.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();

                    const phoneNumber = WHATSAPP_NUMBER;
                    const message = getProfessionalMessage();

                    openWhatsAppChat(phoneNumber, message);
                    trackWhatsAppClick('widget_button');

                    // Close widget after starting chat
                    setTimeout(() => {
                        toggleWidget();
                    }, 500);
                });
            }

            // Close widget when clicking outside
            document.addEventListener('click', function (e) {
                if (widgetVisible &&
                    !whatsappWidget.contains(e.target) &&
                    !whatsappFloat.contains(e.target)) {
                    toggleWidget();
                }
            });
        }

        // Add keyboard shortcut (Alt + W)
        document.addEventListener('keydown', function (e) {
            if (e.altKey && e.key === 'w') {
                e.preventDefault();
                const phoneNumber = WHATSAPP_NUMBER;
                const message = getProfessionalMessage();
                openWhatsAppChat(phoneNumber, message);
            }
        });

        console.log('WhatsApp functionality ready');
    }

    function openWhatsAppChat(phoneNumber, message) {
        // Format phone number (remove any non-digit characters)
        const formattedNumber = phoneNumber.replace(/\D/g, '');

        // Create WhatsApp URL
        const whatsappUrl = `https://wa.me/${formattedNumber}?text=${message}`;

        // Open in new tab
        window.open(whatsappUrl, '_blank', 'noopener,noreferrer');

        // Show confirmation
        showNotification('Opening WhatsApp chat...', 'info');

        // Mark chat as started
        chatStarted = true;

        // Send analytics event
        sendWhatsAppAnalytics('chat_started', {
            phoneNumber: formattedNumber,
            messageType: getMessageType(message)
        });
    }

    function getProfessionalMessage() {
        // Get current page information
        const currentPage = window.location.pathname;
        const pageTitle = document.title;

        let messageType = 'general';
        let customMessage = '';

        // Determine message based on current page
        if (currentPage.includes('/Cart') || currentPage.includes('/Checkout')) {
            messageType = 'order';
            customMessage = 'I%20need%20assistance%20with%20my%20order%20or%20checkout%20process.';
        } else if (currentPage.includes('/Explore') || currentPage.includes('/Product')) {
            messageType = 'product';
            customMessage = 'I%20have%20a%20question%20about%20a%20product%20on%20your%20website.';
        } else if (currentPage.includes('/Contact')) {
            messageType = 'support';
            customMessage = 'I%20would%20like%20to%20contact%20support%20regarding%20an%20issue.';
        } else if (currentPage.includes('/Shipping') || currentPage.includes('/Delivery')) {
            messageType = 'shipping';
            customMessage = 'I%20have%20a%20question%20about%20shipping%20options%20and%20delivery%20times.';
        } else if (currentPage.includes('/Return') || currentPage.includes('/Exchange')) {
            messageType = 'return';
            customMessage = 'I%20need%20help%20with%20a%20return%20or%20exchange%20request.';
        }

        // Add page reference to message
        const pageReference = encodeURIComponent(` (Page: ${pageTitle})`);

        // Return the appropriate message
        return MESSAGE_TEMPLATES[messageType] + pageReference;
    }

    function getMessageType(message) {
        // Determine message type from content
        if (message.includes('order')) return 'order';
        if (message.includes('product')) return 'product';
        if (message.includes('support')) return 'support';
        if (message.includes('shipping')) return 'shipping';
        if (message.includes('return')) return 'return';
        return 'general';
    }

    function toggleWidget() {
        const whatsappWidget = document.getElementById('whatsappWidget');
        const whatsappFloat = document.getElementById('whatsappFloat');

        if (!whatsappWidget) return;

        widgetVisible = !widgetVisible;

        if (widgetVisible) {
            whatsappWidget.style.display = 'block';
            whatsappWidget.classList.add('show');
            whatsappFloat.classList.add('active');

            // Add animation
            setTimeout(() => {
                whatsappWidget.style.transform = 'translateY(0)';
                whatsappWidget.style.opacity = '1';
            }, 10);
        } else {
            whatsappWidget.style.transform = 'translateY(20px)';
            whatsappWidget.style.opacity = '0';

            setTimeout(() => {
                whatsappWidget.style.display = 'none';
                whatsappWidget.classList.remove('show');
                whatsappFloat.classList.remove('active');
            }, 300);
        }
    }

    function trackWhatsAppClick(source) {
        // Here you can integrate with Google Analytics, Facebook Pixel, etc.
        console.log(`WhatsApp clicked from: ${source}`);

        // Example: Send to Google Analytics (if available)
        if (typeof gtag !== 'undefined') {
            gtag('event', 'whatsapp_click', {
                'event_category': 'engagement',
                'event_label': source,
                'value': 1
            });
        }
    }

    function sendWhatsAppAnalytics(action, data) {
        // Send analytics data to your server
        fetch('/Analytics/WhatsApp', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                action: action,
                timestamp: new Date().toISOString(),
                page: window.location.pathname,
                userAgent: navigator.userAgent,
                data: data
            })
        }).catch(error => {
            console.error('Analytics error:', error);
        });
    }

    function showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `whatsapp-notification whatsapp-notification-${type}`;
        notification.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="fab fa-whatsapp me-2"></i>
                <div>${message}</div>
            </div>
            <button type="button" class="whatsapp-notification-close">
                <i class="fas fa-times"></i>
            </button>
        `;

        // Style the notification
        notification.style.cssText = `
            position: fixed;
            bottom: 100px;
            right: 30px;
            background: ${type === 'info' ? '#25D366' : type === 'success' ? '#28a745' : '#dc3545'};
            color: white;
            padding: 15px 20px;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            z-index: 10000;
            max-width: 300px;
            display: flex;
            align-items: center;
            justify-content: space-between;
            animation: slideIn 0.3s ease;
        `;

        // Add keyframes for animation
        const style = document.createElement('style');
        style.textContent = `
            @keyframes slideIn {
                from {
                    transform: translateX(100%);
                    opacity: 0;
                }
                to {
                    transform: translateX(0);
                    opacity: 1;
                }
            }
            
            @keyframes slideOut {
                from {
                    transform: translateX(0);
                    opacity: 1;
                }
                to {
                    transform: translateX(100%);
                    opacity: 0;
                }
            }
        `;
        document.head.appendChild(style);

        // Add close button functionality
        const closeBtn = notification.querySelector('.whatsapp-notification-close');
        closeBtn.addEventListener('click', function () {
            notification.style.animation = 'slideOut 0.3s ease';
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.remove();
                }
            }, 300);
        });

        // Add to page
        document.body.appendChild(notification);

        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.style.animation = 'slideOut 0.3s ease';
                setTimeout(() => {
                    if (notification.parentNode) {
                        notification.remove();
                    }
                }, 300);
            }
        }, 5000);
    }

    function updateCartCount() {
        // Fetch cart count from server
        fetch('/Cart/GetCartCount')
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(data => {
                const cartCount = document.getElementById('cartCount');
                if (cartCount && data && data.count !== undefined) {
                    cartCount.textContent = data.count;
                    cartCount.style.display = data.count > 0 ? 'block' : 'none';
                }
            })
            .catch(error => {
                console.error('Error fetching cart count:', error);
            });
    }

    // Make functions available globally if needed
    window.WhatsAppChat = {
        openChat: function (customMessage = '') {
            const phoneNumber = WHATSAPP_NUMBER;
            const message = customMessage || getProfessionalMessage();
            openWhatsAppChat(phoneNumber, message);
        },
        showWidget: toggleWidget,
        sendCustomMessage: function (message) {
            const phoneNumber = WHATSAPP_NUMBER;
            const encodedMessage = encodeURIComponent(message);
            openWhatsAppChat(phoneNumber, encodedMessage);
        }
    };

    // Add WhatsApp icon animation on hover
    document.addEventListener('DOMContentLoaded', function () {
        const whatsappFloat = document.getElementById('whatsappFloat');
        if (whatsappFloat) {
            whatsappFloat.addEventListener('mouseenter', function () {
                this.style.transform = 'scale(1.1)';
            });

            whatsappFloat.addEventListener('mouseleave', function () {
                this.style.transform = 'scale(1)';
            });
        }
    });

})();

// Add this comprehensive modal initialization code to site.js
(function () {
    'use strict';

    // Initialize product modals on page load and after AJAX updates
    function initializeProductModals() {
        console.log('Initializing product modals...');

        // Remove old event listeners by cloning and replacing all view detail buttons
        const viewDetailButtons = document.querySelectorAll('.btn-view-details');
        
        viewDetailButtons.forEach(button => {
            // Create a new button element to remove all old event listeners
            const newButton = button.cloneNode(true);
            button.parentNode.replaceChild(newButton, button);

            // Add new click event listener
            newButton.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                const modalId = this.getAttribute('data-bs-target');
                console.log('View details clicked for modal:', modalId);

                if (!modalId) {
                    console.error('No modal target specified');
                    return;
                }

                // Check if modal exists
                let modalElement = document.querySelector(modalId);

                if (!modalElement) {
                    console.warn('Modal element not found:', modalId);
                    // Try to find or create the modal
                    createAndShowModal(modalId);
                    return;
                }

                // Show the modal
                try {
                    const modal = new bootstrap.Modal(modalElement, {
                        backdrop: 'static',
                        keyboard: false
                    });
                    modal.show();
                    console.log('Modal shown successfully:', modalId);
                } catch (error) {
                    console.error('Error showing modal:', error);
                }
            });
        });
    }

    function createAndShowModal(modalId) {
        // Extract product ID from modal ID
        const productId = modalId.replace('#productModal_', '').replace('#', '');
        
        if (!productId) {
            console.error('Could not extract product ID from modal ID:', modalId);
            return;
        }

        console.log('Creating modal for product:', productId);

        // Try to get product data from window object or data attributes
        const productCard = document.querySelector(`[data-product-id="${productId}"]`);
        if (!productCard) {
            console.error('Product card not found for ID:', productId);
            return;
        }

        // Extract product data from the card
        const productData = extractProductDataFromCard(productCard, productId);

        // Create the modal HTML
        createProductModalHTML(productData);

        // Now show the modal
        const modalElement = document.querySelector(modalId);
        if (modalElement) {
            setTimeout(() => {
                try {
                    const modal = new bootstrap.Modal(modalElement, {
                        backdrop: 'static',
                        keyboard: false
                    });
                    modal.show();
                } catch (error) {
                    console.error('Error showing newly created modal:', error);
                }
            }, 100);
        }
    }

    function extractProductDataFromCard(card, productId) {
        const title = card.querySelector('.card-title')?.textContent || 'Product';
        const brand = card.querySelector('.card-text.text-muted')?.textContent || '';
        const priceText = card.querySelector('.price')?.textContent || '$0';
        const price = parseFloat(priceText.replace('$', '').replace('Rs ', ''));

        const originalPriceText = card.querySelector('.original-price')?.textContent || '';
        const originalPrice = originalPriceText ? parseFloat(originalPriceText.replace('$', '').replace('Rs ', '')) : 0;

        const ratingText = card.querySelector('.rating .text-muted')?.textContent?.match(/\(([0-9.]+)\)/)?.[1] || '0';
        const rating = parseFloat(ratingText);

        const imagePath = card.querySelector('.product-img')?.src || '';
        const isNew = !!card.querySelector('.product-badge:not(.sale-badge)');
        const isSale = !!card.querySelector('.sale-badge');

        return {
            productId: parseInt(productId),
            name: title,
            brand: brand,
            price: price,
            originalPrice: originalPrice,
            rating: rating,
            imagePath: imagePath,
            isNew: isNew,
            isSale: isSale,
            description: 'Premium quality product available now',
            size: 'S,M,L,XL',
            color: 'Black,White,Blue',
            quantity: 10,
            categoryName: 'Fashion'
        };
    }

    function createProductModalHTML(product) {
        const modalId = `productModal_${product.productId}`;
        
        // Check if modal already exists
        if (document.getElementById(modalId)) {
            console.log('Modal already exists:', modalId);
            return;
        }

        const modalHTML = `
        <div class="modal fade" id="${modalId}" tabindex="-1" aria-labelledby="${modalId}Label" aria-hidden="true">
            <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="${modalId}Label">${escapeHtml(product.name)}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-md-6">
                                ${product.imagePath
                                    ? `<img src="${product.imagePath}" class="modal-product-img w-100 rounded" alt="${escapeHtml(product.name)}">`
                                    : `<img src="https://via.placeholder.com/500x400?text=No+Image" class="modal-product-img w-100 rounded" alt="No Image Available">`
                                }
                            </div>
                            <div class="col-md-6">
                                <h4 class="text-primary">Rs ${product.price.toFixed(2)}</h4>
                                <p class="text-muted">${escapeHtml(product.brand)}</p>
                                <div class="rating mb-3">
                                    ${getRatingStars(product.rating)}
                                    <span class="ms-2">${product.rating.toFixed(1)}</span>
                                </div>
                                <p class="mb-3">Premium quality product available now.</p>

                                <div class="mb-4">
                                    <strong>Quantity:</strong>
                                    <div class="input-group mt-2" style="width: 150px;">
                                        <button class="btn btn-outline-secondary quantity-minus" type="button">-</button>
                                        <input type="text" class="form-control text-center quantity-input"
                                               value="1"
                                               data-product-id="${product.productId}"
                                               data-max-stock="${product.quantity || 10}">
                                        <button class="btn btn-outline-secondary quantity-plus" type="button">+</button>
                                    </div>
                                    <small class="text-muted">${product.quantity || 10} items in stock</small>
                                </div>

                                <div class="d-grid gap-2">
                                    <button class="btn btn-dark btn-lg add-to-cart-from-modal"
                                            data-product-id="${product.productId}"
                                            data-product-name="${escapeHtml(product.name)}">
                                        <i class="fas fa-shopping-cart me-2"></i> Add to Cart
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        `;

        // Create a container if it doesn't exist
        let modalsContainer = document.getElementById('productModalsContainer');
        if (!modalsContainer) {
            modalsContainer = document.createElement('div');
            modalsContainer.id = 'productModalsContainer';
            document.body.appendChild(modalsContainer);
        }

        // Add the modal to the container
        modalsContainer.insertAdjacentHTML('beforeend', modalHTML);

        // Initialize the modal's event listeners
        initializeModalEvents(product.productId);
    }

    function initializeModalEvents(productId) {
        const modalId = `productModal_${productId}`;
        const modalElement = document.getElementById(modalId);

        if (!modalElement) return;

        // Initialize quantity controls
        const quantityMinus = modalElement.querySelector('.quantity-minus');
        const quantityPlus = modalElement.querySelector('.quantity-plus');
        const quantityInput = modalElement.querySelector('.quantity-input');

        if (quantityMinus && quantityInput) {
            quantityMinus.addEventListener('click', function () {
                if (parseInt(quantityInput.value) > 1) {
                    quantityInput.value = parseInt(quantityInput.value) - 1;
                }
            });
        }

        if (quantityPlus && quantityInput) {
            quantityPlus.addEventListener('click', function () {
                const maxStock = parseInt(quantityInput.dataset.maxStock || 100);
                if (parseInt(quantityInput.value) < maxStock) {
                    quantityInput.value = parseInt(quantityInput.value) + 1;
                }
            });
        }

        // Initialize add to cart button
        const addToCartBtn = modalElement.querySelector('.add-to-cart-from-modal');
        if (addToCartBtn) {
            addToCartBtn.addEventListener('click', function (e) {
                e.preventDefault();
                const productId = this.dataset.productId;
                const productName = this.dataset.productName;
                const quantity = quantityInput ? parseInt(quantityInput.value) : 1;

                window.addToCart(productId, productName, quantity);

                // Close modal
                const modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) {
                    modal.hide();
                }
            });
        }
    }

    function getRatingStars(rating) {
        let stars = '';
        for (let i = 1; i <= 5; i++) {
            if (i <= Math.floor(rating)) {
                stars += '<i class="fas fa-star text-warning"></i>';
            } else if (i === Math.ceil(rating) && rating % 1 > 0) {
                stars += '<i class="fas fa-star-half-alt text-warning"></i>';
            } else {
                stars += '<i class="far fa-star text-warning"></i>';
            }
        }
        return stars;
    }

    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Initialize modals when DOM is loaded
    document.addEventListener('DOMContentLoaded', function () {
        console.log('Initializing modals on page load...');
        initializeProductModals();
    });

    // Re-initialize modals after AJAX content update
    // Call this after any AJAX filter/search that updates product grid
    window.reinitializeModals = function () {
        console.log('Re-initializing modals after AJAX update...');
        setTimeout(() => {
            initializeProductModals();
        }, 100);
    };

})();

// Add event listener for dynamically loaded "Add to Cart" buttons in modals
(function () {
    'use strict';

    document.addEventListener('click', function (e) {
        if (e.target && e.target.classList.contains('add-to-cart-from-modal')) {
            const button = e.target;
            const productId = button.getAttribute('data-product-id');
            const productName = button.getAttribute('data-product-name');
            const quantityInput = button.closest('.modal-body')?.querySelector('.quantity-input');
            const quantity = quantityInput ? parseInt(quantityInput.value) : 1;

            window.addToCart(productId, productName, quantity, null, null, e);
        }
    });
})();