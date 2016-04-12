/*
 * check : 
 * https://developer.mozilla.org/en-US/docs/Web/API/Page_Visibility_API?redirectlocale=en-US&redirectslug=DOM%2FUsing_the_Page_Visibility_API
 */
(function () {
    'use strict';
    // Set the name of the "hidden" property and the change event for visibility
    var hidden,visibilityChange;
    if (typeof document.hidden !== "undefined") {
        hidden = "hidden";
        visibilityChange = "visibilitychange";
    } else if (typeof document.mozHidden !== "undefined") { // Firefox up to v17
        hidden = "mozHidden";
        visibilityChange = "mozvisibilitychange";
    } else if (typeof document.webkitHidden !== "undefined") { // Chrome up to v32, Android up to v4.4, Blackberry up to v10
        hidden = "webkitHidden";
        visibilityChange = "webkitvisibilitychange";
    }

    var controlToCheck = document.getElementById("chatTabIsVisibile");
    function handleVisibilityChange() {
        if (document[hidden]) {
            $(controlToCheck).val("false");
        } else {
            $(controlToCheck).val("true");
        }
    }

    // Warn if the browser doesn't support addEventListener or the Page Visibility API
    if (typeof document.addEventListener === "undefined" || typeof document[hidden] === "undefined") {
        console.error("Your browser does not support Page Visibility API.");
    } else {
        // Handle page visibility change   
        document.addEventListener(visibilityChange,handleVisibilityChange,false);
    }
})();