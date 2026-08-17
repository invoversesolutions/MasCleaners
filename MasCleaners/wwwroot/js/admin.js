document.addEventListener("DOMContentLoaded", function () {

    const menuButton = document.getElementById("adminMenuToggle");
    const sidebar = document.getElementById("adminSidebar");
    const overlay = document.getElementById("adminOverlay");

    if (!menuButton || !sidebar || !overlay) {
        return;
    }


    function openMenu() {

        sidebar.classList.add("active");
        overlay.classList.add("active");

    }


    function closeMenu() {

        sidebar.classList.remove("active");
        overlay.classList.remove("active");

    }


    menuButton.addEventListener("click", function () {

        if (sidebar.classList.contains("active")) {
            closeMenu();
        }
        else {
            openMenu();
        }

    });


    overlay.addEventListener("click", function () {

        closeMenu();

    });


    window.addEventListener("resize", function () {

        if (window.innerWidth > 991) {
            closeMenu();
        }

    });

});