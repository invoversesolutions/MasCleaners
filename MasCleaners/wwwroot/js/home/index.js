document.addEventListener("DOMContentLoaded", () => {

    /* =====================================================
       HERO CAROUSEL
    ===================================================== */

    const slides = document.querySelectorAll(".mas-hero-slide");
    const indicators = document.querySelectorAll(".hero-indicator");

    const previousButton = document.getElementById("heroPrevious");
    const nextButton = document.getElementById("heroNext");

    if (slides.length > 0) {

        let currentSlide = 0;
        let autoPlay;


        function showSlide(index) {

            if (index < 0) {
                index = slides.length - 1;
            }

            if (index >= slides.length) {
                index = 0;
            }

            currentSlide = index;


            slides.forEach((slide, i) => {

                slide.classList.toggle(
                    "active",
                    i === currentSlide
                );

            });


            indicators.forEach((indicator, i) => {

                indicator.classList.toggle(
                    "active",
                    i === currentSlide
                );

            });

        }


        function nextSlide() {

            showSlide(currentSlide + 1);

        }


        function previousSlide() {

            showSlide(currentSlide - 1);

        }


        function startAutoPlay() {

            clearInterval(autoPlay);

            autoPlay = setInterval(() => {

                nextSlide();

            }, 6000);

        }


        if (nextButton) {

            nextButton.addEventListener(
                "click",
                () => {

                    nextSlide();

                    startAutoPlay();

                }
            );

        }


        if (previousButton) {

            previousButton.addEventListener(
                "click",
                () => {

                    previousSlide();

                    startAutoPlay();

                }
            );

        }


        indicators.forEach((indicator, index) => {

            indicator.addEventListener(
                "click",
                () => {

                    showSlide(index);

                    startAutoPlay();

                }
            );

        });


        showSlide(0);

        startAutoPlay();

    }


    /* =====================================================
       SERVICE SEARCH
    ===================================================== */

    const searchInput =
        document.getElementById("serviceSearch");

    const serviceItems =
        document.querySelectorAll(".service-item");

    const noServices =
        document.getElementById("noServices");


    if (searchInput && serviceItems.length > 0) {

        searchInput.addEventListener(
            "input",
            () => {

                const searchTerm =
                    searchInput.value
                        .trim()
                        .toLowerCase();

                let visibleServices = 0;


                serviceItems.forEach(item => {

                    const searchData =
                        (
                            item.dataset.search || ""
                        ).toLowerCase();


                    const matches =
                        searchTerm === "" ||
                        searchData.includes(searchTerm);


                    item.style.display =
                        matches
                            ? ""
                            : "none";


                    if (matches) {
                        visibleServices++;
                    }

                });


                if (noServices) {

                    noServices.style.display =
                        visibleServices === 0
                            ? "block"
                            : "none";

                }

            }
        );

    }

});