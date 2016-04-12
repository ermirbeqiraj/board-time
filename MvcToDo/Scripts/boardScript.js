function setDataMark(argFrom,argId,argMark) {
    $.ajax({
        url: '/TaskItems/BoardData',
        dataType: "json",
        type: "POST",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify({ id: argId,mark: argMark,from:argFrom }),
        async: true,
        processData: false,
        cache: false,
        success: function (data) {
            if (data !== "1") {
                console.error(data);
            }
        },
        error: function (xhr) {
            console.error(xhr);
        }
    });
}

$(document).ready(function () {
    $(function () {
        $(".boardCol").sortable({
            connectWith: ".boardCol",
            handle: ".portlet-header",
            cancel: ".portlet-toggle",
            placeholder: "portlet-placeholder ui-corner-all",
            receive: function (event,ui) {
                setDataMark(ui.sender.attr("id"),ui.item.attr("id"),this.id);
            }
        });
        $(".portlet")
          .addClass("ui-widget ui-widget-content ui-helper-clearfix ui-corner-all")
          .find(".portlet-header")
            .addClass("ui-widget-header ui-corner-all")
            .prepend("<span class='ui-icon ui-icon-minusthick portlet-toggle'></span>");

        $(".portlet-toggle").click(function () {
            var icon = $(this);
            icon.toggleClass("ui-icon-minusthick ui-icon-plusthick");
            icon.closest(".portlet").find(".portlet-content").toggle();
        });
    });
});