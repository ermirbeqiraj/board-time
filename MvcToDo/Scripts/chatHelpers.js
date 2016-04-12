/*
generate a unique id for the list item,
add the message in list,
return id of list to be used in case the insert or send fails
*/
function addMyMsgInList() {
    var uniqueUidMsg = (Date.now().toString(36) + Math.random().toString(36).substr(2,5)).toUpperCase();
    $('#chatList').append('<li id="' + uniqueUidMsg + '"  class="right clearfix">'
                               + '<div class="chat-body clearfix">'
                               + '<div class="header pull-right">'
                               + '<strong class="primary-font">' + htmlEncode($('#displayname').val()) + '</strong>&nbsp;&nbsp;'
                               + '<small class="text-muted">'
                               + '<span class="glyphicon glyphicon-time"></span>'
                               + '<span id="msgTime_' + uniqueUidMsg + '"> Just Now </span>'
                               + '</small>'
                               + '</div>'
                               + '<p class="col-md-10 pull-right">'
                               + htmlEncode($('#message').val())
                               + '</p>'
                               + '</div>'
                               + '</li>');


    $(".panel-body").scrollTop(10000);
    return uniqueUidMsg;
}

/*
call c# for inserting the message in db, in case of success call method for sending the message in hub
*/
function saveMessage(receiver,content,uniqueListId) {
    try {
        $.ajax({
            url: '/Home/InsertMessage',
            dataType: "json",
            type: "POST",
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ to: receiver,message: content }),
            async: true,
            processData: false,
            cache: false,
            success: function (data) {
                if (data.Status) {
                    callHubNow(receiver,content);
                    var dtid = "#msgTime_".concat(uniqueListId);
                    $(dtid).text(data.Content);
                }
                else {
                    console.error(data.Content);
                }
            },
            error: function (xhr) {
                console.error(xhr);
            }
        });
    }
    catch (e) {
        console.error(e.description);
    }
}

/*  -puts the Name of "To" in the hidden field
    -cleans the div of messages
    -read last 20 messages
    -clear the mark of badge
*/
function prepareSend(value) {
    $("#sendto").val(value);
    $("#chatList").empty();
    getlastMessages(value);
    var badgespan = document.getElementById("badge_" + value.toString());
    $(badgespan).hide();
    $("#message").prop('disabled',false);
    $("#sendOnEnterKey").prop('disabled',false);
    $("#sendmessage").prop('disabled',false);
    $('#message').val('').focus();
}

/*
get last messages in case when user clicks an item in list of users
*/
function getlastMessages(value) {
    try {
        $.ajax({
            url: '/Home/GetConversation',
            dataType: "json",
            type: "POST",
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ partner: value }),
            async: true,
            processData: false,
            cache: false,
            success: function (data) {
                var myid = $('#myid').val();
                var toid = $('#sendto').val();

                var myDisplayName = $('#displayname').val();
                var nameof = "displayas_" + toid;
                var otherDisplayName = document.getElementById(nameof).innerText;

                for (var i = 0 ; i < data.length; i++) {
                    var oneitem = data[i];
                    /*   {Created , Sender , Receiver , Message}    */
                    /*  enough with shortcuts we go long way this time */
                    var senderTrimmed = oneitem.Sender.trim();
                    switch (senderTrimmed) {
                        case myid:
                            $('#chatList').append('<li class="right clearfix">'
                               + '<div class="chat-body clearfix">'
                               + '<div class="header pull-right">'
                               + '<strong class="primary-font">' + myDisplayName + '</strong>&nbsp;&nbsp;'
                               + '<small class=" text-muted"><span class="glyphicon glyphicon-time"></span>' + oneitem.Created + '</small>'
                               + '</div>'
                               + '<p class="col-md-10 pull-right">'
                               + htmlEncode(oneitem.Message)
                               + '</p>'
                               + '</div>'
                               + '</li>');
                            break;
                        case toid:
                            $('#chatList').append('<li class="left clearfix">'
                        + '<div class="chat-body clearfix">'
                        + '<div class="header pull-left">'
                        + '<strong class="primary-font">' + otherDisplayName + '</strong>&nbsp;&nbsp;'
                        + '<small class=" text-muted"><span class="glyphicon glyphicon-time"></span>' + oneitem.Created + '</small>'
                        + '</div>'
                        + '<p class="col-md-10 pull-right">'
                        + htmlEncode(oneitem.Message)
                        + '</p>'
                        + '</div>'
                        + '</li>');
                            break;
                    }
                }
                $(".panel-body").scrollTop(10000);
            },
            error: function (xhr) {
                console.error(xhr);
            }
        });
    }
    catch (e) {
        console.error(e.description);
    }
}
