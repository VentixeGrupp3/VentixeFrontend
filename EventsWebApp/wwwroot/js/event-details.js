function updateTicketPrice(selectElement) {
    const selectedOption = selectElement.options[selectElement.selectedIndex];
    const priceField = document.querySelector('#ticketPrice');
    const descriptionDiv = document.querySelector('#ticketDescription');
    const descriptionText = document.querySelector('#descriptionText');
    const quantityInput = document.querySelector('#ticketQuantity');

    if (selectedOption.value && priceField) {
        const price = selectedOption.getAttribute('data-price') || '0';
        const available = parseInt(selectedOption.getAttribute('data-available')) || 0;
        const description = selectedOption.getAttribute('data-description') || '';

        priceField.value = price;

        // Update quantity max based on availability
        if (available > 0 && available < 10) {
            quantityInput.max = available;
            if (parseInt(quantityInput.value) > available) {
                quantityInput.value = available;
            }
        } else {
            quantityInput.max = 10;
        }

        // Show description if available
        if (description) {
            descriptionText.textContent = description;
            descriptionDiv.style.display = 'block';
        } else {
            descriptionDiv.style.display = 'none';
        }

        updateTotal();
    } else {
        descriptionDiv.style.display = 'none';
        document.querySelector('#totalDisplay').style.display = 'none';
    }
}

function updateTotal() {
    const priceField = document.querySelector('#ticketPrice');
    const quantityInput = document.querySelector('#ticketQuantity');
    const totalDisplay = document.querySelector('#totalDisplay');
    const totalAmount = document.querySelector('#totalAmount');

    if (priceField && quantityInput && priceField.value && quantityInput.value) {
        const price = parseFloat(priceField.value) || 0;
        const quantity = parseInt(quantityInput.value) || 0;
        const total = price * quantity;

        totalAmount.textContent = total.toFixed(2);
        totalDisplay.style.display = 'block';
    } else {
        totalDisplay.style.display = 'none';
    }
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    const quantityInput = document.querySelector('#ticketQuantity');
    if (quantityInput) {
        quantityInput.addEventListener('change', updateTotal);
        quantityInput.addEventListener('input', updateTotal);
    }
});