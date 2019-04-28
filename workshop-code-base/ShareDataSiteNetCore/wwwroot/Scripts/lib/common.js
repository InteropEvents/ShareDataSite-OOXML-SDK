//Date.prototype.toUTCString = function () {

//    function zeroCompletion(time) {
//        return ("00" + time).slice(-2);
//    }
//    return this.getFullYear() + "-" +
//        zeroCompletion(this.getMonth() + 1) + "-" +
//        zeroCompletion(this.getDate()) + "T" +
//        zeroCompletion(this.getHours()) + ":" +
//        zeroCompletion(this.getMinutes()) + ":" +
//        zeroCompletion(this.getSeconds());
//};

String.prototype.endsWith = function (pattern) {
    var d = this.length - pattern.length;
    return d >= 0 && this.lastIndexOf(pattern) === d;
};

if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] !== 'undefined'
                ? args[number]
                : match
                ;
        });
    };
}

function transDateTime(time) {
    var diff = Math.round(new Date().getTime() / 1000) - Math.round(new Date(time).getTime() / 1000);
    if (diff < 60) {
        return "just recently";
    }
    else if (diff > 60 && diff < 3600) {
        return "{0} minutes ago".format(Math.round(diff / 60));
    }
    else if (diff > 3600 && diff < 3600 * 24) {
        return "{0} hours ago".format(Math.round(diff / 3600));
    }
    else if (diff > 3600 * 24 && diff < 3600 * 24 * 30) {
        return "{0} days ago".format(Math.round(diff / 3600 / 24));
    }
    else if (diff > 3600 * 24 * 30 && diff < 3600 * 24 * 30 * 12) {
        return "{0} months ago".format(Math.round(diff / 3600 / 24 / 30));
    }
    else if (diff > 3600 * 24 * 30 * 12) {
        return "{0} years ago".format(Math.round(diff / 3600 / 24 / 30 / 12));
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
        if (resStatus === 'error') {
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
}();
