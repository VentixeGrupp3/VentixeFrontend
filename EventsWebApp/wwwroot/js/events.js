document.addEventListener('DOMContentLoaded', function() {
    // Get DOM elements with null checks
    const searchInput = document.getElementById('searchInput');
    const categoryFilter = document.getElementById('categoryFilter');
    const viewToggleBtns = document.querySelectorAll('.view-toggle-btn');
    const eventsContainer = document.getElementById('eventsContainer');

    // Only add event listeners if elements exist
    if (searchInput) {
        searchInput.addEventListener('input', filterEvents);
    }

    if (categoryFilter) {
        categoryFilter.addEventListener('change', filterEvents);
    }

    if (viewToggleBtns.length > 0) {
        viewToggleBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                toggleView(this.dataset.view);
            });
        });
    }

    function filterEvents() {
        if (!eventsContainer) return;

        const searchTerm = searchInput ? searchInput.value.toLowerCase() : '';
        const selectedCategory = categoryFilter ? categoryFilter.value : '';
        const eventCards = eventsContainer.getElementsByClassName('event-card');

        Array.from(eventCards).forEach(card => {
            const title = card.querySelector('.event-card-title').textContent.toLowerCase();
            const category = card.querySelector('.event-card-badge').textContent;
            
            const matchesSearch = title.includes(searchTerm);
            const matchesCategory = !selectedCategory || category === selectedCategory;
            
            if (matchesSearch && matchesCategory) {
                card.style.display = '';
            } else {
                card.style.display = 'none';
            }
        });
    }

    function toggleView(view) {
        if (!eventsContainer) return;

        // Remove active class from all buttons
        viewToggleBtns.forEach(btn => btn.classList.remove('active'));
        
        // Add active class to clicked button
        event.currentTarget.classList.add('active');
        
        // Switch view
        if (view === 'list') {
            eventsContainer.classList.remove('events-grid');
            eventsContainer.classList.add('events-list');
        } else {
            eventsContainer.classList.remove('events-list');
            eventsContainer.classList.add('events-grid');
        }
    }
});