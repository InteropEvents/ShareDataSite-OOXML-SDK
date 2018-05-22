"use strict";
!function () {
    var app = window.app = {};
    var onready = app.onready = [];
    //register function and called when login success
    app.ready = function (func) {
        onready.push(func);
    }
}()
//when Office loaded complete
Office.initialize = function () {
    var app = window.app;

    $(document).ready(function () {
        //call function in app.onready when login success
        $.graph.login(function (res) {
            if (res) {
                app.onready.map(function (func) {
                    func();
                })
            }
        });

        app.makeid = function () {
            var text = "";
            var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            for (var i = 0; i < 5; i++)
                text += possible.charAt(Math.floor(Math.random() * possible.length));

            return text;
        }

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
                            var tempStart = address.substring(exclamationMark + 1, colon == -1 ? address.length : colon);
                            var firstDigit = tempStart.match(/\d/);
                            var indexed = tempStart.indexOf(firstDigit);
                            row = parseInt(tempStart.substr(indexed)) + tableLength - 1;
                            start = tempStart.substr(0, indexed);
                            end = convert26BSToDS(start) + tableWidth - 1;
                            if (colon == -1) {
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
        }

    })


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
            var m = num % 26
            if (m == 0) {
                m = 26;
            }
            code = String.fromCharCode(64 + parseInt(m)) + code;
            num = (num - m) / 26;
        }
        return code;
    }
}

//convert hash string to Object
$.hashParam = function (hashstr) {
    var hash = hashstr.split("&");
    var params = {}
    for (var i = 0; i < hash.length; i++) {
        var split = hash[i].indexOf("=");
        params[hash[i].substring(0, split)] = hash[i].substring(split + 1);
    }
    return params;
}
//package some request method
var common = function () {
    //get row number and file from stack
    function codeRowNum(depth) {
        if (!depth)
            depth = 1;
        try {
            throw new Error();
        } catch (e) {
            var stack = e.stack.substring(5).replace(/[\r\n]/i, "").split(/[\r\n]/g);
            var codeRow = stack[depth];
            return codeRow.substring(codeRow.lastIndexOf("/") + 1, codeRow.lastIndexOf(":"));
        }
    }

    function response(res, resStatus, resPromiseObj, isLogin) {
        //if requeset is error and response data is different from the succee
        if (resStatus == 'error') {
            var temp = res;
            res = res.responseText;
            resPromiseObj = temp;
        }

        this.res = this.response = res;

        if (!isLogin) {
            var headerArr = resPromiseObj.getAllResponseHeaders().trim().split(/[\r\n]+/);
            var headerObj = {};
            headerArr.forEach(function (line) {
                var parts = line.split(': ');
                var header = parts.shift();
                var value = parts.join(': ');
                headerObj[header] = value;
            });

            vm.response.response_body = res;
            vm.response.response_header = headerObj;
        }

        this.status = resStatus;
    }

    function request(url, data, method, isLogin) {
        var stack = codeRowNum(3);

        if (!method && typeof data === "string")
            method = data, data = null;
        //use Promise to handle async process
        var promise = new Promise(function (resolve, reject) {
            var option = {
                url: url,
                headers: typeof data.request_header == 'object' ? data.request_header : JSON.parse(data.request_header),
                method: method,
                success: function (res, resStatus, resPromiseObj) {
                    var callResponse = response.bind(this, res, resStatus, resPromiseObj, isLogin);
                    callResponse()
                    resolve(this);
                },
                error: function (res, resStatus, resPromiseObj) {
                    var callResponse = response.bind(this, res, resStatus, resPromiseObj, isLogin);
                    callResponse()
                    reject(this);
                }
            };
            option.context = {
                url: option.url,
                method: option.method,
                codeSituation: stack,
                data: {}
            }
            if (data && data.request_body) {
                option.data = typeof data.request_body === 'object' ? data.request_body : JSON.parse(data.request_body);
                option.context.data = option.data;
            }
            $.ajax(option);
        })
        promise.catch(function (ajax) {
            var err = ajax.res;
            console.info(err.status + " " + err.statusText)
            console.info(err.responseText)
        });

        return promise;
    }

    $.post = function post(url, data) {
        if (typeof data === "object")//used to crossDomain
            data = JSON.stringify(data);
        return request.call(this, url, data, "POST");
    }

    $.get = function get(url, data) {
        return request.call(this, url, data, "GET");
    }

    $.patch = function patch(url, data) {
        if (typeof data === "object")//used to crossDomain
            data = JSON.stringify(data);
        return request.call(this, url, data, "PATCH");
    }

    $.del = function del(url, data) {
        if (typeof data === "object")//used to crossDomain
            data = JSON.stringify(data);
        return request.call(this, url, data, "DELETE");
    }

    return {
        Request: request
    };
}()
