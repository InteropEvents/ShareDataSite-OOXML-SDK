$.graph = function () { };

$.graph.prototype.login = function (token, authorization, expire_time) {
    if (token && authorization) {//called from microsoft authentication in dialog
        expire_time = new Date();
        expire_time.setSeconds(expire_time.getSeconds() + authorization.expires_in);
        expire_time = expire_time.toUTCString();
        window.sessionStorage.token = token;
        window.sessionStorage.authorization = JSON.stringify(authorization);
        window.sessionStorage.expire_time = expire_time;
    } else {//call when page loaded
        if (window.sessionStorage.authorization) {//logined and token saved in sessionStorage
            authorization = JSON.parse(window.sessionStorage.authorization);
            token = authorization.access_token;
            expire_time = window.sessionStorage.expire_time;
        } else {
            console.info("sessionStorage.authorization undefined. login failed.");
            return false;
        }
    }
    this.token = token;
    this.authorization = authorization;
    this.expire_time = expire_time;
    //auto refresh token
    setTimeout(this.refreshToken.bind(this), function () {
        var span = new Date(this.expire_time) - new Date();
        return span - 1000000 < 0 ? 0 : span - 1000000;
    }.bind(this)());
    return true;
};

$.graph.prototype.refreshToken = function () {
    var that = this;
    if (!(this.authorization && this.authorization.refresh_token))
        throw "no authorization or refresh_token set";
    //refresh in backend
    $.ajax({
        url: "/Authorization/RefreshToken",
        data: { refresh_token: this.authorization.refresh_token },
        type: 'POST',
        success: function (res) {
            that.login(res.access_token, res);
        },
        error: function (err) {
            console.error("refreshToken failed");
            console.error(err);
        }
    });
};

$.graph.login = function (auth_url) {
    //intialize graph
    var graph = window.graph = new $.graph();
    var _dlg;

    return function (callback) {
        //login from sessionStorge or microsoft authentication endpoint
        if (graph.login()) {
            callback(true);
        } else {
            //open office dialog window
            Office.context.ui.displayDialogAsync(
                location.protocol + '//' + location.hostname + (location.port ? ':' + location.port : '') + "/Login",
                { height: 80, width: 50 },
                function (result) {
                    _dlg = result.value;
                    _dlg.addEventHandler(Microsoft.Office.WebExtension.EventType.DialogMessageReceived, function (msg) {
                        var authorization = $.hashParam(msg.message);
                        //achieve authentication in backend
                        $.ajax({
                            url: "/authorize",
                            data: {
                                "code": authorization.code
                            },
                            type: 'POST',
                            success: function (data) {
                                var access_token;
                                if (data instanceof Object) {
                                    access_token = data.access_token;
                                } else {
                                    access_token = data.getParam("access_token");
                                }
                                if (graph.login(access_token, data)) {
                                    callback(access_token, data);
                                }
                            },
                            error: function (error) {
                                console.error(error);
                            }
                        });
                        console.log(msg);
                        _dlg.close();
                    });
                });
        }
    };
}();
