// This optional function html-encodes messages for display in the page.
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}

function notifyMe(who,what) {
    var options = {
        body: what.toString(),
        icon: '~/Content/image/iconNotify.png'
    }
    // Let's check if the browser supports notifications
    if (!("Notification" in window)) {
        console.error("This browser does not support desktop notification");
    }

        // Let's check whether notification permissions have already been granted
    else if (Notification.permission === "granted") {
        // If it's okay let's create a notification
        var notification = new Notification(who,options);
        setTimeout(notification.close.bind(notification),4500);

    }

        // Otherwise, we need to ask the user for permission
    else if (Notification.permission !== 'denied') {
        Notification.requestPermission(function (permission) {
            // If the user accepts, let's create a notification
            if (permission === "granted") {
                var notification = new Notification(who,options);
                setTimeout(notification.close.bind(notification),4500);
            }
        });
    }

    // At last, if the user has denied notifications, and you 
    // want to be respectful there is no need to bother them any more.
}
