let currentSlide = 0;
const slides = document.querySelectorAll('.carousel-slide');
const dots = document.querySelectorAll('.nav-dot');
const totalSlides = slides.length;

function updateCarousel() {
    const wrapper = document.getElementById('carouselWrapper');
    wrapper.style.transform = `translateX(-${currentSlide * 100}%)`;

    dots.forEach((dot, index) => {
        dot.classList.toggle('active', index === currentSlide);
    });
}

function moveSlide(direction) {
    currentSlide += direction;
    if (currentSlide < 0) {
        currentSlide = totalSlides - 1;
    } else if (currentSlide >= totalSlides) {
        currentSlide = 0;
    }
    updateCarousel();
}

function goToSlide(index) {
    currentSlide = index;
    updateCarousel();
}

let autoSlideInterval = setInterval(() => {
    moveSlide(1);
}, 5000);

const carouselContainer = document.querySelector('.carousel-container');
carouselContainer.addEventListener('mouseenter', () => {
    clearInterval(autoSlideInterval);
});

carouselContainer.addEventListener('mouseleave', () => {
    autoSlideInterval = setInterval(() => {
        moveSlide(1);
    }, 5000);
});

document.addEventListener('keydown', (e) => {
    if (e.key === 'ArrowLeft') {
        moveSlide(-1);
    } else if (e.key === 'ArrowRight') {
        moveSlide(1);
    }
});