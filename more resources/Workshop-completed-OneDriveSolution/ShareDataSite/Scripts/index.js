//import { Promise, resolve } from "../../../../../../AppData/Local/Microsoft/TypeScript/2.6/node_modules/@types/bluebird";

Date.prototype.toUTCString = function () {

    function zeroCompletion(time) {
        return ("00" + time).slice(-2);
    }
    return this.getFullYear() + "-" +
        zeroCompletion(this.getMonth() + 1) + "-" +
        zeroCompletion(this.getDate()) + "T" +
        zeroCompletion(this.getHours()) + ":" +
        zeroCompletion(this.getMinutes()) + ":" +
        zeroCompletion(this.getSeconds())
}

if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
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

Object.defineProperty(Date, "timeZone", {
    get: function () {
        var hourOffset = parseInt(new Date().getTimezoneOffset() / 60);
        return "Etc/GMT" +
            (hourOffset > 0 ? "+" + hourOffset :
                hourOffset == 0 ? "" :
                    "-" + Math.abs(hourOffset));
    }
})

var graph;

String.prototype.endsWith = function (pattern) {
    var d = this.length - pattern.length;
    return d >= 0 && this.lastIndexOf(pattern) === d;
};

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
                "Prefer": 'outlook.timezone="' + Date.timeZone + '"'
            },
            request_body: {
                stamp: app.makeid()//a random string in order to prevent cache
            }
        },
        "GET",
        true);
}

function GetFiles(prefixUrl) {
    return new Promise(function (resolve) {
        graph.GetFileList(prefixUrl).then(function (that) {
            var promises = [], data = [];
            $.each(that.res.value, function (i, item) {
                if (item["@microsoft.graph.downloadUrl"] &&
                    (item.name.endsWith(".pptx") || item.name.endsWith(".ppt") || item.name.endsWith(".docx") || item.name.endsWith(".doc") || item.name.endsWith(".xlsx") || item.name.endsWith(".xls"))) {
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
            })
        });
    }).
        //concat all promise result to one array
        then(function (data) {
            var res = [];
            [].map.call(data, function (item) {
                res = res.concat(item);
            });
            return res;
        })

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
            })
    });
}