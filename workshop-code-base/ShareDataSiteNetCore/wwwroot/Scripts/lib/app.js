"use strict";
!function () {
    var app = window.app = {};
    var onready = app.onready = [];
    //register function and called when login success
    app.ready = function (func) {
        onready.push(func);
    };
}();

//when Office loaded complete
Office.initialize = function () {
    var app = window.app;

    $(document).ready(function () {

        app.makeid = function () {
            var text = "";
            var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            for (var i = 0; i < 5; i++)
                text += possible.charAt(Math.floor(Math.random() * possible.length));

            return text;
        };

        app.insertImage = function (base64, callback) {
            Office.context.document.setSelectedDataAsync(base64, {
                coercionType: Office.CoercionType.Image,
            }, function (asyncResult) {
                callback && callback(asyncResult);
            });
        };

        app.insertText = function (text, callback) {
            Office.context.document.setSelectedDataAsync(text, {
                coercionType: Office.CoercionType.Text,
            }, function (asyncResult) {
                callback && callback(asyncResult);
            });
        };

        app.insertHtml = function (html, callback) {
            Office.context.document.setSelectedDataAsync(html, {
                coercionType: Office.CoercionType.Html,
            }, function (asyncResult) {
                callback && callback(asyncResult);
            });
        };

        app.insertTable = function (tableBody, tableHeader, callback) {
            var table = new Office.TableData();
            if (tableHeader && tableHeader.length) {
                table.headers = [tableHeader];
            }
            table.rows = tableBody;

            Office.context.document.setSelectedDataAsync(table, {
                coercionType: Office.CoercionType.Table,
            }, function (asyncResult) {
                callback && callback(asyncResult);
            });
        };

        if (Office.context.requirements.isSetSupported("ExcelApi")) {
            //rewrite InsertTable in Excel
            app.insertTable = function (tableBody, tableHeader) {
                Excel.run(function (context) {
                    var range = context.workbook.getSelectedRange();
                    range.load("address");

                    var tableWidth = tableBody ? tableBody.length ? tableBody[0].length : 0 : 0;
                    var tableLength = tableBody.length;

                    return context.sync().then(function () {
                        var address = function () {
                            var address = range.address;
                            var exclamationMark = range.address.lastIndexOf("!"), colon = address.lastIndexOf(":");
                            var start, end, row;
                            var tempStart = address.substring(exclamationMark + 1, colon === -1 ? address.length : colon);
                            var firstDigit = tempStart.match(/\d/);
                            var indexed = tempStart.indexOf(firstDigit);
                            row = parseInt(tempStart.substr(indexed)) + tableLength - 1;
                            start = tempStart.substr(0, indexed);
                            end = convert26BSToDS(start) + tableWidth - 1;
                            if (colon === -1) {
                                return address + ":" + convertDSTo26BS(end) + row;
                            } else {
                                return address.substring(0, colon) + ":" + convertDSTo26BS(end) + row;
                            }
                        }()
                        range = context.workbook.worksheets.getActiveWorksheet().getRange(address);
                        range.load("values");
                    }).then(function () {
                        range.values = tableBody;
                    });
                })
                    .catch(function (error) {
                        console.log("Error: " + error);
                        if (error instanceof OfficeExtension.Error) {
                            app.dialog(error.name, error.message)
                            console.log("Debug info: " + JSON.stringify(error.debugInfo));
                        }
                    });
            }
        }

        //show dialog animation
        app.dialog = function (title, content) {
            var dialog = $("#ShareDatadialog");
            dialog.find(".ShareDatadialog-title").text(title);
            dialog.find(".ShareDatadialog-content").text(content);
            dialog.slideDown();
            setTimeout(function () {
                dialog.slideUp();
            }, 3000);
        }

        var element = document.querySelector('.ms-MessageBanner');
        var messageBanner = new fabric.MessageBanner(element);
        messageBanner.hideBanner();
        app.showNotification = function (header, content) {
            $("#notification-header").text(header);
            $("#notification-body").text(content);
            messageBanner.showBanner();
            messageBanner.toggleExpansion();

            setTimeout(messageBanner.hideBanner, 3000);
        };

        //call function in app.onready when login success
        $.graph.login(function (res) {
            if (res) {
                app.onready.map(function (func) {
                    func();
                });
            }
        });
    });

    function convert26BSToDS(code) {
        var num = -1;
        var reg = /^[A-Z]+$/g;
        if (!reg.test(code)) {
            return num;
        }
        num = 0;
        for (var i = code.length - 1, j = 1; i >= 0; i-- , j *= 26) {
            num += (code[i].charCodeAt() - 64) * j;
        }
        return num;
    }

    function convertDSTo26BS(num) {
        var code = '';
        var reg = /^\d+$/g;
        if (!reg.test(num)) {
            return code;
        }
        while (num > 0) {
            var m = num % 26;
            if (m === 0) {
                m = 26;
            }
            code = String.fromCharCode(64 + parseInt(m)) + code;
            num = (num - m) / 26;
        }
        return code;
    }
}

var graph;

$.graph.prototype.GetFileList = function (prefixUrl) {
    //https://docs.microsoft.com/en-us/onedrive/developer/rest-api/api/driveitem_list_children
    var url = prefixUrl + "/children?select=name,id,webUrl,@microsoft.graph.downloadUrl,createdDateTime,folder,parentReference";
    return common.Request.call(
        this,
        url,
        {
            request_header: {
                'Authorization': 'Bearer ' + window.sessionStorage.token,
                "Content-Type": "application/json",
            },
            request_body: {
                stamp: app.makeid()//a random string in order to prevent cache
            }
        },
        "GET",
        true);
};

function GetFiles(prefixUrl) {
    return new Promise(function (resolve) {
        graph.GetFileList(prefixUrl).then(function (that) {
            var promises = [], data = [];
            $.each(that.res.value, function (i, item) {
                if (item["@microsoft.graph.downloadUrl"] &&
                    (item.name.endsWith(".pptx") || item.name.endsWith(".docx") || item.name.endsWith(".xlsx"))) {
                    var object = {
                        Id: item.id,
                        Name: item.name,
                        DownloadPath: item["@microsoft.graph.downloadUrl"],
                        Path: item.parentReference.path,
                        CreatedDateTime: item.createdDateTime
                    };
                    data.push(object);
                }
                else if (item.folder && item.folder.childCount > 0) {
                    prefixUrl = "https://graph.microsoft.com/v1.0/me" + item.parentReference.path + "/" + item.name + ":"
                    promises.push(GetFiles(prefixUrl));
                }
            });
            if (data.length) {
                promises.push(data);
            }
            //sync every promise
            Promise.all(promises).then(function (promises) {
                resolve(promises);
            });
        });
    }).
        //concat all promise result to one array
        then(function (data) {
            var res = [];
            [].map.call(data, function (item) {
                res = res.concat(item);
            });
            return res;
        });

}

function GetRawFiles(prefixUrl) {
    return graph.GetFileList(prefixUrl).then(function (that) {
        return [].filter.call(that.res.value, function (item) {
            return item["@microsoft.graph.downloadUrl"] && item.name.endsWith(".rawdata");
        }).
            map(function (item) {
                return {
                    Name: item.name,
                    DownloadPath: item["@microsoft.graph.downloadUrl"],
                    CreatedDateTime: item.createdDateTime
                };
            });
    });
}