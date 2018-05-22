Office.initialize = function (reason) {
    $(document).ready(function () {
        if (window.location.search && window.location.search.indexOf("?code=") == 0) {
            //post message to add-in page and trigger event named Microsoft.Office.WebExtension.EventType.DialogMessageReceived
            Office.context.ui.messageParent(window.location.search.substr(1));
        }
    });
}