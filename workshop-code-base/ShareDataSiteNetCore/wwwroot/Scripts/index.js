var isMouseDown = false;
var startRowIndex = null;
var startCellIndex = null;

var closedialog = function () {
    $("#dialogcontainer").removeClass("slideIn");
    $("#dialogcontainer").addClass("slideOut");
}
var showdialog = function () {
    $("#dialogcontainer").addClass("slideIn");
    $("#dialogcontainer").removeClass("slideOut").removeClass("hidden");
    var content = $(".content", "#dialogcontainer");
    content.html("");
    $("#mesh").show();

    function showTopBorder() {
        $(".text", content).removeClass("top");
        $(".text", content).not(".hidden").eq(0).addClass("top")
    }

    return {
        load: function (html) {
            $("#mesh").hide();
            content.append($("#checkButton").html());
            content.append(html);
            showTopBorder();
            var ButtonElements = document.querySelectorAll(".ms-Button");
            $(".ms-Button", ".btnlist").click(function () {
                if ($(this).hasClass("ms-Button--primary")) {
                    $(this).removeClass("ms-Button--primary")
                    $("." + $(this).data("type"), "#dialogcontainer").addClass("hidden");
                } else {
                    $(this).addClass("ms-Button--primary")
                    $("." + $(this).data("type"), "#dialogcontainer").removeClass("hidden");
                }
                showTopBorder();
            })
        }
    }
}

var selectTo = function (table, cell) {

    var row = cell.parent();
    var cellIndex = cell.index();
    var rowIndex = row.index();
    var rowStart, rowEnd, cellStart, cellEnd;

    if (rowIndex < startRowIndex) {
        rowStart = rowIndex;
        rowEnd = startRowIndex;
    } else {
        rowStart = startRowIndex;
        rowEnd = rowIndex;
    }

    if (cellIndex < startCellIndex) {
        cellStart = cellIndex;
        cellEnd = startCellIndex;
    } else {
        cellStart = startCellIndex;
        cellEnd = cellIndex;
    }
    for (var i = rowStart; i <= rowEnd; i++) {
        var rowCells = table.find("tr").eq(i).find("td");
        for (var j = cellStart; j <= cellEnd; j++) {
            rowCells.eq(j).addClass("selected");
        }
    }
}

var fileListVM = new Vue({
    el: "#filelist",
    data: {
        fileList: [],
        queryUrl: '',
    },
    methods: {
        getOneDriveFileList: function () {
            var self = this;
            var prefixUrl = "https://graph.microsoft.com/v1.0/me/drive/root";
            GetFiles(prefixUrl).then(function (data) {
                self.fileList = data;
            });
        },
        generateRaw: function (path, id) {
            var _dia = showdialog();
            console.log("file path: " + path);
            var data = {};
            data.accessToken = sessionStorage.token;
            data.downloadUri = path;
            data.fileId = id;
            $.ajax({
                url: "/api/RawData",
                method: "post",
                data: JSON.stringify(data),
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                success: function (data) {
                    //Clear the dialog content
                    var content = $(".content", "#dialogcontainer");
                    content.html("");
                    _dia.load(data);
                    //Get table selected area content
                    var alltable = $(".table");
                    alltable.find("td").mousedown(function (e) {
                        var table = $(this).parent().parent().parent();
                        isMouseDown = true;
                        table.prev().css("display", "block");
                        var cell = $(this);
                        table.find(".selected").removeClass("selected"); // deselect everything
                        if (e.shiftKey) {
                            selectTo(table, cell);
                        } else {
                            cell.addClass("selected");
                            startCellIndex = cell.index();
                            startRowIndex = cell.parent().index();
                        }

                        //insert table buttion click event
                        table.prev().click(function (e) {
                            var tableRegion = new Array();
                            table.find("tr").each(function () {
                                var rowArray = new Array();
                                var rowFlag = false;
                                $(this).find("td").each(function () {
                                    if ($(this).hasClass("selected")) {
                                        rowArray.push($(this).text());
                                        rowFlag = true;
                                    }
                                });
                                if (rowFlag) {
                                    tableRegion.push(rowArray);
                                }
                            });
                            console.log(tableRegion);
                            app.insertTable(tableRegion, null);

                        });

                        return false; // prevent text selection
                    })
                        .mouseover(function () {
                            if (!isMouseDown) return;
                            var table = $(this).parent().parent().parent();
                            table.find(".selected").removeClass("selected");
                            selectTo(table, $(this));
                        })
                        .bind("selectstart", function () {
                            return false;
                        });

                    $("#dialogcontainer .content").find("button.btn").click(function (e) {
                        var table = $(this).next("table");
                        var thead = table.find("thead th").map(function () {
                            return $(this).text();
                        })
                        var tbody = table.find("tbody tr").map(function () {
                            return $(this).find("td").map(function () {
                                return $(this).text();
                            });
                        })
                        thead = [].slice.call(thead);
                        tbody = [].slice.call(tbody);
                        tbody = [].map.call(tbody, function (item) {
                            return [].slice.call(item);
                        });
                        app.insertTable(tbody, thead);
                    });
                    $("#dialogcontainer .content").find(".base.text").click(function () {
                        app.insertText($(this).text());
                    });
                    $("#dialogcontainer .content").find(".base.image").click(function () {
                        var base64 = $(this).find("img").attr("src").substring($(this).find("img").attr("src").indexOf(",") + 1);
                        app.insertImage(base64);
                    });
                },
                error: function (error) {
                    closedialog();
                    if (error.responseText) {
                        app.showNotification("Error：", error.responseText);
                    }
                },
                complete: function (data) {

                }
            });
        },
        transDateTime: transDateTime
    }
});

app.ready(function (graph) {
    fileListVM.getOneDriveFileList();
    //spinner initialize
    var SpinnerElements = document.querySelectorAll(".ms-Spinner");
    for (var i = 0; i < SpinnerElements.length; i++) {
        new fabric['Spinner'](SpinnerElements[i]);
    }
});

$(document).mouseup(function () {
    isMouseDown = false;
});