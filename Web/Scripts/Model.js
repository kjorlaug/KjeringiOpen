// Namespace
var Kjeringi = {};

// Models

Kjeringi.system = function (sysname, sysId, timestamp) {
    var self = this;
    self.sysname = sysname;
    self.id = sysId;
    self.timestamp = timestamp;
}

Kjeringi.logMessage = function (msg) {
    var self = this;
    self.msg = msg;
}

// ViewModels

Kjeringi.connectedSystemViewModel = function () {
    var self = this;
    self.online = ko.observableArray();
    self.offline = ko.observableArray();
    self.log = ko.observableArray();
    self.customRemoveOnline = function (sysToRemove) {
        var sysIdToRemove = sysToRemove.id;
        self.online.remove(function (item) {
            return item.id === sysIdToRemove;
        });
    }
    self.customRemoveOffline = function (sysToRemove) {
        var sysIdToRemove = sysToRemove.id;
        self.offline.remove(function (item) {
            return item.id === sysIdToRemove;
        });
    }
}