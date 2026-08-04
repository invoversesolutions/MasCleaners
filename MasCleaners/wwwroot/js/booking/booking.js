document.addEventListener("DOMContentLoaded", function () {

    const bookingForm = document.getElementById("bookingForm");

    const preferredDate =
        document.getElementById("preferredDate");


    /* =========================================================
       PREVENT PAST DATES
    ========================================================== */

    if (preferredDate) {

        const today = new Date();

        const year = today.getFullYear();

        const month = String(today.getMonth() + 1)
            .padStart(2, "0");

        const day = String(today.getDate())
            .padStart(2, "0");

        preferredDate.min =
            `${year}-${month}-${day}`;
    }


    /* =========================================================
       TEMPORARY FORM SUBMISSION
    ========================================================== */

    if (bookingForm) {

        bookingForm.addEventListener("submit", function (event) {

            event.preventDefault();

            /*
                Backend/payment integration will be added later.

                For now we simply confirm that the form
                has passed browser validation.
            */

            if (!bookingForm.checkValidity()) {

                bookingForm.reportValidity();

                return;
            }


            console.log("Booking details ready.");

            /*
                Later:

                1. Save booking
                2. Calculate price
                3. Move to Payment
                4. Process payment
                5. Move to Confirmed
            */

        });

    }

});