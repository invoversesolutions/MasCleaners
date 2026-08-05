document.addEventListener("DOMContentLoaded", () => {

    const menuButton = document.getElementById("mobileMenuButton");
    const mobileMenu = document.getElementById("mobileMenu");

    if (!menuButton || !mobileMenu)
        return;

    menuButton.addEventListener("click", () => {

        mobileMenu.classList.toggle("active");
        menuButton.classList.toggle("active");

    });

});